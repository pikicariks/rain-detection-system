# Hardware Setup Guide

Complete guide for setting up the hardware components of the Rain Detection System.

## Required Components

### Essential Components
- **NodeMCU (ESP8266)** - Main microcontroller
- **Rain Sensor Module** - Analog + Digital rain detection
- **Servo Motor (SG90)** - Automatic positioning control
- **Ultrasonic Sensor (HC-SR04)** - Proximity detection
- **Breadboard** - For prototyping
- **Jumper Wires** - For connections
- **Power Supply** - 5V for servo, 3.3V for NodeMCU

### Optional Components
- **Relay Module** - For high-power devices
- **LED Indicators** - Status indication
- **Buzzer** - Audio alerts
- **LCD Display** - Local status display

## Pin Configuration

### NodeMCU Pin Assignments
```
NodeMCU Pin    Component           Notes
-----------    ---------           -----
A0            Rain Sensor (Analog) Analog input (0-1024)
D2            Rain Sensor (Digital) Digital input (HIGH/LOW)
D1            Servo Signal         PWM output (0-180°)
D5            Ultrasonic Trigger   Digital output
D6            Ultrasonic Echo      Digital input
3.3V          Rain Sensor VCC      Power supply
5V            Servo VCC            Power supply
GND           All GND connections  Common ground
```

### ESP32 Pin Assignments (Alternative)
```
ESP32 Pin      Component           Notes
---------      ---------           -----
GPIO36 (A0)    Rain Sensor (Analog) Analog input
GPIO2          Rain Sensor (Digital) Digital input
GPIO4          Servo Signal         PWM output
GPIO5          Ultrasonic Trigger   Digital output
GPIO6          Ultrasonic Echo      Digital input
3.3V           Rain Sensor VCC      Power supply
5V             Servo VCC            Power supply
GND            All GND connections  Common ground
```

## 🔧 Wiring Instructions

### Step 1: Power Connections
1. Connect **GND** from NodeMCU to breadboard ground rail
2. Connect **3.3V** from NodeMCU to breadboard power rail
3. Connect **5V** from external power supply to breadboard (for servo)

### Step 2: Rain Sensor Connections
1. **VCC** → 3.3V power rail
2. **GND** → Ground rail
3. **Analog** → A0 (NodeMCU)
4. **Digital** → D2 (NodeMCU)

### Step 3: Servo Motor Connections
1. **Red wire (VCC)** → 5V power rail
2. **Black wire (GND)** → Ground rail
3. **Yellow/Orange wire (Signal)** → D1 (NodeMCU)

### Step 4: Ultrasonic Sensor Connections
1. **VCC** → 5V power rail
2. **GND** → Ground rail
3. **Trigger** → D5 (NodeMCU)
4. **Echo** → D6 (NodeMCU)

## Power Requirements

### Power Consumption
- **NodeMCU**: ~80mA (3.3V)
- **Rain Sensor**: ~5mA (3.3V)
- **Ultrasonic Sensor**: ~15mA (5V)
- **Servo Motor**: ~200-500mA (5V, during movement)

### Recommended Power Supply
- **USB Power**: 5V/1A (for development)
- **External Supply**: 5V/2A (for production)
- **Battery**: 18650 Li-ion cells (for portable setup)

## 🛠️ Assembly Instructions

### Step 1: Prepare Components
1. Gather all components
2. Check component specifications
3. Prepare breadboard and jumper wires

### Step 2: Mount Components
1. Place NodeMCU on breadboard
2. Mount rain sensor (consider weather protection)
3. Position servo motor (consider mechanical mounting)
4. Place ultrasonic sensor (consider mounting angle)

### Step 3: Make Connections
1. Follow wiring diagram
2. Double-check all connections
3. Secure loose wires
4. Test continuity with multimeter

### Step 4: Power On Test
1. Connect power supply
2. Check LED indicators
3. Verify no components get hot
4. Test basic functionality

## Enclosure Design

### Weather Protection
- **Rain Sensor**: Mount outside, protect from direct sun
- **NodeMCU**: Use waterproof enclosure
- **Servo Motor**: Protect from moisture
- **Ultrasonic Sensor**: Shield from rain

### Mounting Considerations
- **Servo Motor**: Secure mounting for mechanical load
- **Rain Sensor**: Level mounting for accurate readings
- **Ultrasonic Sensor**: Clear line of sight
- **NodeMCU**: Accessible for maintenance

## 🔧 Calibration

### Rain Sensor Calibration
1. **Dry Environment Test**
   - Note analog reading when completely dry
   - Should read close to 1024

2. **Wet Environment Test**
   - Note analog reading when wet
   - Should read significantly lower

3. **Threshold Setting**
   - Set threshold between dry and wet values
   - Test with actual water spray

### Servo Calibration
1. **Position 0°**
   - Should be fully closed/retracted
   - Adjust if needed

2. **Position 180°**
   - Should be fully open/extended
   - Adjust if needed

3. **Smooth Movement**
   - Test full range of motion
   - Ensure no binding or stalling

### Ultrasonic Sensor Calibration
1. **Distance Testing**
   - Test known distances
   - Verify accuracy

2. **Threshold Setting**
   - Set appropriate detection range
   - Test with various objects

## Troubleshooting

### Common Issues

1. **NodeMCU Won't Connect to WiFi**
   - Check SSID and password
   - Verify 2.4GHz network
   - Check signal strength

2. **Servo Not Moving**
   - Check power supply (5V)
   - Verify signal connection
   - Test with simple code

3. **Rain Sensor Not Reading**
   - Check analog connection
   - Verify power supply
   - Test with multimeter

4. **Ultrasonic Sensor Not Working**
   - Check trigger/echo connections
   - Verify power supply
   - Test with known distances

### Testing Procedures

1. **Individual Component Tests**
   ```cpp
   // Test rain sensor
   int analogValue = analogRead(A0);
   Serial.println("Rain sensor: " + String(analogValue));
   
   // Test servo
   servo.write(90);
   delay(1000);
   servo.write(0);
   
   // Test ultrasonic
   digitalWrite(TRIG_PIN, HIGH);
   delayMicroseconds(10);
   digitalWrite(TRIG_PIN, LOW);
   long duration = pulseIn(ECHO_PIN, HIGH);
   long distance = duration * 0.034 / 2;
   Serial.println("Distance: " + String(distance) + " cm");
   ```

2. **System Integration Test**
   - Upload complete code
   - Monitor serial output
   - Test web interface
   - Verify all functions

## Performance Optimization

### Power Management
- Use deep sleep mode when possible
- Implement power-saving algorithms
- Consider battery backup

### Signal Quality
- Use proper grounding
- Minimize wire lengths
- Shield sensitive components

### Mechanical Considerations
- Secure mounting points
- Consider vibration damping
- Plan for maintenance access

## Maintenance

### Regular Tasks
- Clean rain sensor monthly
- Check all connections
- Verify power supply stability
- Update firmware as needed

### Seasonal Maintenance
- Check weather protection
- Verify mounting security
- Test all functions
- Clean enclosure

---

For software setup, refer to the Arduino README.md file in the Arduino directory.
