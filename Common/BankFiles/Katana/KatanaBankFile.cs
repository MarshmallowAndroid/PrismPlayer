namespace Common.BankFiles.Katana
{
    /// <summary>
    /// Katana Engine bank file reader.
    /// </summary>
    public sealed class KatanaBankFile : BankFile
    {
        private readonly bool _encrypted;

        /// <inheritdoc/>
        public KatanaBankFile(Stream bankFileStream) : base(bankFileStream)
        {
            BinaryReader reader = new(bankFileStream);

            // Bank file header check
            uint magic1 = reader.ReadUInt32();
            uint magic2 = reader.ReadUInt32();

            if (magic1 != 0x4b524449 || magic2 != 0x30303030) // IDRK0000
            {
                ulong combinedMagic = magic1 | ((ulong)magic2 << 32);
                throw new InvalidBankFileException($"Not a Katana bank file. Magic was 0x{combinedMagic:x16}.");
            }

            uint fileSize = reader.ReadUInt32();

            reader.BaseStream.Position = 0x88; // It seems like the subfiles always start here

            List<Subfile> bankFileInfos = [];

            // Go through all subfile headers
            do
            {
                uint identifier = reader.ReadUInt32();

                if (identifier != 0x15f4d409) // All subfile headers have this identifier
                    throw new InvalidBankFileException("This Katana bank file is either unsupported or corrupt.");

                uint dataLength = reader.ReadUInt32();
                uint unk3 = reader.ReadUInt32(); // Might be the internal name for the file
                uint headerLength = reader.ReadUInt32();
                uint unk5 = reader.ReadUInt32(); // At least 0x20 byte difference from dataLength, must be unaligned length
                reader.BaseStream.Position += 0x0c;

                bankFileInfos.Add(new($"0x{unk3:x8}", reader.BaseStream.Position, dataLength));

                reader.BaseStream.Position += dataLength - 0x20;

            } while (reader.BaseStream.Position != reader.BaseStream.Length);

            // Basic encryption check
            reader.BaseStream.Position = bankFileInfos[0].Offset;
            _encrypted = reader.ReadUInt32() != 0x53564f4b; // If the magic of the first subfile is not KOVS, then it must be encrypted

            Subfiles = [.. bankFileInfos];
        }

        public override Subfile[] Subfiles { get; }

        public override Stream GetRawStream(Subfile subfile)
        {
            Stream rawStream = base.GetRawStream(subfile);
            return _encrypted ? new PrismDecryptStream(rawStream) : rawStream;
        }
    }
}
