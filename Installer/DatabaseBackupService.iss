; Inno Setup Script for Database Backup Service (.NET Framework 4.8)
; Requires Inno Setup 6.x - https://jrsoftware.org/isinfo.php
;
; Build steps:
;   1. Build both projects in Release configuration first (see build-installer.ps1)
;   2. Open this file in Inno Setup Compiler and click Compile, OR
;      run: build-installer.ps1

#define MyAppName      "Database Backup Service"
#define MyAppVersion   "1.0.0"
#define MyAppPublisher "ePoint"
#define MyServiceName  "DatabaseBackupService"
#define MyServiceExe   "DatabaseBackupService.NetFx.exe"
#define MyConfigExe    "DatabaseBackupService.ConfigUI.NetFx.exe"
#define MyInstallDir   "{commonpf}\ePoint\DatabaseBackup"

[Setup]
AppId={{A5E91E7D-1F3B-4A2E-9C3D-8B7A5E91E7D1}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={#MyInstallDir}
DisableDirPage=yes
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=DatabaseBackupServiceSetup
Compression=lzma2
SolidCompression=yes
; Require administrator for service installation
PrivilegesRequired=admin
; Do not allow non-admin users to install
PrivilegesRequiredOverridesAllowed=

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; Service executable - overwrite always
Source: "..\DatabaseBackupService.NetFx\bin\Release\{#MyServiceExe}"; \
    DestDir: "{app}"; Flags: ignoreversion

; Config UI executable - overwrite always
Source: "..\DatabaseBackupService.ConfigUI.NetFx\bin\Release\{#MyConfigExe}"; \
    DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Desktop shortcut to the Config UI
Name: "{commondesktop}\{#MyAppName} Config"; \
    Filename: "{app}\{#MyConfigExe}"; \
    Comment: "Configure the Database Backup Service"

[Run]
; Start the service silently right after installation
Filename: "{sys}\sc.exe"; \
    Parameters: "start {#MyServiceName}"; \
    StatusMsg: "Starting {#MyAppName}..."; \
    Flags: runhidden nowait

; Optional post-install checkbox – lets the user choose whether to start now
Filename: "{sys}\sc.exe"; \
    Parameters: "start {#MyServiceName}"; \
    Description: "Start {#MyAppName} now"; \
    Flags: runhidden nowait postinstall skipifsilent unchecked

[UninstallRun]
; Stop and remove service on uninstall
Filename: "{sys}\sc.exe"; \
    Parameters: "stop {#MyServiceName}"; \
    Flags: runhidden nowait; \
    RunOnceId: "StopService"
Filename: "{sys}\cmd.exe"; \
    Parameters: "/c ping -n 4 127.0.0.1 > nul"; \
    Flags: runhidden; \
    RunOnceId: "WaitForStop"
Filename: "{sys}\sc.exe"; \
    Parameters: "delete {#MyServiceName}"; \
    Flags: runhidden; \
    RunOnceId: "DeleteService"

[Code]
const
  ServiceName    = '{#MyServiceName}';
  ServiceExe     = '{#MyServiceExe}';
  ServiceDisplay = 'Database Backup Service (.NET Framework)';
  ServiceDesc    = 'Automated database backup service for SQL Server and MySQL (.NET Framework 4.8)';

// ---------------------------------------------------------------------------
// Helper: run a command silently and return its exit code
// ---------------------------------------------------------------------------
function ExecSilent(const Exe, Params: String): Integer;
var
  ResultCode: Integer;
begin
  Exec(Exe, Params, '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := ResultCode;
end;

// ---------------------------------------------------------------------------
// Helper: check whether a Windows service exists
// ---------------------------------------------------------------------------
function ServiceExists(const Name: String): Boolean;
var
  ExitCode: Integer;
begin
  ExitCode := ExecSilent(
    ExpandConstant('{sys}\sc.exe'),
    'query "' + Name + '"'
  );
  Result := (ExitCode = 0);
end;

// ---------------------------------------------------------------------------
// Helper: stop a service (ignore errors – service may already be stopped)
// ---------------------------------------------------------------------------
procedure StopService(const Name: String);
begin
  ExecSilent(ExpandConstant('{sys}\sc.exe'), 'stop "' + Name + '"');
  // Brief wait so SCM can process the stop request
  Sleep(3000);
end;

// ---------------------------------------------------------------------------
// Helper: delete a service
// ---------------------------------------------------------------------------
function DeleteService(const Name: String): Boolean;
begin
  Result := (ExecSilent(ExpandConstant('{sys}\sc.exe'), 'delete "' + Name + '"') = 0);
end;

// ---------------------------------------------------------------------------
// Helper: install and configure the service
// ---------------------------------------------------------------------------
function InstallService(const BinPath: String): Boolean;
var
  ExitCode: Integer;
begin
  // Create the service
  ExitCode := ExecSilent(
    ExpandConstant('{sys}\sc.exe'),
    'create "' + ServiceName + '"' +
    ' binPath= "' + BinPath + '"' +
    ' start= auto' +
    ' DisplayName= "' + ServiceDisplay + '"'
  );
  if ExitCode <> 0 then begin
    Result := False;
    Exit;
  end;

  // Set description
  ExecSilent(
    ExpandConstant('{sys}\sc.exe'),
    'description "' + ServiceName + '" "' + ServiceDesc + '"'
  );

  // Configure auto-restart on failure (3 restarts, 1-minute delay, 24-hour reset)
  ExecSilent(
    ExpandConstant('{sys}\sc.exe'),
    'failure "' + ServiceName + '"' +
    ' reset= 86400' +
    ' actions= restart/60000/restart/60000/restart/60000'
  );

  Result := True;
end;

// ---------------------------------------------------------------------------
// CurStepChanged: runs before/after each installer phase
// ---------------------------------------------------------------------------
procedure CurStepChanged(CurStep: TSetupStep);
var
  BinPath: String;
begin
  if CurStep = ssInstall then begin
    // --- Remove existing service before copying new files ---
    if ServiceExists(ServiceName) then begin
      Log('Existing service found – stopping and removing it.');
      StopService(ServiceName);
      if not DeleteService(ServiceName) then
        MsgBox(
          'Warning: could not remove the existing "' + ServiceName + '" service.' + #13#10 +
          'Close the Services console (services.msc) and try again.',
          mbError, MB_OK
        );
      // Give SCM a moment to finish the deletion
      Sleep(2000);
    end;
  end;

  if CurStep = ssPostInstall then begin
    // --- Install the service from its new location ---
    BinPath := ExpandConstant('{app}\') + ServiceExe;
    Log('Installing service from: ' + BinPath);
    if not InstallService(BinPath) then
      MsgBox(
        'The service executable was copied successfully, but the Windows Service' + #13#10 +
        'could not be registered automatically.' + #13#10#13#10 +
        'You can register it manually by running install-service-netfx.ps1' + #13#10 +
        'as Administrator from:' + #13#10 + ExpandConstant('{app}'),
        mbError, MB_OK
      );
  end;
end;
