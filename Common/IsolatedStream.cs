namespace Common
{
    /// <summary>
    /// Isolated stream used to read a <see cref="Stream"/> within a <see cref="Stream"/>
    /// </summary>
    /// <param name="outerStream">The outer stream.</param>
    /// <param name="outerPosition">The position of the outer stream we want to begin with.</param>
    /// <param name="length">The length of data we want accessible.</param>
    public class IsolatedStream(Stream outerStream, long outerPosition, long length) : Stream
    {
        private long _internalPosition = 0;

        private readonly object _streamLock = new();

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => length;

        public override long Position
        {
            get
            {
                lock (_streamLock)
                {
                    return _internalPosition;
                }
            }
            set
            {
                lock (_streamLock)
                {
                    long checkValue = value + outerPosition;
                    if (value < 0 || value > Length) throw new ArgumentOutOfRangeException(nameof(value));
                    _internalPosition = value;
                    outerStream.Position = checkValue;
                }
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_streamLock)
            {
                long restore = outerStream.Position;

                if (_internalPosition + outerPosition > outerStream.Length)
                {
                    _internalPosition = Length;
                    return 0;
                }

                outerStream.Position = _internalPosition + outerPosition;

                int read = outerStream.Read(buffer, offset, Math.Min(count, (int)(Length - Position)));
                _internalPosition += read;

                outerStream.Position = restore;

                return read;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            lock (_streamLock)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        _internalPosition = 0;
                        _internalPosition += offset;
                        break;
                    case SeekOrigin.Current:
                        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(offset, Length, nameof(offset));
                        _internalPosition += offset;
                        break;
                    case SeekOrigin.End:
                        _internalPosition = Length;
                        ArgumentOutOfRangeException.ThrowIfLessThan(outerPosition - offset, outerPosition, nameof(offset));
                        _internalPosition -= offset;
                        break;
                    default:
                        break;
                }

                return _internalPosition;
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
