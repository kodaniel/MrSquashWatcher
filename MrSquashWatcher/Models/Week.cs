using System.Diagnostics.CodeAnalysis;

namespace MrSquashWatcher.Models;

public readonly struct Week : IComparable, IComparable<Week>, IEquatable<Week>
{
    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public static Week Now => new Week(DateTime.Now);

    public Week(DateOnly date)
    {
        StartDate = date.AddDays(-(int)date.DayOfWeek + 1);
        EndDate = StartDate.AddDays(6);
    }

    public Week(DateTime date) : this(new DateOnly(date.Year, date.Month, date.Day))
    {
    }



    public bool IsInWeek(DateOnly date) => date >= StartDate && date < EndDate;

    public Week AddWeeks(int weeks) => new Week(StartDate.AddDays(weeks * 7));

    public int CompareTo(Week value) => StartDate.CompareTo(value.StartDate);

    public int CompareTo(object value)
    {
        if (value == null) return 1;
        if (value is not Week week)
        {
            throw new ArgumentException();
        }

        return CompareTo(week);
    }

    public bool Equals(Week other) => StartDate == other.StartDate;

    public override bool Equals([NotNullWhen(true)] object value) => value is Week week && week.StartDate == StartDate;

    public override int GetHashCode() => StartDate.GetHashCode();

    public static bool operator ==(Week left, Week right) => left.Equals(right);

    public static bool operator !=(Week left, Week right) => !(left == right);

    public static bool operator <(Week left, Week right) => left.CompareTo(right) < 0;

    public static bool operator <=(Week left, Week right) => left.CompareTo(right) <= 0;

    public static bool operator >(Week left, Week right) => left.CompareTo(right) > 0;

    public static bool operator >=(Week left, Week right) => left.CompareTo(right) >= 0;

    public static Week operator ++(Week week) => week + 1;

    public static Week operator --(Week week) => week - 1;

    public static Week operator +(Week week, int value) => week.AddWeeks(value);

    public static Week operator -(Week week, int value) => week.AddWeeks(-value);
}
