using Microsoft.AspNetCore.Mvc;
using RainDetectionApp.Services;
using static RainDetectionApp.Services.RainSystemService;

namespace RainDetectionApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RainSystemController : ControllerBase
    {
        private readonly RainSystemService _rainSystemService;
        private readonly NodeMCUService _nodeMCUService;
        
        public RainSystemController(RainSystemService rainSystemService, NodeMCUService nodeMCUService)
        {
            _rainSystemService = rainSystemService;
            _nodeMCUService = nodeMCUService;
        }
        
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var status = await _nodeMCUService.GetSystemStatusAsync();
            var recentLogs = await _rainSystemService.GetRecentLogsAsync(10);
            
            return Ok(new
            {
                DeviceStatus = status,
                RecentLogs = recentLogs,
                IsOnline = status != null
            });
        }
        

        
        [HttpPost("system/toggle")]
        public async Task<IActionResult> ToggleSystem()
        {
            var success = await _rainSystemService.ExecuteCommandAsync("toggle_system", new { });
            return Ok(new { success, message = "System toggle command sent" });
        }
        
        [HttpPost("settings/update")]
        public async Task<IActionResult> UpdateSettings([FromBody] SettingsUpdateRequest request)
        {
            var success = await _rainSystemService.ExecuteCommandAsync("update_settings", new SettingsUpdateCommand
            {
                RainThreshold = request.RainThreshold,
                NormalPosition = request.NormalPosition,
                RainPosition = request.RainPosition
            });
            
            return Ok(new { success, message = "Settings update command sent" });
        }
        
        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs([FromQuery] int count = 50)
        {
            var logs = await _rainSystemService.GetRecentLogsAsync(count);
            return Ok(logs);
        }
        
        [HttpGet("commands")]
        public async Task<IActionResult> GetCommands([FromQuery] int count = 20)
        {
            var commands = await _rainSystemService.GetRecentCommandsAsync(count);
            return Ok(commands);
        }

        [HttpPost("servo/move")]
        public async Task<IActionResult> MoveServo([FromBody] ServoMoveRequest request)
        {
            if (request.Position < 0 || request.Position > 180)
            {
                return BadRequest(new { message = "Position must be between 0 and 180 degrees" });
            }
            
            var success = await _rainSystemService.ExecuteCommandAsync("servo_move", new ServoMoveCommand 
            { 
                Position = request.Position 
            });
            
            return Ok(new { success, message = $"Servo command sent: {request.Position}°" });
        }
        

[HttpPost("proximity/acknowledge")]
public async Task<IActionResult> AcknowledgeProximityAlert()
{
    var success = await _nodeMCUService.AcknowledgeProximityAlertAsync();
    return Ok(new { success, message = "Proximity alert acknowledged" });
}

[HttpPost("proximity/settings")]
public async Task<IActionResult> UpdateProximitySettings([FromBody] ProximitySettingsRequest request)
{
    if (request.Threshold < 10 || request.Threshold > 200)
    {
        return BadRequest(new { message = "Threshold must be between 10 and 200 cm" });
    }
    
    var success = await _nodeMCUService.UpdateProximitySettingsAsync(request.Threshold);
    return Ok(new { success, message = "Proximity settings updated" });
}

public class ProximitySettingsRequest
{
    public int Threshold { get; set; }
}
        
        public class ServoMoveRequest
        {
            public int Position { get; set; }
        }
        
        public class SettingsUpdateRequest
        {
            public int RainThreshold { get; set; }
            public int NormalPosition { get; set; }
            public int RainPosition { get; set; }
        }
    }
}