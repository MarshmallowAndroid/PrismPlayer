using Common.Audio;
using Common.BankFiles;
using NAudio.VorbisPizza;
using NAudio.Wave;

namespace PrismPlayer.Players
{
    public sealed class BankPlayer : Player
    {
        private readonly BankFile _bankFile;

        private LoopWaveStream? _loopWaveStream;
        private Subfile? _currentBankFileInfo;

        public BankPlayer(BankFile bankFile) : base()
        {
            _bankFile = bankFile;
        }

        public override bool LoopEnabled
        {
            get => _loopWaveStream?.Loop ?? false;
            set
            {
                if (_loopWaveStream is not null)
                    _loopWaveStream.Loop = value;
            }
        }

        public override float LoopStartPercent
        {
            get
            {
                if (_loopWaveStream is not null)
                    return (float)(_loopWaveStream.LoopStartSamples * _loopWaveStream.BlockAlign) / _loopWaveStream.Length;
                else return 0;
            }
        }

        public override float LoopEndPercent
        {
            get
            {
                if (_loopWaveStream is not null)
                    return (float)(_loopWaveStream.LoopEndSamples * _loopWaveStream.BlockAlign) / _loopWaveStream.Length;
                else return 0;
            }
        }

        public void SetBankFileInfo(Subfile bankFileInfo)
        {
            base.Dispose();
            _waveStream?.Dispose();

            _currentBankFileInfo = bankFileInfo;

            Stream oggStream = _bankFile.GetAudioStream(_currentBankFileInfo, out uint loopPoint, out uint _);

            //FileStream test = File.OpenWrite("test.ogg");
            //oggStream.CopyTo(test);
            //test.Dispose();

            //uint loopOverride = loopPoint;

            //string overrideName = "loop_override.txt";
            //if (File.Exists(overrideName))
            //{
            //    StreamReader overrideReader = File.OpenText(overrideName);
            //    while (!overrideReader.EndOfStream)
            //    {
            //        string current = overrideReader.ReadLine() ?? "";
            //        string[] split = current.Split(' ');
            //        if (split[0].StartsWith(bankFileInfo.Name))
            //        {
            //            loopOverride = uint.Parse(split[1]);
            //        }
            //    }
            //    overrideReader.Dispose();
            //}

            _waveStream = _loopWaveStream = new LoopWaveStream(new VorbisWaveReader(oggStream), loopPoint, 0);
            _monitoredSampleProvider = new MonitoredSampleProvider(_waveStream.ToSampleProvider());
        }

        public override void Play()
        {
            base.Dispose();

            OutputDevice = new WasapiOut();

            if (_monitoredSampleProvider is null) return;

            if (_monitoredSampleProvider.WaveFormat.Channels > OutputDevice.OutputWaveFormat.Channels)
                OutputDevice.Init(
                    new ResamplerDmoStream(
                        _monitoredSampleProvider.ToWaveProvider(),
                        WaveFormat.CreateIeeeFloatWaveFormat(
                            _monitoredSampleProvider.WaveFormat.SampleRate, OutputDevice.OutputWaveFormat.Channels)));
            else
                OutputDevice.Init(_monitoredSampleProvider);


            OutputDevice.Play();
        }

        public override void Dispose()
        {
            base.Dispose();
            _bankFile?.Dispose();
        }
    }
}
