﻿using Common.Audio;
using Common.BankFiles;
using NAudio.VorbisPizza;
using NAudio.Wave;

namespace PrismPlayer.Players
{
    public sealed class BankPlayer : Player
    {
        private readonly BankFile _bankFile;

        private LoopWaveStream? _loopWaveStream;
        private Subfile? _currentSubfile;

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

        public void SetSubfile(Subfile subfile)
        {
            base.Dispose();
            _waveStream?.Dispose();

            _currentSubfile = subfile;

            Stream oggStream = _bankFile.GetAudioStream(_currentSubfile, out uint loopPoint, out uint _);

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

        public bool ExportCurrent(out string fileName, string type)
        {
            fileName = "";

            if (_currentSubfile is null) return false;

            fileName = $"{_currentSubfile.Name}.{type}";

            Stream oggStream = _bankFile.GetAudioStream(_currentSubfile, out uint loopPoint, out uint _);

            OutputDevice?.Pause();

            if (type == "ogg")
            {
                using FileStream oggFileStream = File.OpenWrite(fileName);
                oggStream.CopyTo(oggFileStream);
                oggFileStream.Flush();
                oggFileStream.Dispose();
            }
            else if (type == "wav")
            {
                VorbisWaveReader vorbisWaveReader = new(oggStream);
                IWaveProvider target;
                if (loopPoint == 0)
                    target = vorbisWaveReader;
                else
                    target = new LoopSampleProvider(vorbisWaveReader, loopPoint, 0, 30000, 1).ToWaveProvider();
                WaveFileWriter.CreateWaveFile(fileName, target);
            }
            else return false;

            OutputDevice?.Play();

            return true;
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

            base.Play();
        }

        public override void Dispose()
        {
            base.Dispose();
            _bankFile?.Dispose();
        }
    }
}
