using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace PrismPlayer.Views
{
    public class TrackBarView : View
    {
        private int _maxValue;

        private float _trackBarValue;
        private float _loopStartValue;
        private float _loopEndValue;

        private int _sliderPosition;

        private bool _manualChange;

        public TrackBarView()
        {
            _maxValue = Frame.Width;

            _trackBarValue = 0;
            _loopStartValue = 0;
            _loopEndValue = 0;

            _sliderPosition = 0;

            CanFocus = true;

            AddCommand(Command.Left, (ctx) =>
            {
                DecreaseValue();
                return true;
            });
            AddCommand(Command.Right, (ctx) =>
            {
                IncreaseValue();
                return true;
            });

            KeyBindings.Add(Key.CursorLeft, Command.Left);
            KeyBindings.Add(Key.CursorRight, Command.Right);
        }

        public event Action<TrackBarChangedEventArgs>? TrackBarChanged;

        public TrackBarStyle TrackBarStyle { get; set; }

        public float Value
        {
            get => _trackBarValue;
            set
            {
                if (!_manualChange && value != _trackBarValue)
                {
                    _trackBarValue = value;
                    SetNeedsDraw();
                }
            }
        }

        public float LoopStartValue
        {
            get => _loopStartValue;
            set
            {
                _loopStartValue = value;
                SetNeedsDraw();
            }
        }

        public float LoopEndValue
        {
            get => _loopEndValue;
            set
            {
                _loopEndValue = value;
                SetNeedsDraw();
            }
        }

        public bool ContinuousUpdate { get; set; } = false;

        protected override bool OnDrawingContent()
        {
            if (Driver is null || ColorScheme is null) return true;
            if (TrackBarStyle == TrackBarStyle.Hidden) return true;

            //if (!manualChange) sliderPosition = (int)(trackBarValue * maxValue);

            int loopStartPosition = (int)(_loopStartValue * _maxValue);
            int loopEndPosition = (int)(_loopEndValue * _maxValue);
            if (loopEndPosition >= _maxValue) loopEndPosition = _maxValue - 1;

            int sliderPosition = (int)(_trackBarValue * _maxValue);
            if (sliderPosition >= _maxValue) sliderPosition = _maxValue - 1;

            Driver.SetAttribute(ColorScheme.Normal);

            Move(0, 0);

            for (int i = 0; i < _maxValue; i++)
            {
                if (i == sliderPosition)
                {
                    if (HasFocus)
                        Driver.SetAttribute(ColorScheme.Focus);
                    Driver.AddRune(new Rune('\u2588'));
                    Driver.SetAttribute(ColorScheme.Normal);
                }
                else
                {
                    if (i == loopStartPosition || i == loopEndPosition)
                    {
                        if (_loopEndValue > 0)
                        {
                            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.BrightMagenta, ColorScheme.Normal.Background));
                            if (TrackBarStyle == TrackBarStyle.Line) Driver.AddRune(new Rune('\u2666'));
                            else Driver.AddRune(new Rune('\u2592'));
                            Driver.SetAttribute(ColorScheme.Normal);
                        }
                        else DrawBackgroundChar();
                    }
                    else DrawBackgroundChar();
                }
            }
            //Move(0, 1);

            //Driver.AddStr(Value.ToString("f") + " " + sliderPosition.ToString());

            return true;
        }

        private void DrawBackgroundChar()
        {
            if (Driver is null) return;

            if (TrackBarStyle == TrackBarStyle.Line) Driver.AddRune(new Rune('\u2500'));
            else Driver.AddRune(new Rune('\u2591'));
        }

        protected override bool OnMouseEvent(MouseEventArgs eventArgs)
        {
            int location = eventArgs.Position.X;

            if (eventArgs.Flags.HasFlag(MouseFlags.Button1Pressed | MouseFlags.ReportMousePosition))
            {
                Application.GrabMouse(this);
                _manualChange = true;

                SetFocus();

                if (location >= 0 && location <= _maxValue)
                {
                    _sliderPosition = location;
                    _trackBarValue = (float)_sliderPosition / _maxValue;
                }

                SetNeedsDraw();

                if (ContinuousUpdate)
                    TrackBarChanged?.Invoke(new TrackBarChangedEventArgs(_trackBarValue));
            }
            else if (eventArgs.Flags.HasFlag(MouseFlags.Button1Released))
            {
                Application.UngrabMouse();
                _manualChange = false;

                TrackBarChanged?.Invoke(new TrackBarChangedEventArgs(_trackBarValue));
            }

            return true;
        }

        private void IncreaseValue()
        {
            float increment = 1f / _maxValue;

            if (_trackBarValue + increment <= 1.0f) _trackBarValue += increment;
            else _trackBarValue = 1.0f;

            TrackBarChanged?.Invoke(new TrackBarChangedEventArgs(_trackBarValue));
        }

        private void DecreaseValue()
        {
            float increment = 1f / _maxValue;

            if (_trackBarValue - increment >= 0.0f) _trackBarValue -= increment;
            else _trackBarValue = 0.0f;

            TrackBarChanged?.Invoke(new TrackBarChangedEventArgs(_trackBarValue));
        }

        //protected override bool OnKeyDown(Key key)
        //{
        //    float increment = 1f / maxValue;

        //    if (key.KeyCode == Key.CursorRight && key.IsShift)
        //    {
        //        if (trackBarValue + increment <= 1.0f) trackBarValue += increment;
        //        else trackBarValue = 1.0f;
        //    }
        //    else if (key.KeyCode == Key.CursorLeft && key.IsShift)
        //    {
        //        if (trackBarValue - increment >= 0.0f) trackBarValue -= increment;
        //        else trackBarValue = 0.0f;
        //    }
        //    else return true;

        //    SetNeedsDraw();

        //    TrackBarChanged?.Invoke(new TrackBarChangedEventArgs(trackBarValue));

        //    return true;
        //}

        protected override void OnSubViewLayout(LayoutEventArgs eventArgs)
        {
            _maxValue = Frame.Width;
            if (!_manualChange) _sliderPosition = (int)(_trackBarValue * _maxValue);
            base.OnSubViewLayout(eventArgs);
        }

        protected override bool OnMouseEnter(CancelEventArgs eventArgs)
        {
            Driver?.SetCursorVisibility(CursorVisibility.Invisible);
            return base.OnMouseEnter(eventArgs);
        }
    }

    public enum TrackBarStyle
    {
        Line,
        Filled,
        Hidden
    }

    public class TrackBarChangedEventArgs : EventArgs
    {
        public float Value { get; }

        public TrackBarChangedEventArgs(float value)
        {
            Value = value;
        }
    }
}
