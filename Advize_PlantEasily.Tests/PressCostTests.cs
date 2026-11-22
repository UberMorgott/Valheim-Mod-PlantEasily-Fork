namespace Advize_PlantEasily.Tests;

using Xunit;

public class PressCostTests
{
    [Fact]
    public void FullHealthyGridCostsEveryPiece() =>
        Assert.Equal(4, PressCost.Pieces([Status.Healthy, Status.Healthy, Status.Healthy, Status.Healthy], 3, preventInvalidPlanting: true));

    [Fact]
    public void ExtrasShortOfSeedsAreNotCharged() =>
        Assert.Equal(2, PressCost.Pieces([Status.Healthy, Status.Healthy, Status.LackResources, Status.LackResources], 3, preventInvalidPlanting: true));

    [Fact]
    public void InvalidExtrasCostOnlyWhenPlantedAnyway()
    {
        Status[] grid = [Status.Healthy, Status.NoSpace, Status.Healthy];
        Assert.Equal(2, PressCost.Pieces(grid, 2, preventInvalidPlanting: true));
        Assert.Equal(3, PressCost.Pieces(grid, 2, preventInvalidPlanting: false));
    }

    [Fact]
    public void OnlyActiveExtrasCount() =>
        Assert.Equal(2, PressCost.Pieces([Status.Healthy, Status.Healthy, Status.Healthy], 1, preventInvalidPlanting: true));

    [Fact]
    public void LackResourcesPlantsOnlyForFreeBuilders()
    {
        Assert.False(PressCost.CanPlant(Status.LackResources, noPlacementCost: false, preventInvalidPlanting: true));
        Assert.True(PressCost.CanPlant(Status.LackResources, noPlacementCost: true, preventInvalidPlanting: true));
    }
}
