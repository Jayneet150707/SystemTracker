using Microsoft.Data.Sqlite;
using EmployeeActivityMonitor.Models;

namespace EmployeeActivityMonitor.Services;

public class DatabaseService : IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;

    public DatabaseService(string databasePath = "activity_monitor.db")
    {
        _connectionString = $"Data Source={databasePath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();

        var createTableCommand = @"
            CREATE TABLE IF NOT EXISTS ActivityLogs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                ApplicationName TEXT NOT NULL,
                WindowTitle TEXT,
                ProcessName TEXT NOT NULL,
                ProcessId INTEGER,
                DurationSeconds INTEGER DEFAULT 0,
                ActivityType TEXT
            );

            CREATE INDEX IF NOT EXISTS idx_timestamp ON ActivityLogs(Timestamp);
            CREATE INDEX IF NOT EXISTS idx_application ON ActivityLogs(ApplicationName);
            CREATE INDEX IF NOT EXISTS idx_process ON ActivityLogs(ProcessName);

            CREATE TABLE IF NOT EXISTS SystemActivity (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                KeyboardEvents INTEGER DEFAULT 0,
                MouseEvents INTEGER DEFAULT 0,
                IsIdle INTEGER DEFAULT 0
            );
        ";

        using var command = new SqliteCommand(createTableCommand, _connection);
        command.ExecuteNonQuery();
    }

    public void LogActivity(ActivityLog log)
    {
        var insertCommand = @"
            INSERT INTO ActivityLogs (Timestamp, ApplicationName, WindowTitle, ProcessName, ProcessId, DurationSeconds, ActivityType)
            VALUES (@Timestamp, @ApplicationName, @WindowTitle, @ProcessName, @ProcessId, @DurationSeconds, @ActivityType)
        ";

        using var command = new SqliteCommand(insertCommand, _connection);
        command.Parameters.AddWithValue("@Timestamp", log.Timestamp.ToString("o"));
        command.Parameters.AddWithValue("@ApplicationName", log.ApplicationName);
        command.Parameters.AddWithValue("@WindowTitle", log.WindowTitle ?? string.Empty);
        command.Parameters.AddWithValue("@ProcessName", log.ProcessName);
        command.Parameters.AddWithValue("@ProcessId", log.ProcessId);
        command.Parameters.AddWithValue("@DurationSeconds", log.DurationSeconds);
        command.Parameters.AddWithValue("@ActivityType", log.ActivityType);
        command.ExecuteNonQuery();
    }

    public void LogSystemActivity(SystemActivity activity)
    {
        var insertCommand = @"
            INSERT INTO SystemActivity (Timestamp, KeyboardEvents, MouseEvents, IsIdle)
            VALUES (@Timestamp, @KeyboardEvents, @MouseEvents, @IsIdle)
        ";

        using var command = new SqliteCommand(insertCommand, _connection);
        command.Parameters.AddWithValue("@Timestamp", activity.Timestamp.ToString("o"));
        command.Parameters.AddWithValue("@KeyboardEvents", activity.KeyboardEvents);
        command.Parameters.AddWithValue("@MouseEvents", activity.MouseEvents);
        command.Parameters.AddWithValue("@IsIdle", activity.IsIdle ? 1 : 0);
        command.ExecuteNonQuery();
    }

    public List<ApplicationUsage> GetApplicationUsageReport(DateTime startDate, DateTime endDate)
    {
        var query = @"
            SELECT 
                ApplicationName,
                SUM(DurationSeconds) as TotalSeconds,
                COUNT(DISTINCT ProcessId) as SwitchCount,
                MIN(Timestamp) as FirstSeen,
                MAX(Timestamp) as LastSeen
            FROM ActivityLogs
            WHERE Timestamp BETWEEN @StartDate AND @EndDate
            GROUP BY ApplicationName
            ORDER BY TotalSeconds DESC
        ";

        var usageList = new List<ApplicationUsage>();
        using var command = new SqliteCommand(query, _connection);
        command.Parameters.AddWithValue("@StartDate", startDate.ToString("o"));
        command.Parameters.AddWithValue("@EndDate", endDate.ToString("o"));

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            usageList.Add(new ApplicationUsage
            {
                ApplicationName = reader.GetString(0),
                TotalSeconds = reader.GetInt32(1),
                SwitchCount = reader.GetInt32(2),
                FirstSeen = DateTime.Parse(reader.GetString(3)),
                LastSeen = DateTime.Parse(reader.GetString(4))
            });
        }

        return usageList;
    }

    public List<ActivityLog> GetRecentActivities(int count = 50)
    {
        var query = @"
            SELECT Id, Timestamp, ApplicationName, WindowTitle, ProcessName, ProcessId, DurationSeconds, ActivityType
            FROM ActivityLogs
            ORDER BY Timestamp DESC
            LIMIT @Count
        ";

        var activities = new List<ActivityLog>();
        using var command = new SqliteCommand(query, _connection);
        command.Parameters.AddWithValue("@Count", count);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            activities.Add(new ActivityLog
            {
                Id = reader.GetInt32(0),
                Timestamp = DateTime.Parse(reader.GetString(1)),
                ApplicationName = reader.GetString(2),
                WindowTitle = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                ProcessName = reader.GetString(4),
                ProcessId = reader.GetInt32(5),
                DurationSeconds = reader.GetInt32(6),
                ActivityType = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
            });
        }

        return activities;
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}

