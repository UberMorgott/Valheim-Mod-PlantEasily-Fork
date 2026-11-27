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

    [Fact]
    public void AffordableMatchesALinearScanWithFewChecks()
    {
        for (int max = 0; max <= 40; max++)
            for (int stock = 0; stock <= max + 2; stock++)
            {
                AffordProbe probe = new(stock);
                int found = PressCost.Affordable(max, probe.CanAfford);
                Assert.Equal(System.Math.Min(stock, max), found);
                Assert.True(probe.Calls <= 7, $"max {max} stock {stock}: {probe.Calls} checks");
            }
    }

    [Fact]
    public void AffordableFullGridIsOneCheck()
    {
        AffordProbe probe = new(0);
        Assert.Equal(500, PressCost.Affordable(500, probe.AlwaysAfford));
        Assert.Equal(1, probe.Calls);
    }

    private sealed class AffordProbe(int stock)
    {
        public int Calls;

        public bool CanAfford(int n)
        {
            Calls++;
            return n <= stock;
        }

        public bool AlwaysAfford(int _)
        {
            Calls++;
            return true;
        }
    }
}
