# Use the official .NET 10 runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

# Use the SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["DatabaseBackupService.Linux/DatabaseBackupService.Linux.csproj", "DatabaseBackupService.Linux/"]
RUN dotnet restore "DatabaseBackupService.Linux/DatabaseBackupService.Linux.csproj"

# Copy all source files and build
COPY DatabaseBackupService.Linux/ DatabaseBackupService.Linux/
WORKDIR "/src/DatabaseBackupService.Linux"
RUN dotnet build "DatabaseBackupService.Linux.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "DatabaseBackupService.Linux.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage - runtime image
FROM base AS final
WORKDIR /app

# Create backup directory
RUN mkdir -p /backups && chmod 777 /backups

COPY --from=publish /app/publish .

# Set environment variables with default values (can be overridden)
ENV DB_TYPE=mssql
ENV DB_SERVER=""
ENV DB_PORT=""
ENV DB_NAME=""
ENV DB_USER=""
ENV DB_PASSWORD=""
ENV BACKUP_SCHEDULE="02:00:00"
ENV BACKUP_FOLDER_PATH="/backups"
ENV ENABLE_AZURE_BACKUP=false
ENV USE_AZURE_SAS_TOKEN=false
ENV USE_TIME_WINDOW=false
ENV BACKUP_START_TIME="08:00:00"
ENV BACKUP_END_TIME="18:00:00"
ENV BACKUP_INTERVAL="04:00:00"
ENV AZURE_STORAGE_CONNECTION_STRING=""
ENV AZURE_CONTAINER_NAME=""
ENV AZURE_BLOB_PREFIX=""
ENV AZURE_STORAGE_ACCOUNT_NAME=""
ENV AZURE_SAS_TOKEN=""
ENV EMAIL_SENDER_ADDRESS=""
ENV EMAIL_SENDER_PASSWORD=""
ENV EMAIL_RECIPIENT_ADDRESS=""
ENV BACKUP_RETENTION_DAYS=30
ENV MINIMUM_BACKUP_FILES=2

# Volume for backups
VOLUME ["/backups"]

ENTRYPOINT ["dotnet", "DatabaseBackupService.Linux.dll"]
