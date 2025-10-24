using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WorkTimer.App.Models;
using WorkTimer.App.Services;

namespace WorkTimer.App;

public class FormHistory : Form
{
 private readonly TreeView _tv = new() { Dock = DockStyle.Fill, HideSelection = false };
 private readonly Button _btnClose = new() { Dock = DockStyle.Bottom };

 private readonly AppLanguage _lang;

 public FormHistory(AppLanguage lang)
 {
 _lang = lang;
 Text = Localization.T(_lang, "HistoryTitle"); Width =720; Height =520; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
 Controls.Add(_tv);
 Controls.Add(_btnClose);
 _btnClose.Text = Localization.T(_lang, "Close");
 _btnClose.Click += (_, __) => Close();
 _tv.NodeMouseDoubleClick += (_, e) => { if (e.Node.IsExpanded) e.Node.Collapse(); else e.Node.Expand(); };
 Load += (_, __) => LoadData();
 }

 private void LoadData()
 {
 var history = PersistenceService.LoadHistory()
 .OrderByDescending(s => s.End ?? s.Start)
 .ToList();
 _tv.BeginUpdate();
 _tv.Nodes.Clear();
 foreach (var s in history)
 {
 var end = s.End;
 var dur = (end ?? DateTimeOffset.Now) - s.Start;
 // Breaks summary
 long breakTicks =0; int breakCount =0;
 if (s.Breaks != null)
 {
 breakCount = s.Breaks.Count;
 foreach (var b in s.Breaks) breakTicks += b.Duration.Ticks;
 }
 var totalBreak = TimeSpan.FromTicks(breakTicks);

 // Tasks summary
 var groups = new System.Collections.Generic.List<(string Name,long Ticks)>();
 if (s.Segments != null)
 {
 var tmp = s.Segments
 .GroupBy(seg => string.IsNullOrWhiteSpace(seg.Task) ? Localization.T(_lang, "Unnamed") : seg.Task!)
 .Select(g => new { Name = g.Key, Ticks = g.Sum(x => x.Duration.Ticks) })
 .OrderByDescending(x => x.Ticks)
 .ToList();
 groups = tmp.Select(x => (x.Name, x.Ticks)).ToList();
 }

 var rootText = new StringBuilder()
 .Append(s.Start.ToString("g"))
 .Append(" – ")
 .Append(end?.ToString("g") ?? "")
 .Append(" | ")
 .Append(Localization.T(_lang, "ColumnDuration")).Append(' ').Append(dur.ToString("hh\\:mm\\:ss"))
 .Append(" | ")
 .Append(Localization.T(_lang, "ColumnBreaks")).Append(' ').Append(totalBreak.ToString("hh\\:mm\\:ss"))
 .ToString();
 var root = new TreeNode(rootText) { Tag = s };

 // Tasks group
 var tasksHeader = new TreeNode($"{Localization.T(_lang, "ColumnTasks")} ({groups.Count})");
 foreach (var g in groups)
 {
 var t = TimeSpan.FromTicks(g.Ticks);
 tasksHeader.Nodes.Add(new TreeNode($"{g.Name}: {t:hh\\:mm\\:ss}"));
 }
 root.Nodes.Add(tasksHeader);

 // Breaks group
 var breaksHeader = new TreeNode($"{Localization.T(_lang, "ColumnBreaks")} ({breakCount})");
 if (s.Breaks != null)
 {
 foreach (var b in s.Breaks)
 {
 var bText = $"{b.Start:g} – {(b.End?.ToString("g") ?? "")} [{b.Duration:hh\\:mm\\:ss}]";
 breaksHeader.Nodes.Add(new TreeNode(bText));
 }
 }
 root.Nodes.Add(breaksHeader);

 _tv.Nodes.Add(root);
 }
 _tv.EndUpdate();
 if (_tv.Nodes.Count >0) _tv.Nodes[0].Expand();
 }
}
