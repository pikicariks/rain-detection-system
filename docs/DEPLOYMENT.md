# Deployment Guide

This guide covers deploying the Rain Detection System to various platforms and environments.

## Prerequisites

### Development Environment
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- SQL Server (LocalDB, Express, or full instance)
- Git

### Production Environment
- Windows Server 2019+ or Linux (Ubuntu 20.04+)
- .NET 8.0 Runtime
- SQL Server or PostgreSQL
- IIS (Windows) or Nginx (Linux)
- SSL Certificate (recommended)

## Local Development Deployment

### 1. Clone Repository
```bash
git clone https://github.com/pikicariks/rain-detection-system.git
cd rain-detection-system
```

### 2. Database Setup
```bash
# Update connection string in appsettings.json
# Run migrations
dotnet ef database update
```

### 3. Configure NodeMCU IP
```json
// appsettings.json
{
  "ConnectionStrings": {
    "NodeMCUDevice": "http://YOUR_NODEMCU_IP"
  }
}
```

### 4. Run Application
```bash
dotnet run
```

Access: `https://localhost:5001`

## Windows Server Deployment

### 1. Install Prerequisites
- .NET 8.0 Hosting Bundle
- SQL Server (Express or full)
- IIS with ASP.NET Core Module

### 2. Publish Application
```bash
dotnet publish -c Release -o ./publish
```

### 3. Configure IIS
1. Create new site in IIS Manager
2. Set physical path to publish folder
3. Configure application pool (.NET CLR Version: No Managed Code)
4. Set environment variable: `ASPNETCORE_ENVIRONMENT=Production`

### 4. Database Configuration
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=RainDetectionDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
  }
}
```

### 5. SSL Configuration
- Install SSL certificate
- Configure HTTPS redirect
- Update NodeMCU IP in production settings

## Linux Deployment (Ubuntu)

### 1. Install Prerequisites
```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install .NET 8.0
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-8.0

# Install Nginx
sudo apt install -y nginx

# Install PostgreSQL (optional, alternative to SQL Server)
sudo apt install -y postgresql postgresql-contrib
```

### 2. Create Systemd Service
```bash
# Create service file
sudo nano /etc/systemd/system/raindetection.service
```

```ini
[Unit]
Description=Rain Detection System
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/raindetection/RainDetectionApp.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=raindetection
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

### 3. Configure Nginx
```bash
sudo nano /etc/nginx/sites-available/raindetection
```

```nginx
server {
    listen 80;
    server_name your-domain.com;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### 4. Enable and Start Services
```bash
# Enable Nginx site
sudo ln -s /etc/nginx/sites-available/raindetection /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx

# Enable and start application
sudo systemctl enable raindetection
sudo systemctl start raindetection
```

## Cloud Deployment

### Azure App Service

1. **Create App Service**
   - Go to Azure Portal
   - Create new App Service
   - Choose .NET 8.0 runtime

2. **Configure Database**
   - Create Azure SQL Database
   - Update connection string in App Service settings

3. **Deploy Code**
   ```bash
   # Install Azure CLI
   az login
   az webapp deployment source config --name your-app-name --resource-group your-rg --repo-url https://github.com/yourusername/rain-detection-system.git --branch main --manual-integration
   ```

4. **Environment Variables**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DefaultConnection=Server=tcp:your-server.database.windows.net,1433;Database=RainDetectionDb;User ID=your-user;Password=your-password;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;
   ConnectionStrings__NodeMCUDevice=http://YOUR_NODEMCU_IP
   ```

### AWS Elastic Beanstalk

1. **Create Application**
   - Go to AWS Elastic Beanstalk
   - Create new application
   - Choose .NET Core platform

2. **Configure Environment**
   - Set environment variables
   - Configure RDS database
   - Set up load balancer

3. **Deploy**
   ```bash
   # Install EB CLI
   pip install awsebcli
   
   # Initialize and deploy
   eb init
   eb create production
   eb deploy
   ```

### Docker Deployment

1. **Create Dockerfile**
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
   WORKDIR /app
   EXPOSE 80
   EXPOSE 443

   FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
   WORKDIR /src
   COPY ["RainDetectionApp.csproj", "."]
   RUN dotnet restore
   COPY . .
   RUN dotnet build -c Release -o /app/build

   FROM build AS publish
   RUN dotnet publish -c Release -o /app/publish

   FROM base AS final
   WORKDIR /app
   COPY --from=publish /app/publish .
   ENTRYPOINT ["dotnet", "RainDetectionApp.dll"]
   ```

2. **Build and Run**
   ```bash
   docker build -t raindetection .
   docker run -p 8080:80 -e ConnectionStrings__DefaultConnection="your-connection-string" raindetection
   ```

## Configuration Management

### Environment Variables
```bash
# Production settings
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="your-connection-string"
export ConnectionStrings__NodeMCUDevice="http://your-nodemcu-ip"
```

### Configuration Files
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides

### Secrets Management
```bash
# Use .NET User Secrets for development
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"

# Use Azure Key Vault for production
# Use AWS Secrets Manager for AWS deployments
```

## Security Considerations

### 1. Database Security
- Use strong passwords
- Enable SSL/TLS connections
- Restrict database access by IP
- Regular security updates

### 2. Application Security
- Use HTTPS in production
- Implement authentication (if needed)
- Validate all inputs
- Regular dependency updates

### 3. Network Security
- Firewall configuration
- VPN for remote access
- Secure NodeMCU communication

## Monitoring and Logging

### Application Insights (Azure)
```csharp
// Add to Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### Logging Configuration
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Health Checks
```csharp
// Add to Program.cs
builder.Services.AddHealthChecks()
    .AddDbContext<DataContext>()
    .AddCheck<NodeMCUHealthCheck>("nodemcu");

app.MapHealthChecks("/health");
```

## CI/CD Pipeline

### GitHub Actions
```yaml
name: Deploy to Production

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Publish
      run: dotnet publish -c Release -o ./publish
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'your-app-name'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

## Maintenance

### Regular Tasks
- Database backups
- Log rotation
- Security updates
- Performance monitoring
- NodeMCU firmware updates

### Backup Strategy
```bash
# Database backup
sqlcmd -S your-server -d RainDetectionDb -Q "BACKUP DATABASE RainDetectionDb TO DISK = '/backup/raindetection.bak'"

# Application backup
tar -czf raindetection-backup.tar.gz /var/www/raindetection/
```

## Troubleshooting

### Common Issues

1. **Database Connection Failed**
   - Check connection string
   - Verify database server is running
   - Check firewall settings

2. **NodeMCU Not Responding**
   - Verify IP address
   - Check WiFi connection
   - Test with curl: `curl http://nodemcu-ip/api/health`

For additional support, please refer to the main README.md or create an issue in the GitHub repository.
