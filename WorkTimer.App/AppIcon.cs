using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace WorkTimer.App;

internal static class AppIcon
{
 private static Icon? _icon;

 [DllImport("user32.dll", SetLastError = true)]
 private static extern bool DestroyIcon(IntPtr hIcon);

 public static Icon GetAppIcon()
 {
 return _icon ??= CreateIcon();
 }

 private static Icon CreateIcon()
 {
 // Draw a simple clock glyph as the app icon (works for taskbar and tray)
 using var bmp = new Bitmap(32,32);
 using (var g = Graphics.FromImage(bmp))
 {
 g.SmoothingMode = SmoothingMode.AntiAlias;
 g.Clear(Color.Transparent);

 var center = new PointF(16,16);
 using var faceBrush = new SolidBrush(Color.White);
 using var facePen = new Pen(Color.Black,2f);
 using var handPen = new Pen(Color.Black,2.4f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
 using var tickPen = new Pen(Color.Black,1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round };

 // Clock face
 g.FillEllipse(faceBrush,3,3,26,26);
 g.DrawEllipse(facePen,3,3,26,26);

 // Hour ticks (12,3,6,9)
 g.DrawLine(tickPen,16,6,16,9);
 g.DrawLine(tickPen,26,16,23,16);
 g.DrawLine(tickPen,16,26,16,23);
 g.DrawLine(tickPen,6,16,9,16);

 // Hands (10:10 style)
 // Hour hand
 g.DrawLine(handPen, center, new PointF(12,14));
 // Minute hand
 g.DrawLine(handPen, center, new PointF(20,12));

 // Center hub
 using var hubBrush = new SolidBrush(Color.Black);
 g.FillEllipse(hubBrush,14.5f,14.5f,3,3);
 }

 // Convert bitmap to icon
 var hIcon = bmp.GetHicon();
 try
 {
 using var tmp = Icon.FromHandle(hIcon);
 return (Icon)tmp.Clone();
 }
 finally
 {
 // Release native handle to avoid leak
 DestroyIcon(hIcon);
 }
 }
}
