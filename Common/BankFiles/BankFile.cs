namespace Common.BankFiles
{
    /// <summary>
    /// Abstract class for bank file reader implementations.
    /// </summary>
    public abstract class BankFile : IDisposable
    {
        protected readonly Stream _bankFileStream;

        /// <summary>
        /// BankFile constructor.
        /// </summary>
        /// <exception cref="InvalidBankFileException">Thrown when the bank file is invalid or unsupported.</exception>
        /// <param name="bankFileStream"></param>
        public BankFile(Stream bankFileStream)
        {
            _bankFileStream = bankFileStream;
        }

        /// <summary>
        /// The subfiles.
        /// </summary>
        public abstract Subfile[] Subfiles { get; }

        /// <summary>
        /// Get the audio file stream by <see cref="Subfile"/>.
        /// </summary>
        /// <param name="subfile">The subfile information.</param>
        /// <param name="loopPoint">The loop point of the audio file.</param>
        /// <param name="loopType">The loop type of the audio file.</param>
        /// <returns>The audio file stream.</returns>
        public virtual Stream GetAudioStream(Subfile subfile, out uint loopPoint, out uint loopType)
        {
            // Read the header
            BinaryReader kovsReader = new(GetRawStream(subfile));

            uint magic = kovsReader.ReadUInt32(); // Should be KOVS

            if (magic != 0x53564f4b) // KOVS
                throw new Exception("Not a KOVS file.");

            uint oggSize = kovsReader.ReadUInt32();
            loopPoint = kovsReader.ReadUInt32();
            loopType = kovsReader.ReadUInt32();
            kovsReader.BaseStream.Position += 16;

            int dataSize = (int)(oggSize + (8 - oggSize % 8 % 8)); // Align by 8 bytes.

            // KOVS streams XOR the first 256 bytes from 0-255
            // Unmasked data is Ogg Vorbis
            return new KovsStream(new IsolatedStream(kovsReader.BaseStream, kovsReader.BaseStream.Position, dataSize));
        }

        /// <summary>
        /// Get the audio file stream by index from <see cref="Subfiles"/>.
        /// </summary>
        /// <param name="index">The index of the subfile.</param>
        /// <param name="loopPoint">The loop point of the audio file.</param>
        /// <param name="loopType">The loop type of the audio file.</param>
        /// <returns>The audio file stream.</returns>
        public virtual Stream GetAudioStream(int index, out uint loopPoint, out uint loopType)
        {
            return GetAudioStream(Subfiles[index], out loopPoint, out loopType);
        }

        /// <summary>
        /// Get the raw audio file stream by <see cref="Subfile"/>.
        /// The stream is composed of the KOVS header as well as the un-XOR-ed bytes of the
        /// Ogg Vorbis header.
        /// </summary>
        /// <param name="subfile">The subfile information.</param>
        /// <returns>The raw audio file stream.</returns>
        public virtual Stream GetRawStream(Subfile subfile)
        {
            return new IsolatedStream(_bankFileStream, subfile.Offset, subfile.Length);
        }

        /// <summary>
        /// Get the raw audio file stream by index.
        /// The stream will include the KOVS header as well as the un-XOR-ed bytes of the
        /// Ogg Vorbis header.
        /// </summary>
        /// <param name="index">The index of the subfile.</param>
        /// <returns>The raw audio file stream.</returns>
        public virtual Stream GetRawStream(int index)
        {
            return GetRawStream(Subfiles[index]);
        }

        public void Dispose()
        {
            _bankFileStream.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}