namespace Common
{
    /// <summary>
    /// Stream that XORs the first 256 bytes from 0-255. The source stream must already be positioned at the Ogg Vorbis header (usually at 32 bytes).
    /// </summary>
    /// <param name="source">The source stream.</param>
    public class KovsStream(Stream source) : Stream
    {
        private int _bytesChanged;

        public override bool CanRead => source.CanRead;

        public override bool CanSeek => source.CanSeek;

        public override bool CanWrite => source.CanWrite;

        public override long Length => source.Length;

        public override long Position
        {
            get => source.Position;
            set
            {
                source.Position = value;
                _bytesChanged = Position < 256 ? (byte)Position : 256;
            }
        }

        public override void Flush() => source.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);

            if (_bytesChanged < 256)
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[i + offset] ^= (byte)_bytesChanged++;
                    if (_bytesChanged >= 256) break;
                }
            }

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long ret = source.Seek(offset, origin);
            _bytesChanged = ret < 256 ? (byte)ret : 256;
            return ret;
        }

        public override void SetLength(long value) => source.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_bytesChanged < 256)
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[i + offset] ^= (byte)_bytesChanged++;
                    if (_bytesChanged >= 256) break;
                }
            }

            source.Write(buffer, offset, count);
        }
    }
}
