using EmployeeActivityMonitor.Services;

namespace EmployeeActivityMonitor;

class Program
{
    private static ActivityMonitorService? _monitorService;
    private static DatabaseService? _databaseService;
    private static bool _isRunning = true;

    static void Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     EMPLOYEE ACTIVITY MONITOR - .NET Console App          ║");
        Console.WriteLine("║     Track Applications, Window Titles & System Activity   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Setup database
        _databaseService = new DatabaseService("activity_monitor.db");
        Console.WriteLine("✅ Database initialized: activity_monitor.db");
        Console.WriteLine();

        // Setup monitoring service
        _monitorService = new ActivityMonitorService(_databaseService);

        // Handle Ctrl+C for graceful shutdown
        Console.CancelKeyPress += OnCancelKeyPress;

        // Show menu
        ShowMenu();

        while (_isRunning)
        {
            Thread.Sleep(100);
        }

        Cleanup();
    }

    private static void ShowMenu()
    {
        Console.WriteLine("Choose an option:");
        Console.WriteLine("  1. Start Monitoring (Real-time tracking)");
        Console.WriteLine("  2. View Today's Report");
        Console.WriteLine("  3. Start Monitoring + Auto Report on Exit");
        Console.WriteLine("  4. Exit");
        Console.Write("\nYour choice: ");

        var choice = Console.ReadLine();
        Console.WriteLine();

        switch (choice)
        {
            case "1":
                StartMonitoring(false);
                break;
            case "2":
                ViewReport();
                _isRunning = false;
                break;
            case "3":
                StartMonitoring(true);
                break;
            case "4":
                _isRunning = false;
                break;
            default:
                Console.WriteLine("⚠️  Invalid choice. Please try again.\n");
                ShowMenu();
                break;
        }
    }

    private static void StartMonitoring(bool showReportOnExit)
    {
        if (_monitorService == null)
            return;

        _monitorService.Start();

        // Keep running until Ctrl+C
        while (_isRunning)
        {
            Thread.Sleep(1000);
        }

        _monitorService.Stop();

        if (showReportOnExit)
        {
            _monitorService.GenerateReport();
        }
    }

    private static void ViewReport()
    {
        if (_monitorService == null)
            return;

        _monitorService.GenerateReport();
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        _isRunning = false;
        
        Console.WriteLine("\n\n🛑 Stopping monitor...");
    }

    private static void Cleanup()
    {
        _monitorService?.Stop();
        _databaseService?.Dispose();
        
        Console.WriteLine("\n👋 Thank you for using Employee Activity Monitor!");
        Console.WriteLine("📊 All data saved to: activity_monitor.db\n");
    }
}

