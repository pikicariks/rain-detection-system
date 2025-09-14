#include <ESP8266WiFi.h>
#include <ESP8266WebServer.h>
#include <ArduinoJson.h>
#include <Servo.h>

const char* ssid = "YOUR_WIFI_SSID";
const char* password = "YOUR_WIFI_PASSWORD";

#define RAIN_DIGITAL_PIN  D2
#define RAIN_ANALOG_PIN   A0
#define SERVO_PIN         D1
#define TRIG_PIN          D7    // HC-SR04 Trigger pin
#define ECHO_PIN          D5    // HC-SR04 Echo pin


#define PROXIMITY_THRESHOLD 30  
#define MIN_DISTANCE 2         
#define PROXIMITY_HYSTERESIS 10 

Servo myServo;
ESP8266WebServer server(80);

struct SystemState {
  bool systemEnabled = true;
  int rainThreshold = 600;
  int servoNormalPosition = 0;
  int servoRainPosition = 180;
  bool isRaining = false;
  bool servoMoved = false;
  int currentAnalogValue = 0;
  int currentDigitalValue = 1;
  unsigned long lastRainChange = 0;
  String status = "System Ready";
  
  
  long currentDistance = 0;
  long baselineDistance = 0;
  bool intruderDetected = false;
  bool proximityAlert = false;
  long proximityDistance = 0;
  unsigned long lastProximityTime = 0;
  int proximityThreshold = PROXIMITY_THRESHOLD;
} systemState;

struct RainEvent {
  unsigned long timestamp;
  String event;
  int analogValue;
  bool isRaining;
  long distance; 
};

RainEvent recentEvents[10];
int eventIndex = 0;


long getDistance() {
  digitalWrite(TRIG_PIN, LOW);
  delayMicroseconds(2);
  digitalWrite(TRIG_PIN, HIGH);
  delayMicroseconds(10);
  digitalWrite(TRIG_PIN, LOW);
  
  long duration = pulseIn(ECHO_PIN, HIGH, 30000); // 30ms timeout
  if (duration == 0) return -1; 
  
  long distance = duration * 0.034 / 2; 
  return distance;
}


void checkProximity() {
  long distance = getDistance();
  
  if (distance == -1 || distance > 400) return; 
  
  systemState.currentDistance = distance;
  
  
  if (distance < systemState.proximityThreshold && distance > MIN_DISTANCE) {
    if (!systemState.intruderDetected) {
      systemState.intruderDetected = true;
      systemState.proximityAlert = true;
      systemState.proximityDistance = distance;
      systemState.lastProximityTime = millis();
      systemState.status = "PROXIMITY ALERT: Object detected at " + String(distance) + "cm";
      
      logEvent("proximity_alert", systemState.currentAnalogValue, systemState.isRaining, distance);
      
      Serial.println("PROXIMITY ALERT: Object detected at " + String(distance) + "cm");
    }
  } else if (distance > systemState.proximityThreshold + PROXIMITY_HYSTERESIS) {
    if (systemState.intruderDetected) {
      systemState.intruderDetected = false;
      systemState.status = "Proximity area clear - " + String(distance) + "cm";
      
      logEvent("proximity_clear", systemState.currentAnalogValue, systemState.isRaining, distance);
      
      Serial.println("Proximity area clear - " + String(distance) + "cm");
    }
  }
}

void setup() {
  Serial.begin(9600);
  delay(1000);
  
  Serial.println("=== Smart Rain Detection System with Proximity ===");
  
  pinMode(RAIN_DIGITAL_PIN, INPUT);
  pinMode(TRIG_PIN, OUTPUT);
  pinMode(ECHO_PIN, INPUT);
  myServo.attach(SERVO_PIN);
  myServo.write(systemState.servoNormalPosition);
  
  
  delay(2000); 
  systemState.baselineDistance = getDistance();
  Serial.println("Baseline distance: " + String(systemState.baselineDistance) + "cm");
  
  WiFi.begin(ssid, password);
  Serial.print("Connecting to WiFi");
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println();
  Serial.print("Connected! IP address: ");
  Serial.println(WiFi.localIP());

  setupWebRoutes();
  
  server.enableCORS(true);
  server.begin();
  
  Serial.println("Web server started");
  Serial.println("Rain detection system ready");
  Serial.println("Proximity sensor monitoring");
  Serial.println("Access web API at: http://" + WiFi.localIP().toString());
  Serial.println("=====================================");
}

