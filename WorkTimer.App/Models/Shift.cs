using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace WorkTimer.App.Models;

public class Shift
{
 public DateTimeOffset Start { get; set; }
 public TimeSpan Target { get; set; } = TimeSpan.FromHours(8.5);
 public BindingList<WorkSegment> Segments { get; } = new();

 public TimeSpan Worked => TimeSpan.FromTicks(Segments.Sum(s => s.Duration.Ticks));
 public TimeSpan Remaining => Target > Worked ? Target - Worked : TimeSpan.Zero;
 public DateTimeOffset PlannedEnd => Start + (DateTimeOffset.Now - Start) + (Remaining - (DateTimeOffset.Now - Start - Worked));

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
}
