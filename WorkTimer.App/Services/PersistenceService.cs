using System;
using System.IO;
using System.Text.Json;
using WorkTimer.App.Models;

namespace WorkTimer.App.Services;

public static class PersistenceService
{
 private static readonly JsonSerializerOptions Options = new()
 {
 WriteIndented = true
 };

 public static string AppDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorkTimer_OM");
 public static string ShiftFile => Path.Combine(AppDir, "shift.json");
 public static string SettingsFile => Path.Combine(AppDir, "settings.json");

 public static void EnsureDir()
 {
 if (!Directory.Exists(AppDir)) Directory.CreateDirectory(AppDir);
 }

 public static void SaveShift(Shift shift)
 {
 EnsureDir();
 File.WriteAllText(ShiftFile, JsonSerializer.Serialize(shift, Options));
 }

 public static Shift? LoadShift()
 {
 try
 {
 if (!File.Exists(ShiftFile)) return null;
 var json = File.ReadAllText(ShiftFile);
 return JsonSerializer.Deserialize<Shift>(json, Options);
 }
 catch { return null; }
 }

 public static void SaveSettings(AppSettings settings)
 {
 EnsureDir();
 File.WriteAllText(SettingsFile, JsonSerializer.Serialize(settings, Options));
 }

 public static AppSettings LoadSettings()
 {
 try
 {
 if (!File.Exists(SettingsFile)) return new AppSettings();
 var json = File.ReadAllText(SettingsFile);
 return JsonSerializer.Deserialize<AppSettings>(json, Options) ?? new AppSettings();
 }
 catch { return new AppSettings(); }
 }
}
