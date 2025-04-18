using PrismPlayer.Players;
using Terminal.Gui;

namespace PrismPlayer.Views
{
    class PlayerView : View
    {
        private const string PlayText = " ► ";
        private const string PauseText = " ▼ ";
        private const string PreviousText = " ◄◄ ";
        private const string NextText = " ►► ";
        private const string LoopOnText = " Loop: On ";
        private const string LoopOffText = " Loop: Off ";

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

        private Player? _player;
        private bool _loopSetting = true;

        public PlayerView()
        {
            _name = new Label()
            {
                X = Pos.Center(),
                Y = Pos.Percent(10)
            };
            _elapsed = new Label();
            _length = new Label();

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
                if (_player is null) return;

                _player.TrackProgress = e.Value;
            };

            _playPause = new Button()
            {
                Text = PlayText,
                X = Pos.Center(),
                Y = Pos.Bottom(_trackProgress) + 1,
                NoDecorations = true,
                HighlightStyle = HighlightStyle.None
            };
            _previous = new Button()
            {
                Text = PreviousText,
                Y = Pos.Y(_playPause),
                NoDecorations = true,
                HighlightStyle = HighlightStyle.None
            };
            _next = new Button()
            {
                Text = NextText,
                Y = Pos.Y(_playPause),
                NoDecorations = true,
                HighlightStyle = HighlightStyle.None
            };
            _loop = new Button()
            {
                Text = " Loop:  ",
                X = Pos.Center(),
                Y = Pos.Bottom(_playPause) + 1,
                NoDecorations = true,
                HighlightStyle = HighlightStyle.None
            };

            _previous.X = Pos.Left(_playPause) - (Pos.Right(_previous) - Pos.Left(_previous)) - 5;
            _next.X = Pos.Right(_playPause) + 5;

            _playPause.Accepting += (s, e) =>
            {
                if (_player is null) return;

                _player.PauseResume();
                if (_player.PlaybackState == NAudio.Wave.PlaybackState.Paused)
                    _playPause.Text = PauseText;
                else
                    _playPause.Text = PlayText;
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
                if (_player is null) return;

                _player.LoopEnabled = !_player.LoopEnabled;
                _loop.Text = _player.LoopEnabled ? LoopOnText : LoopOffText;
                _loopSetting = _player.LoopEnabled;
            };

            _volume = new TrackBarView()
            {
                Width = Dim.Percent(50),
                Height = 1,
                X = Pos.Center(),
                TrackBarStyle = TrackBarStyle.Filled,
                ContinuousUpdate = true
            };
            _volumeValue = new Label()
            {
                X = Pos.Center()
            };

            _volume.Y = Pos.Top(_volumeValue) - 2;
            _volumeValue.Y = Pos.AnchorEnd() - 1;

            _volume.TrackBarChanged += (e) =>
            {
                if (_player?.OutputDevice is not null)
                    _player.OutputDevice.Volume = (float)Math.Round(e.Value, 2);

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

            CanFocus = true;
            //TabStop = TabBehavior.TabGroup;
        }

        public event Action? PreviousRequested;

        public event Action? NextRequested;

        public void Initialize(Player player)
        {
            _player = player;

            _name.Text = "";

            _elapsed.Text = GetTimeStringFromSamples(0);
            _length.Text = GetTimeStringFromSamples(0);

            _trackProgress.Value = 0f;
            _trackProgress.LoopStartValue = 0f;
            _trackProgress.LoopEndValue = 0f;

            _volume.Value = _player.OutputDevice?.Volume ?? 0;
            _volumeValue.Text = ((int)Math.Round(_volume.Value * 100)).ToString() + "%";
        }

        public void UpdateInfoOneShot(string name)
        {
            if (_player is null) return;

            _player.LoopEnabled = _loopSetting;

            _name.Text = name;
            _name.ColorScheme = new()
            {
                Normal = new Terminal.Gui.Attribute(Color.BrightYellow, ColorScheme?.Normal.Background ?? Color.Black)
            };

            _elapsed.ColorScheme = new()
            {
                Normal = new Terminal.Gui.Attribute(Color.White, ColorScheme?.Normal.Background ?? Color.Black)
            };
            _length.ColorScheme = _elapsed.ColorScheme;

            _trackProgress.LoopStartValue = _player.LoopStartPercent;
            _trackProgress.LoopEndValue = _player.LoopEndPercent;
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

            _loop.Text = _player.LoopEnabled ? LoopOnText : LoopOffText;
            _loop.ColorScheme = _playPause.ColorScheme;

            _volume.ColorScheme = new()
            {
                Normal = new Terminal.Gui.Attribute(Color.BrightMagenta, ColorScheme?.Normal.Background ?? Color.Black)
            };
            _volumeValue.ColorScheme = new()
            {
                Normal = new Terminal.Gui.Attribute(Color.Gray, ColorScheme?.Normal.Background ?? Color.Black)
            };
        }

        public void UpdateInfo()
        {
            if (_player is null) return;

            if (_player.AudioFormat is not null)
            {
                long positionSamples = _player.Position / _player.AudioFormat.BlockAlign;
                _elapsed.Text = GetTimeStringFromSamples(positionSamples);

                long lengthSamples = _player.Length / _player.AudioFormat.BlockAlign;
                _length.Text = GetTimeStringFromSamples(lengthSamples);
            }

            _trackProgress.Value = _player.TrackProgress;

            _volume.Value = _player.OutputDevice?.Volume ?? 0;
            _volumeValue.Text = ((int)Math.Round(_volume.Value * 100)).ToString() + "%";
        }

        private string GetTimeStringFromSamples(long value)
        {
            if (_player?.AudioFormat is not null)
                return string.Format("{0:mm\\:ss}", TimeSpan.FromSeconds(value / _player.AudioFormat.SampleRate));
            else
                return string.Format("{0:mm\\:ss}", TimeSpan.FromSeconds(0));
        }
    }
}
