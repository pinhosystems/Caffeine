using System.Reflection;

namespace Caffeine;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApp());
    }
}

/// <summary>
/// System tray application that manages the Caffeine service.
/// </summary>
class TrayApp : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly CaffeineService _service;
    private readonly AppSettings _settings;
    private readonly System.Windows.Forms.Timer _scheduleTimer;

    // Menu items that need updating
    private readonly ToolStripMenuItem _statusItem;
    private readonly ToolStripMenuItem _pauseItem;
    private readonly ToolStripMenuItem _displayToggle;
    private readonly ToolStripMenuItem _intervalMenu;
    private readonly ToolStripMenuItem[] _intervalItems;

    private bool _isPaused = false;
    private bool _isOutsideSchedule = false;

    public TrayApp()
    {
        _settings = AppSettings.Load();
        _service = new CaffeineService
        {
            IntervalSeconds = _settings.PingIntervalSeconds,
            KeepDisplayOn = _settings.KeepDisplayOn
        };

        // Apply startup setting on first run
        if (_settings.StartWithWindows && !CaffeineService.IsStartupEnabled())
        {
            CaffeineService.SetStartupEnabled(true);
        }

        // Create menu items
        _statusItem = new ToolStripMenuItem("Active") { Enabled = false };
        _pauseItem = new ToolStripMenuItem("Pause", null, TogglePause);

        // Display toggle
        _displayToggle = new ToolStripMenuItem("Keep display on")
        {
            Checked = _settings.KeepDisplayOn,
            CheckOnClick = true
        };
        _displayToggle.Click += ToggleDisplay;

        // Interval submenu
        _intervalMenu = new ToolStripMenuItem("Interval");
        var intervals = new[] { 30, 60, 120 };
        _intervalItems = intervals.Select(sec =>
        {
            var item = new ToolStripMenuItem($"{sec} seconds");
            item.Click += (s, e) => SetInterval(sec);
            return item;
        }).ToArray();
        foreach (var item in _intervalItems)
            _intervalMenu.DropDownItems.Add(item);
        UpdateIntervalChecks();

        // Build context menu
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(_statusItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(_displayToggle);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(_intervalMenu);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(new ToolStripMenuItem("Settings...", null, OpenSettings));
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(_pauseItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(new ToolStripMenuItem("Exit", null, Exit));

        // Create tray icon
        _trayIcon = new NotifyIcon
        {
            Icon = _activeIcon,
            Text = "Caffeine - Keeping you online",
            ContextMenuStrip = contextMenu,
            Visible = true
        };
        // Service event
        _service.OnPing += OnServicePing;

        // Schedule check timer (every minute)
        _scheduleTimer = new System.Windows.Forms.Timer { Interval = 60000 };
        _scheduleTimer.Tick += CheckSchedule;
        _scheduleTimer.Start();

        // Initial schedule check and start
        InitializeState();

        // Show balloon based on actual state
        if (_isOutsideSchedule)
            ShowBalloon("Outside Schedule", "Waiting for scheduled hours.");
        else
            ShowBalloon("Caffeine Active", "Running in the system tray.");
    }

    private void InitializeState()
    {
        _isOutsideSchedule = _settings.ScheduleEnabled && !_settings.IsWithinSchedule();
        UpdateServiceState();
    }

    private void OnServicePing(int count)
    {
        UpdateStatus($"Active (ping #{count})");
    }

    private void TogglePause(object? sender, EventArgs e)
    {
        _isPaused = !_isPaused;
        UpdateServiceState();
    }

    private void ToggleDisplay(object? sender, EventArgs e)
    {
        _settings.KeepDisplayOn = _displayToggle.Checked;
        _service.KeepDisplayOn = _displayToggle.Checked;
        _settings.Save();
    }

    private void SetInterval(int seconds)
    {
        _settings.PingIntervalSeconds = seconds;
        _service.IntervalSeconds = seconds;
        _settings.Save();
        UpdateIntervalChecks();
    }

    private void UpdateIntervalChecks()
    {
        var intervals = new[] { 30, 60, 120 };
        for (int i = 0; i < _intervalItems.Length; i++)
            _intervalItems[i].Checked = intervals[i] == _settings.PingIntervalSeconds;
    }

    private void OpenSettings(object? sender, EventArgs e)
    {
        using var form = new SettingsForm(_settings);
        if (form.ShowDialog() == DialogResult.OK)
        {
            // Apply new settings
            _service.IntervalSeconds = _settings.PingIntervalSeconds;
            _service.KeepDisplayOn = _settings.KeepDisplayOn;

            // Sync menu items
            _displayToggle.Checked = _settings.KeepDisplayOn;
            UpdateIntervalChecks();

            // Recheck schedule
            CheckScheduleAndUpdateState();
        }
    }

    private void CheckSchedule(object? sender, EventArgs e)
    {
        CheckScheduleAndUpdateState();
    }

    private void CheckScheduleAndUpdateState()
    {
        var wasOutside = _isOutsideSchedule;
        _isOutsideSchedule = _settings.ScheduleEnabled && !_settings.IsWithinSchedule();

        if (wasOutside == _isOutsideSchedule) return;

        UpdateServiceState();

        if (_isOutsideSchedule)
        {
            ShowBalloon("Outside Schedule", "Caffeine paused - outside scheduled hours");
        }
        else if (!_isPaused)
        {
            ShowBalloon("Schedule Active", "Caffeine resumed - within scheduled hours");
        }
    }

    private void UpdateServiceState()
    {
        var shouldRun = !_isPaused && !_isOutsideSchedule;

        if (shouldRun && !_service.IsRunning)
            _service.Start();
        else if (!shouldRun && _service.IsRunning)
            _service.Stop();

        // UI - prioridade: pausado > scheduled > active
        if (_isPaused)
        {
            _trayIcon.Icon = _pausedIcon;
            UpdateStatus(_isOutsideSchedule ? "Paused (outside schedule)" : "Paused");
            _pauseItem.Text = "Resume";
        }
        else if (_isOutsideSchedule)
        {
            _trayIcon.Icon = _scheduledIcon;
            UpdateStatus("Outside schedule");
            _pauseItem.Text = "Pause";
        }
        else
        {
            _trayIcon.Icon = _activeIcon;
            UpdateStatus("Active");
            _pauseItem.Text = "Pause";
        }
    }

    private void UpdateStatus(string status)
    {
        if (_statusItem.GetCurrentParent()?.InvokeRequired == true)
            _statusItem.GetCurrentParent()?.BeginInvoke(() => _statusItem.Text = status);
        else
            _statusItem.Text = status;
    }

    private void ShowBalloon(string title, string text)
    {
        _trayIcon.BalloonTipTitle = title;
        _trayIcon.BalloonTipText = text;
        _trayIcon.ShowBalloonTip(2000);
    }

    // Ícones carregados dos recursos embutidos
    private static readonly Icon _activeIcon = LoadIcon("active");
    private static readonly Icon _pausedIcon = LoadIcon("paused");
    private static readonly Icon _scheduledIcon = LoadIcon("scheduled");

    private static Icon LoadIcon(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream($"Caffeine.images.{name}.ico");
        return new Icon(stream!);
    }

    private void Exit(object? sender, EventArgs e)
    {
        _scheduleTimer.Stop();
        _service.Stop();
        _trayIcon.Visible = false;
        Application.Exit();
    }
}
