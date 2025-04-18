using NAudio.Wave;
using System;

namespace Common.Audio
{
    public class LoopWaveStream : WaveStream
    {
        private readonly WaveStream sourceStream;

        private readonly long loopStart;
        private readonly long loopEnd;
        private readonly int loopCount;

        private readonly object streamLock = new();

        private int remainingLoops;

        public LoopWaveStream(WaveStream waveStream, long loopStart, long loopEnd, int loopCount = -1)
        {
            sourceStream = waveStream;

            // Convert samples to bytes
            this.loopStart = loopStart * WaveFormat.BlockAlign;
            this.loopEnd = loopEnd * WaveFormat.BlockAlign;

            if (this.loopStart < 0) this.loopStart = 0;
            if (this.loopEnd <= 0) this.loopEnd = Length;

            this.loopCount = loopCount;
            remainingLoops = loopCount;
        }

        public bool Loop { get; set; } = true;

        public long LoopStartSamples => loopStart / WaveFormat.BlockAlign;

        public long LoopEndSamples => loopEnd / WaveFormat.BlockAlign;

        public override WaveFormat WaveFormat => sourceStream.WaveFormat;

        public override long Length => sourceStream.Length;

        public override long Position
        {
            get
            {
                lock (streamLock)
                {
                    return sourceStream.Position;
                }
            }
            set
            {
                lock (streamLock)
                {
                    sourceStream.Position = value;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;
            int advanced = (int)(sourceStream.Position + count); // Get read-ahead position

            // Keep on reading until the appropriate byte count has been read
            while (totalBytesRead < count)
            {
                // Check if next position passes over the end position
                if (advanced > loopEnd && Loop)
                {
                    // Edge case
                    if (loopEnd > loopStart)
                    {
                        // Get remaining bytes between the current position and the loop end
                        int byteDifference = (int)(loopEnd - sourceStream.Position);

                        // Read the remaining bytes into the buffer
                        if (byteDifference > 0)
                        {
                            lock (streamLock)
                            {
                                totalBytesRead += sourceStream.Read(buffer, offset, byteDifference);
                            }
                        }

                        // End on the last loop if set
                        if (remainingLoops == 0) return totalBytesRead;
                        if (remainingLoops > 0) remainingLoops--;

                        // Set position back to the beginning
                        sourceStream.Position = loopStart;

                        //Console.WriteLine("Loop triggered. Read remaining " + byteDifference + " bytes.");
                    }
                }

                // Read and account for read bytes that is less than expected
                int bytesRead = 0;
                lock (streamLock)
                {
                    bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
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

                        sourceStream.Position = loopStart;
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
