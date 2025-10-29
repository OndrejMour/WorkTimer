using System;
using System.Windows.Forms;

namespace WorkTimer.App.Tray;

public class TrayManager : IDisposable
{
 public NotifyIcon NotifyIcon { get; } = new NotifyIcon();
 public event EventHandler? ShowRequested;
 public event EventHandler? TogglePauseRequested;
 public event EventHandler? SettingsRequested;

 public TrayManager()
 {
 try { NotifyIcon.Icon = AppIcon.GetAppIcon(); } catch { NotifyIcon.Icon = System.Drawing.SystemIcons.Information; }
 NotifyIcon.Visible = true;
 NotifyIcon.Text = "Work Timer";
 var menu = new ContextMenuStrip();
 var show = new ToolStripMenuItem("Show", null, (_,__) => ShowRequested?.Invoke(this, EventArgs.Empty));
 var pauseResume = new ToolStripMenuItem("Pause/Resume", null, (_,__) => TogglePauseRequested?.Invoke(this, EventArgs.Empty));
 var settings = new ToolStripMenuItem("Settings", null, (_,__) => SettingsRequested?.Invoke(this, EventArgs.Empty));
 var exit = new ToolStripMenuItem("Exit", null, (_,__) => Application.Exit());
 menu.Items.AddRange(new ToolStripItem[] { show, pauseResume, settings, new ToolStripSeparator(), exit });
 NotifyIcon.ContextMenuStrip = menu;
 NotifyIcon.DoubleClick += (_, __) => ShowRequested?.Invoke(this, EventArgs.Empty);
 }

 /// <summary>
 /// Updates the tooltip text shown when hovering over the tray icon
 /// </summary>
 /// <param name="text">Tooltip text (max 63 characters due to Windows limitation)</param>
 public void UpdateTooltip(string text)
 {
 // Windows has a 63-character limit for NotifyIcon.Text
 if (text.Length > 63)
 {
 NotifyIcon.Text = text.Substring(0, 60) + "...";
 }
 else
 {
 NotifyIcon.Text = text;
 }
 }

 public void Balloon(string title, string text, ToolTipIcon icon = ToolTipIcon.Info, int timeoutMs =5000)
 {
 NotifyIcon.BalloonTipTitle = title;
 NotifyIcon.BalloonTipText = text;
 NotifyIcon.BalloonTipIcon = icon;
 NotifyIcon.ShowBalloonTip(timeoutMs);
 }

 public void Dispose()
 {
 NotifyIcon.Visible = false;
 NotifyIcon.Dispose();
 }
}
