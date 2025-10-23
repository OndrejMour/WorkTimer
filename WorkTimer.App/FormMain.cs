using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WorkTimer.App.Models;
using WorkTimer.App.Services;
using WorkTimer.App.Tray;

namespace WorkTimer.App;

public class FormMain : Form
{
 private readonly ProgressBar _shiftBar = new() { Dock = DockStyle.Top, Height =20 }; 
 private readonly Label _lbStart = new() { Dock = DockStyle.Top };
 private readonly Label _lbRemaining = new() { Dock = DockStyle.Top };
 private readonly Label _lbEnd = new() { Dock = DockStyle.Top };

 private readonly ProgressBar _escapeBar = new() { Dock = DockStyle.Top, Height =20, ForeColor = Color.Green };
 private readonly Label _lbNextEscape = new() { Dock = DockStyle.Top };

 private readonly Button _btnPauseResume = new() { Text = "Pause" };
 private readonly Button _btnHide = new() { Text = "Hide" };
 private readonly Button _btnExport = new() { Text = "Export CSV" };
 private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };

 private readonly System.Windows.Forms.Timer _timer = new() { Interval =1000, Enabled = true };
 private readonly TrayManager _tray = new();

 private Shift _shift;
 private AppSettings _settings;

 private bool _halfNotified; 
 private bool _endNotified;
 private DateTimeOffset _lastEscapeShown;

 public FormMain()
 {
 Text = "Work Timer";
 Width =420; Height =420; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;

 _settings = PersistenceService.LoadSettings();
 _shift = PersistenceService.LoadShift() ?? new Shift { Start = DateTimeOffset.Now, Target = _settings.TargetShift };
 if (!_shift.Segments.Any()) _shift.Resume("Work");

 var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height =120, FlowDirection = FlowDirection.TopDown };
 top.Controls.Add(_lbStart);
 top.Controls.Add(_shiftBar);
 top.Controls.Add(_lbRemaining);
 top.Controls.Add(_lbEnd);
 top.Controls.Add(_escapeBar);
 top.Controls.Add(_lbNextEscape);
 Controls.Add(top);

 var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height =40 };
 bottom.Controls.Add(_btnPauseResume);
 bottom.Controls.Add(_btnExport);
 bottom.Controls.Add(_btnHide);
 Controls.Add(bottom);

 _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(WorkSegment.Start), HeaderText = "Start", Width =120 });
 _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(WorkSegment.End), HeaderText = "End", Width =120 });
 _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(WorkSegment.Duration), HeaderText = "Duration", Width =80 });
 _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(WorkSegment.Task), HeaderText = "Task", Width =120 });
 _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(WorkSegment.Note), HeaderText = "Note", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
 _grid.DataSource = _shift.Segments;
 Controls.Add(_grid);

 _timer.Tick += (_, __) => TickUpdate();
 _btnHide.Click += (_, __) => Hide();
 _btnPauseResume.Click += (_, __) => TogglePause();
 _btnExport.Click += (_, __) => ExportCsv();

 _tray.ShowRequested += (_, __) => { Show(); Activate(); }; 
 _tray.TogglePauseRequested += (_, __) => TogglePause();

 FormClosing += (_, e) => { PersistenceService.SaveShift(_shift); PersistenceService.SaveSettings(_settings); _tray.Dispose(); };
 TickUpdate();
 }

 private static DateTimeOffset NextEscapeWindow(DateTimeOffset start, DateTimeOffset now)
 {
 var minutes = Math.Ceiling((now - start).TotalMinutes /15d) *15d;
 return start.AddMinutes(minutes);
 }

 private void TogglePause()
 {
 if (_shift.IsRunning)
 {
 _shift.Pause();
 _btnPauseResume.Text = "Resume";
 }
 else
 {
 using var dlg = new InputBox("Task name (optional):");
 var task = dlg.ShowDialog(this) == DialogResult.OK ? dlg.Value : null;
 _shift.Resume(task);
 _btnPauseResume.Text = "Pause";
 }
 PersistenceService.SaveShift(_shift);
 }

 private void TickUpdate()
 {
 var worked = _shift.Worked;
 var remaining = _shift.Remaining;
 var target = _shift.Target;
 var start = _shift.Start;
 var now = DateTimeOffset.Now;
 var plannedEnd = start + target + (now - start - worked); // now = start + worked + idle; End = now + remaining - idle

 _lbStart.Text = $"Start: {start:HH:mm} Now: {now:HH:mm:ss}";
 _lbRemaining.Text = $"Worked: {worked:hh\\:mm\\:ss} Remaining: {remaining:hh\\:mm\\:ss}";
 _lbEnd.Text = $"End: {plannedEnd:HH:mm}";

 _shiftBar.Maximum = (int)target.TotalSeconds;
 _shiftBar.Value = (int)Math.Min(target.TotalSeconds, Math.Max(0, worked.TotalSeconds));

 var nextEsc = NextEscapeWindow(start, now);
 var winStart = nextEsc.AddMinutes(-15);
 var escProg = (now - winStart).TotalSeconds;
 _escapeBar.Maximum = (int)TimeSpan.FromMinutes(15).TotalSeconds;
 _escapeBar.Value = (int)Math.Clamp(escProg,0, _escapeBar.Maximum);
 _lbNextEscape.Text = $"Escape window: {winStart:HH:mm} - {nextEsc:HH:mm}";

 // Notifications
 if (_settings.NotifyHalf && !_halfNotified && worked >= target/2)
 {
 _tray.Balloon("Work Timer", "Polovina smìny dosažena.");
 _halfNotified = true;
 }
 if (_settings.NotifyEnd && !_endNotified && worked >= target)
 {
 _tray.Balloon("Work Timer", "Konec smìny.");
 _endNotified = true;
 }
 if (_settings.NotifyEscapeWindow && nextEsc != _lastEscapeShown && now >= nextEsc)
 {
 _tray.Balloon("Work Timer", $"Únikové okno: {winStart:HH:mm}-{nextEsc:HH:mm}");
 _lastEscapeShown = nextEsc;
 }
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

 private static string EscapeCsv(string? t)
 {
 if (string.IsNullOrEmpty(t)) return "";
 if (t.Contains('"') || t.Contains(',') || t.Contains('\n'))
 return '"' + t.Replace("\"", "\"\"") + '"';
 return t;
 }
}

internal class InputBox : Form
{
 private readonly TextBox _tb = new() { Dock = DockStyle.Top };
 private readonly Button _ok = new() { Text = "OK", DialogResult = DialogResult.OK };
 private readonly Button _cancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };
 public string? Value => _tb.Text;
 public InputBox(string prompt)
 {
 Text = "Input"; Width =320; Height =120; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
 var lbl = new Label { Text = prompt, Dock = DockStyle.Top };
 var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height =40 };
 panel.Controls.Add(_ok); panel.Controls.Add(_cancel);
 Controls.Add(_tb); Controls.Add(lbl); Controls.Add(panel);
 AcceptButton = _ok; CancelButton = _cancel;
 }
}
