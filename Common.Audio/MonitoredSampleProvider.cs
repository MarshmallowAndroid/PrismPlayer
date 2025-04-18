using NAudio.Wave;
using System;
using System.Threading;

namespace Common.Audio
{
    /// <summary>
    /// Sample provider that monitors the average value of the left and right channels.
    /// </summary>
    public class MonitoredSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _sampleProvider;
        private readonly int _channels;
        private readonly float[] _channelSamples;

        public MonitoredSampleProvider(ISampleProvider sampleProvider)
        {
            _sampleProvider = sampleProvider;
            _channels = _sampleProvider.WaveFormat.Channels;
            _channelSamples = new float[_channels];
        }

        public float LeftSampleAverage { get; private set; }

        public float RightSampleAverage { get; private set; }

        public WaveFormat WaveFormat => _sampleProvider.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _sampleProvider.Read(buffer, offset, count);

            for (int i = 0; i < _channelSamples.Length; i++)
            {
                _channelSamples[i] = 0;
            }

            for (int i = 0; i < count / _channels; i++)
            {
                for (int j = 0; j < _channels; j++)
                {
                    _channelSamples[j] += Math.Abs(buffer[offset + i * _channels + j]);
                }
            }

            if (WaveFormat.Channels >= 2)
            {
                LeftSampleAverage = _channelSamples[0] / count;
                RightSampleAverage = _channelSamples[1] / count;
            }
            else
            {
                LeftSampleAverage = _channelSamples[0] / count;
                RightSampleAverage = _channelSamples[0] / count;
            }

            return samplesRead;
        }
    }
}
