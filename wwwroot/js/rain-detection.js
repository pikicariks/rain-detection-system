let systemData = null;
let isOnline = false;

document.addEventListener('DOMContentLoaded', function() {
    initializeControls();
    refreshData();
    setInterval(refreshData, 5000); 
});

function initializeControls() {
    const slider = document.getElementById('servoSlider');
    const sliderValue = document.getElementById('sliderValue');
    
    slider.addEventListener('input', function() {
        sliderValue.textContent = this.value;
    });
}

async function refreshData() {
    try {
        const response = await fetch('/api/RainSystem/status');
        const data = await response.json();
        
        systemData = data.deviceStatus;
        isOnline = data.isOnline;
        
        updateUI(data);
        updateConnectionStatus(true);
        
    } catch (error) {
        console.error('Error fetching data:', error);
        updateConnectionStatus(false);
    }
}

function updateUI(data) {
    const deviceStatus = data.deviceStatus;
    const recentLogs = data.recentLogs;
    
    if (deviceStatus) {
        
        updateDeviceCard(true);
        updateRainCard(deviceStatus.isRaining);
        updateServoPosition(deviceStatus.servoPosition);
        updateSensorReading(deviceStatus.analogValue);
        
        
        document.getElementById('thresholdInput').value = deviceStatus.rainThreshold;
        
        
        document.getElementById('toggleText').textContent = 
            deviceStatus.systemEnabled ? 'Disable System' : 'Enable System';
    } else {
        updateDeviceCard(false);
    }
    
    
    updateActivityLog(recentLogs);

    if (deviceStatus && deviceStatus.proximityAlert) {
        showProximityAlert(deviceStatus.proximityDistance);
        
        fetch('/api/RainSystem/proximity/acknowledge', { method: 'POST' });
    }
    
}

function showProximityAlert(distance) {
    showAlert(`PROXIMITY ALERT: Object detected ${distance}cm from sensor!`, 'warning');
}

async function moveServo() {
    const position = document.getElementById('servoSlider').value;
    const button = event.target;
    
    button.disabled = true;
    button.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Moving...';
    
    try {
        const response = await fetch('/api/RainSystem/servo/move', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ position: parseInt(position) })
        });
        
        const result = await response.json();
        
        if (result.success) {
            showAlert('Servo moved successfully!', 'success');
            setTimeout(refreshData, 1000);
        } else {
            showAlert('Failed to move servo', 'danger');
        }
    } catch (error) {
        showAlert('Error communicating with system', 'danger');
    } finally {
        button.disabled = false;
        button.innerHTML = '<i class="fas fa-paper-plane me-1"></i>Move Servo';
    }
}

function updateConnectionStatus(online) {
    const indicator = document.getElementById('statusIndicator');
    const statusText = document.getElementById('statusText');
    
    if (online) {
        indicator.className = 'status-indicator status-online';
        statusText.textContent = 'Connected';
    } else {
        indicator.className = 'status-indicator status-offline';
        statusText.textContent = 'Disconnected';
    }
}

function updateDeviceCard(online) {
    const card = document.getElementById('deviceCard');
    const status = document.getElementById('deviceStatus');
    
    if (online) {
        card.className = 'card status-card device-online';
        status.innerHTML = '<i class="fas fa-check-circle me-1 text-success"></i><span>Online</span>';
    } else {
        card.className = 'card status-card device-offline';
        status.innerHTML = '<i class="fas fa-times-circle me-1 text-danger"></i><span>Offline</span>';
    }
}

function updateRainCard(isRaining) {
    const card = document.getElementById('rainCard');
    const status = document.getElementById('rainStatus');
    
    if (isRaining) {
        card.className = 'card status-card rain-detected';
        status.innerHTML = '<i class="fas fa-cloud-rain me-1 text-primary"></i><span>Rain Detected</span>';
    } else {
        card.className = 'card status-card system-dry';
        status.innerHTML = '<i class="fas fa-sun me-1 text-warning"></i><span>Dry</span>';
    }
}

function updateServoPosition(position) {
    document.getElementById('servoPosition').innerHTML = `<span>${position}°</span>`;
}

function updateSensorReading(analogValue) {
    document.getElementById('sensorReading').innerHTML = `<span>${analogValue}/1024</span>`;
}

