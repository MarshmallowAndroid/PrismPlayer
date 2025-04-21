using NAudio.Wave;

namespace PrismPlayer.Players
{
    class BasicPlayer : Player
    {
        public BasicPlayer(string filePath)
        {
            _waveStream = new MediaFoundationReader(filePath);

            _monitoredSampleProvider = new(_waveStream.ToSampleProvider());

            OutputDevice = new WasapiOut();
            OutputDevice.Init(_monitoredSampleProvider);
        }

        public override bool LoopEnabled { get; set; } = false;

        public override float LoopStartPercent => 0;

        public override float LoopEndPercent => 0;
    }
}
