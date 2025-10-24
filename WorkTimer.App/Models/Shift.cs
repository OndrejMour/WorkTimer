using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace WorkTimer.App.Models;

public class Shift
{
 public DateTimeOffset Start { get; set; }
 public DateTimeOffset? End { get; set; }
 public TimeSpan Target { get; set; } = TimeSpan.FromHours(8.5);
 public BindingList<WorkSegment> Segments { get; set; } = new();
 public List<string> FinishedTasks { get; set; } = new();

 // New: explicit breaks independent of task activity
 public BindingList<BreakSegment> Breaks { get; set; } = new();

 // Time spent working on tasks (sum of segments)
 public TimeSpan WorkedTasks => TimeSpan.FromTicks(Segments.Sum(s => s.Duration.Ticks));

 // Total break time (includes running break)
 public TimeSpan TotalBreak => TimeSpan.FromTicks(Breaks.Sum(b => b.Duration.Ticks));

 // Worked shift time = total elapsed since start (or End) minus breaks
 public TimeSpan WorkedShift
 {
 get
 {
 var now = End ?? DateTimeOffset.Now;
 var elapsed = now - Start;
 var worked = elapsed - TotalBreak;
 return worked < TimeSpan.Zero ? TimeSpan.Zero : worked;
 }
 }

 // Remaining counts only worked shift time toward the target; break time extends the shift separately
 public TimeSpan Remaining
 {
 get
 {
 var remainingWork = Target > WorkedShift ? Target - WorkedShift : TimeSpan.Zero;
 return remainingWork;
 }
 }

 // Planned end = start + target + taken (and ongoing) breaks
 public DateTimeOffset PlannedEnd => Start + Target + TotalBreak;

 public void Resume(string? task = null, string? note = null)
 {
 Segments.Add(new WorkSegment { Start = DateTimeOffset.Now, Task = task, Note = note });
 }

 public void Pause()
 {
 var current = Segments.LastOrDefault(s => s.End == null);
 if (current != null) current.End = DateTimeOffset.Now;
 }

 public bool IsRunning => Segments.Any(s => s.End == null);

 // Break control
 public bool IsOnBreak => Breaks.Any(b => b.End == null);
 public void StartBreak()
 {
 if (IsOnBreak) return;
 // Pause all tasks
 Pause();
 Breaks.Add(new BreakSegment { Start = DateTimeOffset.Now });
 }
 public void EndBreak()
 {
 var b = Breaks.LastOrDefault(b => b.End == null);
 if (b != null) b.End = DateTimeOffset.Now;
 }
}
