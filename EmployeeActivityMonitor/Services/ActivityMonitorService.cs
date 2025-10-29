using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using EmployeeActivityMonitor.Models;

namespace EmployeeActivityMonitor.Services;

public class ActivityMonitorService
{
    private readonly DatabaseService _database;
    private string? _currentApplicationName;
    private string? _currentWindowTitle;
    private int _currentProcessId;
    private DateTime _lastActivityTime;
    private int _keyboardEventCount;
    private int _mouseEventCount;
    private Timer? _monitorTimer;
    private Timer? _systemActivityTimer;

    // Windows API imports
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    public ActivityMonitorService(DatabaseService database)
    {
        _database = database;
        _lastActivityTime = DateTime.Now;
    }

    public void Start()
    {
        Console.WriteLine("🚀 Employee Activity Monitor Started!");
        Console.WriteLine("📊 Monitoring system activity...\n");

        // Monitor active window every 2 seconds
        _monitorTimer = new Timer(MonitorActiveWindow, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

        // Log system activity every 30 seconds
        _systemActivityTimer = new Timer(LogSystemActivity, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

        Console.WriteLine("Press Ctrl+C to stop monitoring and view report...\n");
    }

    private void MonitorActiveWindow(object? state)
    {
        try
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // For non-Windows systems, use process-based monitoring
                MonitorProcesses();
                return;
            }

            IntPtr handle = GetForegroundWindow();
            if (handle == IntPtr.Zero)
                return;

            // Get window title
            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);
            GetWindowText(handle, buff, nChars);
            string windowTitle = buff.ToString();

            // Get process ID
            uint processId;
            GetWindowThreadProcessId(handle, out processId);

            if (processId == 0)
                return;

            var process = Process.GetProcessById((int)processId);
            string applicationName = process.ProcessName;
            string mainModuleName = string.Empty;

            try
            {
                mainModuleName = process.MainModule?.FileName ?? applicationName;
            }
            catch
            {
                mainModuleName = applicationName;
            }

            // Check if application changed
            if (_currentApplicationName != applicationName || _currentProcessId != (int)processId)
            {
                // Log the previous application usage
                if (_currentApplicationName != null)
                {
                    var duration = (int)(DateTime.Now - _lastActivityTime).TotalSeconds;
                    LogApplicationSwitch(_currentApplicationName, _currentWindowTitle ?? string.Empty, 
                                       _currentProcessId, duration);
                }

                // Update current application
                _currentApplicationName = applicationName;
                _currentWindowTitle = windowTitle;
                _currentProcessId = (int)processId;
                _lastActivityTime = DateTime.Now;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔄 Switched to: {applicationName}");
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    Console.WriteLine($"               📝 Window: {windowTitle}");
                }
            }
            else if (_currentWindowTitle != windowTitle)
            {
                // Same app but different window/tab (e.g., different browser tab)
                _currentWindowTitle = windowTitle;
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📝 Window changed: {windowTitle}");
                }

                // Log window change
                _database.LogActivity(new ActivityLog
                {
                    Timestamp = DateTime.Now,
                    ApplicationName = applicationName,
                    WindowTitle = windowTitle,
                    ProcessName = applicationName,
                    ProcessId = (int)processId,
                    DurationSeconds = 0,
                    ActivityType = "WindowChange"
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error monitoring: {ex.Message}");
        }
    }

    private void MonitorProcesses()
    {
        // Fallback for non-Windows systems - monitor top CPU process
        var processes = Process.GetProcesses()
            .Where(p => !string.IsNullOrEmpty(p.ProcessName))
            .OrderByDescending(p =>
            {
                try { return p.TotalProcessorTime.TotalMilliseconds; }
                catch { return 0; }
            })
            .Take(1)
            .FirstOrDefault();

        if (processes != null)
        {
            string applicationName = processes.ProcessName;
            int processId = processes.Id;

            if (_currentApplicationName != applicationName)
            {
                if (_currentApplicationName != null)
                {
                    var duration = (int)(DateTime.Now - _lastActivityTime).TotalSeconds;
                    LogApplicationSwitch(_currentApplicationName, string.Empty, _currentProcessId, duration);
                }

                _currentApplicationName = applicationName;
                _currentProcessId = processId;
                _lastActivityTime = DateTime.Now;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔄 Active process: {applicationName}");
            }
        }
    }

