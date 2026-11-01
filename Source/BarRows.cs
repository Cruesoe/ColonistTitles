using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ColonistTitles;

/// <summary>Finds the top of the next row on a colonist bar. Row tops are gathered once per frame.</summary>
public static class BarRows
{
    private static readonly List<float> rowTops = new List<float>();
    private static int rowTopsFrame = -1;

    /// <summary>Top of the first row below y, or null on the last row.</summary>
    public static float? NextRowTop(List<Vector2> locs, float y)
    {
        if (StartFrame())
        {
            for (int i = 0; i < locs.Count; i++)
            {
                AddRow(locs[i].y);
            }
        }

        return NextBelow(y);
    }

    /// <summary>Top of the first row below y, or null on the last row.</summary>
    public static float? NextRowTop(List<Rect> locs, float y)
    {
        if (StartFrame())
        {
            for (int i = 0; i < locs.Count; i++)
            {
                AddRow(locs[i].y);
            }
        }

        return NextBelow(y);
    }

    private static bool StartFrame()
    {
        if (rowTopsFrame == RealTime.frameCount)
        {
            return false;
        }

        rowTopsFrame = RealTime.frameCount;
        rowTops.Clear();
        return true;
    }

    private static void AddRow(float top)
    {
        if (!rowTops.Contains(top))
        {
            rowTops.Add(top);
        }
    }

    private static float? NextBelow(float y)
    {
        float? next = null;
        for (int i = 0; i < rowTops.Count; i++)
        {
            if (rowTops[i] > y + 1f && (next == null || rowTops[i] < next.Value))
            {
                next = rowTops[i];
            }
        }

        return next;
    }
}
