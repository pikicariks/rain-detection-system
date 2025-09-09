# Arduino/NodeMCU Code for Rain Detection System

This directory contains the Arduino IDE code for the NodeMCU (ESP8266/ESP32) controller that interfaces with the rain detection sensors and servo motor.

## Required Libraries

Install these libraries in Arduino IDE:

1. **ESP8266WebServer** (built-in with ESP8266 board package)
2. **ArduinoJson** by Benoit Blanchon (v6.x)
3. **Servo** (built-in, or ESP32Servo for ESP32)

### Installing Libraries

1. Open Arduino IDE
2. Go to **Tools → Manage Libraries**
3. Search for each library and click **Install**

## Hardware Configuration

### Pin Assignments

| Component | NodeMCU Pin | ESP32 Pin | Notes |
|-----------|-------------|-----------|-------|
| Rain Sensor (Analog) | A0 | A0 | Analog input |
| Rain Sensor (Digital) | D2 | GPIO2 | Digital input |
| Servo Signal | D1 | GPIO4 | PWM output |
| Ultrasonic Trigger | D5 | GPIO5 | Digital output |
| Ultrasonic Echo | D6 | GPIO6 | Digital input |

### Power Requirements

- **NodeMCU**: 3.3V (USB or external)
- **Servo Motor**: 5V (external power supply recommended)
- **Rain Sensor**: 3.3V
- **Ultrasonic Sensor**: 5V

## Network Configuration

### WiFi Setup

1. Update WiFi credentials in the code:
   ```cpp
   const char* ssid = "YOUR_WIFI_SSID";
   const char* password = "YOUR_WIFI_PASSWORD";
   ```

2. Set static IP address:
   ```cpp
   IPAddress local_IP(10, 12, 12, 93);
   IPAddress gateway(10, 12, 12, 1);
   IPAddress subnet(255, 255, 255, 0);
   ```

### Port Configuration

- **HTTP Server**: Port 80 (default)
- **WebSocket**: Port 81 (if implemented)

## API Endpoints

The NodeMCU exposes these REST API endpoints:

### System Status
- `GET /api/status` - Get current system status
- `GET /api/health` - Health check endpoint

### Control
- `POST /api/servo/move` - Move servo to specific position
- `POST /api/system/toggle` - Toggle system on/off
- `POST /api/settings` - Update system settings

### Proximity
- `POST /api/proximity/acknowledge` - Acknowledge proximity alert
- `POST /api/proximity/settings` - Update proximity settings

## Configuration Parameters

### Rain Detection
- **Rain Threshold**: 600 (0-1024 analog range)
- **Debounce Time**: 2000ms (2 seconds)
- **Check Interval**: 1000ms (1 second)

### Servo Control
- **Normal Position**: 0° (dry weather)
- **Rain Position**: 180° (rain detected)
- **Movement Speed**: 15ms delay between steps

### Proximity Detection
- **Default Threshold**: 50cm
- **Check Interval**: 500ms
- **Alert Duration**: 5000ms (5 seconds)

## Upload Instructions

1. **Connect NodeMCU**
   - Connect via USB cable
   - Ensure drivers are installed

2. **Configure Arduino IDE**
   - Select correct board: "NodeMCU 1.0 (ESP-12E Module)"
   - Select correct port (COM port on Windows)
   - Set upload speed to 115200

3. **Upload Code**
   - Click Upload button
   - Wait for compilation and upload to complete

4. **Monitor Serial Output**
   - Open Serial Monitor (Tools → Serial Monitor)
   - Set baud rate to 115200
   - Check for WiFi connection and IP address

## Troubleshooting

### Common Issues

1. **WiFi Connection Failed**
   - Check SSID and password
   - Ensure 2.4GHz network (ESP8266 doesn't support 5GHz)
   - Check signal strength

2. **Upload Failed**
   - Hold FLASH button during upload
   - Try different USB cable
   - Check COM port selection

3. **Servo Not Moving**
   - Check power supply (5V for servo)
   - Verify wiring connections
   - Check servo library compatibility

4. **Sensors Not Reading**
   - Verify pin connections
   - Check power supply
   - Test with multimeter

### Serial Monitor Output

Expected output after successful startup:
```
WiFi connected!
IP address: 10.12.12.93
HTTP server started
Rain Detection System Ready
```

## Calibration

### Rain Sensor Calibration

1. **Dry Environment**
   - Note analog reading when completely dry
   - This should be close to 1024

2. **Wet Environment**
   - Note analog reading when wet
   - Adjust threshold between dry and wet values

3. **Threshold Setting**
   - Set threshold in web dashboard
   - Test with actual rain or water spray

### Servo Calibration

1. **Position 0°**
   - Should be fully closed/retracted
   - Adjust if needed

2. **Position 180°**
   - Should be fully open/extended
   - Adjust if needed

## Updates and Maintenance

### OTA Updates (Optional)

For remote updates without USB connection:

1. Enable OTA in code
2. Use Arduino IDE OTA upload
3. Or implement custom OTA server

### Regular Maintenance

- **Clean sensors** monthly
- **Check connections** for corrosion
- **Update firmware** as needed
- **Monitor power supply** stability

## Customization

### Adding New Sensors

1. Define pin in configuration
2. Add reading function
3. Include in status API
4. Update web dashboard

### Modifying Behavior

1. Adjust timing parameters
2. Change threshold values
3. Modify servo positions
4. Update API responses

## Support

For issues with the Arduino code:

1. Check Serial Monitor output
2. Verify hardware connections
3. Test individual components
4. Review library versions
5. Check power supply stability

