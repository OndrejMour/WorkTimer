using System;

namespace WorkTimer.App.Models;

public enum AppLanguage
{
 Cs,
 En
}

public class AppSettings
{
 public TimeSpan TargetShift { get; set; } = TimeSpan.FromHours(8.5);
 public bool NotifyHalf { get; set; } = true;
 public bool NotifyEnd { get; set; } = true;
 public bool NotifyEscapeWindow { get; set; } = false;
 public AppLanguage Language { get; set; } = AppLanguage.Cs;
 public bool MinimizeToTray { get; set; } = false;
 public bool MinimizeToTrayOnClose { get; set; } = false;
 public bool StartWithWindows { get; set; } = false;
}
