namespace Advize_PlantEasily;

using System.Collections.Generic;

// Pure rule (no Unity references): compiled into Advize_PlantEasily.Tests as well.
internal static class PressCost
{
    /// <summary>An extra ghost PlacePiece plants (and charges for, unless the player builds for free).</summary>
    internal static bool CanPlant(Status status, bool noPlacementCost, bool preventInvalidPlanting) =>
        status == Status.Healthy ||
        (status == Status.LackResources && noPlacementCost) ||
        (!preventInvalidPlanting && status > Status.LackResources);

    /// <summary>Pieces one press plants: the root plus every plantable extra (statuses[0] = root, extras from 1).</summary>
    internal static int Pieces(IReadOnlyList<Status> statuses, int maxExtras, bool preventInvalidPlanting)
    {
        int pieces = 1;
        for (int i = 1; i <= maxExtras && i < statuses.Count; i++)
            if (CanPlant(statuses[i], noPlacementCost: false, preventInvalidPlanting))
                pieces++;
        return pieces;
    }

    /// <summary>Largest piece count in [0, max] the player can pay for, given a cost check that holds up to some count
    /// (paying for n pieces implies paying for fewer): the full count first, then a binary search, so a grid costs
    /// O(log n) requirement checks (each may scan chests: V+ / Morgott.Patcher StationNetwork) instead of one per ghost.</summary>
    internal static int Affordable(int max, System.Func<int, bool> canAfford)
    {
        if (max <= 0 || canAfford(max))
            return System.Math.Max(0, max);
        int lo = 0, hi = max - 1;
        while (lo < hi)
        {
            int mid = lo + (hi - lo + 1) / 2;
            if (canAfford(mid))
                lo = mid;
            else
                hi = mid - 1;
        }
        return lo;
    }
}
