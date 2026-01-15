using UnityEngine;

namespace DualGrid.Core.Utility
{
    public static class ShaderUtility
    {
        public static ComputeBuffer CreateStructuredBuffer(int[] data)
        {
            var dataBuffer = new ComputeBuffer(data.Length, sizeof(int));
            dataBuffer.SetData(data);

            return dataBuffer;
        }
        
        public static ComputeBuffer CreateStructuredBuffer(DataMap data)
        {
            var dataBuffer = new ComputeBuffer(data.Length, sizeof(int));
            dataBuffer.SetData(data.InternalArray);

            return dataBuffer;
        }
        
        /// <summary>
        /// Creates a Texture2D from an 1D Array of tile indices. 
        /// Each tile index is stored as a single byte in a red channel texture.
        /// </summary>
        public static Texture2D CreateDataTexture(int[] data, int width, int height)
        {
            // TextureFormat.R8: single-channel 8-bit texture (red channel only)
            // mipChain: false (no mipmaps)
            // linear: true (use linear color space)
            var tex = new Texture2D(width, height, TextureFormat.R8, false, true)
            {
                // Set the filter mode to Point (no smoothing) since this is data, not a visual texture
                filterMode = FilterMode.Point,

                // Clamp texture coordinates outside [0,1] to the edge pixel
                wrapMode = TextureWrapMode.Clamp
            };

            // Set the data directly into the texture as bytes
            var textureData = tex.GetRawTextureData<byte>();
            for (var i = 0; i < data.Length; i++)
            {
                textureData[i] = (byte)data[i];
            }

            tex.Apply();
            return tex;
        }
    }
}