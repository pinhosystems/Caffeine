using System.Text.Json;

namespace Caffeine;

/// <summary>
/// Application settings with schedule configuration.
/// </summary>
public class AppSettings
{
    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Caffeine"
    );

    private static readonly string SettingsFile = Path.Combine(SettingsFolder, "settings.json");

    /// <summary>
    /// Whether schedule-based activation is enabled.
    /// </summary>
    public bool ScheduleEnabled { get; set; } = false;

    /// <summary>
    /// Start time for scheduled activation (hour:minute).
    /// </summary>
    public TimeOnly StartTime { get; set; } = new TimeOnly(9, 0);

    /// <summary>
    /// End time for scheduled activation (hour:minute).
    /// </summary>
    public TimeOnly EndTime { get; set; } = new TimeOnly(18, 0);

    /// <summary>
    /// Days of the week when the schedule is active.
    /// </summary>
    public List<DayOfWeek> ActiveDays { get; set; } = new()
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    };

    /// <summary>
    /// Whether to start with Windows.
    /// </summary>
    public bool StartWithWindows { get; set; } = true;

    /// <summary>
    /// Ping interval in seconds (30, 60, or 120).
    /// </summary>
    public int PingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to keep the display on (prevents monitor from turning off).
    /// </summary>
    public bool KeepDisplayOn { get; set; } = true;

    /// <summary>
    /// Checks if the current time is within the scheduled active window.
    /// </summary>
    public bool IsWithinSchedule()
    {
        if (!ScheduleEnabled) return true;

        var now = DateTime.Now;
        var currentTime = TimeOnly.FromDateTime(now);
        var currentDay = now.DayOfWeek;

        // Check if today is an active day
        if (!ActiveDays.Contains(currentDay)) return false;

        // Check if current time is within the window
        if (StartTime <= EndTime)
        {
            // Normal case: e.g., 9:00 to 18:00
            return currentTime >= StartTime && currentTime <= EndTime;
        }
        else
        {
            // Overnight case: e.g., 22:00 to 06:00
            return currentTime >= StartTime || currentTime <= EndTime;
        }
    }

    /// <summary>
    /// Loads settings from the JSON file, or returns defaults if not found.
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch
        {
            // Return defaults on error
        }

        return new AppSettings();
    }

    /// <summary>
    /// Saves settings to the JSON file.
    /// </summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsFolder);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(SettingsFile, json);
        }
        catch
        {
            // Silently fail - not critical
        }
    }
}
