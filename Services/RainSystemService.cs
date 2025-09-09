using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RainDetectionApp.Data;
using RainDetectionApp.Models;

namespace RainDetectionApp.Services
{
    public class RainSystemService
    {
        private readonly DataContext _context;
        private readonly NodeMCUService _nodeMCUService;
        
        public RainSystemService(DataContext context, NodeMCUService nodeMCUService)
        {
            _context = context;
            _nodeMCUService = nodeMCUService;
        }
        
        public async Task LogRainEventAsync(string eventType, int analogValue, int digitalValue, bool isRaining, int servoPosition, string? notes = null)
        {
            var rainLog = new RainLog
            {
                EventType = eventType,
                AnalogValue = analogValue,
                DigitalValue = digitalValue,
                IsRaining = isRaining,
                ServoPosition = servoPosition,
                Notes = notes,
                Timestamp = DateTime.UtcNow
            };
            
            _context.RainLogs.Add(rainLog);
            await _context.SaveChangesAsync();
        }
        
        public async Task<List<RainLog>> GetRecentLogsAsync(int count = 50)
        {
            return await _context.RainLogs
                .OrderByDescending(r => r.Timestamp)
                .Take(count)
                .ToListAsync();
        }
        
        public async Task<bool> ExecuteCommandAsync(string commandType, object commandData)
        {
            // Save command to database
            var command = new DeviceCommand
            {
                CommandType = commandType,
                CommandData = JsonSerializer.Serialize(commandData),
                CreatedAt = DateTime.UtcNow
            };
            
            _context.DeviceCommands.Add(command);
            await _context.SaveChangesAsync();
            
            // Execute command
            bool success = false;
            string response = "";
            
            try
            {
                switch (commandType)
                {
                    case "servo_move":
                        var servoData = JsonSerializer.Deserialize<ServoMoveCommand>(command.CommandData);
                        success = await _nodeMCUService.MoveServoAsync(servoData!.Position);
                        response = $"Servo moved to {servoData.Position}°";
                        
                        // Log the manual servo movement
                        await LogRainEventAsync("manual_servo", 0, 0, false, servoData.Position, $"Manual command via web app");
                        break;
                        
                    case "toggle_system":
                        success = await _nodeMCUService.ToggleSystemAsync();
                        response = "System toggled";
                        break;
                        
                    case "update_settings":
                        var settingsData = JsonSerializer.Deserialize<SettingsUpdateCommand>(command.CommandData);
                        success = await _nodeMCUService.UpdateSettingsAsync(
                            settingsData!.RainThreshold, 
                            settingsData.NormalPosition, 
                            settingsData.RainPosition
                        );
                        response = "Settings updated";
                        
                        // Save settings to database
                        await UpdateSettingAsync("RainThreshold", settingsData.RainThreshold.ToString());
                        await UpdateSettingAsync("NormalPosition", settingsData.NormalPosition.ToString());
                        await UpdateSettingAsync("RainPosition", settingsData.RainPosition.ToString());
                        break;
                }
                
                // Update command status
                command.IsExecuted = true;
                command.WasSuccessful = success;
                command.ExecutedAt = DateTime.UtcNow;
                command.Response = response;
                
                await _context.SaveChangesAsync();
                
            }
            catch (Exception ex)
            {
                command.IsExecuted = true;
                command.WasSuccessful = false;
                command.ExecutedAt = DateTime.UtcNow;
                command.Response = $"Error: {ex.Message}";
                
                await _context.SaveChangesAsync();
            }
            
            return success;
        }
        
        public async Task<string?> GetSettingAsync(string settingName)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingName == settingName);
            return setting?.SettingValue;
        }
        
        public async Task UpdateSettingAsync(string settingName, string settingValue, string? description = null)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingName == settingName);
                
            if (setting == null)
            {
                setting = new SystemSettings
                {
                    SettingName = settingName,
                    SettingValue = settingValue,
                    Description = description,
                    LastModified = DateTime.UtcNow
                };
                _context.SystemSettings.Add(setting);
            }
            else
            {
                setting.SettingValue = settingValue;
                setting.LastModified = DateTime.UtcNow;
                if (description != null) setting.Description = description;
            }
            
            await _context.SaveChangesAsync();
        }
        
        public async Task<List<DeviceCommand>> GetRecentCommandsAsync(int count = 20)
        {
            return await _context.DeviceCommands
                .OrderByDescending(c => c.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        // Services/RainSystemService.cs - Update LogRainEventAsync method

public async Task LogRainEventAsync(string eventType, int analogValue, int digitalValue, bool isRaining, int servoPosition, long? distance = null, string? notes = null)
{
    var rainLog = new RainLog
    {
        EventType = eventType,
        AnalogValue = analogValue,
        DigitalValue = digitalValue,
        IsRaining = isRaining,
        ServoPosition = servoPosition,
        Distance = distance, // New field
        Notes = notes,
        Timestamp = DateTime.UtcNow
    };
    
    _context.RainLogs.Add(rainLog);
    await _context.SaveChangesAsync();
}
        
        // Command DTOs
        public class ServoMoveCommand
        {
            public int Position { get; set; }
        }
        
        public class SettingsUpdateCommand
        {
            public int RainThreshold { get; set; }
            public int NormalPosition { get; set; }
            public int RainPosition { get; set; }
        }
    }
}