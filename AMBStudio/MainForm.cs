using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace AmtEditor
{
    public partial class MainForm : Form
    {
        // Estrutura para suportar múltiplos arquivos abertos
        public class OpenedFile
        {
            public string FilePath { get; set; }
            public AmbFile AmbFile { get; set; }
            public AmtFile AmtFile { get; set; }
        }

        private List<OpenedFile> openedFiles = new List<OpenedFile>();

        private Bitmap currentOriginalImage; // Mantém a imagem original sem texto
        private Bitmap currentComposedImage; // Imagem com texto

        // Configurações globais
        private EditorSettings globalSettings = new EditorSettings();
        private const string SETTINGS_FILE = "settings.xml";

        // Dicionário de fontes carregadas (Nome Exibição -> Objeto Fonte)
        private Dictionary<string, BitmapFont> loadedFonts = new Dictionary<string, BitmapFont>();

        // Configurações de mapeamento de nome de arquivo para nome legível
        private Dictionary<string, string> fontNameMapping = new Dictionary<string, string>()
        {
            { "Budokai3.png", "Font: Budokai 3" },
        };

        private bool ignoreEvents = false; // Evita loop ao carregar settings

        public MainForm()
        {
            InitializeComponent();

            // Vincula o evento para o CheckBox de fundo escuro
            toggleDarkBG.CheckedChanged += ToggleDarkBG_CheckedChanged;

            // Define o background xadrez para visualizar transparência, fora do Designer para evitar erros de renderização na IDE
            pbPreview.BackgroundImage = GetCheckerPattern(false);

            LoadFontsEmbedded(); // Carrega do executável
            LoadSettings();      // Carrega configurações salvas (X, Y, Fonte, etc.)

            // Evento para desenhar a borda vermelha
            pbPreview.Paint += PbPreview_Paint;

            // Adicionado evento de clique para o Conta-Gotas
            pbPreview.MouseClick += PbPreview_MouseClick;

            // Evento para salvar configurações ao fechar
            this.FormClosing += MainForm_FormClosing;
        }

        // Evento que altera o background quando o CheckBox muda de valor
        private void ToggleDarkBG_CheckedChanged(object sender, EventArgs e)
        {
            if (ignoreEvents) return;

            globalSettings.DarkBackground = toggleDarkBG.Checked;
            pbPreview.BackgroundImage = GetCheckerPattern(globalSettings.DarkBackground);
        }

        // Helper para background xadrez movido para fora do Designer
        private System.Drawing.Bitmap GetCheckerPattern(bool isDark = false)
        {
            var bmp = new System.Drawing.Bitmap(20, 20);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                if (isDark)
                {
                    // Usa cores mais escuras se a opção Dark Background estiver ativada
                    using (var darkBrush = new System.Drawing.SolidBrush(Color.FromArgb(100, 100, 100)))
                    using (var lightBrush = new System.Drawing.SolidBrush(Color.FromArgb(150, 150, 150)))
                    {
                        g.FillRectangle(lightBrush, 0, 0, 10, 10);
                        g.FillRectangle(darkBrush, 10, 0, 10, 10);
                        g.FillRectangle(darkBrush, 0, 10, 10, 10);
                        g.FillRectangle(lightBrush, 10, 10, 10, 10);
                    }
                }
                else
                {
                    using (var greenBrush = new System.Drawing.SolidBrush(Color.FromArgb(200, 210, 200)))
                    using (var whiteBrush = new System.Drawing.SolidBrush(Color.FromArgb(250, 250, 250)))
                    {
                        g.FillRectangle(whiteBrush, 0, 0, 10, 10);
                        g.FillRectangle(greenBrush, 10, 0, 10, 10);
                        g.FillRectangle(greenBrush, 0, 10, 10, 10);
                        g.FillRectangle(whiteBrush, 10, 10, 10, 10);
                    }
                }
            }
            return bmp;
        }

        // Atualiza a Label Hex com a Cor especificada em formato #RGBA
        private void UpdateHexColorLabel(Color c)
        {
            if (lblHexColor != null)
            {
                lblHexColor.Text = $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        // --- Persistência de Configurações ---
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SETTINGS_FILE))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EditorSettings));
                    using (StreamReader reader = new StreamReader(SETTINGS_FILE))
                    {
                        globalSettings = (EditorSettings)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading settings: " + ex.Message);
            }

            // Evita disparar eventos de alteração enquanto aplica as propriedades
            ignoreEvents = true;

            // Restaura o estado da caixa de fundo escuro e atualiza o picture box
            toggleDarkBG.Checked = globalSettings.DarkBackground;
            pbPreview.BackgroundImage = GetCheckerPattern(globalSettings.DarkBackground);

            // Aplica Settings Visuais (Cor do Fundo)
            Color bgColor = Color.FromArgb(globalSettings.BackColorArgb);
            pnlBackColor.BackColor = bgColor;
            UpdateHexColorLabel(bgColor); // Atualiza a Label com o Hex

            ignoreEvents = false;
        }

        private void SaveSettings()
        {
            try
            {
                // Garante que a preferência do fundo está gravada corretamente
                globalSettings.DarkBackground = toggleDarkBG.Checked;

                XmlSerializer serializer = new XmlSerializer(typeof(EditorSettings));
                using (StreamWriter writer = new StreamWriter(SETTINGS_FILE))
                {
                    serializer.Serialize(writer, globalSettings);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving settings: " + ex.Message);
            }
        }

        // --- Conta-Gotas ---
        private void PbPreview_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (currentComposedImage == null) return;

                // Como o PictureBox está em CenterImage, precisamos calcular o offset
                // para encontrar o pixel correto na imagem.

                // Tamanho da área cliente do PictureBox
                int ctrlW = pbPreview.ClientSize.Width;
                int ctrlH = pbPreview.ClientSize.Height;

                // Tamanho da imagem
                int imgW = currentComposedImage.Width;
                int imgH = currentComposedImage.Height;

                // Posição do canto superior esquerdo da imagem dentro do controle
                int startX = (ctrlW - imgW) / 2;
                int startY = (ctrlH - imgH) / 2;

                int pixelX = e.X - startX;
                int pixelY = e.Y - startY;

                // Verifica se o clique foi dentro da imagem
                if (pixelX >= 0 && pixelX < imgW && pixelY >= 0 && pixelY < imgH)
                {
                    Color pickedColor = currentComposedImage.GetPixel(pixelX, pixelY);

                    // Atualiza Global Settings e UI
                    globalSettings.BackColorArgb = pickedColor.ToArgb();
                    pnlBackColor.BackColor = pickedColor;
                    UpdateHexColorLabel(pickedColor); // Atualiza a Label com o Hex					

                    // Força atualização para garantir que a nova cor seja usada no fundo se expandir
                    UpdateComposition();
                }
            }
        }

        private void LoadFontsEmbedded()
        {
            // Obtém o assembly atual (o executável)
            var assembly = Assembly.GetExecutingAssembly();

            // Lista todos os recursos. Formato padrão: Namespace.Folder.Filename
            // Como você criou a pasta "Resources", deve ser "AmtEditor.Resources.nome.png"
            string[] resourceNames = assembly.GetManifestResourceNames();

            cmbFonts.Items.Clear();

            foreach (string resName in resourceNames)
            {
                // Filtra apenas PNGs
                if (!resName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;

                // Tenta limpar o nome do resource (Namespace.Folder.File.png)
                string fileName = resName;
                if (fileName.Contains("Resources."))
                {
                    fileName = fileName.Substring(fileName.IndexOf("Resources.") + 10);
                }

                // Verifica mapeamento
                string displayName = fileName;
                if (fontNameMapping.ContainsKey(fileName))
                {
                    displayName = fontNameMapping[fileName];
                }

                try
                {
                    // Carrega o stream direto do executável
                    using (Stream stream = assembly.GetManifestResourceStream(resName))
                    {
                        if (stream != null)
                        {
                            BitmapFont font = new BitmapFont(stream);
                            loadedFonts[displayName] = font;
                            cmbFonts.Items.Add(displayName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading font {fileName}: {ex.Message}");
                }
            }

            if (cmbFonts.Items.Count > 0) cmbFonts.SelectedIndex = 0;
        }

        // Classe BitmapFont baseada no código Python fornecido
        public class BitmapFont
        {
            public Bitmap Image { get; private set; }
            public int CharHeight { get; private set; }
            public Dictionary<char, Rectangle> CharMap { get; private set; }

            // Construtor modificado para aceitar Stream
            public BitmapFont(Stream stream)
            {
                // Carrega convertendo para ARGB
                using (Bitmap temp = new Bitmap(stream))
                {
                    // Converte para formato padrão para evitar erros de pixel format
                    Image = new Bitmap(temp.Width, temp.Height, PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(Image))
                    {
                        g.DrawImage(temp, 0, 0);
                    }
                }

                CharHeight = Image.Height;
                CharMap = GenerateCharMap();
            }

            private Dictionary<char, Rectangle> GenerateCharMap()
            {
                var map = new Dictionary<char, Rectangle>();
                int width = Image.Width;
                int x = 0;
                int asciiCode = 32; // Inicia no Espaço

                // Lock bits para leitura rápida
                BitmapData data = Image.LockBits(new Rectangle(0, 0, width, Image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    int stride = data.Stride;
                    byte[] pixels = new byte[stride * Image.Height];
                    Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

                    while (x < width && asciiCode <= 255)
                    {
                        // Verifica pixel (x, 0). Formato ARGB: B, G, R, A (Little Endian)
                        int offset = x * 4;
                        byte b = pixels[offset];
                        byte g = pixels[offset + 1];
                        byte r = pixels[offset + 2];

                        bool isGreen = (r == 0 && g == 255 && b == 0);

                        if (isGreen)
                        {
                            x++;
                            continue;
                        }

                        // Início do caractere
                        int startX = x;

                        // Busca o próximo verde ou fim da imagem
                        while (x < width)
                        {
                            offset = x * 4;
                            b = pixels[offset];
                            g = pixels[offset + 1];
                            r = pixels[offset + 2];
                            isGreen = (r == 0 && g == 255 && b == 0);

                            if (isGreen) break;
                            x++;
                        }

                        int charWidth = x - startX;
                        map[(char)asciiCode] = new Rectangle(startX, 0, charWidth, CharHeight);
                        asciiCode++;
                    }
                }
                finally
                {
                    Image.UnlockBits(data);
                }

                return map;
            }

            // Adicionado alignment e totalWidth para calcular offset por linha
            public void DrawText(Graphics g, string text, int startX, int startY, int lineHeightOffset, int alignment, int totalWidth)
            {
                int cursorY = startY;

                // Processa linha por linha para tratar LineHeight corretamente
                string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    // 1. Calcular a largura desta linha específica
                    int currentLineWidth = 0;
                    foreach (char c in line)
                    {
                        if (CharMap.ContainsKey(c))
                            currentLineWidth += CharMap[c].Width;
                    }

                    // 2. Definir o offset X baseado no alinhamento e largura total do bloco
                    int alignOffsetX = 0;
                    if (alignment == 1) // Center
                    {
                        alignOffsetX = (totalWidth - currentLineWidth) / 2;
                    }
                    else if (alignment == 2) // Right
                    {
                        alignOffsetX = totalWidth - currentLineWidth;
                    }

                    // A posição inicial desta linha é StartX (base do bloco) + Offset do alinhamento
                    int cursorX = startX + alignOffsetX;

                    foreach (char c in line)
                    {
                        if (CharMap.ContainsKey(c))
                        {
                            Rectangle srcRect = CharMap[c];
                            // Desenha o caractere na posição atual
                            g.DrawImage(Image, new Rectangle(cursorX, cursorY, srcRect.Width, srcRect.Height), srcRect, GraphicsUnit.Pixel);
                            cursorX += srcRect.Width;
                        }
                    }

                    // Avança linha: Altura da fonte + Altura extra definida pelo usuário
                    cursorY += CharHeight + lineHeightOffset;
                }
            }

            // Método auxiliar para calcular tamanho final do texto
            public Size MeasureText(string text, int lineHeightOffset)
            {
                string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                int maxWidth = 0;
                int totalHeight = 0;

                foreach (string line in lines)
                {
                    int lineWidth = 0;
                    foreach (char c in line)
                    {
                        if (CharMap.ContainsKey(c))
                            lineWidth += CharMap[c].Width;
                    }
                    if (lineWidth > maxWidth) maxWidth = lineWidth;
                    totalHeight += CharHeight + lineHeightOffset;
                }

                // Remove o ultimo offset extra pois não há linha abaixo
                if (lines.Length > 0) totalHeight -= lineHeightOffset;
                return new Size(maxWidth, totalHeight);
            }
        }

        private void OnTextSettingsChanged(object sender, EventArgs e)
        {
            if (ignoreEvents) return;

            // Atualizar variáveis globais em vez de propriedades do nó
            globalSettings.OverlayX = (int)numPosX.Value;
            globalSettings.OverlayY = (int)numPosY.Value;
            globalSettings.LineHeight = (int)numLineHeight.Value;
            globalSettings.FinalSpacing = (int)numFinalSpacing.Value;

            if (cmbFonts.SelectedItem != null)
                globalSettings.SelectedFontName = cmbFonts.SelectedItem.ToString();

            // Salva estado dos radio buttons
            if (rbAlignLeft.Checked) globalSettings.Alignment = 0;
            else if (rbAlignCenter.Checked) globalSettings.Alignment = 1;
            else if (rbAlignRight.Checked) globalSettings.Alignment = 2;

            // O texto ainda é específico do nó
            var entry = GetSelectedEntry();
            if (entry != null)
            {
                entry.OverlayText = txtInput.Text;
            }

            UpdateComposition();
        }

        private Bitmap CloneWithDoubledAlpha(Bitmap original)
        {
            if (original == null) return null;
            Bitmap clone = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(clone))
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = 2.0f; // Multiplica o canal Alpha por 2 (128 -> 255 visual)

                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(original, new Rectangle(0, 0, clone.Width, clone.Height),
                        0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return clone;
        }

        /// Função baseada no Game Graphic Studio (GGS) para checar texturas alpha suspeitas.
        private bool CheckAutomaticWorthAlpha(Bitmap bmp)
        {
            if (bmp == null)
                return false;

            // Regra especial para 4bpp / 8bpp paletizada
            if ((bmp.PixelFormat == PixelFormat.Format4bppIndexed ||
                 bmp.PixelFormat == PixelFormat.Format8bppIndexed) &&
                bmp.Palette != null)
            {
                bool onlyLimitedAlpha = true;

                foreach (var entry in bmp.Palette.Entries)
                {
                    byte a = entry.A;

                    // Se encontrar qualquer alpha diferente de 0 / 128 / 255
                    if (a != 0 && a != 128 && a != 255)
                    {
                        onlyLimitedAlpha = false;
                        break;
                    }
                }

                // Se só tiver 0 / 128 / 255 → NÃO considerar alpha real
                if (onlyLimitedAlpha)
                    return false;
            }

            using (Bitmap temp32 = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(temp32))
                    g.DrawImage(bmp, 0, 0);

                BitmapData data = temp32.LockBits(
                    new Rectangle(0, 0, temp32.Width, temp32.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                byte[] pixels = new byte[data.Stride * temp32.Height];
                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                temp32.UnlockBits(data);

                int totalPixels = temp32.Width * temp32.Height;
                int nonOpaquePixels = 0;

                bool hasIntermediateAlpha = false;
                bool hasOnlyBinaryAlpha = true;

                Dictionary<int, HashSet<byte>> rgbAlphaMap = new Dictionary<int, HashSet<byte>>();

                for (int y = 0; y < temp32.Height; y++)
                {
                    int rowOffset = y * data.Stride;

                    for (int x = 0; x < temp32.Width; x++)
                    {
                        int index = rowOffset + x * 4;

                        byte b = pixels[index + 0];
                        byte gVal = pixels[index + 1];
                        byte r = pixels[index + 2];
                        byte a = pixels[index + 3];

                        if (a < 255)
                            nonOpaquePixels++;

                        if (a != 0 && a != 255)
                        {
                            hasIntermediateAlpha = true;
                            hasOnlyBinaryAlpha = false;
                        }

                        int rgb = (r << 16) | (gVal << 8) | b;

                        if (!rgbAlphaMap.ContainsKey(rgb))
                            rgbAlphaMap[rgb] = new HashSet<byte>();

                        rgbAlphaMap[rgb].Add(a);
                    }
                }

                double nonOpaquePercent = (double)nonOpaquePixels / totalPixels * 100.0;

                // 1 — 100% opaca
                if (nonOpaquePixels == 0)
                    return false;

                // 2 — Quase toda opaca (menos de 5% transparente)
                if (nonOpaquePercent < 5.0)
                    return false;

                // 3 — Apenas alpha binário (0 e 255)
                if (!hasIntermediateAlpha && hasOnlyBinaryAlpha)
                    return false;

                // 4 — Detecta conflito RGB com múltiplos alphas
                int rgbConflicts = 0;

                foreach (var pair in rgbAlphaMap)
                {
                    if (pair.Value.Count > 1)
                        rgbConflicts++;
                }

                if (rgbConflicts > 3)
                    return true;

                // 5 — Se houver alpha intermediário relevante
                if (hasIntermediateAlpha && nonOpaquePercent > 5.0)
                    return true;

                return false;
            }
        }

        /// Gera uma cópia da imagem para Preview que ignora ou aplica o Alpha Blending
        private Bitmap GeneratePreview(Bitmap source, bool needsDoubling)
        {
            if (source == null) return null;

            bool isWorthAlpha = CheckAutomaticWorthAlpha(source);

            Bitmap preview = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(preview))
            {
                ColorMatrix matrix = new ColorMatrix();

                if (isWorthAlpha)
                {
                    // Aplica o "Show alpha view" do Game Graphic Studio caso seja "suspeito".
                    if (needsDoubling)
                        matrix.Matrix33 = 2.0f; // Multiplica o canal Alpha por 2 para exibir no PC
                    else
                        matrix.Matrix33 = 1.0f; // Mantém a composição de alpha como foi renderizada
                }
                else
                {
                    // POR PADRÃO exibe sem Alpha Channel Blending (Imagem opaca), permitindo ver os RGB Brutos
                    matrix.Matrix00 = 1;
                    matrix.Matrix11 = 1;
                    matrix.Matrix22 = 1;
                    matrix.Matrix33 = 0; // Remove a opacidade original
                    matrix.Matrix43 = 1; // Adiciona +1.0f base no alpha final = 255 (Opaco)
                    matrix.Matrix44 = 1;
                }

                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(source, new Rectangle(0, 0, preview.Width, preview.Height),
                        0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return preview;
        }

        private void UpdateComposition()
        {
            // Para atualizar a composição, precisamos da imagem original "limpa"
            if (currentOriginalImage == null) return;

            var entry = GetSelectedEntry();
            if (entry == null) return;

            string text = entry.OverlayText;

            // Se não houver texto, mostra a original
            if (string.IsNullOrEmpty(text))
            {
                if (pbPreview.Image != null) pbPreview.Image.Dispose();

                if (currentComposedImage != null) currentComposedImage.Dispose();

                // Exibe clone da original
                currentComposedImage = (Bitmap)currentOriginalImage.Clone();

                // Aplica renderização segura APENAS para a janela de visualização baseada na checagem de Alpha:
                pbPreview.Image = GeneratePreview(currentComposedImage, true);
                pbPreview.Invalidate();

                // Atualização em Tempo Real Sem Texto
                if (tvFiles.SelectedNode != null)
                {
                    tvFiles.SelectedNode.Text = $"Imagem (ID: {entry.Id}) - {currentComposedImage.Width}x{currentComposedImage.Height}";
                }

                return;
            }

            // Usar configurações globais
            string fontName = globalSettings.SelectedFontName;
            if (string.IsNullOrEmpty(fontName) && cmbFonts.Items.Count > 0) fontName = cmbFonts.Items[0].ToString();

            if (!loadedFonts.ContainsKey(fontName)) return;

            BitmapFont font = loadedFonts[fontName];

            // Configurações Globais
            int lineHeight = globalSettings.LineHeight;
            int finalSpacing = globalSettings.FinalSpacing;

            // 1. Calcular tamanho necessário do texto
            Size textSize = font.MeasureText(text, lineHeight);

            int imgWidth = currentOriginalImage.Width;
            int imgHeight = currentOriginalImage.Height;

            // Cálculo da Posição X Inicial do Texto (Relativo à Imagem Original 0,0)
            int textX = 0;
            int initialSpacing = 0; // Padding Esquerdo (Controlado pelo OverlayX no Centro/Dir)

            if (globalSettings.Alignment == 0)
            {
                // Esquerda (0): Padrão
                // OverlayX move o texto fisicamente.
                textX = 0;
                textX += globalSettings.OverlayX;
            }
            else
            {
                // O texto é calculado centralizado/direita relativo à imagem ORIGINAL.
                // OverlayX funciona como InitialSpacing (Padding Esquerdo da imagem final).
                if (globalSettings.Alignment == 1) textX = (imgWidth - textSize.Width) / 2;
                else textX = imgWidth - textSize.Width;

                // Definimos o valor do padding esquerdo usando OverlayX
                initialSpacing = globalSettings.OverlayX;
            }

            // Adiciona o offset manual (Pos X)
            // Isso agora moverá o bloco inteiro, independentemente do alinhamento interno das linhas
            int textY = globalSettings.OverlayY;

            // Definir retângulos conceituais no espaço
            Rectangle rectImg = new Rectangle(0, 0, imgWidth, imgHeight);
            Rectangle rectText = new Rectangle(textX, textY, textSize.Width, textSize.Height);

            // Calcula a união dos dois retângulos (Bounding Box total)
            Rectangle union = Rectangle.Union(rectImg, rectText);

            // Aplica Spacings na largura total
            union.Width += finalSpacing; // Direita
            union.Width += initialSpacing; // Esquerda (InitialSpacing)

            // Se o texto ou offset jogou para coordenadas negativas (Esquerda/Cima),
            // precisamos de um "Shift" para trazer tudo para o positivo (0,0) na nova imagem.
            int shiftX = (union.X < 0) ? -union.X : 0;
            int shiftY = (union.Y < 0) ? -union.Y : 0;

            // Adicionamos o InitialSpacing ao ShiftX. 
            // Isso empurra todo o conteúdo desenhado (Imagem + Texto) para a direita.
            shiftX += initialSpacing;

            int newWidth = union.Width;
            int newHeight = union.Height;

            // Arredonda a largura atual para o próximo múltiplo de 4
            int paddedWidth = (newWidth + 3) & ~3;
            int diffWidth = paddedWidth - newWidth;

            // Compensa a diferença do múltiplo de 4 no shiftX respeitando o alinhamento
            if (globalSettings.Alignment == 1) // Centro
            {
                shiftX += diffWidth / 2; // Distribui o respiro igualmente
            }
            else if (globalSettings.Alignment == 2) // Direita
            {
                shiftX += diffWidth; // Joga toda a sobra pra esquerda pra manter o texto colado na direita
            }

            newWidth = paddedWidth;

            if (newWidth < 1) newWidth = 1;
            if (newHeight < 1) newHeight = 1;

            Bitmap composed = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(composed))
            {
                // Preenche tudo com a cor selecionada no conta-gotas.
                // Isso garante que áreas expandidas (por texto ou backspace) tenham a cor certa.
                // Dobrar a opacidade da cor de fundo para acompanhar a imagem, caso tenha alpha 128.
                Color bgColor = Color.FromArgb(globalSettings.BackColorArgb);
                int doubledBgAlpha = (bgColor.A >= 128) ? 255 : (bgColor.A * 2);
                g.Clear(Color.FromArgb(doubledBgAlpha, bgColor.R, bgColor.G, bgColor.B));

                // Desenha Imagem e Texto com o Shift calculado (incluindo o initialSpacing se aplicável)
                // Dobrar a opacidade da imagem base para corrigir o problema de perder a opacidade 2x ao ter texto.
                using (Bitmap doubledOriginal = CloneWithDoubledAlpha(currentOriginalImage))
                {
                    g.DrawImage(doubledOriginal, shiftX, shiftY);
                }

                // O texto também é deslocado pelo shiftX
                font.DrawText(g, text, textX + shiftX, textY + shiftY, lineHeight, globalSettings.Alignment, textSize.Width);
            }

            if (currentComposedImage != null) currentComposedImage.Dispose();
            currentComposedImage = composed;

            // Aplica renderização segura APENAS para a janela de visualização baseada na checagem de Alpha:
            pbPreview.Image = GeneratePreview(currentComposedImage, false);
            pbPreview.Invalidate();

            if (currentComposedImage != null && tvFiles.SelectedNode != null)
            {
                tvFiles.SelectedNode.Text = $"Image (ID: {entry.Id}) - {currentComposedImage.Width}x{currentComposedImage.Height}";
            }
        }

        // Botão Limpar Texto (Redimensiona e Preenche)
        private void btnClearText_Click(object sender, EventArgs e)
        {
            var entry = GetSelectedEntry();
            if (entry == null) return;

            // Determinar tamanho alvo
            int targetWidth = 8;
            int targetHeight = 8;

            // Encontrar índice da cor na paleta existente (Preservação de Paleta)
            Color targetColor = Color.FromArgb(globalSettings.BackColorArgb);
            byte bestIndex = 0;

            if (entry.PaletteRawData != null && entry.PaletteRawData.Length > 0)
            {
                double minDistance = double.MaxValue;
                int count = entry.PaletteRawData.Length / 4;

                for (int i = 0; i < count; i++)
                {
                    byte r = entry.PaletteRawData[i * 4 + 0];
                    byte g = entry.PaletteRawData[i * 4 + 1];
                    byte b = entry.PaletteRawData[i * 4 + 2];

                    double dist = Math.Pow(targetColor.R - r, 2) + Math.Pow(targetColor.G - g, 2) + Math.Pow(targetColor.B - b, 2);

                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        bestIndex = (byte)i;
                    }
                }
            }

            // Criar novo RawData 8x8
            bool is4bpp = (entry.PaletteSize == 0x40);

            // Atualiza dimensões do nó para 8x8
            entry.Width = (ushort)targetWidth;
            entry.Height = (ushort)targetHeight;

            byte[] newRaw;

            if (is4bpp)
            {
                // 8x8 = 1 pixel. Em 4bpp, 1 byte guarda 2 pixels. Precisamos de pelo menos 1 byte.
                newRaw = new byte[1];
                // Preenche com bestIndex (parte alta e baixa)
                byte packed = (byte)((bestIndex << 4) | (bestIndex & 0x0F));
                newRaw[0] = packed;
            }
            else
            {
                // 8bpp = 1 byte por pixel
                newRaw = new byte[1];
                newRaw[0] = bestIndex;
            }

            entry.ImageRawData = newRaw;

            // Recarrega o "Original" a partir do novo RawData (agora 8x8 da cor escolhida)
            tvFiles_AfterSelect(null, new TreeViewEventArgs(tvFiles.SelectedNode));

            // Garante que o texto digitado na caixa seja aplicado sobre essa nova base 8x8
            UpdateComposition();
            MessageBox.Show($"Image cleaned.");
        }

        // Borda Vermelha
        private void PbPreview_Paint(object sender, PaintEventArgs e)
        {
            if (pbPreview.Image != null)
            {
                // O PictureBox está em CenterImage. Precisamos calcular a posição da imagem para desenhar a borda ao redor dela.
                // Contudo, se a imagem for maior que o PictureBox, ela é mostrada parcialmente ou redimensionada.

                Size imgSize = pbPreview.Image.Size;
                Size ctrlSize = pbPreview.ClientSize;

                int x = (ctrlSize.Width - imgSize.Width) / 2;
                int y = (ctrlSize.Height - imgSize.Height) / 2;

                Rectangle rect = new Rectangle(x, y, imgSize.Width - 1, imgSize.Height - 1);

                // Caneta vermelha 1px
                using (Pen p = new Pen(Color.Red, 1))
                {
                    e.Graphics.DrawRectangle(p, rect);
                }
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported Files|*.amb;*.amt;*.bin|Todos|*.*";
                ofd.Multiselect = true; // Permite a seleção de múltiplos arquivos
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // Limpa os arquivos da memória e da lista visual antes de abrir os novos
                    openedFiles.Clear();
                    tvFiles.Nodes.Clear();

                    foreach (string fileName in ofd.FileNames)
                    {
                        try
                        {
                            LoadFile(fileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error opening file: {fileName}: {ex.Message}");
                        }
                    }

                    PopulateTree();
                }
            }
        }

        private void LoadFile(string path)
        {
            byte[] header = new byte[4];
            using (var fs = File.OpenRead(path))
            {
                fs.Read(header, 0, 4);
            }
            string sig = System.Text.Encoding.ASCII.GetString(header);

            OpenedFile newFile = new OpenedFile { FilePath = path };

            if (sig == "#AMB")
            {
                newFile.AmbFile = new AmbFile();
                newFile.AmbFile.Load(path);
            }
            else if (sig == "#AMT")
            {
                newFile.AmtFile = new AmtFile();
                newFile.AmtFile.Load(path);
            }
            else
            {
                throw new Exception("Format not supported.");
            }

            openedFiles.Add(newFile);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (openedFiles.Count == 0) return;

            try
            {
                foreach (var file in openedFiles)
                {
                    string dir = Path.GetDirectoryName(file.FilePath);
                    string name = Path.GetFileName(file.FilePath);
                    string newPath = Path.Combine(dir, "new_" + name);

                    if (file.AmbFile != null)
                    {
                        // Propaga o commit de composições recursivamente para toda a árvore AMB
                        CommitAllAmtCompositions(file.AmbFile);
                        file.AmbFile.Save(newPath);
                    }
                    else if (file.AmtFile != null)
                    {
                        CommitAmtCompositions(file.AmtFile);
                        file.AmtFile.Save(newPath);
                    }
                }

                MessageBox.Show("Files saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving: " + ex.Message);
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            string title = "Sobre";
            string message = "AMB/AMT Studio 1.0\n\n" +
                             "AMB/AMT file editor for:\n" +
                             "Dragon Ball Z: Budokai 3\n" +
                             "Dragon Ball Z: Infinite World\n\n" +
                             "Program made by Gledson999.";
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Helper para percorrer recursivamente todos os nós e aplicar Commit no AMT
        private void CommitAllAmtCompositions(AmbFile ambFile)
        {
            if (ambFile == null) return;

            foreach (var entry in ambFile.Entries)
            {
                if (entry.Type == AmbEntryType.AMT && entry.AmtFile != null)
                {
                    CommitAmtCompositions(entry.AmtFile);
                }
                else if (entry.Type == AmbEntryType.AMB && entry.SubAmbFile != null)
                {
                    CommitAllAmtCompositions(entry.SubAmbFile); // Desce para o AMB aninhado
                }
            }
        }

        private void CommitAmtCompositions(AmtFile file)
        {
            foreach (var loopEntry in file.Entries)
            {
                if (!string.IsNullOrEmpty(loopEntry.OverlayText))
                {
                    using (Bitmap baseBmp = loopEntry.ToBitmap())
                    {
                        // 2. Aplica o texto usando configurações GLOBAIS
                        if (loadedFonts.ContainsKey(globalSettings.SelectedFontName))
                        {
                            BitmapFont font = loadedFonts[globalSettings.SelectedFontName];
                            Size textSize = font.MeasureText(loopEntry.OverlayText, globalSettings.LineHeight);

                            int imgWidth = baseBmp.Width;
                            int imgHeight = baseBmp.Height;
                            int textX = 0;
                            int initialSpacing = 0;

                            if (globalSettings.Alignment == 0)
                            {
                                textX = 0 + globalSettings.OverlayX;
                            }
                            else
                            {
                                if (globalSettings.Alignment == 1) textX = (imgWidth - textSize.Width) / 2;
                                else textX = imgWidth - textSize.Width;
                                initialSpacing = globalSettings.OverlayX;
                            }

                            int textY = globalSettings.OverlayY;

                            Rectangle rectImg = new Rectangle(0, 0, imgWidth, imgHeight);
                            Rectangle rectText = new Rectangle(textX, textY, textSize.Width, textSize.Height);
                            Rectangle union = Rectangle.Union(rectImg, rectText);
                            union.Width += globalSettings.FinalSpacing;
                            union.Width += initialSpacing;

                            int shiftX = (union.X < 0) ? -union.X : 0;
                            int shiftY = (union.Y < 0) ? -union.Y : 0;
                            shiftX += initialSpacing; // Aplica o padding inicial

                            int w = union.Width;
                            int h = union.Height;

                            int paddedW = (w + 3) & ~3;
                            int diffW = paddedW - w;

                            if (globalSettings.Alignment == 1) shiftX += diffW / 2;
                            else if (globalSettings.Alignment == 2) shiftX += diffW;

                            w = paddedW;

                            if (w < 1) w = 1; if (h < 1) h = 1;

                            using (Bitmap composed = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                            {
                                using (Graphics g = Graphics.FromImage(composed))
                                {
                                    // Usa a cor de fundo escolhida no preenchimento
                                    // Aplicar mesmo ajuste de alpha no background para a hora de salvar.
                                    Color bgColor = Color.FromArgb(globalSettings.BackColorArgb);
                                    int doubledBgAlpha = (bgColor.A >= 128) ? 255 : (bgColor.A * 2);
                                    g.Clear(Color.FromArgb(doubledBgAlpha, bgColor.R, bgColor.G, bgColor.B));

                                    // Desenhar a base já com alpha duplicado.
                                    using (Bitmap doubledBase = CloneWithDoubledAlpha(baseBmp))
                                    {
                                        g.DrawImage(doubledBase, shiftX, shiftY);
                                    }

                                    // Atualizado para passar os parâmetros de alinhamento ao salvar
                                    font.DrawText(g, loopEntry.OverlayText, textX + shiftX, textY + shiftY, globalSettings.LineHeight, globalSettings.Alignment, textSize.Width);
                                }
                                loopEntry.ImportBitmap(composed);
                            }
                        }
                    }
                }
            }
        }

        private void PopulateTree()
        {
            tvFiles.Nodes.Clear();

            foreach (var file in openedFiles)
            {
                TreeNode root = new TreeNode(Path.GetFileName(file.FilePath));
                root.Tag = "ROOT";
                root.ContextMenuStrip = ctxParent;

                if (file.AmbFile != null)
                {
                    PopulateAmbNode(root, file.AmbFile);
                }
                else if (file.AmtFile != null)
                {
                    for (int i = 0; i < file.AmtFile.Entries.Count; i++)
                    {
                        AmtEntry entry = file.AmtFile.Entries[i];
                        TreeNode node = new TreeNode($"Image (ID: {entry.Id}) - {entry.Width}x{entry.Height}");
                        node.Tag = entry;
                        node.ContextMenuStrip = ctxNeto; // Nó Neto AMT direto
                        root.Nodes.Add(node);
                    }
                }

                tvFiles.Nodes.Add(root);
                root.ExpandAll();
            }
        }

        // Função recursiva para montar a árvore independente de quantos sub-AMB existirem
        private void PopulateAmbNode(TreeNode parentNode, AmbFile ambFile)
        {
            for (int i = 0; i < ambFile.Entries.Count; i++)
            {
                var entry = ambFile.Entries[i];
                string typeStr = entry.Type.ToString();
                TreeNode node = new TreeNode($"Arquivo {typeStr} (ID: {entry.FileId})");
                node.Tag = entry;
                node.ContextMenuStrip = ctxChild; // Nó Filho AMB (Serve para Filho/Neto dinamicamente pelo tipo)

                if (entry.Type == AmbEntryType.AMT && entry.AmtFile != null)
                {
                    for (int j = 0; j < entry.AmtFile.Entries.Count; j++)
                    {
                        var amtEntry = entry.AmtFile.Entries[j];

                        // Esconder arquivos DUMMY do TreeView
                        if (amtEntry.IsDummy) continue;

                        TreeNode childNode = new TreeNode($"Imagem (ID: {amtEntry.Id}) - {amtEntry.Width}x{amtEntry.Height}");
                        childNode.Tag = amtEntry;
                        childNode.ContextMenuStrip = ctxNeto; // Nó Neto AMT (Images)
                        node.Nodes.Add(childNode);
                    }
                }
                else if (entry.Type == AmbEntryType.AMB && entry.SubAmbFile != null)
                {
                    // Desce nível chamando a recursividade novamente
                    PopulateAmbNode(node, entry.SubAmbFile);
                }

                parentNode.Nodes.Add(node);
            }
        }

        private void tvFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Seleciona o nó ao clicar com botão direito para o ContextMenu funcionar no item certo
            if (e.Button == MouseButtons.Right)
            {
                tvFiles.SelectedNode = e.Node;
            }
        }

        private void tvFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is AmtEntry entry)
            {
                // Carrega a imagem original do AMT
                var bmp = entry.ToBitmap();
                if (bmp != null)
                {
                    // Descarta anteriores
                    if (currentOriginalImage != null) currentOriginalImage.Dispose();

                    // Clona para garantir que temos controle sobre o bitmap
                    currentOriginalImage = (Bitmap)bmp.Clone();
                    bmp.Dispose();
                }

                // 2. Preenche os controles de UI
                ignoreEvents = true; // Bloqueia update visual enquanto preenchemos campos

                // Texto vem da entrada específica
                txtInput.Text = entry.OverlayText;

                // Configurações vêm do objeto Global (Settings) e não do nó
                numPosX.Value = (decimal)globalSettings.OverlayX;
                numPosY.Value = (decimal)globalSettings.OverlayY;
                numLineHeight.Value = (decimal)globalSettings.LineHeight;
                numFinalSpacing.Value = (decimal)globalSettings.FinalSpacing;

                // Restaura Alinhamento Visual
                switch (globalSettings.Alignment)
                {
                    case 0: rbAlignLeft.Checked = true; break;
                    case 1: rbAlignCenter.Checked = true; break;
                    case 2: rbAlignRight.Checked = true; break;
                }

                if (!string.IsNullOrEmpty(globalSettings.SelectedFontName) && cmbFonts.Items.Contains(globalSettings.SelectedFontName))
                {
                    cmbFonts.SelectedItem = globalSettings.SelectedFontName;
                }
                else if (cmbFonts.Items.Count > 0)
                {
                    cmbFonts.SelectedIndex = 0;
                }

                ignoreEvents = false;

                // 3. Gera o preview combinando Original + Texto deste nó
                UpdateComposition();
            }
            else
            {
                if (pbPreview.Image != null)
                {
                    pbPreview.Image = null;
                }
                txtInput.Text = "";
            }
        }

        // Context Menu do Pai
        private void mnuExpandAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in tvFiles.Nodes)
            {
                node.ExpandAll();
            }
        }

        private void mnuCollapseAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in tvFiles.Nodes)
            {
                node.Collapse();
            }
        }

        private void ctxChild_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (tvFiles.SelectedNode != null && tvFiles.SelectedNode.Tag is AmbEntry entry)
            {
                string ext = entry.Type.ToString();
                mnuExportExt.Text = $"Export {ext}";
                mnuImportExt.Text = $"Import {ext}";
            }
        }

        private void mnuExportExt_Click(object sender, EventArgs e)
        {
            if (tvFiles.SelectedNode != null && tvFiles.SelectedNode.Tag is AmbEntry entry)
            {
                string ext = entry.Type.ToString().ToLower();
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = $"File {ext.ToUpper()}|*.{ext}";
                    sfd.FileName = $"file_{entry.FileId}.{ext}";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        if (entry.Type == AmbEntryType.AMT && entry.AmtFile != null)
                        {
                            CommitAmtCompositions(entry.AmtFile);
                            using (var ms = new MemoryStream())
                            {
                                entry.AmtFile.Save(ms);
                                File.WriteAllBytes(sfd.FileName, ms.ToArray());
                                MessageBox.Show("File exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else if (entry.Type == AmbEntryType.AMB && entry.SubAmbFile != null)
                        {
                            CommitAllAmtCompositions(entry.SubAmbFile); // Consolida dados antes de exportar
                            using (var ms = new MemoryStream())
                            {
                                entry.SubAmbFile.Save(ms);
                                File.WriteAllBytes(sfd.FileName, ms.ToArray());
                                MessageBox.Show("File exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            File.WriteAllBytes(sfd.FileName, entry.RawData);
                            MessageBox.Show("File exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void mnuImportExt_Click(object sender, EventArgs e)
        {
            if (tvFiles.SelectedNode != null && tvFiles.SelectedNode.Tag is AmbEntry entry)
            {
                string ext = entry.Type.ToString().ToLower();
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = $"File {ext.ToUpper()}|*.{ext}";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        byte[] raw = File.ReadAllBytes(ofd.FileName);
                        entry.RawData = raw;

                        if (entry.Type == AmbEntryType.AMT)
                        {
                            entry.AmtFile = new AmtFile();
                            using (var ms = new MemoryStream(raw))
                            {
                                entry.AmtFile.Load(ms);
                            }
                        }
                        else if (entry.Type == AmbEntryType.AMB)
                        {
                            entry.SubAmbFile = new AmbFile();
                            using (var ms = new MemoryStream(raw))
                            {
                                entry.SubAmbFile.Load(ms);
                            }
                        }

                        PopulateTree();
                        MessageBox.Show("File imported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private AmtEntry GetSelectedEntry()
        {
            if (tvFiles.SelectedNode != null && tvFiles.SelectedNode.Tag is AmtEntry entry) return entry;
            return null;
        }

        private void mnuExportBin_Click(object sender, EventArgs e)
        {
            // Exporta apenas o RawData atual (sem texto, a menos que Salve primeiro)
            var entry = GetSelectedEntry();
            if (entry == null) return;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Raw Image|*.bin";
                sfd.FileName = $"image_{entry.Id}.bin";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName, entry.ImageRawData);
                    if (entry.PaletteRawData != null) File.WriteAllBytes(sfd.FileName + ".pal", entry.PaletteRawData);
                    MessageBox.Show("BIN file exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void mnuImportBin_Click(object sender, EventArgs e)
        {
            var entry = GetSelectedEntry();
            if (entry == null) return;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Raw Image|*.bin";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    entry.ImageRawData = File.ReadAllBytes(ofd.FileName);
                    entry.OverlayText = "";
                    tvFiles_AfterSelect(null, new TreeViewEventArgs(tvFiles.SelectedNode));
                    MessageBox.Show("BIN file imported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void mnuExportPng_Click(object sender, EventArgs e)
        {
            // Exporta o que está no PREVIEW (Com texto)
            var entry = GetSelectedEntry();
            if (entry == null || currentComposedImage == null) return;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image|*.png";
                sfd.FileName = $"image_{entry.Id}.png";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    currentComposedImage.Save(sfd.FileName, ImageFormat.Png);

                    // Dobra os valores de opacidade (Alpha) diretamente no PNG gerado pelo GDI+
                    try
                    {
                        // Executa o FixPngAlpha somente se não for 32bpp, pois em 32bpp (com texto) já corrigimos na memória
                        if (currentComposedImage.PixelFormat != PixelFormat.Format32bppArgb)
                        {
                            FixPngAlpha(sfd.FileName);
                        }
                        MessageBox.Show("Image exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("The image was saved, but an error occurred while fix the opacity in the palette: " + ex.Message);
                    }
                }
            }
        }

        // Helper: Edita o bloco tRNS diretamente no arquivo PNG para forçar o Alpha de PC (Correção do bug do GDI+ do .NET)
        private void FixPngAlpha(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);

            // Verifica Assinatura PNG válida
            if (fileBytes.Length < 8 ||
                fileBytes[0] != 0x89 || fileBytes[1] != 0x50 ||
                fileBytes[2] != 0x4E || fileBytes[3] != 0x47 ||
                fileBytes[4] != 0x0D || fileBytes[5] != 0x0A ||
                fileBytes[6] != 0x1A || fileBytes[7] != 0x0A)
            {
                return; // Não é um arquivo PNG
            }

            int offset = 8;
            bool modified = false;

            while (offset + 12 <= fileBytes.Length)
            {
                int length = (fileBytes[offset] << 24) | (fileBytes[offset + 1] << 16) | (fileBytes[offset + 2] << 8) | fileBytes[offset + 3];
                string type = System.Text.Encoding.ASCII.GetString(fileBytes, offset + 4, 4);

                if (type == "tRNS")
                {
                    // Altera os bytes de transparência diretamente duplicando seus valores
                    for (int i = 0; i < length; i++)
                    {
                        byte a = fileBytes[offset + 8 + i];
                        // PS2 usa limite de 128, PC usa 255. Resolvemos os valores duplicando diretamente
                        int newAlpha = (a >= 128) ? 255 : (a * 2);
                        fileBytes[offset + 8 + i] = (byte)newAlpha;
                    }

                    // Após modificar, precisamos recalcular e gravar o novo checksum (CRC) do bloco
                    uint crc = CalculateCrc32(fileBytes, offset + 4, length + 4);

                    // Grava o CRC em Big Endian
                    fileBytes[offset + 8 + length] = (byte)((crc >> 24) & 0xFF);
                    fileBytes[offset + 8 + length + 1] = (byte)((crc >> 16) & 0xFF);
                    fileBytes[offset + 8 + length + 2] = (byte)((crc >> 8) & 0xFF);
                    fileBytes[offset + 8 + length + 3] = (byte)(crc & 0xFF);

                    modified = true;
                    break;
                }

                offset += 12 + length; // Salta para o próximo bloco
            }

            if (modified)
            {
                File.WriteAllBytes(filePath, fileBytes);
            }
        }

        // Calcula o CRC32 padrão utilizado por arquivos PNG (Correção do bug do GDI+ do .NET)
        private uint CalculateCrc32(byte[] data, int offset, int length)
        {
            uint[] table = new uint[256];
            uint poly = 0xEDB88320;
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                        crc = (crc >> 1) ^ poly;
                    else
                        crc >>= 1;
                }
                table[i] = crc;
            }

            uint c = 0xFFFFFFFF;
            for (int i = 0; i < length; i++)
            {
                c = table[(c ^ data[offset + i]) & 0xFF] ^ (c >> 8);
            }
            return c ^ 0xFFFFFFFF;
        }

        private void mnuImportPng_Click(object sender, EventArgs e)
        {
            var entry = GetSelectedEntry();
            if (entry == null) return;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "PNG Image|*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        entry.ImportBitmap(BitmapHandler.LoadBitmap(File.ReadAllBytes(ofd.FileName)));
                        entry.OverlayText = ""; // Limpa texto pois importou imagem pronta

                        // Atualizar Preview em tempo real
                        tvFiles_AfterSelect(null, new TreeViewEventArgs(tvFiles.SelectedNode));
                        MessageBox.Show("Image imported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error importing PNG: " + ex.Message);
                    }
                }
            }
        }
    }

    // Classe para persistência das configurações globais
    public class EditorSettings
    {
        public int OverlayX { get; set; } = 0;
        public int OverlayY { get; set; } = 0;
        public int LineHeight { get; set; } = 0;
        public int FinalSpacing { get; set; } = 0;
        public string SelectedFontName { get; set; } = "";
        public int Alignment { get; set; } = 0; // 0=Esquerda, 1=Centralizada, 2=Direita
        public int BackColorArgb { get; set; } = Color.Black.ToArgb(); // Cor padrão
        public bool DarkBackground { get; set; } = false; // Fundo xadrez escuro ou claro
    }
}