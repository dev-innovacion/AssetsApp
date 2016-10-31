using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.IO.Compression;
namespace Rivka.Base64
{
    public static class Base64
    {
        /// <summary> Compress a string </summary>
        /// <param name=”inputText”> The input text. </param>
        /// <returns> Base64 encoded compressed text </returns>
        public static string Compress(string inputText)
        {
            return Compress(System.Text.Encoding.UTF8.GetBytes(inputText));
        }

        /// <summary> Compress a string </summary>
        /// <param name=”inputBytes”> The input text. </param>
        /// <returns> Base64 encoded compressed text </returns>
        public static string Compress(byte[] inputBytes)
        {
            byte[] compressed;

            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress))
                {
                    zip.Write(inputBytes, 0, inputBytes.Length);
                    zip.Close();

                    compressed = ms.ToArray();
                }
            }

            return Convert.ToBase64String(compressed);
        }

        /// <summary> Decompress the given compressedText. </summary>
        /// <param name=”compressedText”> Base64 encoded compressed text </param>
        /// <returns> decompressed text </returns>
        public static string DecompressText(string compressedText)
        {
            return System.Text.Encoding.UTF8.GetString(DecompressBytes(compressedText));
        }

        /// <summary> Decompress the given compressedText. </summary>
        /// <param name=”compressedText”> Base64 encoded compressed text </param>
        /// <returns> decompressed byte array </returns>
        public static byte[] DecompressBytes(string compressedText)
        {
            byte[] bytes = Convert.FromBase64String(compressedText);
            byte[] outputBytes;

            using (MemoryStream inputStream = new MemoryStream(bytes))
            {
                using (GZipStream zip = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    using (MemoryStream outputStream = new MemoryStream())
                    {
                        zip.CopyTo(outputStream);

                        outputBytes = outputStream.ToArray();
                    }
                }
            }

            return outputBytes;
        }
    }
}