using System.Runtime.CompilerServices;

namespace DualGrid.Core.Utility
{
    public interface IDataMap
    {
        public int GetNoBoundsCheck(int x, int y);
        public void SetNoBoundsCheck(int value, int x, int y);
        public bool IsInBounds(int x, int y);
        public bool TryGet(int x, int y, out int value);
        public bool TrySet(int value, int x, int y);
    }

    /// <summary>
    /// Wrapper for 1D int array for easier access with coordinates.
    /// </summary>
    public class DataMap : IDataMap
    {
        public readonly int[] InternalArray;
        public readonly uint Width;
        public readonly uint Height;
        public readonly int Length;
        
        public DataMap(int[] array, int width, int height)
        {
            InternalArray = array;
            Length = array.Length;
            Width = (uint)width;
            Height = (uint)height;
        }
        
        public DataMap(bool[,] array, int width, int height)
        {
            Length = width * height;
            InternalArray = new int[Length];
            
            var i = 0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    InternalArray[i++] = array[x, y] ? 1 : 0;
                }
            }
            
            Width = (uint)width;
            Height = (uint)height;
        }
        
        public DataMap(int[,] array, int width, int height)
        {
            Length = width * height;
            InternalArray = new int[Length];
            
            var i = 0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    InternalArray[i++] = array[x, y];
                }
            }
            
            Width = (uint)width;
            Height = (uint)height;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNoBoundsCheck(int x, int y) => InternalArray[y * Width + x];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetNoBoundsCheck(int value, int x, int y) => InternalArray[y * Width + x] = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInBounds(int x, int y) => (uint)x < Width && (uint)y < Height;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(int x, int y, out int value)
        {
            if (!IsInBounds(x, y))
            {
                value = 0;
                return false;
            }

            value = GetNoBoundsCheck(x, y);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySet(int value, int x, int y)
        {
            if (!IsInBounds(x, y))
                return false;

            SetNoBoundsCheck(value, x, y);
            return true;
        }
    }
}