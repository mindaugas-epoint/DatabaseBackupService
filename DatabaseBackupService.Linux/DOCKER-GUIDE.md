# Database Backup Service - Docker Container Guide

## Overview

The Database Backup Service is a containerized application that automatically backs up SQL Server (MSSQL) and MySQL databases on a scheduled basis. It supports local file backups, Azure Blob Storage backups, and email notifications.

## Features

- **Multi-Database Support**: Works with both Microsoft SQL Server and MySQL databases
- **Flexible Scheduling**: 
  - Daily scheduled backups at a specific time
  - Time window-based backups with configurable intervals
- **Multiple Backup Destinations**:
  - Local filesystem (with volume mounting)
  - Azure Blob Storage (using connection string or SAS token)
- **Automated Retention Management**: Automatically removes old backups based on retention policies
- **Email Notifications**: Sends email alerts on backup failures
- **Containerized**: Runs as a standalone Docker container

## Prerequisites

- Docker installed on your system
- Access to a SQL Server or MySQL database
- (Optional) Azure Storage account for cloud backups
- (Optional) Gmail account for email notifications

## Quick Start

### 1. Using Docker Compose (Recommended)

The easiest way to run the service is using Docker Compose:

```bash
# Navigate to the project directory
cd DatabaseBackupService.Linux

# Edit docker-compose.yml and configure your environment variables
nano docker-compose.yml

# Start the service
docker-compose up -d

# View logs
docker-compose logs -f

# Stop the service
docker-compose down
```

### 2. Using Docker CLI

#### Build the Docker Image

```bash
# From the repository root directory
docker build -t database-backup-service:latest -f DatabaseBackupService.Linux/Dockerfile .
```

#### Run the Container

**For SQL Server:**

```bash
docker run -d \
  --name database-backup-service \
  --restart unless-stopped \
  -v $(pwd)/backups:/backups \
  -e DB_TYPE=mssql \
  -e DB_SERVER=your-sqlserver-host \
  -e DB_PORT=1433 \
  -e DB_NAME=YourDatabase \
  -e DB_USER=sa \
  -e DB_PASSWORD=YourStrongPassword \
  -e BACKUP_SCHEDULE=02:00:00 \
  -e BACKUP_FOLDER_PATH=/backups \
  -e BACKUP_RETENTION_DAYS=30 \
  database-backup-service:latest
```

**For MySQL:**

```bash
docker run -d \
  --name database-backup-service \
  --restart unless-stopped \
  -v $(pwd)/backups:/backups \
  -e DB_TYPE=mysql \
  -e DB_SERVER=your-mysql-host \
  -e DB_PORT=3306 \
  -e DB_NAME=YourDatabase \
  -e DB_USER=root \
  -e DB_PASSWORD=YourPassword \
  -e BACKUP_SCHEDULE=02:00:00 \
  -e BACKUP_FOLDER_PATH=/backups \
  database-backup-service:latest
```

## Configuration

All configuration is done through environment variables. Here's a complete reference:

### Required Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `DB_TYPE` | Database type | `mssql` or `mysql` |
| `DB_SERVER` | Database server hostname or IP address | `192.168.1.100` or `db.example.com` |
| `DB_NAME` | Database name to backup | `ProductionDB` |
| `DB_USER` | Database username | `sa` or `root` |
| `DB_PASSWORD` | Database password | `YourSecurePassword` |

### Optional Environment Variables

#### Backup Schedule Configuration

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `DB_PORT` | Database port | `1433` (MSSQL) / `3306` (MySQL) | `1433` |
| `BACKUP_SCHEDULE` | Daily backup time (HH:mm:ss) | `02:00:00` | `03:30:00` |
| `BACKUP_FOLDER_PATH` | Path for local backups inside container | `/backups` | `/backups` |

#### Time Window Configuration

Use time windows for multiple backups throughout the day:

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `USE_TIME_WINDOW` | Enable time window backups | `false` | `true` |
| `BACKUP_START_TIME` | Time window start (HH:mm:ss) | `08:00:00` | `08:00:00` |
| `BACKUP_END_TIME` | Time window end (HH:mm:ss) | `18:00:00` | `20:00:00` |
| `BACKUP_INTERVAL` | Interval between backups (HH:mm:ss) | `04:00:00` | `02:00:00` |

#### Azure Blob Storage Configuration

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `ENABLE_AZURE_BACKUP` | Enable Azure backup | `false` | `true` |
| `USE_AZURE_SAS_TOKEN` | Use SAS token instead of connection string | `false` | `true` |
| `AZURE_STORAGE_CONNECTION_STRING` | Azure storage connection string | (empty) | `DefaultEndpointsProtocol=https;...` |
| `AZURE_CONTAINER_NAME` | Azure blob container name | (empty) | `database-backups` |
| `AZURE_BLOB_PREFIX` | Prefix for blob names | (empty) | `prod/` |
| `AZURE_STORAGE_ACCOUNT_NAME` | Storage account name (for SAS token) | (empty) | `mystorageaccount` |
| `AZURE_SAS_TOKEN` | SAS token with write permissions | (empty) | `?sv=2020-08-04&ss=b...` |

