using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AmtEditor
{
    /// Classe de conjunto de ferramentas para carregamento de imagens que corrige o bug que impede que imagens PNG com transparência sejam carregadas como paletizadas.
    internal static class BitmapHandler
    {
        private static Byte[] PNG_IDENTIFIER = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        /// Carrega uma imagem, verifica se é um PNG com transparência de paleta e, em caso afirmativo, garante que seja carregada corretamente.
        public static Bitmap LoadBitmap(Byte[] data)
        {
            Byte[] transparencyData = null;
            if (data.Length > PNG_IDENTIFIER.Length)
            {
                // Verifique se a imagem é um PNG.
                Byte[] compareData = new Byte[PNG_IDENTIFIER.Length];
                Array.Copy(data, compareData, PNG_IDENTIFIER.Length);
                if (PNG_IDENTIFIER.SequenceEqual(compareData))
                {
                    // Verifique se contém uma paleta de cores.
                    Int32 plteOffset = FindChunk(data, "PLTE");
                    if (plteOffset != -1)
                    {
                        // Verifique se contém um bloco de transparência de paleta.
                        Int32 trnsOffset = FindChunk(data, "tRNS");
                        if (trnsOffset != -1)
                        {
                            // Pega o bloco
                            Int32 trnsLength = GetChunkDataLength(data, trnsOffset);
                            transparencyData = new Byte[trnsLength];
                            Array.Copy(data, trnsOffset + 8, transparencyData, 0, trnsLength);
                            // Filtrar o bloco alfa da paleta e criar uma nova matriz de dados.
                            Byte[] data2 = new Byte[data.Length - (trnsLength + 12)];
                            Array.Copy(data, 0, data2, 0, trnsOffset);
                            Int32 trnsEnd = trnsOffset + trnsLength + 12;
                            Array.Copy(data, trnsEnd, data2, trnsOffset, data.Length - trnsEnd);
                            data = data2;
                        }
                    }
                }
            }
            using (MemoryStream ms = new MemoryStream(data))
            using (Bitmap loadedImage = new Bitmap(ms))
            {
                if (loadedImage.Palette.Entries.Length != 0 && transparencyData != null)
                {
                    ColorPalette pal = loadedImage.Palette;
                    for (int i = 0; i < pal.Entries.Length; i++)
                    {
                        if (i >= transparencyData.Length)
                            break;
                        Color col = pal.Entries[i];
                        pal.Entries[i] = Color.FromArgb(transparencyData[i], col.R, col.G, col.B);
                    }
                    loadedImage.Palette = pal;
                }
                // Copiando seu conteúdo interno para um novo objeto Bitmap.
                return CloneImage(loadedImage);
            }
        }

        /// Encontra o início de um fragmento PNG. Isso pressupõe que a imagem já esteja identificada como PNG.
        /// Não abrange os primeiros 8 bytes, mas começa no início do bloco de cabeçalho.
        private static Int32 FindChunk(Byte[] data, String chunkName)
        {
            if (data == null)
                throw new ArgumentNullException("data", "No data given!");
            if (chunkName == null)
                throw new ArgumentNullException("chunkName", "No chunk name given!");
            // Utilizando UTF-8 como verificação adicional para garantir que o nome não contenha mais de 127 valores.
            Byte[] chunkNamebytes = Encoding.UTF8.GetBytes(chunkName);
            if (chunkName.Length != 4 || chunkNamebytes.Length != 4)
                throw new ArgumentException("Chunk name must be 4 ASCII characters!", "chunkName");
            Int32 offset = PNG_IDENTIFIER.Length;
            Int32 end = data.Length;
            Byte[] testBytes = new Byte[4];
            // Continue até chegar ao fim ou até não haver espaço suficiente atrás para ler um novo trecho.
            while (offset + 12 < end)
            {
                Array.Copy(data, offset + 4, testBytes, 0, 4);
                if (chunkNamebytes.SequenceEqual(testBytes))
                    return offset;
                Int32 chunkLength = GetChunkDataLength(data, offset);
                // Tamanho do bloco + cabeçalho do bloco + soma de verificação do bloco = 12 bytes.
                offset += 12 + chunkLength;
            }
            return -1;
        }

        private static Int32 GetChunkDataLength(Byte[] data, Int32 offset)
        {
            if (offset + 4 > data.Length)
                throw new IndexOutOfRangeException("Bad chunk size in png image.");
            // Não quero usar o BitConverter; aí tenho que verificar a ordem dos bytes da plataforma e toda essa complicação.
            Int32 length = data[offset + 3] + (data[offset + 2] << 8) + (data[offset + 1] << 16) + (data[offset] << 24);
            if (length < 0)
                throw new IndexOutOfRangeException("Bad chunk size in png image.");
            return length;
        }

        /// Clona um objeto de imagem para liberá-lo de quaisquer recursos subjacentes.
        public static Bitmap CloneImage(Bitmap sourceImage)
        {
            Rectangle rect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
            Bitmap targetImage = new Bitmap(rect.Width, rect.Height, sourceImage.PixelFormat);
            targetImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
            BitmapData sourceData = sourceImage.LockBits(rect, ImageLockMode.ReadOnly, sourceImage.PixelFormat);
            BitmapData targetData = targetImage.LockBits(rect, ImageLockMode.WriteOnly, targetImage.PixelFormat);
            Int32 actualDataWidth = ((Image.GetPixelFormatSize(sourceImage.PixelFormat) * rect.Width) + 7) / 8;
            Int32 h = sourceImage.Height;
            Int32 origStride = sourceData.Stride;
            Int32 targetStride = targetData.Stride;
            Byte[] imageData = new Byte[actualDataWidth];
            IntPtr sourcePos = sourceData.Scan0;
            IntPtr destPos = targetData.Scan0;
            // Copiar linha por linha, pulando os aumentos, mas copiando a largura real dos dados.
            for (Int32 y = 0; y < h; y++)
            {
                Marshal.Copy(sourcePos, imageData, 0, actualDataWidth);
                Marshal.Copy(imageData, 0, destPos, actualDataWidth);
                sourcePos = new IntPtr(sourcePos.ToInt64() + origStride);
                destPos = new IntPtr(destPos.ToInt64() + targetStride);
            }
            targetImage.UnlockBits(targetData);
            sourceImage.UnlockBits(sourceData);
            // Restaura a paleta para imagens indexadas.
            if ((sourceImage.PixelFormat & PixelFormat.Indexed) != 0)
                targetImage.Palette = sourceImage.Palette;
            // Restaurar as configurações de DPI
            targetImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
            return targetImage;
        }

    }
}
