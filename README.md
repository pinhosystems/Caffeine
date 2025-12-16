<div align="center">

# ☕ Caffeine

### Keep your computer awake. Always.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Windows](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**No more interruptions. No more locked screens in the middle of a meeting.**

[Installation](#-installation) • [Usage](#-usage) • [Features](#-features) • [Settings](#%EF%B8%8F-settings)

</div>

---

## 😤 The Problem

You know this situation:

- You're in an **important meeting** and your screen locks *(classic)*
- Giving a **presentation** to 50 people and Windows decides it's nap time *(always at the best slide)*
- Watching an **online training** and doing bicep curls with your mouse every 5 minutes *(gains, at least)*
- Running a **long task** and the system suspends *(because Windows knows better, right?)*
- You're **surfing at the beach** and... wait, no, that one's actually fine 🏄

Oh, and let's not forget the **"just stepped away for coffee"** scenario where you return to find your laptop has entered witness protection program.

**Frustrating? Absolutely. Preventable? Now, yes.**

---

## 💡 The Solution

**Caffeine** is a minimalist Windows utility that keeps your computer awake intelligently and discreetly. Think of it as a virtual assistant that gently pokes your computer so it doesn't fall asleep on the job. *(Unlike some coworkers we know.)*

```
✅ Zero configuration needed - works right after install
✅ Runs silently in the system tray (more silent than your muted Zoom mic)
✅ Doesn't interfere with your work
✅ Minimal resource footprint (uses less RAM than a Chrome tab... so, basically nothing)
```

### How does it work?

Caffeine simulates user activity by periodically pressing the **F15** key - a "ghost key" that exists in the keyboard standard but isn't physically present on any keyboard. It's like having an invisible intern whose only job is to tap a key that doesn't exist. *Peak efficiency.*

This keeps Windows active without interfering with any application. No random characters appearing in your documents. No accidental shortcuts triggered. Just pure, silent wakefulness.

---

## ✨ Features

<table>
<tr>
<td width="50%">

### 🎯 Simple & Efficient
- One click to activate/pause
- System tray icon
- Visual status indicators

</td>
<td width="50%">

### ⏰ Smart Scheduling
- Configure days of the week
- Set start and end times
- Support for overnight schedules (10 PM to 6 AM)

</td>
</tr>
<tr>
<td width="50%">

### 🖥️ Display Control
- Keep monitor on
- Prevent screen lock
- Configurable intervals (30s, 60s, 120s)

</td>
<td width="50%">

### 🚀 Auto-Start
- Start with Windows
- Remember your settings
- Ready to use

</td>
</tr>
</table>

---

## 📥 Installation

### Method 1: PowerShell Script (Recommended)

```powershell
# Clone the repository
git clone https://github.com/pinhosystems/Caffeine.git
cd Caffeine

# Run the installer
.\install.ps1
```

The script will:
- Build the project
- Install to `%LOCALAPPDATA%\Caffeine`
- Create a desktop shortcut

### Method 2: Manual Build

```powershell
dotnet publish -c Release -r win-x64 --self-contained
```

### Uninstall

```powershell
.\install.ps1 -Uninstall
```

---

## 🚀 Usage

1. **Launch Caffeine** - Click the desktop icon or find it in the system tray *(it's the coffee cup, obviously)*

2. **Check status** by the icon:
   - ✅ **Green** = Active (your computer is now a certified insomniac)
   - ⏸️ **Paused** = Taking a break (even Caffeine needs a breather sometimes)
   - 🕐 **Clock** = Outside scheduled hours (Caffeine respects work-life balance, even if you don't)

3. **Right-click** the icon to:
   - Pause/Resume
   - Open Settings
   - Exit application *(but why would you?)*

**That's it!** Your computer won't sleep while Caffeine is active. Now go get that actual coffee. ☕

---

## ⚙️ Settings

Access settings by right-clicking the tray icon → **Settings**

| Option | Description |
|--------|-------------|
| **Activity interval** | How often to simulate activity (30s, 60s, 120s) |
| **Keep display on** | Prevents the monitor from turning off |
| **Start with Windows** | Launches automatically on login |
| **Use schedule** | Only active during specific times |
| **Active days** | Select days of the week |
| **Start/End time** | Define the activation period |

---

## 🛠️ Tech Stack

- **C# / .NET 8.0** - Performance and modernity
- **Windows Forms** - Native and lightweight UI
- **Win32 API** - Deep system integration

---

## 🤝 Contributing

Contributions are welcome!

1. Fork the project
2. Create your branch (`git checkout -b feature/NewFeature`)
3. Commit your changes (`git commit -m 'Add new feature'`)
4. Push to the branch (`git push origin feature/NewFeature`)
5. Open a Pull Request

---

## 📄 License

This project is under the MIT license. See the [LICENSE](LICENSE) file for more details.

---

<div align="center">

**Made with ☕ (and actual caffeine) by [Pinho Systems](https://github.com/pinhosystems)**

*Stay awake. Stay productive. Stay caffeinated.*

*No computers were harmed in the making of this software. They were just... never allowed to sleep.* 😴🚫

</div>
