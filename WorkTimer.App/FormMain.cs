using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using WorkTimer.App.Models;
using WorkTimer.App.Services;
using WorkTimer.App.Tray;

namespace WorkTimer.App;

public class FormMain : Form
{
 // Header controls
 private readonly ProgressBar _shiftBar = new() { Height =20, Dock = DockStyle.Fill };
 private readonly Label _lbStart = new() { AutoSize = true };
 private readonly Label _lbShiftElapsed = new() { AutoSize = true }; // Odpracováno (smìna)
 private readonly Label _lbTasksSummary = new() { AutoSize = true }; // Odpracováno (úkoly) + Zbývá (do cíle)
 private readonly Label _lbEnd = new() { AutoSize = true };
 private readonly Label _lbActiveTask = new() { AutoSize = true, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold) };
 private readonly Label _lbShiftState = new() { AutoSize = true, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold) }; // Stav smìny

 private readonly ProgressBar _escapeBar = new() { Height =20, ForeColor = Color.Green, Dock = DockStyle.Fill };
 private readonly Label _lbNextEscape = new() { AutoSize = true };

 // Command buttons
 private readonly Button _btnStartTask = new();
 private readonly Button _btnHide = new();
 private readonly Button _btnExport = new();
 private readonly Button _btnSettings = new();
 private readonly Button _btnHistory = new();
 private readonly Button _btnSetStart = new() { AutoSize = true };
 private readonly Button _btnEndShift = new() { AutoSize = true };
 private readonly Button _btnBreak = new() { AutoSize = true }; // Pøestávka

 // New task inline input
 private readonly TextBox _tbNewTask = new() { Width =240 };

 // Tasks area (scroll -> table with full-width cards)
 private readonly GroupBox _tasksGroup = new() { Dock = DockStyle.Fill, Padding = new Padding(8), Margin = new Padding(8,6,8,0) };
 private readonly Panel _tasksScroll = new() { Dock = DockStyle.Fill, AutoScroll = true };
 private readonly NoFlickerTableLayoutPanel _tasksTable = new() { Dock = DockStyle.Top, ColumnCount =1, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = SystemColors.Control };

 private readonly System.Windows.Forms.Timer _timer = new() { Interval =1000, Enabled = true };
 private readonly TrayManager _tray = new();
 private readonly ToolTip _tip = new();

 private Shift _shift;
 private AppSettings _settings;

 private bool _halfNotified;
 private bool _endNotified;
 private DateTimeOffset _lastEscapeShown;

 private readonly Dictionary<string, TaskRow> _rowsByTask = new(StringComparer.Ordinal);

 public FormMain()
 {
 // Load persisted
 _settings = PersistenceService.LoadSettings();
 _shift = PersistenceService.LoadShift() ?? new Shift { Start = DateTimeOffset.Now, Target = _settings.TargetShift };

 // Sync StartWithWindows setting with actual registry state
 _settings.StartWithWindows = StartupManager.IsStartWithWindowsEnabled();

 // Form title
 Text = $"{Localization.T(_settings.Language, "AppName")} {DateTime.Now:HH:mm:ss}";
 Width =620; Height =560; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
 SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
 UpdateStyles();

 // Set app icon for window
 try { Icon = AppIcon.GetAppIcon(); } catch { /* ignore */ }

 // Header layout
 var header = new TableLayoutPanel
 {
 Dock = DockStyle.Fill,
 ColumnCount =1,
 RowCount =8,
 Padding = new Padding(8), // slightly tighter padding
 AutoSize = true,
 AutoSizeMode = AutoSizeMode.GrowAndShrink
 };
 header.RowStyles.Add(new RowStyle(SizeType.AutoSize)); //0: start row
 header.RowStyles.Add(new RowStyle(SizeType.Absolute,26)); //1: shift bar taller
 header.RowStyles.Add(new RowStyle(SizeType.AutoSize)); //2: elapsed shift
 header.RowStyles.Add(new RowStyle(SizeType.AutoSize)); //3: tasks summary
 header.RowStyles.Add(new RowStyle(SizeType.AutoSize)); //4: active task
 header.RowStyles.Add(new RowStyle(SizeType.AutoSize)); //5: end label
 header.RowStyles.Add(new RowStyle(SizeType.Absolute,26)); //6: escape bar taller
 header.RowStyles.Add(new RowStyle(SizeType.AutoSize)); //7: escape label

 _shiftBar.Margin = new Padding(0,4,0,6);
 _escapeBar.Margin = new Padding(0,6,0,4);

 var startRow = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Fill, AutoSize = true };
 startRow.Controls.Add(_lbShiftState);
 startRow.Controls.Add(_lbStart);
 startRow.Controls.Add(_btnSetStart);
 startRow.Controls.Add(_btnBreak);
 startRow.Controls.Add(_btnEndShift);
 header.Controls.Add(startRow,0,0);
 header.Controls.Add(_shiftBar,0,1);
 header.Controls.Add(_lbShiftElapsed,0,2);
 header.Controls.Add(_lbTasksSummary,0,3);
 header.Controls.Add(_lbActiveTask,0,4);
 header.Controls.Add(_lbEnd,0,5);
 header.Controls.Add(_escapeBar,0,6);
 header.Controls.Add(_lbNextEscape,0,7);

 // Tasks group content
 if (_tasksGroup.Controls.Count ==0)
 {
 _tasksScroll.Controls.Add(_tasksTable);
 _tasksGroup.Controls.Add(_tasksScroll);
 _tasksTable.ColumnStyles.Clear();
 _tasksTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f));
 }

 // Inline new task panel (below tasks, above buttons)
 var newTaskPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Height =36, Padding = new Padding(8,6,8,0) };
 newTaskPanel.Controls.Add(_tbNewTask);
 newTaskPanel.Controls.Add(_btnStartTask);

 // Bottom commands panel
 var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, Height =44, AutoSize = true };
 // Removed _btnStartTask from bottom, it lives next to the textbox above
 bottom.Controls.Add(_btnExport);
 bottom.Controls.Add(_btnSettings);
 bottom.Controls.Add(_btnHide);
 bottom.Controls.Add(_btnHistory);

 // Root layout
 var root = new TableLayoutPanel
 {
 Dock = DockStyle.Fill,
 ColumnCount =1,
 RowCount =4,
 Padding = new Padding(0)
 };
 root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // header
 root.RowStyles.Add(new RowStyle(SizeType.Percent,100)); // tasks
 root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // new task inline
 root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // bottom
 root.Controls.Add(header,0,0);
 root.Controls.Add(_tasksGroup,0,1);
 root.Controls.Add(newTaskPanel,0,2);
 root.Controls.Add(bottom,0,3);
 Controls.Add(root);

 // Events
 _timer.Tick += (_, __) => TickUpdate();
 _btnHide.Click += (_, __) => Hide();
 _btnStartTask.Click += (_, __) => StartTaskFromInline();
 _tbNewTask.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; StartTaskFromInline(); } };
 _btnExport.Click += (_, __) => ExportCsv();
 _btnSettings.Click += (_, __) => OpenSettings();
 _btnSetStart.Click += (_, __) => SetStartTime();
 _btnBreak.Click += (_, __) => ToggleBreak();
 _btnEndShift.Click += (_, __) => EndShiftNow();
 _btnHistory.Click += (_, __) => OpenHistory();

 _tray.ShowRequested += (_, __) => { Show(); Activate(); };
 _tray.TogglePauseRequested += (_, __) => PauseCurrent();
 _tray.SettingsRequested += (_, __) => OpenSettings();

 _shift.Segments.ListChanged += (_, __) => BuildTasksPanel();
 _tasksScroll.Resize += (_, __) => EnforceCardWidth();

 // Persist on close and on system session ending (logoff/shutdown)
 FormClosing += (_, e) =>
 {
 // Handle minimize to tray on close
 if (_settings.MinimizeToTrayOnClose && e.CloseReason == CloseReason.UserClosing)
 {
 e.Cancel = true;
 Hide();
 return;
 }
 
 // If shift ended, purge finished tasks from current shift so they don't reappear next run
 if (_shift.End.HasValue && _shift.FinishedTasks.Count >0)
 {
 try
 {
 var finished = new HashSet<string>(_shift.FinishedTasks, StringComparer.Ordinal);
 for (int i = _shift.Segments.Count -1; i >=0; i--)
 {
 var seg = _shift.Segments[i];
 var name = string.IsNullOrWhiteSpace(seg.Task) ? Localization.T(_settings.Language, "Unnamed") : seg.Task!;
 if (finished.Contains(name)) _shift.Segments.RemoveAt(i);
 }
 _shift.FinishedTasks.Clear();
 }
 catch { /* ignore */ }
 }
 PersistenceService.SaveShift(_shift);
 PersistenceService.SaveSettings(_settings);
 _tray.Dispose();
 };
 
 // Handle minimize to tray on minimize
 Resize += (_, __) =>
 {
 if (_settings.MinimizeToTray && WindowState == FormWindowState.Minimized)
 {
 Hide();
 }
 };
 
 SystemEvents.SessionEnding += SystemEvents_SessionEnding;
 SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

 // Initial localization for UI captions
 ApplyLocalization();
 TickUpdate();
 }

 private void SystemEvents_SessionEnding(object? sender, SessionEndingEventArgs e)
 {
 try
 {
 // Keep current state, just persist so it continues after restart
 PersistenceService.SaveShift(_shift);
 PersistenceService.SaveSettings(_settings);
 }
 catch { /* ignore */ }
 }

 private void SystemEvents_PowerModeChanged(object? sender, PowerModeChangedEventArgs e)
 {
 if (e.Mode == PowerModes.Suspend)
 {
 try
 {
 // Keep current state, just persist
 PersistenceService.SaveShift(_shift);
 }
 catch { /* ignore */ }
 }
 }

 private void ApplyLocalization()
 {
 var L = _settings.Language;
 _btnStartTask.Text = Localization.T(L, "StartTask");
 _tbNewTask.PlaceholderText = Localization.T(L, "TaskNamePrompt");
 _btnSetStart.Text = Localization.T(L, "SetStart");
 _btnEndShift.Text = Localization.T(L, "EndShift");
 _btnHide.Text = Localization.T(L, "Hide");
 _btnExport.Text = Localization.T(L, "Export");
 _btnExport.Visible = false; // optional feature hidden
 _btnSettings.Text = Localization.T(L, "Settings");
 _btnHistory.Text = Localization.T(L, "History");
 _btnBreak.Text = _shift.IsOnBreak ? Localization.T(L, "EndBreak") : Localization.T(L, "StartBreak");
 _tasksGroup.Text = Localization.T(L, "TasksCaption");
 Text = $"{Localization.T(L, "AppName")} {DateTime.Now:HH:mm:ss}";
 BuildTasksPanel();
 }

 private static DateTimeOffset NextEscapeWindow(DateTimeOffset start, DateTimeOffset now)
 {
 var minutes = Math.Ceiling((now - start).TotalMinutes /15d) *15d;
 return start.AddMinutes(minutes);
 }

 private void StartTaskPrompt()
 {
 var L = _settings.Language;
 using var dlg = new InputBox(Localization.T(L, "TaskNamePrompt"));
 dlg.ApplyLocalization(L);
 if (dlg.ShowDialog(this) != DialogResult.OK) return;
 var task = (dlg.Value ?? string.Empty).Trim();
 if (string.IsNullOrEmpty(task)) return;
 StartTask(task);
 }

 private void StartTaskFromInline()
 {
 var task = (_tbNewTask.Text ?? string.Empty).Trim();
 if (string.IsNullOrEmpty(task)) return;
 StartTask(task);
 _tbNewTask.Clear();
 _tbNewTask.Focus();
 }

 private void StartTask(string task)
 {
 if (_shift.IsRunning) _shift.Pause();
 _shift.FinishedTasks.Remove(task);
 _shift.Resume(task);
 _shift.End = null; // smìna bìží
 PersistenceService.SaveShift(_shift);
 BuildTasksPanel();
 }

 private void PauseCurrent()
 {
 if (!_shift.IsRunning) return;
 _shift.Pause();
 PersistenceService.SaveShift(_shift);
 UpdateTaskRowsTick();
 }

 private void ToggleBreak()
 {
 if (_shift.IsOnBreak) _shift.EndBreak();
 else
 {
 // pause any running task and start break
 _shift.Pause();
 _shift.StartBreak();
 }
 PersistenceService.SaveShift(_shift);
 ApplyLocalization();
 TickUpdate();
 }

 private void EndShiftNow()
 {
 if (_shift.IsRunning) _shift.Pause();
 if (_shift.IsOnBreak) _shift.EndBreak();
 _shift.End = DateTimeOffset.Now;
 PersistenceService.SaveShift(_shift);
 PersistenceService.AddToHistory(_shift);
 _halfNotified = false; _endNotified = false; _lastEscapeShown = default;
 BuildTasksPanel();
 TickUpdate();
 }

 private void SetStartTime()
 {
 var L = _settings.Language;
 using var f = new FormSetStart(DateTime.Now.TimeOfDay);
 f.ApplyLocalization(L);
 if (f.ShowDialog(this) == DialogResult.OK)
 {
 var today = DateTime.Now.Date;
 var dtLocal = today.Add(f.SelectedTime);
 _shift.Start = new DateTimeOffset(dtLocal, DateTimeOffset.Now.Offset);
 _shift.End = null; // reset
 _halfNotified = false; _endNotified = false; _lastEscapeShown = default;
 PersistenceService.SaveShift(_shift);
 TickUpdate();
 }
 }

 private sealed class TaskRow
 {
 public string Name = string.Empty;
 public Label LName = new() { AutoSize = true };
 public Label LStatus = new() { AutoSize = true };
 public Label LDuration = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
 public Button BtnToggle = new() { Width =28, Height =28, Margin = new Padding(3), Text = string.Empty };
 public Button BtnFinish = new() { Width =28, Height =28, Margin = new Padding(3), Text = string.Empty };
 public Button BtnRename = new() { Width =28, Height =28, Margin = new Padding(3), Text = string.Empty };
 public Button BtnDelete = new() { Width =28, Height =28, Margin = new Padding(3), Text = string.Empty };
 public TaskCardPanel Card = new() { BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0,0,0,6), Padding = new Padding(8), MinimumSize = new Size(0,48), Height =48, Anchor = AnchorStyles.Left | AnchorStyles.Right };
 }

 private void BuildTasksPanel()
 {
 _tasksTable.SuspendLayout();
 _tasksTable.Controls.Clear();
 _tasksTable.RowStyles.Clear();
 _tasksTable.RowCount =0;
 _rowsByTask.Clear();

 var now = DateTimeOffset.Now;
 var groups = _shift.Segments
 .GroupBy(s => string.IsNullOrWhiteSpace(s.Task) ? Localization.T(_settings.Language, "Unnamed") : s.Task!)
 .Select(g => new
 {
 Name = g.Key,
 Duration = TimeSpan.FromTicks(g.Sum(s => s.Duration.Ticks)),
 IsActive = g.Any(s => s.End == null),
 LastActivity = g.Max(s => (s.End ?? now))
 })
 .ToList();

 // When the shift already ended, do not show finished tasks in the list
 if (_shift.End.HasValue && _shift.FinishedTasks.Count >0)
 {
 groups = groups.Where(g => !_shift.FinishedTasks.Contains(g.Name)).ToList();
 }

 // Add finished tasks without segments only if shift is still running
 if (!_shift.End.HasValue)
 {
 foreach (var ft in _shift.FinishedTasks.Distinct())
 {
 if (!groups.Any(g => g.Name == ft))
 groups.Add(new { Name = ft, Duration = TimeSpan.Zero, IsActive = false, LastActivity = now });
 }
 }

 if (groups.Count ==0)
 {
 _tasksTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
 _tasksTable.RowCount++;
 _tasksTable.Controls.Add(new Label { Text = Localization.T(_settings.Language, "NoTasksYet"), AutoSize = true, Padding = new Padding(8) },0, _tasksTable.RowCount -1);
 _tasksTable.ResumeLayout();
 return;
 }

 groups = groups
 .OrderByDescending(g => g.IsActive)
 .ThenBy(g => _shift.FinishedTasks.Contains(g.Name))
 .ThenByDescending(g => g.LastActivity)
 .ToList();

 const int maxFinishedShown =5;
 var finishedCount = groups.Count(g => _shift.FinishedTasks.Contains(g.Name));
 if (finishedCount > maxFinishedShown)
 {
 var finishedToHide = finishedCount - maxFinishedShown;
 for (int i = groups.Count -1; i >=0 && finishedToHide >0; i--)
 if (_shift.FinishedTasks.Contains(groups[i].Name)) { groups.RemoveAt(i); finishedToHide--; }
 }

 foreach (var g in groups)
 {
 var row = CreateTaskCard(g.Name, g.IsActive, g.Duration);
 _rowsByTask[row.Name] = row;

 _tasksTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
 _tasksTable.RowCount++;
 _tasksTable.Controls.Add(row.Card,0, _tasksTable.RowCount -1);
 }

 EnforceCardWidth();
 _tasksTable.ResumeLayout();
 }

 private TaskRow CreateTaskCard(string name, bool isActive, TimeSpan duration)
 {
 var L = _settings.Language;
 var row = new TaskRow { Name = name };
 row.LName.Text = name;
 row.LStatus.Text = isActive ? Localization.T(L, "Active") : (_shift.FinishedTasks.Contains(name) ? Localization.T(L, "Finished") : Localization.T(L, "Paused"));
 row.LStatus.ForeColor = isActive ? Color.DarkGreen : (_shift.FinishedTasks.Contains(name) ? Color.Gray : Color.DarkOrange);
 row.LDuration.Text = duration.ToString("hh\\:mm\\:ss");
 row.LDuration.Width = TextRenderer.MeasureText("00:00:00", Font).Width +4;

 // Icon images (drawn -> works on all fonts)
 row.BtnToggle.Image = isActive ? IconFactory.Pause16() : IconFactory.Play16();
 row.BtnFinish.Image = IconFactory.Stop16();
 row.BtnRename.Image = IconFactory.Pencil16();
 row.BtnDelete.Image = IconFactory.Trash16();

 // Fix: Always check current state instead of using captured isActive value
 row.BtnToggle.Click += (_, __) =>
 {
 // Check if this task is currently active
 var currentSegment = _shift.Segments.LastOrDefault(s => s.End == null);
 var currentTaskName = currentSegment != null 
     ? (string.IsNullOrWhiteSpace(currentSegment.Task) ? Localization.T(L, "Unnamed") : currentSegment.Task!)
     : null;
 bool isCurrentlyActive = currentTaskName == name;
 
 if (isCurrentlyActive) 
     PauseCurrent(); 
 else 
     StartTask(name);
 };
 
 row.BtnFinish.Click += (_, __) =>
 {
 var cur = _shift.Segments.LastOrDefault(s => s.End == null);
 if (cur != null && (cur.Task ?? Localization.T(L, "Unnamed")) == name) _shift.Pause();
 if (!_shift.FinishedTasks.Contains(name)) _shift.FinishedTasks.Add(name);
 PersistenceService.SaveShift(_shift);
 BuildTasksPanel();
 };
 row.BtnRename.Click += (_, __) => RenameTask(name);
 row.BtnDelete.Click += (_, __) => DeleteTask(name);

 var inner = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount =2, RowCount =1 };
 inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
 inner.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
 inner.RowStyles.Add(new RowStyle(SizeType.Absolute,40));

 var info = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Dock = DockStyle.Fill, WrapContents = false, Margin = new Padding(0), Padding = new Padding(0) };
 info.Controls.Add(row.LName);
 info.Controls.Add(new Label { Text = " | ", AutoSize = true });
 info.Controls.Add(row.LStatus);
 info.Controls.Add(new Label { Text = " | ", AutoSize = true });
 info.Controls.Add(new Label { Text = Localization.T(L, "Time"), AutoSize = true });
 info.Controls.Add(row.LDuration);

 var btns = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Dock = DockStyle.Right, WrapContents = false, Margin = new Padding(0), Padding = new Padding(0) };
 btns.Controls.Add(row.BtnToggle);
 btns.Controls.Add(row.BtnFinish);
 btns.Controls.Add(row.BtnRename);
 btns.Controls.Add(row.BtnDelete);

 inner.Controls.Add(info,0,0);
 inner.Controls.Add(btns,1,0);
 row.Card.Controls.Add(inner);
 return row;
 }

 private void EnforceCardWidth()
 {
 var w = _tasksScroll.ClientSize.Width -2 *8; // minus typical padding
 foreach (Control ctl in _tasksTable.Controls)
 {
 ctl.Width = Math.Max(0, w);
 }
 }

 private void UpdateTaskRowsTick()
 {
 if (_rowsByTask.Count ==0) return;
 var groups = _shift.Segments
 .GroupBy(s => string.IsNullOrWhiteSpace(s.Task) ? Localization.T(_settings.Language, "Unnamed") : s.Task!)
 .Select(g => new
 {
 Name = g.Key,
 Duration = TimeSpan.FromTicks(g.Sum(s => s.Duration.Ticks)),
 IsActive = g.Any(s => s.End == null)
 })
 .ToDictionary(g => g.Name, g => g);

 if (groups.Count != _rowsByTask.Count || _rowsByTask.Keys.Any(k => !groups.ContainsKey(k)))
 { BuildTasksPanel(); return; }

 foreach (var (name, row) in _rowsByTask)
 {
 var g = groups[name];
 row.LDuration.Text = g.Duration.ToString("hh\\:mm\\:ss");
 bool isFinished = _shift.FinishedTasks.Contains(name);
 row.LStatus.Text = g.IsActive ? Localization.T(_settings.Language, "Active") : (isFinished ? Localization.T(_settings.Language, "Finished") : Localization.T(_settings.Language, "Paused"));
 row.LStatus.ForeColor = g.IsActive ? Color.DarkGreen : (isFinished ? Color.Gray : Color.DarkOrange);
 row.BtnToggle.Image = g.IsActive ? IconFactory.Pause16() : IconFactory.Play16();
 row.BtnToggle.Enabled = !isFinished;
 row.BtnFinish.Enabled = !isFinished;
 }
 }

 private void RenameTask(string oldName)
 {
 var L = _settings.Language;
 using var dlg = new InputBox(string.Format(Localization.T(L, "RenameTaskPrompt"), oldName));
 dlg.ApplyLocalization(L);
 if (dlg.ShowDialog(this) != DialogResult.OK) return;
 var newName = (dlg.Value ?? string.Empty).Trim();
 if (string.IsNullOrEmpty(newName) || newName == oldName) return;

 foreach (var s in _shift.Segments)
 {
 var name = string.IsNullOrWhiteSpace(s.Task) ? Localization.T(L, "Unnamed") : s.Task!;
 if (name == oldName) s.Task = newName;
 }
 for (int i =0; i < _shift.FinishedTasks.Count; i++)
 if (_shift.FinishedTasks[i] == oldName) _shift.FinishedTasks[i] = newName;
 PersistenceService.SaveShift(_shift);
 BuildTasksPanel();
 }

 private void DeleteTask(string name)
 {
 var L = _settings.Language;
 var result = MessageBox.Show(this,
 string.Format(Localization.T(L, "DeleteTaskConfirm"), name),
 Localization.T(L, "DeleteTaskTitle"),
 MessageBoxButtons.YesNo,
 MessageBoxIcon.Warning,
 MessageBoxDefaultButton.Button2);
 if (result != DialogResult.Yes) return;

 var current = _shift.Segments.LastOrDefault(s => s.End == null);
 if (current != null)
 {
 var curName = string.IsNullOrWhiteSpace(current.Task) ? Localization.T(L, "Unnamed") : current.Task!;
 if (curName == name) _shift.Pause();
 }
 for (int i = _shift.Segments.Count -1; i >=0; i--)
 {
 var seg = _shift.Segments[i];
 var segName = string.IsNullOrWhiteSpace(seg.Task) ? Localization.T(L, "Unnamed") : seg.Task!;
 if (segName == name)
 {
 _shift.Segments.RemoveAt(i);
 }
 }
 _shift.FinishedTasks.RemoveAll(t => t == name);

 PersistenceService.SaveShift(_shift);
 BuildTasksPanel();
 }

 private void TickUpdate()
 {
 var L = _settings.Language;
 var start = _shift.Start;
 var nowReal = DateTimeOffset.Now;
 var nowSnap = _shift.End ?? nowReal; // freeze counters when shift ended
 var target = _shift.Target;

 // Progress of the shift based on worked shift time (excludes breaks)
 var workedShift = _shift.WorkedShift;
 _shiftBar.Maximum = (int)Math.Max(1, target.TotalSeconds);
 _shiftBar.Value = (int)Math.Clamp(workedShift.TotalSeconds,0, _shiftBar.Maximum);
 _lbShiftElapsed.Text = $"{Localization.T(L, "WorkedShift")} {workedShift:hh\\:mm\\:ss}";

 var workedTasks = _shift.WorkedTasks;
 var remainingToTarget = _shift.Remaining;

 // Start label without current time (now time is in window title)
 _lbStart.Text = $"{Localization.T(L, "Start")}: {start:HH:mm}";
 _lbTasksSummary.Text = $"{Localization.T(L, "WorkedTasks")} {workedTasks:hh\\:mm\\:ss} {Localization.T(L, "RemainToTarget")} {remainingToTarget:hh\\:mm\\:ss}";

 // Planned end time includes breaks
 var plannedEnd = _shift.PlannedEnd;
 _lbEnd.Text = $"{Localization.T(L, "ShiftEnd")} {plannedEnd:HH:mm}";

 var nextEsc = NextEscapeWindow(start, nowSnap);
 var winStart = nextEsc.AddMinutes(-15);
 var escProg = (nowSnap - winStart).TotalSeconds;
 _escapeBar.Maximum = (int)TimeSpan.FromMinutes(15).TotalSeconds;
 _escapeBar.Value = (int)Math.Clamp(escProg,0, _escapeBar.Maximum);
 _lbNextEscape.Text = $"{Localization.T(L, "NextEscape")} {winStart:HH:mm} - {nextEsc:HH:mm}";

 var current = _shift.Segments.LastOrDefault(s => s.End == null);
 if (current != null)
 {
 var curDur = (nowSnap - current.Start);
 _lbActiveTask.Text = $"{Localization.T(L, "ActiveTask")} {current.Task ?? Localization.T(L, "Unnamed")} {Localization.T(L, "From")} {current.Start:HH:mm} – {curDur:hh\\:mm\\:ss}";
 _lbActiveTask.ForeColor = Color.DarkGreen;
 }
 else { _lbActiveTask.Text = $"{Localization.T(L, "ActiveTask")} — {Localization.T(L, "PausedState")}"; _lbActiveTask.ForeColor = SystemColors.ControlText; }

 // Shift state indicator
 if (_shift.IsOnBreak)
 {
 _lbShiftState.Text = $"{Localization.T(L, "ShiftState")} {Localization.T(L, "OnBreak")}";
 _lbShiftState.ForeColor = Color.DarkOrange;
 }
 else if (_shift.End.HasValue)
 {
 _lbShiftState.Text = $"{Localization.T(L, "ShiftState")} {Localization.T(L, "ShiftEnded")}";
 _lbShiftState.ForeColor = Color.DarkRed;
 }
 else
 {
 _lbShiftState.Text = $"{Localization.T(L, "ShiftState")} {Localization.T(L, "ShiftRunning")}";
 _lbShiftState.ForeColor = Color.DarkGreen;
 }

 // Break button text
 _btnBreak.Text = _shift.IsOnBreak ? Localization.T(L, "EndBreak") : Localization.T(L, "StartBreak");

 if (!_shift.End.HasValue)
 {
 if (_settings.NotifyHalf && !_halfNotified && workedTasks >= target /2) { _tray.Balloon(Localization.T(L, "AppName"), Localization.T(L, "HalfReached")); _halfNotified = true; }
 if (_settings.NotifyEnd && !_endNotified && workedTasks >= target) { _tray.Balloon(Localization.T(L, "AppName"), Localization.T(L, "ShiftEndReached")); _endNotified = true; }
 if (_settings.NotifyEscapeWindow && nextEsc != _lastEscapeShown && nowSnap >= nextEsc) { _tray.Balloon(Localization.T(L, "AppName"), $"{Localization.T(L, "NextEscape")} {winStart:HH:mm}-{nextEsc:HH:mm}"); _lastEscapeShown = nextEsc; }
 }

 UpdateTaskRowsTick();
 }

 private void ExportCsv()
 {
 using var sfd = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = $"worklog_{DateTime.Now:yyyyMMdd}.csv" };
 if (sfd.ShowDialog(this) != DialogResult.OK) return;
 using var w = new System.IO.StreamWriter(sfd.FileName);
 w.WriteLine("Start,End,Duration,Task,Note");
 foreach (var s in _shift.Segments)
 {
 var end = s.End?.ToString("o") ?? "";
 w.WriteLine($"{s.Start:o},{end},{s.Duration:c},{EscapeCsv(s.Task)},{EscapeCsv(s.Note)}");
 }
 }

 private void OpenSettings()
 {
 using var f = new FormSettings(_settings, _shift);
 if (f.ShowDialog(this) == DialogResult.OK)
 {
 var oldStartWithWindows = _settings.StartWithWindows;
 _settings = f.Settings;
 _shift.Target = _settings.TargetShift;
 
 // Apply Start with Windows setting if changed
 if (oldStartWithWindows != _settings.StartWithWindows)
 {
 StartupManager.SetStartWithWindows(_settings.StartWithWindows);
 }
 
 PersistenceService.SaveSettings(_settings);
 PersistenceService.SaveShift(_shift);
 // Re-apply localization after changing language
 ApplyLocalization();
 TickUpdate();
 }
 }

 private void OpenHistory()
 {
 using var f = new FormHistory(_settings.Language);
 f.ShowDialog(this);
 }

 private static string EscapeCsv(string? t)
 {
 if (string.IsNullOrEmpty(t)) return "";
 if (t.Contains('"') || t.Contains(',') || t.Contains('\n')) return '"' + t.Replace("\"", "\"\"") + '"';
 return t;
 }
}

