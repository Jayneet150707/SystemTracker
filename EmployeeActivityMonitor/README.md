# Employee Activity Monitor - .NET Console Application

A comprehensive .NET 8.0 console application for monitoring employee activity on Windows laptops. This tool tracks application usage, window titles, system activity, and generates detailed reports.

## 🎯 Features

### Core Tracking Capabilities
- ✅ **Active Application Monitoring** - Tracks which applications are being used
- ✅ **Window Title Tracking** - Captures window titles to see what's being searched/viewed (browser tabs, document names, etc.)
- ✅ **Usage Time Tracking** - Records how long each application is used
- ✅ **Application Switching Detection** - Logs every time user switches between applications
- ✅ **Idle Time Detection** - Identifies when system is idle (no keyboard/mouse activity for 5+ minutes)
- ✅ **Process Information** - Tracks process names and IDs
- ✅ **Real-time Console Output** - See activity as it happens
- ✅ **SQLite Database Storage** - All data persisted to local database
- ✅ **Comprehensive Reports** - Daily activity summaries with usage statistics

### What Gets Tracked

1. **Application Usage**
   - Application name (e.g., Chrome, Excel, VS Code)
   - Time spent in each application
   - Number of switches to that application

2. **Window/Search Activity**
   - Browser tab titles (see what websites/searches are being viewed)
   - Document names being edited
   - Any window title text

3. **System Activity**
   - Keyboard events count
   - Mouse events count
   - Idle periods
   - Active work periods

## 📋 Requirements

- **Operating System**: Windows (uses Windows API for detailed tracking)
  - *Note: Basic process monitoring works on Linux/Mac but with reduced features*
- **.NET 8.0 SDK or Runtime**
- **Administrator privileges** (recommended for full access to process information)

## 🚀 Installation & Setup

### 1. Install .NET 8.0 SDK
Download from: https://dotnet.microsoft.com/download/dotnet/8.0

### 2. Build the Application
```bash
cd EmployeeActivityMonitor
dotnet restore
dotnet build -c Release
```

### 3. Run the Application
```bash
cd bin/Release/net8.0
EmployeeActivityMonitor.exe
```

Or directly:
```bash
dotnet run --project EmployeeActivityMonitor
```

## 📖 Usage Guide

### Starting the Monitor

When you run the application, you'll see a menu:

```
╔════════════════════════════════════════════════════════════╗
║     EMPLOYEE ACTIVITY MONITOR - .NET Console App          ║
║     Track Applications, Window Titles & System Activity   ║
╚════════════════════════════════════════════════════════════╝

Choose an option:
  1. Start Monitoring (Real-time tracking)
  2. View Today's Report
  3. Start Monitoring + Auto Report on Exit
  4. Exit

Your choice:
```

### Option 1: Start Monitoring
- Begins real-time tracking
- Shows application switches as they happen
- Displays window title changes
- Press `Ctrl+C` to stop

### Option 2: View Today's Report
- Shows detailed report of today's activity
- Application usage statistics
- Recent activity log
- No new monitoring, just view data

### Option 3: Monitor + Report
- Starts monitoring AND automatically shows report when you exit
- Best for daily use

### Real-time Output Example

```
🚀 Employee Activity Monitor Started!
📊 Monitoring system activity...

Press Ctrl+C to stop monitoring and view report...

[14:32:15] 🔄 Switched to: chrome
               📝 Window: Employee Performance Dashboard - Google Chrome
               ⏱️  Duration: 45 seconds

[14:32:20] 📝 Window changed: Gmail - Inbox (234) - myemail@company.com
[14:33:01] 🔄 Switched to: EXCEL
               📝 Window: Budget_2024.xlsx - Excel
               ⏱️  Duration: 41 seconds

[14:35:00] 😴 System is idle
```

## 📊 Reports

### Daily Activity Report Example

```
================================================================================
📊 DAILY ACTIVITY REPORT
================================================================================

📅 Date: 2024-01-15
⏰ Total Applications Tracked: 8

┌─────────────────────────────────────────┬──────────────┬────────────┐
│ Application Name                        │ Time Used    │ Switches   │
├─────────────────────────────────────────┼──────────────┼────────────┤
│ chrome                                  │ 03:24:15     │         45 │
│ EXCEL                                   │ 02:15:30     │         12 │
│ OUTLOOK                                 │ 01:45:20     │         23 │
│ devenv                                  │ 01:12:45     │          8 │
│ slack                                   │ 00:45:10     │         34 │
│ Teams                                   │ 00:32:05     │         15 │
│ notepad++                               │ 00:18:30     │          6 │
│ explorer                                │ 00:12:15     │         18 │
└─────────────────────────────────────────┴──────────────┴────────────┘

⏱️  Total Active Time: 10:25:50

📋 Recent Activity Log (Last 10 entries):
────────────────────────────────────────────────────────────────────────────────
[15:45:32] chrome
           └─ GitHub - Employee Activity Monitor Pull Request
[15:43:15] devenv
           └─ ActivityMonitorService.cs - Visual Studio
[15:40:22] chrome
           └─ Stack Overflow - C# Windows API GetForegroundWindow
...
================================================================================
```

