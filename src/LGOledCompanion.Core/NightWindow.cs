// NightWindow.cs
// Top 5: IsActive O(1), Parse O(1)

namespace LGOledCompanion.Core;

public readonly record struct NightWindow(TimeOnly Start, TimeOnly End)
{
    public static NightWindow Parse(string start, string end)
    {
        return new NightWindow(TimeOnly.Parse(start), TimeOnly.Parse(end));
    }

    public bool IsActive(TimeOnly now)
    {
        if (Start == End)
        {
            return false;
        }

        if (Start < End)
        {
            return now >= Start && now < End;
        }

        return now >= Start || now < End;
    }
}
