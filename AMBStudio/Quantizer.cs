using System;
using System.Collections.Generic;
using System.Linq;

namespace AmtEditor
{
    static class Quantizer
    {
        public static byte[] Quantize(int[] image, int maxColors, out int[] palette)
        {
            Dictionary<int, int> colorUsage = new Dictionary<int, int>();

            for (int i = 0; i < image.Length; i++)
            {
                if (colorUsage.ContainsKey(image[i]))
                    colorUsage[image[i]]++;
                else
                    colorUsage.Add(image[i], 1);
            }

            SortedSet<ulong> colors = new SortedSet<ulong>();

            foreach (KeyValuePair<int, int> entry in colorUsage.OrderBy(x => x.Value))
            {
                colors.Add((uint)entry.Key | (ulong)entry.Value << 32);
            }

            while (colors.Count > maxColors)
            {
                ulong currentColorEntry = colors.First();
                colors.Remove(currentColorEntry);
                int currentColor = (int)currentColorEntry;

                ulong idealColorEntry = 0;
                int bestColorDistance = int.MaxValue;

                foreach (ulong entry in colors)
                {
                    int candidateColor = (int)entry;
                    int distance = Math.Abs(ColorSquaredDistance(currentColor, candidateColor));

                    if (bestColorDistance >= distance)
                    {
                        bestColorDistance = distance;
                        idealColorEntry = entry;
                    }
                    if (distance == 0) break;
                }

                colors.Remove(idealColorEntry);

                int idealColor = (int)idealColorEntry;
                const ulong high32Mask = ~0xffffffffUL;
                ulong diffSumHigh32 = (currentColorEntry & high32Mask) + (idealColorEntry & high32Mask);

                int mergedColorsUsage = (int)(diffSumHigh32 >> 32);
                int currentColorUsage = (int)(currentColorEntry >> 32);

                int lerpFactor = (mergedColorsUsage == 0) ? 0 : (currentColorUsage << 8) / mergedColorsUsage;

                int newColor = LerpPackedFixed8(currentColor, idealColor, lerpFactor);
                colors.Add((uint)newColor | diffSumHigh32);
            }

            palette = new int[colors.Count];
            int idx = colors.Count;
            foreach (ulong entry in colors)
            {
                palette[--idx] = (int)entry;
            }

            byte[] map = new byte[image.Length];

            for (idx = 0; idx < map.Length; idx++)
            {
                int idealPaletteIndex = 0;
                int bestColorDistance = int.MaxValue;

                for (int palIndex = 0; palIndex < palette.Length; palIndex++)
                {
                    int candidateColor = palette[palIndex];
                    int distance = Math.Abs(ColorSquaredDistance(image[idx], candidateColor));

                    if (bestColorDistance >= distance)
                    {
                        bestColorDistance = distance;
                        idealPaletteIndex = palIndex;
                    }
                    if (distance == 0) break;
                }
                map[idx] = (byte)idealPaletteIndex;
            }

            return map;
        }

        private static int LerpPackedFixed8(int c0, int c1, int factor)
        {
            int c0_0 = (c0 >> 0) & 0xff; int c0_1 = (c0 >> 8) & 0xff;
            int c0_2 = (c0 >> 16) & 0xff; int c0_3 = (c0 >> 24) & 0xff;

            int c1_0 = (c1 >> 0) & 0xff; int c1_1 = (c1 >> 8) & 0xff;
            int c1_2 = (c1 >> 16) & 0xff; int c1_3 = (c1 >> 24) & 0xff;

            return LerpFixed8(c0_0, c1_0, factor) << 0 |
                   LerpFixed8(c0_1, c1_1, factor) << 8 |
                   LerpFixed8(c0_2, c1_2, factor) << 16 |
                   LerpFixed8(c0_3, c1_3, factor) << 24;
        }

        private static int LerpFixed8(int c0, int c1, int factor)
        {
            return ((c0 * factor) >> 8) + ((c1 * (0x100 - factor)) >> 8);
        }

        private static int ColorSquaredDistance(int c0, int c1)
        {
            int c0_0 = (c0 >> 0) & 0xff; int c0_1 = (c0 >> 8) & 0xff;
            int c0_2 = (c0 >> 16) & 0xff; int c0_3 = (c0 >> 24) & 0xff;

            int c1_0 = (c1 >> 0) & 0xff; int c1_1 = (c1 >> 8) & 0xff;
            int c1_2 = (c1 >> 16) & 0xff; int c1_3 = (c1 >> 24) & 0xff;

            int d0 = c1_0 - c0_0; int d1 = c1_1 - c0_1;
            int d2 = c1_2 - c0_2; int d3 = c1_3 - c0_3;

            return d0 * d0 + d1 * d1 + d2 * d2 + d3 * d3;
        }
    }
}