using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AmtEditor
{
    // Estrutura para representar a Parte 4 (Array Info) + Parte 5 (Meta) + Parte 6 (Dados)
    public class AmtEntry
    {
        // Identificador se esta entrada é um espaço vazio (ponteiro 00 00 00 00)
        public bool IsDummy { get; set; } = false;

        // --- Parte 4: Arrays (48 bytes / 0x30) ---
        public int Id { get; set; }
        public byte[] Unk1_4Bytes { get; set; } = new byte[4]; // 0x04

        // Flag de Entrelaçamento: 0x13 = Tiled Palette + Linear Pixels
        public int InterlaceFlag { get; set; } // 0x08 (Originalmente Unk2)
        public byte[] Unk3_2Bytes { get; set; } = new byte[2]; // 0x0C
        public byte[] Unk4_2Bytes { get; set; } = new byte[2]; // 0x0E
        public ushort Width { get; set; }  // 0x10
        public ushort Height { get; set; } // 0x12

        public uint ImageMetaOffset { get; set; } // 0x14
        public uint ImageSize { get; set; }       // 0x18

        public byte[] Unk5_4Bytes { get; set; } = new byte[4]; // 0x1C
        public byte[] Unk6_4Bytes { get; set; } = new byte[4]; // 0x20

        public uint PaletteMetaOffset { get; set; } // 0x24
        public uint PaletteSize { get; set; }       // 0x28 (0x40 = 16 cores, 0x400 = 256 cores)

        public byte[] Unk7_4Bytes { get; set; } = new byte[4]; // 0x2C

        // --- Parte 5: Meta Dados da Imagem (32 bytes) ---
        // Lidos a partir de ImageMetaOffset
        public byte[] MetaUnk1_4Bytes { get; set; } = new byte[4];
        public byte[] MetaUnk2_4Bytes { get; set; } = new byte[4];
        public byte[] MetaUnk3_4Bytes { get; set; } = new byte[4];
        public byte[] MetaUnk4_4Bytes { get; set; } = new byte[4]; // 00 00 00 08
        public byte[] MetaUnk5_4Bytes { get; set; } = new byte[4];
        public byte[] MetaUnk6_4Bytes { get; set; } = new byte[4];

        // --- Parte 6: Dados Brutos ---
        public byte[] ImageRawData { get; set; }
        public byte[] PaletteRawData { get; set; }

        public string OverlayText { get; set; } = "";

        public Bitmap ToBitmap()
        {
            if (IsDummy || ImageRawData == null || ImageRawData.Length == 0) return null;

            // Determina BPP baseado no tamanho da paleta ou flag
            // 0x40 bytes = 16 cores * 4 bytes -> 4bpp
            // 0x400 bytes = 256 cores * 4 bytes -> 8bpp
            bool is4bpp = (PaletteSize == 0x40) || (PaletteRawData != null && PaletteRawData.Length <= 64);

            // Validação de segurança caso PaletteSize esteja 0
            if (PaletteSize == 0 && ImageRawData.Length == (Width * Height) / 2) is4bpp = true;

            // 1. Processar Dados (Unswizzle se necessário)
            byte[] processedData;

            // Tratamos como Linear (cópia direta)
            processedData = (byte[])ImageRawData.Clone();

            PixelFormat format = is4bpp ? PixelFormat.Format4bppIndexed : PixelFormat.Format8bppIndexed;
            Bitmap bmp = new Bitmap(Width, Height, format);

            // 2. Aplicar Paleta
            if (PaletteRawData != null && PaletteRawData.Length > 0)
            {
                // Imagens com flag 0x13 ou 8bpp usam paleta TILED (Swizzled).
                byte[] effectivePalette = PaletteRawData;
                bool isTiledPalette = (InterlaceFlag == 0x13) || (!is4bpp);

                if (isTiledPalette)
                {
                    // Unswizzle necessário para cores corretas (Isso estava certo e foi mantido)
                    effectivePalette = SwizzlePalette(PaletteRawData);
                }

                ColorPalette pal = bmp.Palette;
                int maxColors = is4bpp ? 16 : 256;
                int colorCount = Math.Min(maxColors, effectivePalette.Length / 4);

                for (int i = 0; i < colorCount; i++)
                {
                    byte r = effectivePalette[i * 4 + 0];
                    byte g = effectivePalette[i * 4 + 1];
                    byte b = effectivePalette[i * 4 + 2];
                    byte a = effectivePalette[i * 4 + 3];

                    pal.Entries[i] = Color.FromArgb(a, r, g, b);
                }
                bmp.Palette = pal;
            }

            // 3. Copiar pixels para o Bitmap
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, format);

            try
            {
                // para evitar o efeito de esticamento em resoluções não múltiplas de 8.
                int rawRowBytes = is4bpp ? (Width + 1) / 2 : Width;
                byte[] finalData = new byte[bmpData.Stride * Height];

                if (is4bpp)
                {
                    // Swap de Nibbles (Windows vs PS2) processado linha a linha
                    for (int y = 0; y < Height; y++)
                    {
                        int srcOffset = y * rawRowBytes;
                        int dstOffset = y * bmpData.Stride;

                        for (int x = 0; x < rawRowBytes; x++)
                        {
                            if (srcOffset + x < processedData.Length)
                            {
                                byte b = processedData[srcOffset + x];
                                byte low = (byte)(b & 0x0F);
                                byte high = (byte)((b & 0xF0) >> 4);
                                finalData[dstOffset + x] = (byte)((low << 4) | high);
                            }
                        }
                    }
                    Marshal.Copy(finalData, 0, bmpData.Scan0, finalData.Length);
                }
                else
                {
                    // Processado linha a linha para 8bpp
                    for (int y = 0; y < Height; y++)
                    {
                        int srcOffset = y * rawRowBytes;
                        int dstOffset = y * bmpData.Stride;

                        int bytesToCopy = Math.Min(rawRowBytes, processedData.Length - srcOffset);
                        if (bytesToCopy > 0)
                        {
                            Array.Copy(processedData, srcOffset, finalData, dstOffset, bytesToCopy);
                        }
                    }
                    Marshal.Copy(finalData, 0, bmpData.Scan0, finalData.Length);
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        public void ImportBitmap(Bitmap bmp)
        {
            if (IsDummy) return; // Segurança

            Width = (ushort)bmp.Width;
            Height = (ushort)bmp.Height;

            bool isIndexed = (bmp.PixelFormat == PixelFormat.Format4bppIndexed ||
                              bmp.PixelFormat == PixelFormat.Format8bppIndexed);

            byte[] linearData;
            byte[] rawPalette;
            bool targetIs4bpp;

            if (isIndexed)
            {
                // --- Importação Direta (Indexada) ---

                // Copiar Paleta
                List<byte> palBytes = new List<byte>();
                foreach (var c in bmp.Palette.Entries)
                {
                    palBytes.Add(c.R);
                    palBytes.Add(c.G);
                    palBytes.Add(c.B);
                    // Contract Alpha (reverso do ExpandAlpha)
                    // valor == 0xff ? 0x80 : (valor >> 1)
                    byte a = ContractAlpha(c.A);
                    palBytes.Add(a);
                }
                rawPalette = palBytes.ToArray();

                // Detectar se é 4bpp
                targetIs4bpp = (bmp.PixelFormat == PixelFormat.Format4bppIndexed);

                // Ler os índices
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                int stride = data.Stride;
                int bytesLen = stride * bmp.Height;
                byte[] tempRaw = new byte[bytesLen];
                Marshal.Copy(data.Scan0, tempRaw, 0, bytesLen);
                bmp.UnlockBits(data);

                if (targetIs4bpp)
                {
                    // Precisamos reverter o Nibble Swap do Windows para o formato do Arquivo
                    // Windows: (P1 << 4) | P2.  Arquivo: (P2 << 4) | P1.
                    linearData = new byte[(Width * Height) / 2];
                    int outIdx = 0;
                    int rowBytes = (Width + 1) / 2;

                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < rowBytes; x++)
                        {
                            byte b = tempRaw[y * stride + x];
                            byte p1 = (byte)((b & 0xF0) >> 4);
                            byte p2 = (byte)(b & 0x0F);

                            // Swap para formato arquivo
                            linearData[outIdx++] = (byte)((p2 << 4) | p1);
                        }
                    }
                }
                else
                {
                    // 8bpp - Apenas remover padding se houver
                    if (stride != Width)
                    {
                        linearData = new byte[Width * Height];
                        for (int y = 0; y < Height; y++)
                        {
                            Array.Copy(tempRaw, y * stride, linearData, y * Width, Width);
                        }
                    }
                    else
                    {
                        linearData = tempRaw;
                    }
                }
            }
            else
            {
                // --- Quantização (TrueColor) ---
                byte[] rgbaBuffer = GetBitmapBuffer(bmp);
                int[] pixelData = PackColors(rgbaBuffer);

                // 2. Decidir quantidade de cores (16 ou 256)
                // Se o arquivo original tinha 16 cores (0x40), tentamos manter 16.
                // Se não, usamos 256.
                int targetColors = (PaletteSize == 0x40) ? 16 : 256;
                targetIs4bpp = (targetColors == 16);

                int[] paletteInts;
                // Chama a classe Quantizer fornecida
                byte[] quantizedIndices = AmtEditor.Quantizer.Quantize(pixelData, targetColors, out paletteInts);

                // 3. Converter Paleta int[] de volta para byte[] (RGBA + Alpha Contract)
                List<byte> palBytes = new List<byte>();

                // Preenche paleta até o tamanho alvo (16 ou 256)
                for (int i = 0; i < targetColors; i++)
                {
                    if (i < paletteInts.Length)
                    {
                        int c = paletteInts[i];
                        byte r = (byte)((c >> 16) & 0xFF);
                        byte g = (byte)((c >> 8) & 0xFF);
                        byte b = (byte)((c >> 0) & 0xFF);
                        byte a = (byte)((c >> 24) & 0xFF);
                        palBytes.Add(r); palBytes.Add(g); palBytes.Add(b); palBytes.Add(a);
                    }
                    else
                    {
                        palBytes.Add(0); palBytes.Add(0); palBytes.Add(0); palBytes.Add(0);
                    }
                }
                rawPalette = palBytes.ToArray();

                // 4. Formatar dados de imagem (Linear)
                if (targetIs4bpp)
                {
                    // EncodeI4 do Program.cs
                    linearData = EncodeI4(quantizedIndices);
                }
                else
                {
                    linearData = quantizedIndices;
                }
            }

            bool targetIsTiled = (InterlaceFlag == 0x13) || (!targetIs4bpp);

            if (targetIsTiled)
            {
                // Converte a paleta linear do Bitmap para o formato Tiled do arquivo
                PaletteRawData = SwizzlePalette(rawPalette);
            }
            else
            {
                PaletteRawData = rawPalette;
            }

            PaletteSize = (uint)PaletteRawData.Length;

            ImageRawData = linearData;
        }

        // Helpers (Mantidos iguais)
        private byte[] GetBitmapBuffer(Bitmap img)
        {
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);
            // Forçamos leitura como 32bpp para o PackColors funcionar
            using (Bitmap temp = img.Clone(rect, PixelFormat.Format32bppArgb))
            {
                BitmapData imgData = temp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte[] output = new byte[imgData.Stride * temp.Height];
                Marshal.Copy(imgData.Scan0, output, 0, output.Length);
                temp.UnlockBits(imgData);
                return output;
            }
        }

        private int[] PackColors(byte[] buffer)
        {
            int[] output = new int[buffer.Length / 4];
            for (int i = 0; i < output.Length; i++)
            {
                int ioffs = i * 4;
                // Windows Bitmap 32bppArgb é BGRA na memória (Little Endian) -> B=0, G=1, R=2, A=3
                byte b = buffer[ioffs + 0];
                byte g = buffer[ioffs + 1];
                byte r = buffer[ioffs + 2];
                byte a = buffer[ioffs + 3];

                int color = 0;
                color |= r << 16;
                color |= g << 8;
                color |= b << 0;
                color |= ContractAlpha(a) << 24;
                output[i] = color;
            }
            return output;
        }

        private byte ContractAlpha(byte value)
        {
            // Implementação da contração para o PS2
            return (byte)(value == 255 ? 128 : (value / 2));
        }

        private byte[] EncodeI4(byte[] buffer)
        {
            byte[] output = new byte[buffer.Length / 2];
            for (int offs = 0; offs < buffer.Length; offs++)
            {
                if ((offs & 1) != 0)
                {
                    output[offs >> 1] |= (byte)(buffer[offs] << 4);
                }
                else
                {
                    output[offs >> 1] = (byte)(buffer[offs] & 0x0F);
                }
            }
            return output;
        }

        private byte[] SwizzlePalette(byte[] palData)
        {
            if (palData == null) return null;
            byte[] output = new byte[palData.Length];
            int count = palData.Length / 4;

            for (int i = 0; i < count; i++)
            {
                // O índice no arquivo 'srcIdx' corresponde ao índice linear 'i'
                // A fórmula troca os bits 3 e 4 do índice.
                int srcIdx = (i & ~0x18) | ((i & 0x08) << 1) | ((i & 0x10) >> 1);

                if ((srcIdx * 4) + 4 <= palData.Length)
                {
                    // Copia 4 bytes (RGBA) da posição swizzled para a posição linear (ou vice-versa)
                    output[i * 4 + 0] = palData[srcIdx * 4 + 0];
                    output[i * 4 + 1] = palData[srcIdx * 4 + 1];
                    output[i * 4 + 2] = palData[srcIdx * 4 + 2];
                    output[i * 4 + 3] = palData[srcIdx * 4 + 3];
                }
            }
            return output;
        }
    }

    // AmtFile mantido exatamente igual ao original, pois não afeta o Problema 1
    public class AmtFile
    {
        public byte[] HeaderSignature { get; set; } = { 0x23, 0x41, 0x4D, 0x54 }; // #AMT
        public int HeaderVersion { get; set; }
        public List<AmtEntry> Entries { get; set; } = new List<AmtEntry>();

        public void Load(string filepath)
        {
            using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                Load(fs);
            }
        }

        public void Load(Stream stream)
        {
            Entries.Clear();
            var br = new BinaryReader(stream);

            // Parte 1: Header Geral
            byte[] sig = br.ReadBytes(4);
            br.ReadInt32(); // 0x20
            br.ReadInt32(); // 0x00
            HeaderVersion = br.ReadInt32();

            // Parte 2: Count
            int imageCount = br.ReadInt32();
            int pointerTableOffset = br.ReadInt32(); // 0x20
            br.ReadInt32(); // 0x00
            br.ReadInt32(); // 0x00

            // Parte 3: Ler Tabela de Ponteiros de forma segura
            stream.Position = pointerTableOffset;
            uint[] pointers = new uint[imageCount];
            for (int i = 0; i < imageCount; i++)
            {
                pointers[i] = br.ReadUInt32();
            }

            // Ler definições de array (Parte 4)
            for (int i = 0; i < imageCount; i++)
            {
                AmtEntry entry = new AmtEntry();

                // Se o ponteiro for 0, é um DUMMY (vazio), marcamos ele e pulamos a leitura de array
                if (pointers[i] == 0)
                {
                    entry.IsDummy = true;
                    Entries.Add(entry);
                    continue;
                }

                // Posiciona o leitor no offset exato deste arquivo
                stream.Position = pointers[i];

                entry.Id = br.ReadInt32();
                entry.Unk1_4Bytes = br.ReadBytes(4);
                entry.InterlaceFlag = br.ReadInt32();
                entry.Unk3_2Bytes = br.ReadBytes(2);
                entry.Unk4_2Bytes = br.ReadBytes(2);
                entry.Width = br.ReadUInt16();
                entry.Height = br.ReadUInt16();
                entry.ImageMetaOffset = br.ReadUInt32();
                entry.ImageSize = br.ReadUInt32();
                entry.Unk5_4Bytes = br.ReadBytes(4);
                entry.Unk6_4Bytes = br.ReadBytes(4);
                entry.PaletteMetaOffset = br.ReadUInt32();
                entry.PaletteSize = br.ReadUInt32();
                entry.Unk7_4Bytes = br.ReadBytes(4);
                Entries.Add(entry);
            }

            // Ler Dados (Parte 5 e 6) usando os Offsets
            foreach (var e in Entries)
            {
                // Ignora Dummys na leitura de dados
                if (e.IsDummy) continue;

                // 1. Ler Imagem Meta e Dados
                if (e.ImageMetaOffset > 0 && e.ImageSize > 0)
                {
                    stream.Position = e.ImageMetaOffset;
                    e.MetaUnk1_4Bytes = br.ReadBytes(4);
                    e.MetaUnk2_4Bytes = br.ReadBytes(4);
                    e.MetaUnk3_4Bytes = br.ReadBytes(4);
                    br.ReadBytes(8);
                    e.MetaUnk4_4Bytes = br.ReadBytes(4);
                    e.MetaUnk5_4Bytes = br.ReadBytes(4);
                    e.MetaUnk6_4Bytes = br.ReadBytes(4);
                    stream.Position = e.ImageMetaOffset + 0x20;
                    e.ImageRawData = br.ReadBytes((int)e.ImageSize);
                }

                // 2. Ler Paleta Meta e Dados
                if (e.PaletteMetaOffset > 0 && e.PaletteSize > 0)
                {
                    stream.Position = e.PaletteMetaOffset + 0x20;
                    e.PaletteRawData = br.ReadBytes((int)e.PaletteSize);
                }
            }
        }

        public void Save(string filepath)
        {
            using (var fs = new FileStream(filepath, FileMode.Create, FileAccess.Write))
            {
                Save(fs);
            }
        }

        public void Save(Stream stream)
        {
            var bw = new BinaryWriter(stream);

            // Parte 1
            bw.Write(HeaderSignature);
            bw.Write(0x20);
            bw.Write(0x00);
            bw.Write(HeaderVersion);

            // Parte 2
            bw.Write(Entries.Count);
            bw.Write(0x20);
            bw.Write(0);
            bw.Write(0);

            // Reservar espaço para a tabela de ponteiros
            long ptrTablePos = stream.Position;
            for (int i = 0; i < Entries.Count; i++) bw.Write((uint)0);

            // Parte 3: Ponteiros
            int pointerBytes = Entries.Count * 4;
            int remainder = pointerBytes % 16;
            int padding = (remainder == 0) ? 0 : 16 - remainder;
            for (int i = 0; i < padding; i++) bw.Write((byte)0);

            // Pré-calcular offsets para os blocos
            long arraysStartOffset = stream.Position;
            int validCount = Entries.Count(e => !e.IsDummy);
            long dataAreaStart = arraysStartOffset + (validCount * 48);

            long currentArrayOffset = arraysStartOffset;
            long currentDataOffset = dataAreaStart;
            uint[] pointers = new uint[Entries.Count];

            // Parte 4: Escrever Arrays com Offsets Calculados (Ignorando DUMMYS)
            for (int i = 0; i < Entries.Count; i++)
            {
                var e = Entries[i];

                if (e.IsDummy)
                {
                    pointers[i] = 0; // Preenche com 00 00 00 00
                    continue;
                }

                pointers[i] = (uint)currentArrayOffset;
                stream.Position = currentArrayOffset;

                bw.Write(e.Id);
                bw.Write(e.Unk1_4Bytes);
                bw.Write(e.InterlaceFlag); // Escreve flag correta (0x13 ou 0x14)
                bw.Write(e.Unk3_2Bytes);
                bw.Write(e.Unk4_2Bytes);
                bw.Write(e.Width);
                bw.Write(e.Height);

                // Ocupação real no arquivo requer alinhamento de múltiplos de 16 bytes
                int imgLen = e.ImageRawData != null ? e.ImageRawData.Length : 0;
                int palLen = e.PaletteRawData != null ? e.PaletteRawData.Length : 0;

                // Imagem Meta Offset
                bw.Write((uint)currentDataOffset);
                bw.Write((uint)imgLen);

                bw.Write(e.Unk5_4Bytes);
                bw.Write(e.Unk6_4Bytes);

                // Ocupação real no arquivo requer alinhamento de múltiplos de 16 bytes
                int imgPaddedSize = (imgLen + 15) & ~15;
                long imageBlockLen = 32 + imgPaddedSize; // 32 bytes do Meta + Tamanho Físico Alinhado

                // Paleta vem logo após imagem
                long paletteMetaOff = currentDataOffset + imageBlockLen;

                if (palLen > 0)
                {
                    bw.Write((uint)paletteMetaOff);
                    bw.Write((uint)palLen);

                    int palPaddedSize = (palLen + 15) & ~15;
                    long paletteBlockLen = 32 + palPaddedSize;
                    currentDataOffset += imageBlockLen + paletteBlockLen;
                }
                else
                {
                    bw.Write(0); // Offset 0
                    bw.Write(0); // Size 0
                    currentDataOffset += imageBlockLen;
                }

                bw.Write(e.Unk7_4Bytes);
                currentArrayOffset += 48; // Próximo bloco tem 48 bytes
            }

            // Parte 5 e 6: Escrever Dados (Meta + Raw)
            stream.Position = dataAreaStart;
            foreach (var e in Entries)
            {
                if (e.IsDummy) continue; // Pula os fantasmas na hora de escrever dados visuais

                // --- IMAGEM ---
                if (e.ImageRawData != null)
                {
                    int imgPaddedSize = (e.ImageRawData.Length + 15) & ~15;

                    WriteMeta(bw, imgPaddedSize, e.MetaUnk1_4Bytes, e.MetaUnk2_4Bytes, e.MetaUnk3_4Bytes, e.MetaUnk4_4Bytes, e.MetaUnk5_4Bytes, e.MetaUnk6_4Bytes);

                    // Escreve os dados reais exatos
                    bw.Write(e.ImageRawData);

                    // Escreve os bytes mortos (padding) para completar o múltiplo de 16
                    int imgPadBytes = imgPaddedSize - e.ImageRawData.Length;
                    for (int i = 0; i < imgPadBytes; i++) bw.Write((byte)0);
                }

                // --- PALETA ---
                if (e.PaletteRawData != null && e.PaletteRawData.Length > 0)
                {
                    int palPaddedSize = (e.PaletteRawData.Length + 15) & ~15;
                    // Geralmente sim, mas os campos de cálculo (Size/16) são baseados no tamanho da paleta.
                    WriteMeta(bw, palPaddedSize, e.MetaUnk1_4Bytes, e.MetaUnk2_4Bytes, e.MetaUnk3_4Bytes, e.MetaUnk4_4Bytes, e.MetaUnk5_4Bytes, e.MetaUnk6_4Bytes);
                    bw.Write(e.PaletteRawData);

                    int palPadBytes = palPaddedSize - e.PaletteRawData.Length;
                    for (int i = 0; i < palPadBytes; i++) bw.Write((byte)0);
                }
            }

            // Retorna ao Offset 0x20 para escrever os ponteiros corretos com os Dummy preservados
            stream.Position = ptrTablePos;
            foreach (uint ptr in pointers)
            {
                bw.Write(ptr);
            }
        }

        private void WriteMeta(BinaryWriter bw, int alignedSize, byte[] u1, byte[] u2, byte[] u3, byte[] u4, byte[] u5, byte[] u6)
        {
            bw.Write(u1);
            bw.Write(u2);
            bw.Write(u3);

            // Cálculos padrão AMT utilizando a base do Aligned Size
            short calc1 = (short)((alignedSize / 16) + 1);
            int calc2 = (alignedSize / 16) + 0x8000;

            bw.Write(calc1);
            bw.Write(new byte[] { 0x00, 0x50 }); // Val1 
            bw.Write(calc2);                     // Val2
            bw.Write(u4); // Unk4 (00 00 00 08)
            bw.Write(u5);
            bw.Write(u6);
        }
    }
}