#### Email Notification Configuration

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `EMAIL_SENDER_ADDRESS` | Gmail address to send from | (empty) | `backup@gmail.com` |
| `EMAIL_SENDER_PASSWORD` | Gmail app password | (empty) | `abcd efgh ijkl mnop` |
| `EMAIL_RECIPIENT_ADDRESS` | Email address to receive notifications | (empty) | `admin@example.com` |

**Note:** For Gmail, you need to create an [App Password](https://support.google.com/accounts/answer/185833).

#### Retention Configuration

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `BACKUP_RETENTION_DAYS` | Number of days to keep backups | `30` | `60` |
| `MINIMUM_BACKUP_FILES` | Minimum backup files to always keep | `2` | `5` |

## Volume Mounting

The container uses `/backups` as the default backup directory. You should mount a host directory to persist backups:

```bash
-v /path/on/host/backups:/backups
```

For example:
```bash
-v /var/backups/database:/backups
```

## Networking

### Connecting to Database on Host Machine

If your database is running on the Docker host machine:

```bash
# On Linux, use host.docker.internal (Docker 20.10+)
-e DB_SERVER=host.docker.internal

# Or use host network mode
--network host
```

### Connecting to Database in Another Container

If your database is in another container, use Docker networks:

```bash
# Create a network
docker network create db-network

# Run your database container with the network
docker run --network db-network --name mysql-db mysql:latest

# Run backup service with the same network
docker run --network db-network -e DB_SERVER=mysql-db database-backup-service:latest
```

## Usage Examples

### Example 1: Daily SQL Server Backup with Local Storage

```bash
docker run -d \
  --name sqlserver-backup \
  --restart unless-stopped \
  -v /opt/backups/sqlserver:/backups \
  -e DB_TYPE=mssql \
  -e DB_SERVER=192.168.1.50 \
  -e DB_PORT=1433 \
  -e DB_NAME=ProductionDB \
  -e DB_USER=sa \
  -e DB_PASSWORD=MyStrongPassword123! \
  -e BACKUP_SCHEDULE=03:00:00 \
  -e BACKUP_RETENTION_DAYS=14 \
  database-backup-service:latest
```

### Example 2: MySQL Backup with Time Windows

Backup every 4 hours between 8 AM and 8 PM:

```bash
docker run -d \
  --name mysql-backup \
  --restart unless-stopped \
  -v /opt/backups/mysql:/backups \
  -e DB_TYPE=mysql \
  -e DB_SERVER=mysql.example.com \
  -e DB_PORT=3306 \
  -e DB_NAME=AppDatabase \
  -e DB_USER=backup_user \
  -e DB_PASSWORD=BackupPassword \
  -e USE_TIME_WINDOW=true \
  -e BACKUP_START_TIME=08:00:00 \
  -e BACKUP_END_TIME=20:00:00 \
  -e BACKUP_INTERVAL=04:00:00 \
  database-backup-service:latest
```

### Example 3: Backup to Azure Blob Storage with Email Notifications

```bash
docker run -d \
  --name db-backup-with-azure \
  --restart unless-stopped \
  -v /opt/backups/local:/backups \
  -e DB_TYPE=mssql \
  -e DB_SERVER=sql.example.com \
  -e DB_NAME=CriticalDB \
  -e DB_USER=backup_admin \
  -e DB_PASSWORD=SecurePass123 \
  -e BACKUP_SCHEDULE=02:00:00 \
  -e ENABLE_AZURE_BACKUP=true \
  -e AZURE_STORAGE_CONNECTION_STRING="DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=mykey;EndpointSuffix=core.windows.net" \
  -e AZURE_CONTAINER_NAME=db-backups \
  -e AZURE_BLOB_PREFIX=production/ \
  -e EMAIL_SENDER_ADDRESS=backup.service@gmail.com \
  -e EMAIL_SENDER_PASSWORD="abcd efgh ijkl mnop" \
  -e EMAIL_RECIPIENT_ADDRESS=admin@example.com \
  -e BACKUP_RETENTION_DAYS=30 \
  database-backup-service:latest
```

### Example 4: Using Environment File

Create a file `.env` with your configuration:

```env
DB_TYPE=mssql
DB_SERVER=192.168.1.100
DB_PORT=1433
DB_NAME=MyDatabase
DB_USER=sa
DB_PASSWORD=MySecurePassword
BACKUP_SCHEDULE=02:00:00
BACKUP_FOLDER_PATH=/backups
BACKUP_RETENTION_DAYS=30
MINIMUM_BACKUP_FILES=3
```

Then run:

```bash
docker run -d \
  --name database-backup \
  --restart unless-stopped \
  -v $(pwd)/backups:/backups \
  --env-file .env \
  database-backup-service:latest
```

## Monitoring and Troubleshooting

### View Container Logs

```bash
# View all logs
docker logs database-backup-service

# Follow logs in real-time
docker logs -f database-backup-service

# View last 100 lines
docker logs --tail 100 database-backup-service
```

### Check Container Status

```bash
docker ps -a | grep database-backup-service
```

### Access Container Shell

```bash
docker exec -it database-backup-service /bin/bash
```

### List Backup Files

```bash
# From host
ls -lh /path/to/backups

# From container
docker exec database-backup-service ls -lh /backups
```

### Test Database Connectivity

```bash
# Enter container
docker exec -it database-backup-service /bin/bash

# For SQL Server (install sqlcmd if needed)
/opt/mssql-tools/bin/sqlcmd -S $DB_SERVER -U $DB_USER -P $DB_PASSWORD -Q "SELECT @@VERSION"

# For MySQL (install mysql client if needed)
mysql -h $DB_SERVER -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e "SELECT VERSION();"
```

## Backup File Format

### Local Backups

- **SQL Server**: `.bak` files
- **MySQL**: `.zip` files containing SQL dumps

File naming convention: `{DatabaseName}_backup_{timestamp}.{extension}`

Example: `ProductionDB_backup_20240315_020000.bak`

### Azure Backups

Stored in the configured Azure Blob Storage container with the same naming convention.

## Security Considerations

1. **Secrets Management**: Use Docker secrets or environment file with restricted permissions for sensitive data:
   ```bash
   chmod 600 .env
   ```

2. **Network Security**: Use Docker networks to isolate database connections

3. **Volume Permissions**: Ensure backup volume has appropriate permissions:
   ```bash
   chmod 700 /opt/backups
   ```

4. **Database User**: Create a dedicated backup user with minimal required permissions

5. **SSL/TLS**: Enable encrypted connections to your database when possible

## Backup and Restore

### Restoring from Backup

**SQL Server:**
```sql
RESTORE DATABASE [DatabaseName] 
FROM DISK = '/backups/DatabaseName_backup_20240315_020000.bak'
WITH REPLACE;
```

**MySQL:**
```bash
# Extract the zip file
unzip DatabaseName_backup_20240315_020000.zip

# Restore the database
mysql -h localhost -u root -p DatabaseName < DatabaseName_backup_20240315_020000.sql
```

## Updating the Container

```bash
# Stop and remove old container
docker stop database-backup-service
docker rm database-backup-service

# Pull/build latest image
docker build -t database-backup-service:latest -f DatabaseBackupService.Linux/Dockerfile .

# Run new container with same configuration
docker run -d ... (use same parameters as before)
```

Or with docker-compose:

```bash
docker-compose down
docker-compose pull
docker-compose up -d
```

## Uninstalling

```bash
# Stop and remove container
docker stop database-backup-service
docker rm database-backup-service

# Remove image
docker rmi database-backup-service:latest

# Remove volumes (CAUTION: This deletes all backups!)
# Only do this if you're sure you don't need the backups
rm -rf /path/to/backups
```

## Support and Troubleshooting

### Common Issues

#### Container Exits Immediately

Check logs for error messages:
```bash
docker logs database-backup-service
```

Common causes:
- Missing required environment variables (DB_SERVER, DB_NAME)
- Invalid database credentials
- Network connectivity issues

#### Backups Not Created

1. Check container logs for errors
2. Verify database connectivity
3. Check volume mount permissions
4. Verify backup folder path exists and is writable

#### Azure Backup Failures

1. Verify Azure credentials are correct
2. Check container has internet connectivity
3. Verify SAS token has write permissions
4. Check Azure Storage firewall rules

#### Email Notifications Not Working

1. Verify Gmail app password is correct
2. Check that sender email has 2FA enabled
3. Verify SMTP settings allow access from Docker container

## Environment Variables Reference (Complete)

```bash
# Database Configuration
DB_TYPE=mssql
DB_SERVER=localhost
DB_PORT=1433
DB_NAME=MyDatabase
DB_USER=sa
DB_PASSWORD=MyPassword

# Backup Schedule
BACKUP_SCHEDULE=02:00:00
BACKUP_FOLDER_PATH=/backups

# Time Window (Optional)
USE_TIME_WINDOW=false
BACKUP_START_TIME=08:00:00
BACKUP_END_TIME=18:00:00
BACKUP_INTERVAL=04:00:00

# Azure Storage (Optional)
ENABLE_AZURE_BACKUP=false
USE_AZURE_SAS_TOKEN=false
AZURE_STORAGE_CONNECTION_STRING=
AZURE_CONTAINER_NAME=
AZURE_BLOB_PREFIX=
AZURE_STORAGE_ACCOUNT_NAME=
AZURE_SAS_TOKEN=

# Email Notifications (Optional)
EMAIL_SENDER_ADDRESS=
EMAIL_SENDER_PASSWORD=
EMAIL_RECIPIENT_ADDRESS=

# Retention Policy
BACKUP_RETENTION_DAYS=30
MINIMUM_BACKUP_FILES=2
```

## License

Please refer to the repository license for usage terms.

## Contributing

For issues, feature requests, or contributions, please visit the project repository.
