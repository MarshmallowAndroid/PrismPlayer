namespace Common
{
    public class KovsStream(Stream stream) : Stream
    {
        private int bytesChanged;

        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => stream.CanWrite;

        public override long Length => stream.Length;

        public override long Position
        {
            get => stream.Position;
            set
            {
                stream.Position = value;
                bytesChanged = Position < 256 ? (byte)Position : 256;
            }
        }

        public override void Flush() => stream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = stream.Read(buffer, offset, count);

            if (bytesChanged < 256)
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[i + offset] ^= (byte)bytesChanged++;
                    if (bytesChanged >= 256) break;
                }
            }

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long ret = stream.Seek(offset, origin);
            bytesChanged = ret < 256 ? (byte)ret : 256;
            return ret;
        }

        public override void SetLength(long value) => stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (bytesChanged < 256)
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[i + offset] ^= (byte)bytesChanged++;
                    if (bytesChanged >= 256) break;
                }
            }

            stream.Write(buffer, offset, count);
        }
    }
}