## 💾 Database

The application stores all data in **`activity_monitor.db`** (SQLite database) in the same directory as the executable.

### Database Schema

#### ActivityLogs Table
- Timestamp
- ApplicationName
- WindowTitle
- ProcessName
- ProcessId
- DurationSeconds
- ActivityType (Active, Idle, WindowChange, etc.)

#### SystemActivity Table
- Timestamp
- KeyboardEvents
- MouseEvents
- IsIdle

### Querying Data Manually

You can use any SQLite browser (e.g., DB Browser for SQLite) to query the database:

```sql
-- Get most used applications
SELECT ApplicationName, SUM(DurationSeconds) as TotalTime
FROM ActivityLogs
GROUP BY ApplicationName
ORDER BY TotalTime DESC;

-- Get all browser searches/tabs today
SELECT Timestamp, WindowTitle
FROM ActivityLogs
WHERE ApplicationName LIKE '%chrome%' 
  AND DATE(Timestamp) = DATE('now')
ORDER BY Timestamp DESC;

-- Get idle periods
SELECT Timestamp, IsIdle
FROM SystemActivity
WHERE IsIdle = 1;
```

## 🔐 Privacy & Security Considerations

### What This Tracks
- Application names and usage times
- Window titles (which may include personal/sensitive information)
- System activity patterns

### Important Notes
⚠️ **Privacy Warning**: This tool captures window titles which may include:
- Personal email subjects
- Private messages
- Sensitive document names
- Personal browsing activity

### Recommendations
1. **Inform employees** this monitoring is taking place
2. **Comply with local laws** regarding employee monitoring
3. **Secure the database** - restrict access to the SQLite database file
4. **Consider data retention** - implement automatic deletion of old data
5. **Use responsibly** - only for legitimate business purposes

## 🛠️ Technical Details

### Architecture
- **Language**: C# (.NET 8.0)
- **Database**: SQLite (via Microsoft.Data.Sqlite)
- **Windows API**: P/Invoke for window tracking
- **Threading**: Timer-based polling (2-second intervals for window tracking)

### Key Windows APIs Used
- `GetForegroundWindow()` - Gets active window handle
- `GetWindowText()` - Retrieves window title
- `GetWindowThreadProcessId()` - Gets process ID
- `GetLastInputInfo()` - Detects idle time

### Performance
- **CPU Usage**: < 1% (lightweight polling)
- **Memory**: ~20-30 MB
- **Database Size**: ~1-5 MB per day of activity
- **Monitoring Interval**: 2 seconds (configurable)

## 🔧 Configuration

You can modify these settings in `ActivityMonitorService.cs`:

```csharp
// Change monitoring frequency (line 45)
TimeSpan.FromSeconds(2)  // Change to desired interval

// Change idle timeout (line 226)
idleMinutes > 5  // Change from 5 to desired minutes

// Change system activity log frequency (line 49)
TimeSpan.FromSeconds(30)  // Change to desired interval
```

## 📝 Example Use Cases

1. **Productivity Analysis** - Understand how time is spent across applications
2. **Time Tracking** - Accurate time logs for billing/payroll
3. **Security Monitoring** - Detect unauthorized application usage
4. **Compliance** - Ensure work-time policies are followed
5. **Performance Reviews** - Data-driven insights for evaluations

## 🐛 Troubleshooting

### "Access Denied" errors
- Run as Administrator
- Some system processes can't be accessed without elevated privileges

### No activity showing
- Ensure application is running as Administrator
- Check if antivirus is blocking Windows API calls
- Verify SQLite database file has write permissions

### High CPU usage
- Increase monitoring interval in code
- Check for stuck processes in logs

## 📦 Dependencies

```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.0" />
<PackageReference Include="System.Management" Version="8.0.0" />
```

## 🚀 Future Enhancements

- [ ] Web dashboard for viewing reports
- [ ] Screenshot capture on demand
- [ ] Email daily reports
- [ ] Network activity tracking
- [ ] Application categorization (Productive vs Unproductive)
- [ ] Multi-user support
- [ ] Cloud sync capabilities
- [ ] Keystroke logging (with appropriate consent)
- [ ] Website URL tracking for browsers

## ⚖️ Legal Disclaimer

This software is provided for legitimate business use only. Users are responsible for:
- Obtaining proper consent from monitored employees
- Complying with all applicable laws and regulations
- Protecting collected data appropriately
- Using the tool ethically and responsibly

## 📄 License

This project is for demonstration and educational purposes.

---

**Built with .NET 8.0 | Made for Windows | SQLite Database**

