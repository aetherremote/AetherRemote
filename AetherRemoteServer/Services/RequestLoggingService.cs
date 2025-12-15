using System.Timers;
using AetherRemoteServer.Domain.Interfaces;
using Timer = System.Timers.Timer;

namespace AetherRemoteServer.Services;

// ReSharper disable RedundantBoolCompare

public class RequestLoggingService : IDisposable, IRequestLoggingService
{
    private const string LogDirectory = "Logs";
    private static readonly string LogsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    
    private readonly List<string> _pendingLogs = [];
    private readonly Timer _flushTimer;

    public RequestLoggingService()
    {
        if (Directory.Exists(LogDirectory) is false)
            Directory.CreateDirectory(LogDirectory);
        
        _flushTimer = new Timer(TimeSpan.FromHours(4));
        _flushTimer.AutoReset = true;
        _flushTimer.Enabled = true;
        _flushTimer.Elapsed += WriteToLogFileAsync;
    }

    public void Log(string message)
    {
        _pendingLogs.Add(message);
    }

    // Non-blocking
    private async void WriteToLogFileAsync(object? sender, ElapsedEventArgs _)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var name = timestamp + ".txt";
            var path = Path.Combine(LogsPath, name);
            
            await File.AppendAllLinesAsync(path, _pendingLogs).ConfigureAwait(false);
            
            _pendingLogs.Clear();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    // Blocking
    private void WriteToLogFile()
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var name = timestamp + ".txt";
            var path = Path.Combine(LogsPath, name);

            File.AppendAllLines(path, _pendingLogs);
            
            _pendingLogs.Clear();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Dispose()
    {
        _flushTimer.Elapsed -= WriteToLogFileAsync;
        _flushTimer.Dispose();

        WriteToLogFile();
        
        GC.SuppressFinalize(this);
    }
}