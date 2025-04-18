using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Audio
{
    internal class LoopSampleProvider : ISampleProvider
    {
        private readonly WaveStream source;
        private readonly long loopStart;
        private readonly long loopEnd;
        private readonly int fadeMs;
        private readonly int loopCount;

        private readonly ISampleProvider sampleProvider;

        private readonly long fadeSamples;
        private readonly long fadePosition;

        private long remainingLoops;
        private long currentSample = 0;
        private long currentFadeSample = 0;

        public LoopSampleProvider(WaveStream source, long loopStart, long loopEnd, int fadeMs, int loopCount = -1)
        {
            this.source = source;
            this.loopStart = loopStart;
            this.loopEnd = loopEnd;
            this.fadeMs = fadeMs;
            this.loopCount = loopCount;

            sampleProvider = source.ToSampleProvider();

            if (loopEnd == 0)
            {
                this.loopEnd = source.Length / WaveFormat.BlockAlign;
            }

            fadeSamples = (long)(fadeMs / 1000f * WaveFormat.SampleRate);
            fadePosition = this.loopEnd - (long)(fadeMs / 1000f * WaveFormat.SampleRate);

            remainingLoops = loopCount;
        }

        public WaveFormat WaveFormat => sampleProvider.WaveFormat;

        public long Position
        {
            get
            {
                return source.Position;
            }
            set
            {
                source.Position = value;
                currentSample = source.Position / WaveFormat.BlockAlign;
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            currentSample = source.Position / WaveFormat.BlockAlign;

            int read = sampleProvider.Read(buffer, offset, count);

            for (int i = 0; i < count / WaveFormat.Channels; i++)
            {
                if (remainingLoops == 0 && currentSample >= loopStart)
                {
                    float value = 1f - (float)currentFadeSample / fadeSamples;
                    for (int j = 0; j < WaveFormat.Channels; j++)
                    {
                        buffer[i * WaveFormat.Channels + j] *= value < 0f ? 0f : value;
                    }

                    if (currentSample <= loopStart + fadeSamples + WaveFormat.SampleRate * 0)
                        currentFadeSample = currentSample - loopStart;
                    else
                        return i * WaveFormat.Channels;
                }

                if (currentSample >= loopEnd)
                {
                    source.Position = loopStart * WaveFormat.BlockAlign;
                    currentSample = loopStart;

                    if (remainingLoops > 0) remainingLoops--;
                    //Console.WriteLine("Remaining loops: " + remainingLoops);
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
