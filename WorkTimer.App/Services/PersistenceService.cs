using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WorkTimer.App.Models;

namespace WorkTimer.App.Services;

public static class PersistenceService
{
 private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

 // Store data next to the application (portable)
 public static string AppDir => AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
 public static string ShiftFile => Path.Combine(AppDir, "shift.json");
 public static string SettingsFile => Path.Combine(AppDir, "settings.json");
 public static string HistoryFile => Path.Combine(AppDir, "history.json");

 public static void EnsureDir()
 {
 if (!Directory.Exists(AppDir)) Directory.CreateDirectory(AppDir);
 }

 private static void WriteAtomic(string path, string content)
 {
 EnsureDir();
 var tmp = path + ".tmp";
 File.WriteAllText(tmp, content);
 // Replace existing file atomically when possible
 try
 {
 // .NET8 has File.Move overwrite overload
 File.Move(tmp, path, true);
 }
 catch
 {
 // Fallback: delete then move
 try { if (File.Exists(path)) File.Delete(path); } catch { /* ignore */ }
 try { File.Move(tmp, path); } catch { /* ignore */ }
 }
 }

 public static void SaveShift(Shift shift)
 {
 try
 {
 var json = JsonSerializer.Serialize(shift, Options);
 WriteAtomic(ShiftFile, json);
 }
 catch
 {
 // Swallow to avoid crashing UI on IO problems
 }
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
 try
 {
 var json = JsonSerializer.Serialize(settings, Options);
 WriteAtomic(SettingsFile, json);
 }
 catch
 {
 // ignore
 }
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

 public static List<Shift> LoadHistory()
 {
 try
 {
 if (!File.Exists(HistoryFile)) return new List<Shift>();
 var json = File.ReadAllText(HistoryFile);
 return JsonSerializer.Deserialize<List<Shift>>(json, Options) ?? new List<Shift>();
 }
 catch { return new List<Shift>(); }
 }

 public static void SaveHistory(List<Shift> history)
 {
 try
 {
 var json = JsonSerializer.Serialize(history, Options);
 WriteAtomic(HistoryFile, json);
 }
 catch
 {
 // ignore
 }
 }

 public static void AddToHistory(Shift ended)
 {
 if (!ended.End.HasValue) return;
 var list = LoadHistory();
 // avoid duplicates by Start+End
 if (!list.Exists(s => s.Start == ended.Start && s.End == ended.End))
 {
 list.Add(ended);
 SaveHistory(list);
 }
 }
}