    private void LogApplicationSwitch(string appName, string windowTitle, int processId, int duration)
    {
        var log = new ActivityLog
        {
            Timestamp = DateTime.Now,
            ApplicationName = appName,
            WindowTitle = windowTitle,
            ProcessName = appName,
            ProcessId = processId,
            DurationSeconds = duration,
            ActivityType = "Active"
        };

        _database.LogActivity(log);
        Console.WriteLine($"               ⏱️  Duration: {duration} seconds");
    }

    private void LogSystemActivity(object? state)
    {
        try
        {
            bool isIdle = CheckIdleTime();
            
            var activity = new SystemActivity
            {
                Timestamp = DateTime.Now,
                KeyboardEvents = _keyboardEventCount,
                MouseEvents = _mouseEventCount,
                IsIdle = isIdle
            };

            _database.LogSystemActivity(activity);

            if (isIdle)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 😴 System is idle");
            }

            // Reset counters
            _keyboardEventCount = 0;
            _mouseEventCount = 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error logging system activity: {ex.Message}");
        }
    }

    private bool CheckIdleTime()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // For non-Windows, check if no recent process activity
            return (DateTime.Now - _lastActivityTime).TotalMinutes > 5;
        }

        try
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

            if (GetLastInputInfo(ref lastInputInfo))
            {
                uint idleTime = (uint)Environment.TickCount - lastInputInfo.dwTime;
                double idleMinutes = idleTime / 60000.0;
                
                return idleMinutes > 5; // Idle if no input for 5 minutes
            }
        }
        catch { }

        return false;
    }

    public void Stop()
    {
        _monitorTimer?.Dispose();
        _systemActivityTimer?.Dispose();

        // Log final application usage
        if (_currentApplicationName != null)
        {
            var duration = (int)(DateTime.Now - _lastActivityTime).TotalSeconds;
            LogApplicationSwitch(_currentApplicationName, _currentWindowTitle ?? string.Empty, 
                               _currentProcessId, duration);
        }

        Console.WriteLine("\n🛑 Monitoring stopped!");
    }

    public void GenerateReport()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("📊 DAILY ACTIVITY REPORT");
        Console.WriteLine(new string('=', 80));

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var usageReport = _database.GetApplicationUsageReport(today, tomorrow);

        if (!usageReport.Any())
        {
            Console.WriteLine("\n⚠️  No activity recorded today.");
            return;
        }

        Console.WriteLine($"\n📅 Date: {today:yyyy-MM-dd}");
        Console.WriteLine($"⏰ Total Applications Tracked: {usageReport.Count}\n");

        Console.WriteLine("┌─────────────────────────────────────────┬──────────────┬────────────┐");
        Console.WriteLine("│ Application Name                        │ Time Used    │ Switches   │");
        Console.WriteLine("├─────────────────────────────────────────┼──────────────┼────────────┤");

        int totalSeconds = 0;
        foreach (var usage in usageReport)
        {
            totalSeconds += usage.TotalSeconds;
            var timeSpan = TimeSpan.FromSeconds(usage.TotalSeconds);
            var timeStr = $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            
            var appName = usage.ApplicationName.Length > 40 
                ? usage.ApplicationName.Substring(0, 37) + "..." 
                : usage.ApplicationName;

            Console.WriteLine($"│ {appName,-39} │ {timeStr,12} │ {usage.SwitchCount,10} │");
        }

        Console.WriteLine("└─────────────────────────────────────────┴──────────────┴────────────┘");

        var totalTimeSpan = TimeSpan.FromSeconds(totalSeconds);
        Console.WriteLine($"\n⏱️  Total Active Time: {(int)totalTimeSpan.TotalHours:D2}:{totalTimeSpan.Minutes:D2}:{totalTimeSpan.Seconds:D2}");

        Console.WriteLine("\n📋 Recent Activity Log (Last 10 entries):");
        Console.WriteLine(new string('-', 80));
        
        var recentActivities = _database.GetRecentActivities(10);
        foreach (var activity in recentActivities)
        {
            Console.WriteLine($"[{activity.Timestamp:HH:mm:ss}] {activity.ApplicationName}");
            if (!string.IsNullOrEmpty(activity.WindowTitle))
            {
                var title = activity.WindowTitle.Length > 60 
                    ? activity.WindowTitle.Substring(0, 57) + "..." 
                    : activity.WindowTitle;
                Console.WriteLine($"           └─ {title}");
            }
        }

        Console.WriteLine(new string('=', 80));
    }
}

