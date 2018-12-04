using System;

public struct DateTimeTicks
{
    public long ticks;
    public static explicit operator DateTime(DateTimeTicks dte) => new DateTime(dte.ticks, DateTimeKind.Utc);
    public static explicit operator DateTimeTicks(DateTime dt) => new DateTimeTicks { ticks = dt.Ticks };
    public static DateTimeTicks Now() => (DateTimeTicks)DateTime.UtcNow;
}
