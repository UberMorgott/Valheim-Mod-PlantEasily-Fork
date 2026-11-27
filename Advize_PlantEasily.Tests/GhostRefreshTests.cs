using System.Linq;
using Advize_PlantEasily;
using Xunit;

namespace Advize_PlantEasily.Tests;

public class GhostRefreshTests
{
    // GhostGrid.Update visits ghost (cursor++ % total) UpdatesThisFrame times.
    private static int[] Visited(bool press, int batch, int total, int cursor)
    {
        int count = GhostRefresh.UpdatesThisFrame(press, batch, total);
        int[] visited = new int[count];
        for (int i = 0; i < count; i++)
            visited[i] = (cursor + i) % total;
        return visited;
    }

    [Theory]
    [InlineData(4, 20, 0)]
    [InlineData(4, 20, 7)]
    [InlineData(500, 20, 13)] // grid larger than the per-frame batch
    [InlineData(500, 1, 499)]
    public void Press_evaluates_every_ghost_once(int total, int batch, int cursor)
    {
        var seen = Visited(true, batch, total, cursor);
        Assert.Equal(Enumerable.Range(0, total), seen.OrderBy(i => i));
    }

    [Theory]
    [InlineData(4, 20, 4)]
    [InlineData(500, 20, 20)]
    [InlineData(1, 20, 1)]
    public void Frame_update_stays_within_batch(int total, int batch, int want) =>
        Assert.Equal(want, GhostRefresh.UpdatesThisFrame(false, batch, total));
}
