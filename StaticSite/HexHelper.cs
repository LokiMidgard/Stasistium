using System;
using System.Collections.Generic;
using System.Text;

namespace StaticSite
{
    internal static class HexHelper
    {

        // values for '\0' to 'f' where 255 indicates invalid input character
        // starting from '\0' and not from '0' costs 48 bytes
        // but results 0 subtructions and less if conditions
        static readonly byte[] fromHexTable = new byte[] {
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 0, 1,
            2, 3, 4, 5, 6, 7, 8, 9, 255, 255,
            255, 255, 255, 255, 255, 10, 11, 12, 13, 14,
            15, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 10, 11, 12,
            13, 14, 15
        };

        // same as above but valid values are multiplied by 16
        static readonly byte[] fromHexTable16 = new byte[] {
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 0, 16,
            32, 48, 64, 80, 96, 112, 128, 144, 255, 255,
            255, 255, 255, 255, 255, 160, 176, 192, 208, 224,
            240, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 160, 176, 192,
            208, 224, 240
        };

        public static byte[] FromHexString(string source)
        {
            // return an empty array in case of null or empty source
            if (string.IsNullOrEmpty(source))
                return Array.Empty<byte>(); // you may change it to return null
            if ((source.Length & 1) == 1) // source length must be even
                throw new ArgumentException();
            var resultLeng = source.Length >> 1; 
            
            
            
            byte[] result = new byte[resultLeng]; // initialization of result for known length

            for (int i = 0; i < result.Length; i++)
            {
                ref var point = ref result[i];
                point = fromHexTable[source[i]];
                point |= fromHexTable16[source[i + 1]];
            }

            return result;
        }

    }
}
