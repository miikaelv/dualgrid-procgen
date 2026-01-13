namespace DualGrid.Core.Utility
{
    public class TileRuleLookup
    {
        private readonly int[] TileIndexesByBitmask;

        public TileRuleLookup()
        {
            TileIndexesByBitmask = new int[16];

            for (var i = 0; i < 16; i++)
            {
                var rule = RuleTemplates3[i];
                var mask = 0;

                // Combine booleans into one integer using a Bitmask
                mask |= rule.Item1 ? 1 << 0 : 0; // TopLeft
                mask |= rule.Item2 ? 1 << 1 : 0; // TopRight
                mask |= rule.Item3 ? 1 << 2 : 0; // BottomLeft
                mask |= rule.Item4 ? 1 << 3 : 0; // BottomRight

                TileIndexesByBitmask[mask] = i;
            }
        }
        
        public bool TryGetTileIndexByRules(bool topLeft, bool topRight, bool bottomLeft, bool bottomRight, out int tile)
        {
            var bitmask = 0;
            bitmask |= topLeft ? 1 << 0 : 0; 
            bitmask |= topRight ? 1 << 1 : 0;
            bitmask |= bottomLeft ? 1 << 2 : 0;
            bitmask |= bottomRight ? 1 << 3 : 0;
            
            tile = TileIndexesByBitmask[bitmask];
            return true;
        }

        public bool TryGetTileIndexByBitmask(int bitmask, out int tile)
        {
            if (bitmask >= TileIndexesByBitmask.Length || bitmask < 0)
            {
                tile = -1;
                return false;
            }

            tile = TileIndexesByBitmask[bitmask];
            return true;
        }

        // Index 0 starting from bottom left of sprite sheet going right and up.
        // Booleans are in order: topLeft, topRight, bottomLeft, bottomRight
        private static readonly (bool, bool, bool, bool)[] RuleTemplates3 =
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