using System.Text.Json;
using RainDetectionApp.Models;

namespace RainDetectionApp.Services
{
    public class NodeMCUService
    {
        private readonly HttpClient _httpClient;
        private readonly string _deviceBaseUrl;
        private readonly ILogger<NodeMCUService> _logger;

        public NodeMCUService(HttpClient httpClient, IConfiguration configuration, ILogger<NodeMCUService> logger)
        {
            _httpClient = httpClient;
            _deviceBaseUrl = configuration.GetConnectionString("NodeMCUDevice") ?? "http://192.168.1.100";
            _logger = logger;

            // Set timeout for HTTP requests
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<RainSystemStatus?> GetSystemStatusAsync()
        {
            try
            {
                _logger.LogInformation("Fetching system status from {DeviceUrl}", _deviceBaseUrl);

                var response = await _httpClient.GetStringAsync($"{_deviceBaseUrl}/api/status");
                var status = JsonSerializer.Deserialize<RainSystemStatus>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Successfully retrieved system status");
                return status;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error when fetching system status");
                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout when fetching system status");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error when fetching system status");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when fetching system status");
                return null;
            }
        }

        public async Task<bool> MoveServoAsync(int position)
        {
            try
            {
                if (position < 0 || position > 180)
                {
                    _logger.LogWarning("Invalid servo position: {Position}. Must be between 0-180", position);
                    return false;
                }

                _logger.LogInformation("Moving servo to position {Position}", position);

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("position", position.ToString())
                });

                var response = await _httpClient.PostAsync($"{_deviceBaseUrl}/api/servo/move", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Servo moved successfully to position {Position}", position);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to move servo. Status code: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving servo to position {Position}", position);
                return false;
            }
        }

        public async Task<bool> ToggleSystemAsync()
        {
            try
            {
                _logger.LogInformation("Toggling system state");

                var response = await _httpClient.PostAsync($"{_deviceBaseUrl}/api/system/toggle", null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("System toggled successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to toggle system. Status code: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling system");
                return false;
            }
        }

        public async Task<bool> UpdateSettingsAsync(int rainThreshold, int normalPosition, int rainPosition)
        {
            try
            {
                _logger.LogInformation("Updating settings: Threshold={Threshold}, Normal={Normal}°, Rain={Rain}°",
                    rainThreshold, normalPosition, rainPosition);

                // Validate inputs
                if (rainThreshold < 0 || rainThreshold > 1024)
                {
                    _logger.LogWarning("Invalid rain threshold: {Threshold}. Must be between 0-1024", rainThreshold);
                    return false;
                }

                if (normalPosition < 0 || normalPosition > 180)
                {
                    _logger.LogWarning("Invalid normal position: {Position}. Must be between 0-180", normalPosition);
                    return false;
                }

                if (rainPosition < 0 || rainPosition > 180)
                {
                    _logger.LogWarning("Invalid rain position: {Position}. Must be between 0-180", rainPosition);
                    return false;
                }

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("rainThreshold", rainThreshold.ToString()),
                    new KeyValuePair<string, string>("normalPosition", normalPosition.ToString()),
                    new KeyValuePair<string, string>("rainPosition", rainPosition.ToString())
                });

                var response = await _httpClient.PostAsync($"{_deviceBaseUrl}/api/settings", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Settings updated successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to update settings. Status code: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings");
                return false;
            }
        }

        public async Task<List<RainEvent>> GetRecentEventsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching recent events from device");

                var response = await _httpClient.GetStringAsync($"{_deviceBaseUrl}/api/events");
                var eventsResponse = JsonSerializer.Deserialize<EventsResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var events = eventsResponse?.Events ?? new List<RainEvent>();
                _logger.LogInformation("Retrieved {Count} recent events", events.Count);

                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent events");
                return new List<RainEvent>();
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing connection to NodeMCU device");

                var response = await _httpClient.GetAsync($"{_deviceBaseUrl}/api/health");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Connection test successful");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Connection test failed. Status code: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed");
                return false;
            }
        }

        public async Task<DeviceInfo?> GetDeviceInfoAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_deviceBaseUrl}/api/status");
                var status = JsonSerializer.Deserialize<RainSystemStatus>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (status != null)
                {
                    return new DeviceInfo
                    {
                        IpAddress = status.Ip,
                        Uptime = TimeSpan.FromMilliseconds(status.Uptime),
                        IsOnline = true,
                        LastSeen = DateTime.UtcNow
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device info");
                return new DeviceInfo
                {
                    IsOnline = false,
                    LastSeen = DateTime.UtcNow
                };
            }
        }

        // Services/NodeMCUService.cs - Add these methods

