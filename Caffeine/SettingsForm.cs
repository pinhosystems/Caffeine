namespace Caffeine;

/// <summary>
/// Settings form for configuring schedule and preferences.
/// </summary>
public class SettingsForm : Form
{
    private readonly AppSettings _settings;

    // Schedule controls
    private CheckBox _scheduleEnabledCheckbox = null!;
    private DateTimePicker _startTimePicker = null!;
    private DateTimePicker _endTimePicker = null!;
    private CheckBox[] _dayCheckboxes = null!;

    // Options
    private ComboBox _intervalComboBox = null!;
    private CheckBox _startupCheckbox = null!;

    // Buttons
    private Button _saveButton = null!;
    private Button _cancelButton = null!;

    public SettingsForm(AppSettings settings)
    {
        _settings = settings;
        SetupUI();
        LoadSettings();
    }

    private void SetupUI()
    {
        Text = "Caffeine Settings";
        Size = new Size(400, 450);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        var yPos = 20;

        // Schedule enabled checkbox
        _scheduleEnabledCheckbox = new CheckBox
        {
            Text = "Enable schedule (only keep awake during specified hours)",
            Location = new Point(20, yPos),
            AutoSize = true
        };
        _scheduleEnabledCheckbox.CheckedChanged += ScheduleEnabled_CheckedChanged;
        Controls.Add(_scheduleEnabledCheckbox);

        yPos += 35;

        // Time range group
        var timeGroupBox = new GroupBox
        {
            Text = "Active Hours",
            Location = new Point(20, yPos),
            Size = new Size(340, 80)
        };
        Controls.Add(timeGroupBox);

        // Start time
        var startLabel = new Label
        {
            Text = "Start:",
            Location = new Point(15, 30),
            AutoSize = true
        };
        timeGroupBox.Controls.Add(startLabel);

        _startTimePicker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Time,
            ShowUpDown = true,
            Location = new Point(60, 27),
            Size = new Size(100, 25)
        };
        timeGroupBox.Controls.Add(_startTimePicker);

        // End time
        var endLabel = new Label
        {
            Text = "End:",
            Location = new Point(180, 30),
            AutoSize = true
        };
        timeGroupBox.Controls.Add(endLabel);

        _endTimePicker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Time,
            ShowUpDown = true,
            Location = new Point(220, 27),
            Size = new Size(100, 25)
        };
        timeGroupBox.Controls.Add(_endTimePicker);

        yPos += 95;

        // Days group
        var daysGroupBox = new GroupBox
        {
            Text = "Active Days",
            Location = new Point(20, yPos),
            Size = new Size(340, 90)
        };
        Controls.Add(daysGroupBox);

        var days = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        var dayValues = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };

        _dayCheckboxes = new CheckBox[7];
        for (int i = 0; i < 7; i++)
        {
            _dayCheckboxes[i] = new CheckBox
            {
                Text = days[i],
                Tag = dayValues[i],
                Location = new Point(15 + (i % 4) * 80, 25 + (i / 4) * 30),
                AutoSize = true
            };
            daysGroupBox.Controls.Add(_dayCheckboxes[i]);
        }

        yPos += 105;

        // Options group
        var optionsGroupBox = new GroupBox
        {
            Text = "Options",
            Location = new Point(20, yPos),
            Size = new Size(340, 100)
        };
        Controls.Add(optionsGroupBox);

        var intervalLabel = new Label
        {
            Text = "Ping interval:",
            Location = new Point(15, 30),
            AutoSize = true
        };
        optionsGroupBox.Controls.Add(intervalLabel);

        _intervalComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(100, 27),
            Size = new Size(120, 25)
        };
        _intervalComboBox.Items.AddRange(new object[] { "30 seconds", "60 seconds", "120 seconds" });
        optionsGroupBox.Controls.Add(_intervalComboBox);

        _startupCheckbox = new CheckBox
        {
            Text = "Start with Windows",
            Location = new Point(15, 60),
            AutoSize = true
        };
        optionsGroupBox.Controls.Add(_startupCheckbox);

        yPos += 115;

        // Buttons
        _saveButton = new Button
        {
            Text = "Save",
            Location = new Point(180, yPos),
            Size = new Size(80, 30),
            DialogResult = DialogResult.OK
        };
        _saveButton.Click += SaveButton_Click;
        Controls.Add(_saveButton);

        _cancelButton = new Button
        {
            Text = "Cancel",
            Location = new Point(270, yPos),
            Size = new Size(80, 30),
            DialogResult = DialogResult.Cancel
        };
        Controls.Add(_cancelButton);

        AcceptButton = _saveButton;
        CancelButton = _cancelButton;
    }

    private void LoadSettings()
    {
        _scheduleEnabledCheckbox.Checked = _settings.ScheduleEnabled;

        // Set time pickers
        _startTimePicker.Value = DateTime.Today.Add(_settings.StartTime.ToTimeSpan());
        _endTimePicker.Value = DateTime.Today.Add(_settings.EndTime.ToTimeSpan());

        // Set day checkboxes
        foreach (var checkbox in _dayCheckboxes)
        {
            if (checkbox.Tag is DayOfWeek day)
            {
                checkbox.Checked = _settings.ActiveDays.Contains(day);
            }
        }

        // Set interval
        _intervalComboBox.SelectedIndex = _settings.PingIntervalSeconds switch
        {
            30 => 0,
            60 => 1,
            120 => 2,
            _ => 0
        };

        // Set startup checkbox
        _startupCheckbox.Checked = CaffeineService.IsStartupEnabled();

        UpdateScheduleControlsEnabled();
    }

    private void ScheduleEnabled_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateScheduleControlsEnabled();
    }

    private void UpdateScheduleControlsEnabled()
    {
        var enabled = _scheduleEnabledCheckbox.Checked;
        _startTimePicker.Enabled = enabled;
        _endTimePicker.Enabled = enabled;
        foreach (var checkbox in _dayCheckboxes)
        {
            checkbox.Enabled = enabled;
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        _settings.ScheduleEnabled = _scheduleEnabledCheckbox.Checked;
        _settings.StartTime = TimeOnly.FromDateTime(_startTimePicker.Value);
        _settings.EndTime = TimeOnly.FromDateTime(_endTimePicker.Value);

        _settings.ActiveDays.Clear();
        foreach (var checkbox in _dayCheckboxes)
        {
            if (checkbox is { Checked: true, Tag: DayOfWeek day })
            {
                _settings.ActiveDays.Add(day);
            }
        }

        _settings.PingIntervalSeconds = _intervalComboBox.SelectedIndex switch
        {
            0 => 30,
            1 => 60,
            2 => 120,
            _ => 30
        };

        // Save startup setting
        if (_startupCheckbox.Checked != CaffeineService.IsStartupEnabled())
        {
            CaffeineService.SetStartupEnabled(_startupCheckbox.Checked);
            _settings.StartWithWindows = _startupCheckbox.Checked;
        }

        _settings.Save();
    }
}
