using Common.Audio;
using NAudio.Wave;

namespace PrismPlayer.Players
{
    public abstract class Player : IDisposable
    {
        protected WaveStream? _waveStream;

        public MonitoredWaveStream? MonitoredWaveStream { get; protected set; }

        public WaveFormat? AudioFormat => _waveStream?.WaveFormat;

        public IWavePlayer? OutputDevice { get; protected set; }

        public PlaybackState PlaybackState => OutputDevice?.PlaybackState ?? PlaybackState.Stopped;

        public long Position
        {
            get
            {
                if (_waveStream is not null)
                    return _waveStream.Position;
                else
                    return 0;
            }

            set
            {
                if (_waveStream is not null)
                    _waveStream.Position = value;
            }
        }

        public long Length => _waveStream?.Length ?? 0;

        public float TrackProgress
        {
            get => _waveStream is not null ? (float)_waveStream.Position / _waveStream.Length : 0;
            set
            {
                if (_waveStream is not null)
                {
                    _waveStream.Position = (long)(value * _waveStream.Length);
                }
            }
        }

        public abstract bool LoopEnabled { get; set; }

        public abstract float LoopStartPercent { get; }

        public abstract float LoopEndPercent { get; }

        public float LeftSampleAverage => MonitoredWaveStream?.MonitoredSampleProvider.LeftSampleAverage ?? 0;

        public float RightSampleAverage => MonitoredWaveStream?.MonitoredSampleProvider.RightSampleAverage ?? 0;

        public virtual void Play() => OutputDevice?.Play();

        public virtual void PauseResume()
        {
            if (OutputDevice?.PlaybackState == PlaybackState.Playing)
                OutputDevice?.Pause();
            else
                OutputDevice?.Play();
        }

        public virtual void Stop() => OutputDevice?.Stop();

        public virtual void Dispose()
        {
            OutputDevice?.Stop();
            OutputDevice?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}