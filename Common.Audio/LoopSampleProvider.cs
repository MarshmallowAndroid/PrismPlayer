using NAudio.Wave;

namespace Common.Audio
{
    public class LoopSampleProvider : ISampleProvider
    {
        private readonly WaveStream _source;
        private readonly long _loopStart;
        private readonly long _loopEnd;

        private readonly ISampleProvider _sampleProvider;

        private readonly long _fadeSamples;

        private long remainingLoops;
        private long currentSample = 0;
        private long currentFadeSample = 0;

        public LoopSampleProvider(WaveStream source, long loopStart, long loopEnd, int fadeMs, int loopCount = -1)
        {
            _source = source;
            _loopStart = loopStart;
            _loopEnd = loopEnd;

            _sampleProvider = source.ToSampleProvider();

            if (loopEnd == 0)
            {
                _loopEnd = source.Length / WaveFormat.BlockAlign;
            }

            _fadeSamples = (long)(fadeMs / 1000f * WaveFormat.SampleRate);

            remainingLoops = loopCount;
        }

        public WaveFormat WaveFormat => _sampleProvider.WaveFormat;

        public long Position
        {
            get
            {
                return _source.Position;
            }
            set
            {
                _source.Position = value;
                currentSample = _source.Position / WaveFormat.BlockAlign;
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            currentSample = _source.Position / WaveFormat.BlockAlign;

            int read = _sampleProvider.Read(buffer, offset, count);

            for (int i = 0; i < count / WaveFormat.Channels; i++)
            {
                if (remainingLoops == 0 && currentSample >= _loopStart)
                {
                    float value = 1f - (float)currentFadeSample / _fadeSamples;
                    for (int j = 0; j < WaveFormat.Channels; j++)
                    {
                        buffer[i * WaveFormat.Channels + j] *= value < 0f ? 0f : value;
                    }

                    if (currentSample <= _loopStart + _fadeSamples + WaveFormat.SampleRate * 0)
                        currentFadeSample = currentSample - _loopStart;
                    else
                        return i * WaveFormat.Channels;
                }

                if (currentSample >= _loopEnd)
                {
                    _source.Position = _loopStart * WaveFormat.BlockAlign;
                    currentSample = _loopStart;

                    if (remainingLoops > 0) remainingLoops--;
                }

                currentSample++;
            }

            return read;
        }

        private long BytesToSamples(long bytes)
        {
            return bytes / (WaveFormat.BitsPerSample / 8) / WaveFormat.Channels;
        }
    }
}
