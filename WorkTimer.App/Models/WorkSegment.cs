using System;

namespace WorkTimer.App.Models;

public class WorkSegment
{
 public DateTimeOffset Start { get; set; }
 public DateTimeOffset? End { get; set; }
 public string? Task { get; set; }
 public string? Note { get; set; }

 public TimeSpan Duration => (End ?? DateTimeOffset.Now) - Start;
}
