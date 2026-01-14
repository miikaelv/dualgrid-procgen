using System.Runtime.CompilerServices;
using UnityEngine;

namespace DualGrid.Core.Utility
{
    public class TileRuleLookup
    {
        private readonly int[] TileIndicesByBitmask;
        private readonly int[] TileRuleBitmasksByIndex;

        public TileRuleLookup()
        {
            // Store rule bitmask for each index of tiles in a dual grid sprite sheet
            TileIndicesByBitmask = new int[16];
            TileRuleBitmasksByIndex = new int[16];

            BuildRuleArrays(DefaultRuleTemplates);
        }

        public TileRuleLookup((bool, bool, bool, bool)[] ruleTemplates)
        {
            if (ruleTemplates.Length != 16)
            {
                Debug.LogError($"{nameof(TileRuleLookup)} only supports sprite sheets with exactly 16 elements.");
                return;
            }

            TileIndicesByBitmask = new int[16];
            TileRuleBitmasksByIndex = new int[16];

            BuildRuleArrays(ruleTemplates);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BuildRuleArrays((bool, bool, bool, bool)[] ruleTemplates)
        {
            for (var i = 0; i < 16; i++)
            {
                var rule = ruleTemplates[i];
                var mask = BuildTileRuleBitmask(rule.Item1, rule.Item2, rule.Item3, rule.Item4);

                TileIndicesByBitmask[mask] = i;
                TileRuleBitmasksByIndex[i] = mask;
            }
        }

        /// <summary>
        /// Build 4 bit bitmask of the 4 booleans passed in.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BuildTileRuleBitmask(bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
        {
            return (topLeft ? 1 : 0) | (topRight ? 2 : 0) | (bottomLeft ? 4 : 0) | (bottomRight ? 8 : 0);
        }

        /// <summary>
        /// Gets the correct Render tile index based on neighbour rules.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRenderTileIndexByNeighbours(bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
        {
            return TileIndicesByBitmask[BuildTileRuleBitmask(topLeft, topRight, bottomLeft, bottomRight)];
        }

        /// <summary>
        /// Gets the correct Render tile index based on neighbour rules (4 booleans packaged into a bitmask).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetRenderTileIndexByRuleBitmask(int bitmask, out int tile)
        {
            if ((uint)bitmask >= (uint)TileIndicesByBitmask.Length)
            {
                tile = 0;
                return false;
            }

            tile = TileIndicesByBitmask[bitmask];
            return true;
        }

        /// <summary>
        /// Gets the correct Render tile index based on neighbour rules (4 booleans packaged into a bitmask).
        /// Beware possible IndexOutOfRangeException if you are not sure of the data passed in.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRenderTileIndexNoBoundsCheck(int bitmask) => TileIndicesByBitmask[bitmask];

        /// <summary>
        /// Gets the neighbour tile rules of the render tile in the index passed in.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetRulesByTileIndex(int index,
            out (bool topLeft, bool topRight, bool bottomLeft, bool bottomRight) rules)
        {
            if ((uint)index >= (uint)TileRuleBitmasksByIndex.Length)
            {
                rules = default;

                return false;
            }

            var bitmask = TileRuleBitmasksByIndex[index];
            rules = ((bitmask & 1) != 0,
                (bitmask & 2) != 0,
                (bitmask & 4) != 0,
                (bitmask & 8) != 0);

            return true;
        }


        /// <summary>
        /// Template for Tile Neighbour rules in 4x4 dual grid sprite sheet.
        /// Index 0 starting from bottom left of sprite sheet going right and up.
        /// Booleans are the neighbours in order: topLeft, topRight, bottomLeft, bottomRight
        /// </summary>
        private static readonly (bool, bool, bool, bool)[] DefaultRuleTemplates =
        {
            (false, false, false, false), // Index 0
            (false, false, false, true), // Index 1
            (false, true, true, false), // Index 2
            (true, false, false, false), // Index 3

            (false, true, false, false), // Index 4
            (true, true, false, false), // Index 5
            (true, true, false, true), // Index 6
            (true, false, true, false), // Index 7

            (true, false, false, true), // Index 8
            (false, true, true, true), // Index 9
            (true, true, true, true), // Index 10
            (true, true, true, false), // Index 11

            (false, false, true, false), // Index 12
            (false, true, false, true), // Index 13
            (true, false, true, true), // Index 14
            (false, false, true, true), // Index 15
        };
    }
}