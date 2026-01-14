namespace DualGrid.Core.Utility
{
    public class TileRuleLookup
    {
        private readonly int[] TileIndicesByBitmask;

        public TileRuleLookup()
        {
            // Store rule bitmask for each index of tiles in a dual grid sprite sheet
            TileIndicesByBitmask = new int[16];

            for (var i = 0; i < 16; i++)
            {
                var rule = RuleTemplates[i];
                var mask = 0;

                // Combine booleans into one integer using a Bitmask
                mask |= rule.Item1 ? 1 << 0 : 0; // TopLeft
                mask |= rule.Item2 ? 1 << 1 : 0; // TopRight
                mask |= rule.Item3 ? 1 << 2 : 0; // BottomLeft
                mask |= rule.Item4 ? 1 << 3 : 0; // BottomRight

                TileIndicesByBitmask[mask] = i;
            }
        }
        
        public int GetRenderTileIndexByNeighbours(bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
        {
            var bitmask = 0;
            bitmask |= topLeft ? 1 << 0 : 0; 
            bitmask |= topRight ? 1 << 1 : 0;
            bitmask |= bottomLeft ? 1 << 2 : 0;
            bitmask |= bottomRight ? 1 << 3 : 0;
            
            return TileIndicesByBitmask[bitmask];
        }

        public bool TryGetRenderTileIndexByRuleBitmask(int bitmask, out int tile)
        {
            if (bitmask >= TileIndicesByBitmask.Length || bitmask < 0)
            {
                tile = 0;
                return false;
            }

            tile = TileIndicesByBitmask[bitmask];
            return true;
        }

        // Index 0 starting from bottom left of sprite sheet going right and up.
        // Booleans are the neighbours in order: topLeft, topRight, bottomLeft, bottomRight
        private static readonly (bool, bool, bool, bool)[] RuleTemplates =
        {
            (false, false, false, false), // Index 0
            (false, false, false, true),  // Index 1
            (false, true, true, false),   // Index 2
            (true, false, false, false),  // Index 3

            (false, true, false, false),  // Index 4
            (true, true, false, false),   // Index 5
            (true, true, false, true),    // Index 6
            (true, false, true, false),   // Index 7

            (true, false, false, true),   // Index 8
            (false, true, true, true),    // Index 9
            (true, true, true, true),     // Index 10
            (true, true, true, false),    // Index 11

            (false, false, true, false),  // Index 12
            (false, true, false, true),   // Index 13
            (true, false, true, true),    // Index 14
            (false, false, true, true),   // Index 15
        };
    }
}