internal class InputBox : Form
{
 private readonly TextBox _tb = new() { Dock = DockStyle.Top };
 private readonly Button _ok = new() { DialogResult = DialogResult.OK };
 private readonly Button _cancel = new() { DialogResult = DialogResult.Cancel };
 public string? Value => _tb.Text;
 public InputBox(string prompt)
 {
 Width =320; Height =120; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
 var lbl = new Label { Text = prompt, Dock = DockStyle.Top };
 var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height =40 };
 panel.Controls.Add(_ok); panel.Controls.Add(_cancel);
 Controls.Add(_tb); Controls.Add(lbl); Controls.Add(panel);
 AcceptButton = _ok; CancelButton = _cancel;
 }
 public void ApplyLocalization(AppLanguage L)
 {
 Text = Localization.T(L, "InputTitle");
 _ok.Text = Localization.T(L, "Ok");
 _cancel.Text = Localization.T(L, "Cancel");
 }
}

public class BreakSegment
{
 public DateTimeOffset Start { get; set; }
 public DateTimeOffset? End { get; set; }
 public TimeSpan Duration => (End ?? DateTimeOffset.Now) - Start;
}

internal sealed class TaskCardPanel : Panel
{
 public TaskCardPanel()
 {
 SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
 UpdateStyles();
 }
}

