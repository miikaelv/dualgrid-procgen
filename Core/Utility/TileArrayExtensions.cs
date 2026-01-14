using System.Runtime.CompilerServices;

namespace DualGrid.Core.Utility
{
    public static class TileArrayExtensions
    {
        // Tell JIT to inline for performance, while maintaining clean look in loop
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetPositionValueOrDefault(this bool[,] map, int x, int y, int width, int height)
        {
            // Cast to uint handles both the negative and the upper-bounds check
            if ((uint)x >= (uint)width || (uint)y >= (uint)height) return false;

            return map[x, y];
        }
    }
}