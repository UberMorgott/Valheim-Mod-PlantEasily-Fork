namespace Advize_PlantEasily;

// Pure rule (no Unity references): compiled into Advize_PlantEasily.Tests as well.
internal static class GhostRefresh
{
    /// <summary>Ghost updates in one GhostGrid.Update: a placement press re-evaluates every ghost (root + extras) before
    /// PlacePiece reads their statuses; otherwise at most the configured batch per frame.</summary>
    internal static int UpdatesThisFrame(bool press, int batchSize, int totalGhosts) =>
        press ? totalGhosts : System.Math.Min(batchSize, totalGhosts);
}