internal sealed class NoFlickerTableLayoutPanel : TableLayoutPanel
{
 public NoFlickerTableLayoutPanel()
 {
 SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
 UpdateStyles();
 }
}

internal static class IconFactory
{
 private static Bitmap Make(int w, int h, Action<Graphics> draw)
 {
 var bmp = new Bitmap(w, h);
 using var g = Graphics.FromImage(bmp);
 g.SmoothingMode = SmoothingMode.AntiAlias;
 g.Clear(Color.Transparent);
 draw(g);
 return bmp;
 }

 public static Bitmap Play16() => Make(16,16, g =>
 {
 using var b = new SolidBrush(Color.Black);
 g.FillPolygon(b, new[] { new PointF(4,3), new PointF(13,8), new PointF(4,13) });
 });

 public static Bitmap Pause16() => Make(16,16, g =>
 {
 using var b = new SolidBrush(Color.Black);
 g.FillRectangle(b, new RectangleF(3,3,4,10));
 g.FillRectangle(b, new RectangleF(9,3,4,10));
 });

 public static Bitmap Stop16() => Make(16,16, g =>
 {
 using var b = new SolidBrush(Color.Black);
 g.FillRectangle(b, new RectangleF(3,3,10,10));
 });

 // Clearer pencil writing on paper
 public static Bitmap Pencil16() => Make(16,16, g =>
 {
 using var pen = new Pen(Color.Black,1.5f) { LineJoin = LineJoin.Round };
 using var brush = new SolidBrush(Color.Black);
 // Paper outline
 g.DrawRectangle(pen, new Rectangle(1,1,10,12));
 // Folded corner
 g.DrawLine(pen,9,1,11,3);
 g.DrawLine(pen,11,3,11,13);

 // Pencil body (diagonal)
 g.TranslateTransform(3,12);
 g.RotateTransform(-40);
 g.FillRectangle(brush, new RectangleF(0,-1,8,2)); // body
 // Pencil tip
 g.FillPolygon(brush, new[] { new PointF(8,-1), new PointF(11,0), new PointF(8,1) });
 g.ResetTransform();

 // Writing stroke on paper
 g.DrawLine(pen,3,11,8,11);
 });

 // Trash bin icon
 public static Bitmap Trash16() => Make(16,16, g =>
 {
 using var pen = new Pen(Color.Black,1.5f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
 using var brush = new SolidBrush(Color.Black);
 // Lid
 g.FillRectangle(brush, new RectangleF(3,3,10,2));
 g.FillRectangle(brush, new RectangleF(6,2,4,1)); // handle
 // Bin
 g.DrawRectangle(pen, new Rectangle(4,5,8,9));
 // Slats
 g.DrawLine(pen,6,6,6,13);
 g.DrawLine(pen,8,6,8,13);
 g.DrawLine(pen,10,6,10,13);
 });
}
