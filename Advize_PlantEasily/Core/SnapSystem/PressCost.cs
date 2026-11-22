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
}
