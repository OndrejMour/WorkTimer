using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WorkTimer.App.UI;

/// <summary>
/// Helper class for enabling dark mode title bar in Windows 10/11
/// </summary>
public static class DarkModeHelper
{
[DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    /// <summary>
    /// Enables or disables dark mode for the window title bar
    /// </summary>
    /// <param name="form">The form to apply dark mode to</param>
    /// <param name="enabled">True to enable dark mode, false to disable</param>
    /// <returns>True if successful, false otherwise</returns>
public static bool SetDarkModeTitleBar(Form form, bool enabled)
    {
        if (form?.Handle == null || form.Handle == IntPtr.Zero)
            return false;

     try
   {
            // Windows 10 version check
          if (Environment.OSVersion.Version.Major >= 10)
   {
 int useImmersiveDarkMode = enabled ? 1 : 0;

      // Try Windows 11 first (build 22000+)
   if (Environment.OSVersion.Version.Build >= 22000)
 {
              if (DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int)) == 0)
          return true;
       }

    // Fallback to Windows 10 20H1+ (build 18985+)
  if (Environment.OSVersion.Version.Build >= 18985)
        {
  if (DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int)) == 0)
       return true;

   // Fallback to older attribute
               if (DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useImmersiveDarkMode, sizeof(int)) == 0)
            return true;
          }
  }
        }
        catch (Exception)
        {
            // Silently fail on unsupported systems
        }

return false;
    }

    /// <summary>
 /// Checks if dark mode title bar is supported on this system
    /// </summary>
    public static bool IsSupported()
    {
        // Windows 10 build 18985 and later
        return Environment.OSVersion.Version.Major >= 10 && 
   Environment.OSVersion.Version.Build >= 18985;
    }
}
