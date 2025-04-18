using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace PrismPlayer.Views
{
    public class PeakMeterView : View
    {
        private float _leftValue;
        private float _prevLeftValue;
        private int leftPosition;

        private float _rightValue;
        private float _prevRightValue;
        private int rightPosition;

        private int _maxValue;

        private Gradient _gradient;

        public PeakMeterView()
        {
            _leftValue = 0;
            _rightValue = 0;
            _maxValue = Frame.Height - 1;

            _gradient = new ([Color.Red, Color.Yellow, Color.Green], [_maxValue > 0 ? _maxValue : 1], false);
        }

        public float LeftValue
        {
            get => _leftValue;
            set
            {
                _prevLeftValue = _leftValue;
                _leftValue = (float)Math.Round(value, 2);
                SetNeedsDraw();
            }
        }

        public float RightValue
        {
            get => _rightValue;
            set
            {
                _prevRightValue = _rightValue;
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

                Color gradientColor = _gradient.GetColorAtFraction((double) i / _maxValue);
                Driver.SetAttribute(new Attribute(gradientColor, prevAttribute.Background));
                //if (i > maxValue / 3 * 1)
                //    Driver.SetAttribute(new Attribute(Color.Yellow, prevAttribute.Background));
                //if (i > maxValue / 3 * 2)
                //    Driver.SetAttribute(new Attribute(Color.Green, prevAttribute.Background));

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

        private void DrawOn(IConsoleDriver driver)
        {
            driver.AddStr("\u2588\u2588");
        }

        private void DrawOff(IConsoleDriver driver)
        {
            //driver.AddStr("  ");
            driver.AddStr("\u2591\u2591");
        }

        int increment;

        public void Update()
        {
            //if (leftPosition <= 0) increment = 1;
            //else if (leftPosition >= maxValue) increment = -1;

            //leftPosition += increment;
            //rightPosition += increment;
        }

        protected override void OnSubViewLayout(LayoutEventArgs args)
        {
            _maxValue = Frame.Height - 1;
            _gradient = new([Color.Red, Color.Yellow, Color.Green], [_maxValue > 0 ? _maxValue : 1], false);
            //leftPosition = (int)((1f - leftValue) * maxValue);
            //rightPosition = (int)((1f - rightValue) * maxValue);
            base.OnSubViewLayout(args);
        }
    }
}
