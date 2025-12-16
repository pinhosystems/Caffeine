using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Caffeine;

/// <summary>
/// Service that keeps the system awake by simulating activity and preventing sleep.
/// </summary>
public class CaffeineService
{
    private CancellationTokenSource? _cts;
    private Task? _task;
    private int _pingCount = 0;

    /// <summary>
    /// Interval between activity simulations in seconds.
    /// </summary>
    public int IntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to keep the display on (prevents monitor from turning off).
    /// </summary>
    public bool KeepDisplayOn { get; set; } = true;

    /// <summary>
    /// Event fired after each ping/activity simulation.
    /// </summary>
    public event Action<int>? OnPing;

    #region Win32 APIs

    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    // Execution state flags
    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;

    // F15 virtual key code (ghost key - doesn't interfere with anything)
    private const byte VK_F15 = 0x7E;

    // Key event flags
    private const uint KEYEVENTF_KEYUP = 0x0002;

    #endregion

    #region Registry Auto-Start

    private const string RegistryRunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "Caffeine";

    /// <summary>
    /// Checks if the application is configured to start with Windows.
    /// </summary>
    public static bool IsStartupEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryRunKey, false);
        return key?.GetValue(AppName) != null;
    }

    /// <summary>
    /// Enables or disables auto-start with Windows.
    /// </summary>
    public static void SetStartupEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryRunKey, true);
        if (key == null) return;

        if (enabled)
        {
            var exePath = Environment.ProcessPath ?? Application.ExecutablePath;
            key.SetValue(AppName, $"\"{exePath}\"");
        }
        else
        {
            key.DeleteValue(AppName, false);
        }
    }

    #endregion

    /// <summary>
    /// Starts the keep-awake service.
    /// </summary>
    public void Start()
    {
        if (_cts != null) return; // Already running

        // Set continuous execution state to prevent sleep
        uint flags = ES_CONTINUOUS | ES_SYSTEM_REQUIRED;
        if (KeepDisplayOn)
            flags |= ES_DISPLAY_REQUIRED;
        SetThreadExecutionState(flags);

        _cts = new CancellationTokenSource();
        _task = RunLoopAsync(_cts.Token);
    }

    /// <summary>
    /// Stops the keep-awake service.
    /// </summary>
    public void Stop()
    {
        var cts = _cts;
        if (cts == null) return;

        cts.Cancel();
        try
        {
            _task?.Wait(1000);
        }
        catch (AggregateException) { /* Expected on cancellation */ }
        catch (OperationCanceledException) { /* Expected */ }

        cts.Dispose();
        _cts = null;

        // Restore normal execution state
        SetThreadExecutionState(ES_CONTINUOUS);
    }

    /// <summary>
    /// Checks if the service is currently running.
    /// </summary>
    public bool IsRunning
    {
        get
        {
            var cts = _cts;
            return cts != null && !cts.IsCancellationRequested;
        }
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(IntervalSeconds * 1000, ct);
                SimulateActivity();
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private void SimulateActivity()
    {
        _pingCount++;

        // Press F15 key (ghost key - doesn't affect anything)
        keybd_event(VK_F15, 0, 0, UIntPtr.Zero);           // Key down
        keybd_event(VK_F15, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Key up

        // Reinforce execution state
        uint flags = ES_SYSTEM_REQUIRED;
        if (KeepDisplayOn)
            flags |= ES_DISPLAY_REQUIRED;
        SetThreadExecutionState(flags);

        OnPing?.Invoke(_pingCount);
    }
}
