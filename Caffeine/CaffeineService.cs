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

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    // Input type constants
    private const uint INPUT_MOUSE = 0;
    private const uint MOUSEEVENTF_MOVE = 0x0001;

    // Execution state flags
    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;

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

        // Invisible mouse move (0 pixels) - doesn't interfere with any application
        SendInvisibleMouseMove();

        // Reinforce execution state
        uint flags = ES_SYSTEM_REQUIRED;
        if (KeepDisplayOn)
            flags |= ES_DISPLAY_REQUIRED;
        SetThreadExecutionState(flags);

        OnPing?.Invoke(_pingCount);
    }

    private void SendInvisibleMouseMove()
    {
        var input = new INPUT
        {
            type = INPUT_MOUSE,
            u = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_MOVE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }
}
