using System;

namespace WorkTimer.App.Models;

public class BreakSegment
{
 public DateTimeOffset Start { get; set; }
 public DateTimeOffset? End { get; set; }
 public TimeSpan Duration => (End ?? DateTimeOffset.Now) - Start;
}
