using NAudio.Wave;

namespace Common.Audio
{
    public class LoopWaveStream : WaveStream
    {
        private readonly WaveStream _source;

        private readonly long _loopStart;
        private readonly long _loopEnd;

        private readonly object streamLock = new();

        private int remainingLoops;

        public LoopWaveStream(WaveStream source, long loopStart, long loopEnd, int loopCount = -1)
        {
            _source = source;

            // Convert samples to bytes
            _loopStart = loopStart * WaveFormat.BlockAlign;
            _loopEnd = loopEnd * WaveFormat.BlockAlign;

            if (_loopStart < 0) _loopStart = 0;
            if (_loopEnd <= 0) _loopEnd = Length;

            remainingLoops = loopCount;
        }

        public bool Loop { get; set; } = true;

        public long LoopStartSamples => _loopStart / WaveFormat.BlockAlign;

        public long LoopEndSamples => _loopEnd / WaveFormat.BlockAlign;

        public override WaveFormat WaveFormat => _source.WaveFormat;

        public override long Length => _source.Length;

        public override long Position
        {
            get
            {
                lock (streamLock)
                {
                    return _source.Position;
                }
            }
            set
            {
                lock (streamLock)
                {
                    _source.Position = value;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;
            int advanced = (int)(_source.Position + count); // Get read-ahead position

            // Keep on reading until the appropriate byte count has been read
            while (totalBytesRead < count)
            {
                // Check if next position passes over the end position
                if (advanced > _loopEnd && Loop)
                {
                    // Edge case
                    if (_loopEnd > _loopStart)
                    {
                        // Get remaining bytes between the current position and the loop end
                        int byteDifference = (int)(_loopEnd - _source.Position);

                        // Read the remaining bytes into the buffer
                        if (byteDifference > 0)
                        {
                            lock (streamLock)
                            {
                                totalBytesRead += _source.Read(buffer, offset, byteDifference);
                            }
                        }

                        // End on the last loop if set
                        if (remainingLoops == 0) return totalBytesRead;
                        if (remainingLoops > 0) remainingLoops--;

                        // Set position back to the beginning
                        _source.Position = _loopStart;

                        //Console.WriteLine("Loop triggered. Read remaining " + byteDifference + " bytes.");
                    }
                }

                // Read and account for read bytes that is less than expected
                int bytesRead = 0;
                lock (streamLock)
                {
                    bytesRead = _source.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                }

                // No bytes read means we reached end of stream
                if (bytesRead == 0)
                {
                    // Reset stream to start position
                    if (Loop)
                    {
                        // End on the last loop if set
                        if (remainingLoops == 0) return totalBytesRead;
                        if (remainingLoops > 0) remainingLoops--;

                        _source.Position = _loopStart;
                    }
                    else
                    {
                        break;
                    }
                }

                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }
    }
}
