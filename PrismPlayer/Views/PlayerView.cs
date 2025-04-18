using PrismPlayer.Players;
using Terminal.Gui;
using static System.Net.Mime.MediaTypeNames;

namespace PrismPlayer.Views
{
    class PlayerView : View
    {
        private readonly Label _name;
        private readonly Label _elapsed;
        private readonly Label _length;
        private readonly TrackBarView _trackProgress;
        private readonly Button _playPause;
        private readonly Button _previous;
        private readonly Button _next;
        private readonly Button _loop;
        private readonly TrackBarView _volume;
        private readonly Label _volumeValue;
        private readonly Player _player;

        public PlayerView(Player player)
        {
            _player = player;

            _name = new Label()
            {
                X = Pos.Center(),
                Y = Pos.Percent(10)
            };
            _elapsed = new Label();
            _length = new Label();

            //var waveForm = new WaveFormView()
            //{
            //    //Y = 2,
            //    Y = Pos.Center(),
            //    Width = Dim.Fill(),
            //    //Height = Dim.Percent(50)
            //    Height = Dim.Percent(50)
            //};

            //playerFrame.Add(waveForm);

            _trackProgress = new()
            {
                Width = Dim.Fill(),
                Height = 1,
                X = Pos.Center(),
                Y = Pos.Percent(40)
            };


            _elapsed.Y = Pos.Top(_trackProgress) - 2;

            _length.X = Pos.AnchorEnd();
            _length.Y = Pos.Top(_trackProgress) - 2;

            _trackProgress.TrackBarChanged += (e) =>
            {
                player.TrackProgress = e.Value;
            };

            _playPause = new Button()
            {
                Text = "►",
                X = Pos.Center(),
                Y = Pos.Bottom(_trackProgress) + 1,
                NoDecorations = true,
                HighlightStyle = HighlightStyle.None
            };
            _previous = new Button()
            {
                Text = "◄◄",
                Y = Pos.Y(_playPause),
                NoDecorations = true,
                HighlightStyle = HighlightStyle.None
            };
            _next = new Button()
            {
                Text = "►►",
                Y = Pos.Y(_playPause),
                NoDecorations = true,
                HighlightStyle = HighlightStyle.None
            };
            _loop = new Button()
            {
                X = Pos.Center(),
                Y = Pos.Bottom(_playPause) + 1,
                NoDecorations = true,
                HighlightStyle = HighlightStyle.None
            };

            _previous.X = Pos.Left(_playPause) - (Pos.Right(_previous) - Pos.Left(_previous)) - 5;
            _next.X = Pos.Right(_playPause) + 5;
            //loop.Text = player.LoopEnabled ? "Loop: On" : "Loop: Off";

            _playPause.Accepting += (s, e) =>
            {
                player.PauseResume();
                if (player.PlaybackState == NAudio.Wave.PlaybackState.Paused)
                    _playPause.Text = "▼";
                else
                    _playPause.Text = "►";
            };
            _previous.Accepting += (s, e) =>
            {
                PreviousRequested?.Invoke();
            };
            _next.Accepting += (s, e) =>
            {
                NextRequested?.Invoke();
            };
            _loop.Accepting += (s, e) =>
            {
                player.LoopEnabled = !player.LoopEnabled;
            };

            _volume = new TrackBarView()
            {
                Width = Dim.Percent(50),
                Height = 1,
                X = Pos.Center(),
                //Y = Pos.Bottom(playerFrame) - Pos.Top(playerFrame) - 6,
                TrackBarStyle = TrackBarStyle.Filled,
                ContinuousUpdate = true
            };
            _volumeValue = new Label()
            {
                X = Pos.Center()
            };

            _volume.Y = Pos.Top(_volumeValue) - 2;
            _volume.Value = player.OutputDevice?.Volume ?? 0;

            _volumeValue.Y = Pos.AnchorEnd() - 1;
            _volumeValue.Text = ((int)Math.Round(_volume.Value * 100)).ToString() + "%";

            _volume.TrackBarChanged += (e) =>
            {
                if (player.OutputDevice is not null)
                    player.OutputDevice.Volume = (float)Math.Round(e.Value, 2);

                _volumeValue.Text = ((int)Math.Round(_volume.Value * 100)).ToString() + "%";
            };

            Add(_name);

            Add(_trackProgress);
            Add(_elapsed);
            Add(_length);

            Add(_previous);
            Add(_playPause);
            Add(_next);
            Add(_loop);

            Add(_volume);
            Add(_volumeValue);

            //playerFrame.LayoutSubviews();
            //vuMeterFrame.LayoutSubviews();
            CanFocus = true;
            TabStop = TabBehavior.TabGroup;
        }

        public event Action? PreviousRequested;

        public event Action? NextRequested;

        public void UpdateInfoOneShot(string name)
        {
            _name.ColorScheme = new()
            {
                Normal = new Terminal.Gui.Attribute(Color.BrightYellow, ColorScheme?.Normal.Background ?? Color.Black)
            };

            _elapsed.ColorScheme = new()
            {
                Normal = new Terminal.Gui.Attribute(Color.White, ColorScheme?.Normal.Background ?? Color.Black)
            };
            _length.ColorScheme = _elapsed.ColorScheme;

            _trackProgress.ColorScheme = new()
            {
                Normal = new Terminal.Gui.Attribute(Color.BrightCyan, ColorScheme?.Normal.Background ?? Color.Black)
            };

            _playPause.ColorScheme = new()
            {
                Normal = new Terminal.Gui.Attribute(Color.BrightGreen, ColorScheme?.Normal.Background ?? Color.Black)
            };
            _previous.ColorScheme = _playPause.ColorScheme;
            _next.ColorScheme = _playPause.ColorScheme;
            _loop.ColorScheme = _playPause.ColorScheme;

            _volume.ColorScheme = new()
            {
                Normal = new Terminal.Gui.Attribute(Color.BrightMagenta, ColorScheme?.Normal.Background ?? Color.Black)
            };
            _volumeValue.ColorScheme = new()
            {
                Normal = new Terminal.Gui.Attribute(Color.Gray, ColorScheme?.Normal.Background ?? Color.Black)
            };

            _name.Text = name;
            _trackProgress.LoopStartValue = _player.LoopStartPercent;
            _trackProgress.LoopEndValue = _player.LoopEndPercent;
        }

        public void UpdateInfo()
        {
            if (_player.AudioFormat is not null)
            {
                long positionSamples = _player.Position / _player.AudioFormat.BlockAlign;
                _elapsed.Text = string.Format("{0:mm\\:ss}", TimeSpan.FromSeconds(positionSamples / _player.AudioFormat.SampleRate));

                long lengthSamples = _player.Length / _player.AudioFormat.BlockAlign;
                _length.Text = string.Format("{0:mm\\:ss}", TimeSpan.FromSeconds(lengthSamples / _player.AudioFormat.SampleRate));
            }

            _trackProgress.Value = _player.TrackProgress;

            _loop.Text = _player.LoopEnabled ? "Loop: On" : "Loop: Off";

            _volume.Value = _player.OutputDevice?.Volume ?? 0;
            _volumeValue.Text = ((int)Math.Round(_volume.Value * 100)).ToString() + "%";
        }
    }
}