void setupWebRoutes() {
  
  server.on("/api/status", HTTP_GET, []() {
    DynamicJsonDocument doc(1024);
    doc["systemEnabled"] = systemState.systemEnabled;
    doc["isRaining"] = systemState.isRaining;
    doc["rainThreshold"] = systemState.rainThreshold;
    doc["analogValue"] = systemState.currentAnalogValue;
    doc["digitalValue"] = systemState.currentDigitalValue;
    doc["servoPosition"] = systemState.isRaining ? systemState.servoRainPosition : systemState.servoNormalPosition;
    doc["status"] = systemState.status;
    doc["lastRainChange"] = systemState.lastRainChange;
    doc["uptime"] = millis();
    doc["ip"] = WiFi.localIP().toString();
    
    
    doc["proximityAlert"] = systemState.proximityAlert;
    doc["proximityDistance"] = systemState.proximityDistance;
    doc["currentDistance"] = systemState.currentDistance;
    doc["lastProximityTime"] = systemState.lastProximityTime;
    doc["intruderDetected"] = systemState.intruderDetected;
    doc["proximityThreshold"] = systemState.proximityThreshold;
    
    String response;
    serializeJson(doc, response);
    server.send(200, "application/json", response);
  });
  
  
  server.on("/api/proximity/acknowledge", HTTP_POST, []() {
    systemState.proximityAlert = false;
    server.send(200, "application/json", "{\"success\": true, \"message\": \"Proximity alert acknowledged\"}");
  });
  
  
  server.on("/api/proximity/settings", HTTP_POST, []() {
    if (server.hasArg("threshold")) {
      int threshold = server.arg("threshold").toInt();
      if (threshold >= 10 && threshold <= 200) {
        systemState.proximityThreshold = threshold;
        server.send(200, "application/json", "{\"success\": true, \"threshold\": " + String(threshold) + "}");
      } else {
        server.send(400, "application/json", "{\"success\": false, \"message\": \"Threshold must be between 10-200cm\"}");
      }
    } else {
      server.send(400, "application/json", "{\"success\": false, \"message\": \"Missing threshold parameter\"}");
    }
  });
  
  server.on("/api/servo/move", HTTP_POST, []() {
    if (server.hasArg("position")) {
      int position = server.arg("position").toInt();
      if (position >= 0 && position <= 180) {
        myServo.write(position);
        systemState.status = "Manual servo control: " + String(position) + "°";
        logEvent("manual_servo", systemState.currentAnalogValue, systemState.isRaining, systemState.currentDistance);
        
        server.send(200, "application/json", "{\"success\": true, \"position\": " + String(position) + "}");
      } else {
        server.send(400, "application/json", "{\"success\": false, \"message\": \"Invalid position (0-180)\"}");
      }
    } else {
      server.send(400, "application/json", "{\"success\": false, \"message\": \"Missing position parameter\"}");
    }
  });

  server.on("/api/servo/move", HTTP_POST, []() {
    if (server.hasArg("position")) {
      int position = server.arg("position").toInt();
      if (position >= 0 && position <= 180) {
        myServo.write(position);
        systemState.status = "Manual servo control: " + String(position) + "°";
        logEvent("manual_servo", systemState.currentAnalogValue, systemState.isRaining);
        
        server.send(200, "application/json", "{\"success\": true, \"position\": " + String(position) + "}");
      } else {
        server.send(400, "application/json", "{\"success\": false, \"message\": \"Invalid position (0-180)\"}");
      }
    } else {
      server.send(400, "application/json", "{\"success\": false, \"message\": \"Missing position parameter\"}");
    }
  });
  
  server.on("/api/system/toggle", HTTP_POST, []() {
    systemState.systemEnabled = !systemState.systemEnabled;
    systemState.status = systemState.systemEnabled ? "System Enabled" : "System Disabled";
    
    DynamicJsonDocument doc(256);
    doc["success"] = true;
    doc["enabled"] = systemState.systemEnabled;
    doc["message"] = systemState.status;
    
    String response;
    serializeJson(doc, response);
    server.send(200, "application/json", response);
  });
  
  server.on("/api/settings", HTTP_POST, []() {
    bool updated = false;
    DynamicJsonDocument response(512);
    
    if (server.hasArg("rainThreshold")) {
      int threshold = server.arg("rainThreshold").toInt();
      if (threshold >= 0 && threshold <= 1024) {
        systemState.rainThreshold = threshold;
        updated = true;
      }
    }
    
    if (server.hasArg("normalPosition")) {
      int pos = server.arg("normalPosition").toInt();
      if (pos >= 0 && pos <= 180) {
        systemState.servoNormalPosition = pos;
        updated = true;
      }
    }
    
    if (server.hasArg("rainPosition")) {
      int pos = server.arg("rainPosition").toInt();
      if (pos >= 0 && pos <= 180) {
        systemState.servoRainPosition = pos;
        updated = true;
      }
    }
    
    if (updated) {
      systemState.status = "Settings updated";
      response["success"] = true;
      response["message"] = "Settings updated successfully";
      response["rainThreshold"] = systemState.rainThreshold;
      response["normalPosition"] = systemState.servoNormalPosition;
      response["rainPosition"] = systemState.servoRainPosition;
    } else {
      response["success"] = false;
      response["message"] = "No valid parameters provided";
    }
    
    String responseStr;
    serializeJson(response, responseStr);
    server.send(updated ? 200 : 400, "application/json", responseStr);
  });
  
  server.on("/api/events", HTTP_GET, []() {
    DynamicJsonDocument doc(2048);
    JsonArray events = doc.createNestedArray("events");
    
    for (int i = 0; i < 10; i++) {
      int idx = (eventIndex - 1 - i + 10) % 10;
      if (recentEvents[idx].timestamp > 0) {
        JsonObject event = events.createNestedObject();
        event["timestamp"] = recentEvents[idx].timestamp;
        event["event"] = recentEvents[idx].event;
        event["analogValue"] = recentEvents[idx].analogValue;
        event["isRaining"] = recentEvents[idx].isRaining;
        event["distance"] = recentEvents[idx].distance;
      }
    }
    
    String response;
    serializeJson(doc, response);
    server.send(200, "application/json", response);
  });
  
  server.on("/api/health", HTTP_GET, []() {
    server.send(200, "application/json", "{\"status\": \"healthy\", \"uptime\": " + String(millis()) + "}");
  });
  
  server.onNotFound([]() {
    if (server.method() == HTTP_OPTIONS) {
      server.send(200);
    } else {
      server.send(404, "application/json", "{\"error\": \"Not found\"}");
    }
  });
}

