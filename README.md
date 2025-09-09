# Rain Detection System

A comprehensive IoT rain detection and automation system that monitors weather conditions and automatically controls servo motors for windows, awnings, or similar mechanisms.

## Features

- **Real-time Rain Detection**: Analog and digital sensors for accurate rain monitoring
- **Automatic Servo Control**: Intelligent positioning based on rain conditions
- **Proximity Detection**: Ultrasonic sensor for object detection and security
- **Web Dashboard**: Modern, responsive interface for monitoring and control
- **Data Logging**: Comprehensive activity tracking and historical data
- **Mobile Support**: Fully responsive design for mobile devices
- **REST API**: Complete API for integration with other systems

## Architecture

### Backend (ASP.NET Core 8.0)
- **Web API**: RESTful endpoints for device communication
- **Entity Framework Core**: SQL Server database with migrations
- **Real-time Updates**: Auto-refresh dashboard every 5 seconds
- **Error Handling**: Comprehensive logging and error management

### Frontend
- **Bootstrap 5**: Modern, responsive UI framework
- **Vanilla JavaScript**: Lightweight, no framework dependencies
- **Real-time Dashboard**: Live status updates and controls

### IoT Device (NodeMCU)
- **ESP8266/ESP32**: WiFi-enabled microcontroller
- **Rain Sensors**: Analog and digital rain detection
- **Servo Motor**: Automatic positioning control
- **Ultrasonic Sensor**: Proximity detection
- **HTTP Server**: REST API for web communication

## Project Structure

```
RainDetectionApp/
├── Controllers/          # API Controllers
├── Data/                # Entity Framework Context
├── Models/              # Data Models
├── Services/            # Business Logic Services
├── Views/               # MVC Views
├── Migrations/          # Database Migrations
├── wwwroot/             # Static Files
├── Arduino/             # NodeMCU Code
│   ├── RainDetectionSystem.ino
│   ├── libraries/       # Required Arduino libraries
│   └── README.md        # Arduino setup instructions
└── docs/                # Documentation
```

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
- Arduino IDE
- NodeMCU (ESP8266 or ESP32)

### Backend Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/pikicariks/rain-detection-system.git
   cd rain-detection-system
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database connection**
   - Edit `appsettings.json` with your SQL Server connection string
   - Or use LocalDB for development

4. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the dashboard**
   - Open browser to `https://localhost:5001` or `http://localhost:5000`

### Arduino/NodeMCU Setup

1. **Install Arduino IDE**
   - Download from [arduino.cc](https://www.arduino.cc/en/software)

2. **Install ESP8266 Board Package**
   - File → Preferences → Additional Board Manager URLs
   - Add: `http://arduino.esp8266.com/stable/package_esp8266com_index.json`
   - Tools → Board → Boards Manager → Search "ESP8266" → Install

3. **Install Required Libraries**
   - ESP8266WebServer
   - ArduinoJson
   - Servo (if using ESP32)

4. **Configure WiFi and Device IP**
   - Edit `Arduino/RainDetectionSystem.ino`
   - Update WiFi credentials
   - Set static IP address

5. **Upload to NodeMCU**
   - Connect NodeMCU via USB
   - Select correct board and port
   - Upload the code

## Configuration

### Web Application Settings

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RainDetectionDb;Trusted_Connection=true;",
    "NodeMCUDevice": "http://YOUR_NODEMCU_IP"
  },
  "SystemSettings": {
    "DefaultRainThreshold": 600,
    "DefaultNormalPosition": 0,
    "DefaultRainPosition": 180,
    "RefreshIntervalSeconds": 5,
    "MaxLogEntries": 1000
  }
}
```

### Arduino Configuration

In `Arduino/RainDetectionSystem.ino`:

```cpp
// WiFi Configuration
const char* ssid = "YOUR_WIFI_SSID";
const char* password = "YOUR_WIFI_PASSWORD";

// Device IP Configuration
IPAddress local_IP(10, 12, 12, 93);
IPAddress gateway(10, 12, 12, 1);
IPAddress subnet(255, 255, 255, 0);

// Pin Configuration
const int RAIN_SENSOR_ANALOG = A0;
const int RAIN_SENSOR_DIGITAL = D2;
const int SERVO_PIN = D1;
const int ULTRASONIC_TRIG = D5;
const int ULTRASONIC_ECHO = D6;
```

## Hardware Setup

### Required Components

- **NodeMCU (ESP8266/ESP32)**
- **Rain Sensor Module** (Analog + Digital)
- **Servo Motor** (SG90 or similar)
- **Ultrasonic Sensor** (HC-SR04)
- **Jumper Wires**
- **Breadboard**
- **Power Supply** (5V for servo, 3.3V for NodeMCU)

### Wiring Diagram

```
NodeMCU    Component
------     ---------
A0    →    Rain Sensor (Analog)
D2    →    Rain Sensor (Digital)
D1    →    Servo Signal
D5    →    Ultrasonic Trigger
D6    →    Ultrasonic Echo
3.3V  →    Rain Sensor VCC
5V    →    Servo VCC
GND   →    All GND connections
```

## API Endpoints

### System Status
- `GET /api/RainSystem/status` - Get system status and recent logs
- `GET /api/RainSystem/logs?count=50` - Get activity logs
- `GET /api/RainSystem/commands?count=20` - Get command history

### Control
- `POST /api/RainSystem/system/toggle` - Toggle system on/off
- `POST /api/RainSystem/settings/update` - Update system settings
- `POST /api/RainSystem/proximity/acknowledge` - Acknowledge proximity alert

## Development

### Adding New Features

1. **Database Changes**
   ```bash
   dotnet ef migrations add FeatureName
   dotnet ef database update
   ```

2. **API Endpoints**
   - Add to `Controllers/RainSystemController.cs`
   - Update `Services/RainSystemService.cs` for business logic

3. **Frontend Updates**
   - Modify `Views/Home/Index.cshtml`
   - Update JavaScript functions as needed

### Testing

```bash
# Run tests
dotnet test

# Run with specific environment
dotnet run --environment Development
```

## Deployment

### Local Deployment

1. **Build for production**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Configure IIS or Kestrel**
   - Update `appsettings.Production.json`
   - Set connection strings and device IP

3. **Database Setup**
   ```bash
   dotnet ef database update --environment Production
   ```

### Cloud Deployment

- **Azure**: Use Azure App Service + SQL Database
- **AWS**: Use EC2 + RDS
- **Docker**: Use provided Dockerfile

## Monitoring

The system provides comprehensive monitoring through:

- **Real-time Dashboard**: Live status updates
- **Activity Logs**: Historical event tracking
- **Command History**: API call tracking
- **Connection Status**: Device connectivity monitoring


**Made for IoT enthusiasts**
