using System;
using System.Collections.Generic;
using System.IO;

namespace AmtEditor
{
    public enum AmbEntryType
    {
        AMT,
        AMA,
        BFC,
        AMB,  // Adicionado para suportar AMB aninhado
        AME,
        AMO,
        AMM,
        BSK,
        AMC,
        AML,
        SPX,
        IECS,
        MDD,
        MAD,
        MAS,
        BCM,
        ATR,
        AST,
        AMP,
        UNK
    }

    public class AmbEntry
    {
        public AmbEntryType Type { get; set; }
        public int FileId { get; set; }
        public byte[] RawData { get; set; }
        public AmtFile AmtFile { get; set; }
        public AmbFile SubAmbFile { get; set; }
    }

    public class AmbFile
    {
        public int Version { get; set; }
        public List<AmbEntry> Entries { get; set; } = new List<AmbEntry>();

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

            // Parte 1: Header (16 bytes)
            byte[] sig = br.ReadBytes(4); // #AMB
            br.ReadInt32(); // 0x20
            br.ReadInt32(); // 0x00
            Version = br.ReadInt32();

            // Parte 2: Quantia & Offsets (16 bytes)
            int count = br.ReadInt32();
            int pointerTableOffset = br.ReadInt32();
            int filesStartOffset = br.ReadInt32();
            br.ReadInt32(); // 0x00

            // Parte 3: Tabela de Ponteiros
            stream.Position = pointerTableOffset;
            for (int i = 0; i < count; i++)
            {
                stream.Position = pointerTableOffset + (i * 16);
                int offset = br.ReadInt32();
                int size = br.ReadInt32();
                int fileId = br.ReadInt32();
                br.ReadInt32(); // 0x00

                // Extrair os dados RAW
                stream.Position = offset;
                byte[] raw = br.ReadBytes(size);

                AmbEntry entry = new AmbEntry
                {
                    FileId = fileId,
                    RawData = raw
                };

                // Identificar o tipo via Magic Header
                if (raw.Length >= 4)
                {
                    string header = System.Text.Encoding.ASCII.GetString(raw, 0, 4);
                    if (header == "#AMT")
                    {
                        entry.Type = AmbEntryType.AMT;
                        entry.AmtFile = new AmtFile();
                        using (var ms = new MemoryStream(raw))
                        {
                            entry.AmtFile.Load(ms);
                        }
                    }
                    else if (header == "#AMB")
                    {
                        entry.Type = AmbEntryType.AMB;
                        entry.SubAmbFile = new AmbFile();
                        using (var ms = new MemoryStream(raw))
                        {
                            entry.SubAmbFile.Load(ms); // Chamada recursiva para ler o sub-AMB
                        }
                    }
                    else if (header == "#AMA") entry.Type = AmbEntryType.AMA;
                    else if (header == "#AME") entry.Type = AmbEntryType.AME;
                    else if (header == "#AMO") entry.Type = AmbEntryType.AMO;
                    else if (header == "#BFC") entry.Type = AmbEntryType.BFC;
                    else if (header == "#AMM") entry.Type = AmbEntryType.AMM;
                    else if (header == "#BSK") entry.Type = AmbEntryType.BSK;
                    else if (header == "#AMC") entry.Type = AmbEntryType.AMC;
                    else if (header == "#AML") entry.Type = AmbEntryType.AML;
                    else if (header == "#SPX") entry.Type = AmbEntryType.SPX;
                    else if (header == "IECS") entry.Type = AmbEntryType.IECS;
                    else if (header == "#MDD") entry.Type = AmbEntryType.MDD;
                    else if (header == "#MAD") entry.Type = AmbEntryType.MAD;
                    else if (header == "#MAS") entry.Type = AmbEntryType.MAS;
                    else if (header == "#BCM") entry.Type = AmbEntryType.BCM;
                    else if (header == "#ATR") entry.Type = AmbEntryType.ATR;
                    else if (header == "#AST") entry.Type = AmbEntryType.AST;
                    else if (header == "#AMP") entry.Type = AmbEntryType.AMP;
                    else
                    {
                        entry.Type = AmbEntryType.UNK;
                    }
                }

                Entries.Add(entry);
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

            // Parte 1: Header
            bw.Write(new byte[] { 0x23, 0x41, 0x4D, 0x42 }); // #AMB
            bw.Write(0x20);
            bw.Write(0x00);
            bw.Write(Version);

            // Parte 2: Quantia & Offsets
            bw.Write(Entries.Count);
            bw.Write(0x20); // Offset tabela de ponteiros
            int filesStartOffset = 0x20 + (Entries.Count * 16);
            bw.Write(filesStartOffset); // Offset inicial onde começa os arquivos
            bw.Write(0x00);

            // Tabela de Ponteiros (Espaço reservado)
            long ptrTablePos = stream.Position;
            for (int i = 0; i < Entries.Count; i++)
            {
                bw.Write(0); bw.Write(0); bw.Write(0); bw.Write(0);
            }

            // Arquivos RAW
            long currentFileOffset = filesStartOffset;
            for (int i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];

                // Se for um nó AMT, atualizamos o buffer interno com suas novas alterações (textos, imgs)
                if (entry.Type == AmbEntryType.AMT && entry.AmtFile != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        entry.AmtFile.Save(ms);
                        entry.RawData = ms.ToArray();
                    }
                }
                // Se for um nó AMB aninhado, salvamos suas alterações recursivamente
                else if (entry.Type == AmbEntryType.AMB && entry.SubAmbFile != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        entry.SubAmbFile.Save(ms);
                        entry.RawData = ms.ToArray();
                    }
                }

                stream.Position = currentFileOffset;
                if (entry.RawData != null)
                {
                    bw.Write(entry.RawData);
                    int size = entry.RawData.Length;

                    // Atualiza a tabela de ponteiros deste arquivo
                    stream.Position = ptrTablePos + (i * 16);
                    bw.Write((int)currentFileOffset);
                    bw.Write(size);
                    bw.Write(entry.FileId);
                    bw.Write(0);

                    currentFileOffset += size;
                }
            }
        }
    }
}