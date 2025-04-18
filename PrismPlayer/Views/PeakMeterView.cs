using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace PrismPlayer.Views
{
    public class PeakMeterView : View
    {
        private float _leftValue;
        private float _rightValue;

        private int _maxValue;

        private Gradient _gradient;

        public PeakMeterView()
        {
            _leftValue = 0;
            _rightValue = 0;
            _maxValue = Frame.Height - 1;

            _gradient = new([Color.Red, Color.Yellow, Color.Green], [_maxValue > 0 ? _maxValue : 1], false);
        }

        public float LeftValue
        {
            get => _leftValue;
            set
            {
                _leftValue = (float)Math.Round(value, 2);
                SetNeedsDraw();
            }
        }

        public float RightValue
        {
            get => _rightValue;
            set
            {
                _rightValue = (float)Math.Round(value, 2);
                SetNeedsDraw();
            }
        }

        protected override bool OnDrawingContent()
        {
            if (Driver is null) return true;

            int leftPosition = CalculatePosition(_leftValue);
            int rightPosition = CalculatePosition(_rightValue);

            Attribute prevAttribute = Driver.GetAttribute();

            int i;
            for (i = 0; i < _maxValue; i++)
            {
                Move(0, i);

                Color gradientColor = _gradient.GetColorAtFraction((double)i / _maxValue);
                Driver.SetAttribute(new Attribute(gradientColor, prevAttribute.Background));

                if (i >= leftPosition)
                    DrawOn(Driver);
                else DrawOff(Driver);

                Driver.AddStr(" ");

                if (i >= rightPosition)
                    DrawOn(Driver);
                else DrawOff(Driver);
            }

            Driver.SetAttribute(new Attribute(Color.White, prevAttribute.Background));

            Move(0, i);

            Driver.AddStr("L  R");

            return true;
        }

        private int CalculatePosition(float value) => (int)((1f - value) * _maxValue);

        private static void DrawOn(IConsoleDriver driver)
        {
            driver.AddStr("\u2588\u2588");
        }

        private static void DrawOff(IConsoleDriver driver)
        {
            //driver.AddStr("  ");
            driver.AddStr("\u2591\u2591");
        }

        protected override void OnSubViewLayout(LayoutEventArgs args)
        {
            _maxValue = Frame.Height - 1;
            _gradient = new([Color.Red, Color.Yellow, Color.Green], [_maxValue > 0 ? _maxValue : 1], false);
            base.OnSubViewLayout(args);
        }
    }
}
