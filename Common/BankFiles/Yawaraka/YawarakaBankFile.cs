namespace Common.BankFiles.Yawaraka
{
    /// <summary>
    /// Yawaraka Engine (Soft Engine) bank file reader. Subfiles are named according to index.
    /// </summary>
    public sealed class YawarakaBankFile : BankFile
    {
        /// <inheritdoc/>
        public YawarakaBankFile(Stream bankFileStream) : base(bankFileStream)
        {
            BinaryReader reader = new(bankFileStream);

            uint magic1 = reader.ReadUInt32();
            uint magic2 = reader.ReadUInt32();

            if (magic1 != 0x47314c5f || magic2 != 0x30303030) // _L1G0000
                throw new InvalidBankFileException("Not a Yawaraka bank file.");

            uint fileSize = reader.ReadUInt32();
            uint subfilesOffset = reader.ReadUInt32();
            uint headerSize = reader.ReadUInt32();
            uint fileCount = reader.ReadUInt32();

            uint[] offsets = new uint[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                offsets[i] = reader.ReadUInt32();
            }

            List<Subfile> subfiles = [];
            for (int offsetsIndex = 0; offsetsIndex < fileCount; offsetsIndex++)
            {
                uint length;
                if (offsetsIndex < offsets.Length - 1)
                    length = offsets[offsetsIndex + 1] - offsets[offsetsIndex];
                else
                    length = fileSize - offsets[offsetsIndex];

                uint currentOffset = offsets[offsetsIndex];

                reader.BaseStream.Position = currentOffset;

                uint magic = reader.ReadUInt32();
                if (magic == 0x4c535441)
                {
                    reader.BaseStream.Position = currentOffset + 0x14;
                    uint kovsCount = reader.ReadUInt32();

                    reader.BaseStream.Position = currentOffset + 0x2c;
                    uint firstSuboffset = reader.ReadUInt32();
                    uint firstLength = reader.ReadUInt32();

                    if (kovsCount > 1)
                    {
                        subfiles.Add(new Subfile($"{offsetsIndex:d4}[0]", currentOffset + firstSuboffset, firstLength));

                        reader.BaseStream.Position = currentOffset + 0x54;
                        for (int kovsIndex = 1; kovsIndex < kovsCount; kovsIndex++)
                        {
                            uint suboffset = reader.ReadUInt32();
                            uint sublength = reader.ReadUInt32();
                            reader.BaseStream.Position += 0x20;
                            subfiles.Add(new Subfile($"{offsetsIndex:d4}[{kovsIndex}]", currentOffset + suboffset, sublength));
                        }
                    }
                    else
                        subfiles.Add(new Subfile($"{offsetsIndex:d4}", currentOffset + firstSuboffset, firstLength));
                }
                else
                    subfiles.Add(new Subfile($"{offsetsIndex:d4}", offsets[offsetsIndex], length));
            }

            Subfiles = [.. subfiles];
        }

        public override Subfile[] Subfiles { get; }
    }
}
