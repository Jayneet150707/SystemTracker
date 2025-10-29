namespace EmployeeActivityMonitor.Models;

public class ActivityLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string WindowTitle { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int ProcessId { get; set; }
    public int DurationSeconds { get; set; }
    public string ActivityType { get; set; } = string.Empty; // Active, Idle, Switched
}

public class ApplicationUsage
{
    public string ApplicationName { get; set; } = string.Empty;
    public int TotalSeconds { get; set; }
    public int SwitchCount { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
}

public class SystemActivity
{
    public DateTime Timestamp { get; set; }
    public int KeyboardEvents { get; set; }
    public int MouseEvents { get; set; }
    public bool IsIdle { get; set; }
}

