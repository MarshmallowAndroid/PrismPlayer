using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrismPlayer.Players
{
    class BasicPlayer : Player
    {
        public BasicPlayer(string filePath)
        {
            _waveStream = new MediaFoundationReader(filePath);

            MonitoredWaveStream = new(_waveStream);

            OutputDevice = new WasapiOut();
            OutputDevice.Init(MonitoredWaveStream);
        }

        public override bool LoopEnabled { get; set; } = false;

        public override float LoopStartPercent => 0;

        public override float LoopEndPercent => 0;
    }
}