void loop() {
  server.handleClient();
  
  
  checkProximity();
  
  if (!systemState.systemEnabled) {
    delay(100);
    return;
  }
  
  systemState.currentDigitalValue = digitalRead(RAIN_DIGITAL_PIN);
  systemState.currentAnalogValue = analogRead(RAIN_ANALOG_PIN);
  
  bool rainDetected = (systemState.currentDigitalValue == LOW) || 
                     (systemState.currentAnalogValue < systemState.rainThreshold);
  
  if (rainDetected && !systemState.isRaining) {
    systemState.isRaining = true;
    systemState.lastRainChange = millis();
    myServo.write(systemState.servoRainPosition);
    systemState.status = "Rain detected - Servo activated";
    logEvent("rain_start", systemState.currentAnalogValue, true, systemState.currentDistance);
    
    Serial.println("RAIN DETECTED! Servo moved to " + String(systemState.servoRainPosition) + "°");
    
  } else if (!rainDetected && systemState.isRaining) {
    systemState.isRaining = false;
    systemState.lastRainChange = millis();
    myServo.write(systemState.servoNormalPosition);
    systemState.status = "Rain stopped - Servo returned";
    logEvent("rain_stop", systemState.currentAnalogValue, false, systemState.currentDistance);
    
    Serial.println("RAIN STOPPED! Servo returned to " + String(systemState.servoNormalPosition) + "°");
  }
  
  static unsigned long lastPrint = 0;
  if (millis() - lastPrint > 5000) {
    Serial.print("Sensor: Digital=" + String(systemState.currentDigitalValue));
    Serial.print(" Analog=" + String(systemState.currentAnalogValue));
    Serial.print(" | Status: ");
    Serial.print(systemState.isRaining ? "RAINING" : "DRY");
    Serial.print(" | Servo: " + String(systemState.isRaining ? systemState.servoRainPosition : systemState.servoNormalPosition) + "°");
    Serial.print(" | Distance: " + String(systemState.currentDistance) + "cm");
    if (systemState.intruderDetected) {
      Serial.print(" | PROXIMITY ALERT");
    }
    Serial.println();
    lastPrint = millis();
  }
  
  delay(200);  
}

void logEvent(String event, int analogValue, bool isRaining, long distance) {
  recentEvents[eventIndex].timestamp = millis();
  recentEvents[eventIndex].event = event;
  recentEvents[eventIndex].analogValue = analogValue;
  recentEvents[eventIndex].isRaining = isRaining;
  recentEvents[eventIndex].distance = distance;
  
  eventIndex = (eventIndex + 1) % 10;
}


void logEvent(String event, int analogValue, bool isRaining) {
  logEvent(event, analogValue, isRaining, systemState.currentDistance);
}