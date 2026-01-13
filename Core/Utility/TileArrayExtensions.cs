namespace DualGrid.Core.Utility
{
    public static class TileArrayExtensions
    {
        public static bool HasPosition(this bool[,] map, int x, int y, int width, int height)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return false;

            return map[x, y];
        }
    }
}