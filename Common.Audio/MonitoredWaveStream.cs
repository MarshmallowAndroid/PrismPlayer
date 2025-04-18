using NAudio.Wave;

namespace Common.Audio
{
    public class MonitoredWaveStream : WaveStream
    {
        private readonly IWaveProvider waveProvider;

        public MonitoredWaveStream(WaveStream inputStream)
        {
            SourceStream = inputStream;
            MonitoredSampleProvider = new MonitoredSampleProvider(inputStream.ToSampleProvider());

            waveProvider = MonitoredSampleProvider.ToWaveProvider();
        }

        public WaveStream SourceStream { get; }

        public MonitoredSampleProvider MonitoredSampleProvider { get; }

        public bool IsPaused { get; set; } = false;

        public override WaveFormat WaveFormat => SourceStream.WaveFormat;

        public override long Length => SourceStream.Length;

        public override long Position { get => SourceStream.Position; set => SourceStream.Position = value; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (IsPaused)
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[i] = 0;
                }

                return count;
            }
            else return waveProvider.Read(buffer, offset, count);
        }
    }
}
