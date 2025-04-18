namespace Common.BankFiles.Yawaraka
{
    /// <summary>
    /// Yawaraka Engine (Soft Engine) bank file reader.
    /// </summary>
    public sealed class YawarakaBankFile : BankFile
    {
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

            Subfiles = new Subfile[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                uint length;
                if (i < offsets.Length - 1)
                    length = offsets[i + 1] - offsets[i];
                else
                    length = fileSize - offsets[i];

                Subfiles[i] = new Subfile($"0x{offsets[i]:x8}", offsets[i], length);
            }
        }

        public override Subfile[] Subfiles { get; }
    }
}