function updateActivityLog(logs) {
    const logContainer = document.getElementById('activityLog');
    const logWrapper = document.getElementById('activityLogWrapper');
    
    if (!logs || logs.length === 0) {
        logContainer.innerHTML = '<div class="text-center text-muted py-3">No recent activity</div>';
        logWrapper.classList.remove('has-scroll');
        return;
    }
    
    const logHTML = logs.map(log => {
        const time = new Date(log.timestamp).toLocaleString('en-GB', {
            timeZone: 'Europe/Sarajevo',
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
            hour12: false
        });

        let className = 'log-item';
        let icon = 'fas fa-info-circle';
        
        switch (log.eventType) {
            case 'rain_start':
                className += ' log-rain-start';
                icon = 'fas fa-cloud-rain';
                break;
            case 'rain_stop':
                className += ' log-rain-stop';
                icon = 'fas fa-sun';
                break;
            case 'manual_servo':
                className += ' log-manual';
                icon = 'fas fa-hand-paper';
                break;
        }
        
        return `
            <div class="${className}">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <i class="${icon} me-2"></i>
                        <strong>${log.eventType.replace('_', ' ').toUpperCase()}</strong>
                        <span class="text-muted ms-2">Analog: ${log.analogValue}, Servo: ${log.servoPosition}°</span>
                    </div>
                    <small class="text-muted">${time}</small>
                </div>
                ${log.notes ? `<div class="mt-1 text-muted small">${log.notes}</div>` : ''}
            </div>
        `;
    }).join('');
    
    logContainer.innerHTML = logHTML;
    
    
    setTimeout(() => {
        const container = document.getElementById('activityLog');
        if (container.scrollHeight > container.clientHeight) {
            logWrapper.classList.add('has-scroll');
        } else {
            logWrapper.classList.remove('has-scroll');
        }
    }, 100);
}

async function toggleSystem() {
    const button = event.target;
    const originalText = button.innerHTML;
    
    button.disabled = true;
    button.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Processing...';
    
    try {
        const response = await fetch('/api/RainSystem/system/toggle', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            }
        });
        
        const result = await response.json();
        
        if (result.success) {
            showAlert('System toggled successfully!', 'success');
            setTimeout(refreshData, 1000);
        } else {
            showAlert('Failed to toggle system', 'danger');
        }
    } catch (error) {
        showAlert('Error communicating with system', 'danger');
    } finally {
        button.disabled = false;
        button.innerHTML = originalText;
    }
}

async function updateSettings() {
    const threshold = document.getElementById('thresholdInput').value;
    const normalPos = document.getElementById('normalPosInput').value;
    const rainPos = document.getElementById('rainPosInput').value;
    const button = event.target;
    
    button.disabled = true;
    button.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Saving...';
    
    try {
        const response = await fetch('/api/RainSystem/settings/update', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                rainThreshold: parseInt(threshold),
                normalPosition: parseInt(normalPos),
                rainPosition: parseInt(rainPos)
            })
        });
        
        const result = await response.json();
        
        if (result.success) {
            showAlert('Settings updated successfully!', 'success');
            setTimeout(refreshData, 1000);
        } else {
            showAlert('Failed to update settings', 'danger');
        }
    } catch (error) {
        showAlert('Error communicating with system', 'danger');
    } finally {
        button.disabled = false;
        button.innerHTML = '<i class="fas fa-save me-1"></i>Save Settings';
    }
}

function showAlert(message, type) {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    alertDiv.style.top = '20px';
    alertDiv.style.right = '20px';
    alertDiv.style.zIndex = '9999';
    alertDiv.style.minWidth = window.innerWidth < 768 ? '90%' : '300px';
    alertDiv.style.left = window.innerWidth < 768 ? '5%' : 'auto';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(alertDiv);
    
    setTimeout(() => {
        if (alertDiv.parentNode) {
            alertDiv.parentNode.removeChild(alertDiv);
        }
    }, 5000);
}

document.addEventListener('keydown', function(event) {
    if (window.innerWidth > 768 && event.ctrlKey) {
        switch(event.key) {
            case 'r':
                event.preventDefault();
                refreshData();
                break;
            case 't':
                event.preventDefault();
                toggleSystem();
                break;
        }
    }
});
