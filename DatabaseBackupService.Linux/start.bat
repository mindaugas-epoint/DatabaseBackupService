@echo off
REM Database Backup Service - Docker Quick Start Script (Windows)

echo ==================================
echo Database Backup Service - Docker
echo ==================================
echo.

REM Check if .env file exists
if not exist .env (
    echo Creating .env file from template...
    copy .env.template .env
    echo.
    echo WARNING: Please edit .env file with your configuration before running!
    echo    Run: notepad .env
    echo.
    pause
    exit /b 1
)

REM Check if Docker is running
docker info >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker is not running. Please start Docker and try again.
    pause
    exit /b 1
)

echo Docker is running
echo.

REM Ask what to do
echo What would you like to do?
echo 1^) Build and start the container
echo 2^) Start existing container
echo 3^) Stop the container
echo 4^) View logs
echo 5^) Rebuild container
echo 6^) Remove container and volumes
echo.
set /p choice="Enter your choice (1-6): "

if "%choice%"=="1" goto build_start
if "%choice%"=="2" goto start
if "%choice%"=="3" goto stop
if "%choice%"=="4" goto logs
if "%choice%"=="5" goto rebuild
if "%choice%"=="6" goto remove
goto invalid

:build_start
echo.
echo Building and starting container...
docker-compose up -d --build
echo.
echo Container started!
echo    View logs with: docker-compose logs -f
goto end

:start
echo.
echo Starting container...
docker-compose up -d
echo.
echo Container started!
goto end

:stop
echo.
echo Stopping container...
docker-compose down
echo.
echo Container stopped!
goto end

:logs
echo.
echo Viewing logs (Ctrl+C to exit)...
docker-compose logs -f
goto end

:rebuild
echo.
echo Rebuilding container...
docker-compose down
docker-compose up -d --build
echo.
echo Container rebuilt and started!
goto end

:remove
echo.
set /p confirm="WARNING: This will remove the container and backup volumes. Continue? (y/N): "
if /i "%confirm%"=="y" (
    docker-compose down -v
    echo Container and volumes removed!
) else (
    echo Cancelled.
)
goto end

:invalid
echo Invalid choice
pause
exit /b 1

:end
echo.
pause