public async Task<bool> AcknowledgeProximityAlertAsync()
{
    try
    {
        var response = await _httpClient.PostAsync($"{_deviceBaseUrl}/api/proximity/acknowledge", null);
        return response.IsSuccessStatusCode;
    }
    catch
    {
        return false;
    }
}

public async Task<bool> UpdateProximitySettingsAsync(int threshold)
{
    try
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("threshold", threshold.ToString())
        });
        
        var response = await _httpClient.PostAsync($"{_deviceBaseUrl}/api/proximity/settings", content);
        return response.IsSuccessStatusCode;
    }
    catch
    {
        return false;
    }
}

        // Helper method to validate device connection and return detailed status
        public async Task<ConnectionStatus> GetConnectionStatusAsync()
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.GetAsync($"{_deviceBaseUrl}/api/health");
                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    return new ConnectionStatus
                    {
                        IsConnected = true,
                        ResponseTime = stopwatch.Elapsed,
                        StatusCode = response.StatusCode,
                        LastChecked = DateTime.UtcNow
                    };
                }
                else
                {
                    return new ConnectionStatus
                    {
                        IsConnected = false,
                        ResponseTime = stopwatch.Elapsed,
                        StatusCode = response.StatusCode,
                        LastChecked = DateTime.UtcNow,
                        ErrorMessage = $"HTTP {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ConnectionStatus
                {
                    IsConnected = false,
                    LastChecked = DateTime.UtcNow,
                    ErrorMessage = ex.Message
                };
            }
        }

        // Private helper classes
        private class EventsResponse
        {
            public List<RainEvent> Events { get; set; } = new();
        }
    }

    public class DeviceInfo
    {
        public string IpAddress { get; set; } = string.Empty;
        public TimeSpan Uptime { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; }
    }
    
    public class ConnectionStatus
    {
        public bool IsConnected { get; set; }
        public TimeSpan? ResponseTime { get; set; }
        public System.Net.HttpStatusCode? StatusCode { get; set; }
        public DateTime LastChecked { get; set; }
        public string? ErrorMessage { get; set; }
    }

    // Main models (if not already defined elsewhere)
    public class RainSystemStatus
    {
        public bool SystemEnabled { get; set; }
        public bool IsRaining { get; set; }
        public int RainThreshold { get; set; }
        public int AnalogValue { get; set; }
        public int DigitalValue { get; set; }
        public int ServoPosition { get; set; }
        public string Status { get; set; } = string.Empty;
        public long LastRainChange { get; set; }
        public long Uptime { get; set; }
        public string Ip { get; set; } = string.Empty;
        
    public bool ProximityAlert { get; set; }
    public long ProximityDistance { get; set; }
    public long CurrentDistance { get; set; }
    public long LastProximityTime { get; set; }
    public bool IntruderDetected { get; set; }
    public int ProximityThreshold { get; set; }
    }
    
    public class RainEvent
    {
        public long Timestamp { get; set; }
        public string Event { get; set; } = string.Empty;
        public int AnalogValue { get; set; }
        public bool IsRaining { get; set; }
        public DateTime DateTime => DateTime.FromBinary(Timestamp);
    }
}

