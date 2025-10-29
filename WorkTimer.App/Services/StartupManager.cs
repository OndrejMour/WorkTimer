using System;
using Microsoft.Win32;

namespace WorkTimer.App.Services;

public static class StartupManager
{
    private const string AppName = "WorkTimer";
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// Enables or disables the application to start with Windows.
    /// </summary>
    /// <param name="enable">True to enable startup, false to disable.</param>
    public static void SetStartWithWindows(bool enable)
    {
  try
        {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true);
 if (key == null) return;

   if (enable)
            {
      var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
       if (!string.IsNullOrEmpty(exePath))
            {
    key.SetValue(AppName, $"\"{exePath}\"");
  }
       }
    else
      {
                key.DeleteValue(AppName, throwOnMissingValue: false);
            }
        }
 catch (Exception)
        {
  // Silently fail if registry access is denied
        }
    }

    /// <summary>
    /// Checks if the application is set to start with Windows.
  /// </summary>
    /// <returns>True if enabled, false otherwise.</returns>
    public static bool IsStartWithWindowsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: false);
          var value = key?.GetValue(AppName) as string;
 return !string.IsNullOrEmpty(value);
        }
 catch (Exception)
      {
  return false;
        }
    }
}
