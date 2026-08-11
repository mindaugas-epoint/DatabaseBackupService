# Database Backup Service - Linux Container Edition

A containerized database backup service for SQL Server and MySQL databases, designed to run in Docker containers on Linux.

## Features

✅ **Multi-Database Support**: SQL Server (MSSQL) and MySQL  
✅ **Flexible Scheduling**: Daily backups or time-window based intervals  
✅ **Multiple Storage Options**: Local filesystem and Azure Blob Storage  
✅ **Automated Retention**: Configurable backup retention policies  
✅ **Email Notifications**: Alert on backup failures  
✅ **Environment Variable Configuration**: No config files needed  
✅ **Docker & Docker Compose Support**: Easy deployment

## Quick Start

### 1. Clone and Navigate

```bash
git clone <repository-url>
cd DatabaseBackupService/DatabaseBackupService.Linux
```

### 2. Configure Environment Variables

Copy the template and edit with your settings:

```bash
cp .env.template .env
nano .env
```

### 3. Run with Docker Compose

```bash
docker-compose up -d
```

### 4. View Logs

```bash
docker-compose logs -f
```

## Docker Hub (Optional)

Build and push to Docker Hub:

```bash
# Build the image
docker build -t yourusername/database-backup-service:latest -f Dockerfile ..

# Push to Docker Hub
docker push yourusername/database-backup-service:latest

# Run from Docker Hub
docker run -d --env-file .env -v $(pwd)/backups:/backups yourusername/database-backup-service:latest
```

## Configuration

All configuration is done via environment variables. See [DOCKER-GUIDE.md](DOCKER-GUIDE.md) for complete documentation.

### Required Variables

- `DB_TYPE`: `mssql` or `mysql`
- `DB_SERVER`: Database server hostname
- `DB_NAME`: Database name to backup
- `DB_USER`: Database username
- `DB_PASSWORD`: Database password

### Example Configuration

```env
DB_TYPE=mssql
DB_SERVER=192.168.1.100
DB_PORT=1433
DB_NAME=ProductionDB
DB_USER=sa
DB_PASSWORD=YourPassword
BACKUP_SCHEDULE=02:00:00
BACKUP_FOLDER_PATH=/backups
```

## Documentation

📖 **[Complete Docker Guide](DOCKER-GUIDE.md)** - Comprehensive setup and usage documentation

## Project Structure

```
DatabaseBackupService.Linux/
├── Dockerfile                  # Docker image definition
├── docker-compose.yml         # Docker Compose configuration
├── .env.template              # Environment variables template
├── DOCKER-GUIDE.md            # Complete documentation
├── Program.cs                 # Application entry point
├── EnvironmentConfigReader.cs # Environment variable configuration
├── BackupServiceConfig.cs     # Configuration model
├── DbBackupWorker.cs          # Backup worker service
├── DatabaseBackup/            # Database backup implementations
├── Logger/                    # Logging infrastructure
└── EmailService/              # Email notification service
```

## Building from Source

### Build Docker Image

```bash
# From repository root
docker build -t database-backup-service:latest -f DatabaseBackupService.Linux/Dockerfile .
```

### Build .NET Application

```bash
cd DatabaseBackupService.Linux
dotnet restore
dotnet build
dotnet publish -c Release -o ./publish
```

## Usage Scenarios

### Scenario 1: SQL Server Daily Backup

```bash
docker run -d \
  --name sqlserver-backup \
  -v /opt/backups:/backups \
  -e DB_TYPE=mssql \
  -e DB_SERVER=sql.example.com \
  -e DB_NAME=MyDatabase \
  -e DB_USER=sa \
  -e DB_PASSWORD=Password123 \
  -e BACKUP_SCHEDULE=03:00:00 \
  database-backup-service:latest
```

### Scenario 2: MySQL Time-Window Backups

```bash
docker run -d \
  --name mysql-backup \
  -v /opt/backups:/backups \
  -e DB_TYPE=mysql \
  -e DB_SERVER=mysql.example.com \
  -e DB_NAME=AppDB \
  -e DB_USER=root \
  -e DB_PASSWORD=Password123 \
  -e USE_TIME_WINDOW=true \
  -e BACKUP_START_TIME=08:00:00 \
  -e BACKUP_END_TIME=20:00:00 \
  -e BACKUP_INTERVAL=04:00:00 \
  database-backup-service:latest
```

### Scenario 3: Azure Backup with Notifications

```bash
docker run -d \
  --name db-backup-azure \
  -v /opt/backups:/backups \
  --env-file .env \
  -e ENABLE_AZURE_BACKUP=true \
  -e AZURE_STORAGE_CONNECTION_STRING="YourConnectionString" \
  -e AZURE_CONTAINER_NAME=backups \
  -e EMAIL_SENDER_ADDRESS=backup@gmail.com \
  -e EMAIL_SENDER_PASSWORD="AppPassword" \
  -e EMAIL_RECIPIENT_ADDRESS=admin@example.com \
  database-backup-service:latest
```

## Monitoring

### View Logs

```bash
# Docker Compose
docker-compose logs -f

# Docker CLI
docker logs -f database-backup-service
```

### Check Status

```bash
docker ps | grep database-backup-service
```

### Verify Backups

```bash
ls -lh ./backups
```

## Troubleshooting

### Container Exits Immediately

Check the logs:
```bash
docker logs database-backup-service
```

Common issues:
- Missing required environment variables
- Invalid database credentials
- Network connectivity problems

### No Backups Created

1. Check container logs for errors
2. Verify database connectivity
3. Check volume mount permissions
4. Ensure backup directory is writable

### Azure Upload Failures

1. Verify Azure credentials
2. Check internet connectivity
3. Verify container permissions
4. Check Azure Storage firewall

## Requirements

- Docker 20.10 or later
- SQL Server or MySQL database
- Minimum 512MB RAM
- Disk space for backups

## Differences from Windows Version

| Feature | Windows Version | Linux Version |
|---------|----------------|---------------|
| Configuration | Windows Registry | Environment Variables |
| UI | WPF Configuration UI | Command-line/Environment Variables |
| Platform | Windows only | Linux, Windows, macOS |
| Deployment | Windows Service | Docker Container |

## Security Best Practices

1. **Use secrets management** for sensitive data
2. **Restrict file permissions** on .env files
3. **Use dedicated backup user** with minimal permissions
4. **Enable database encryption** in transit
5. **Secure volume mounts** with appropriate permissions

## Support

For detailed documentation, see [DOCKER-GUIDE.md](DOCKER-GUIDE.md)

For issues and questions, please create an issue in the repository.

## License

See repository license for details.
