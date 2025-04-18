//using NAudio.SoundFont;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terminal.Gui;

//namespace AudioPlayerTUI.Views
//{
//    class WaveFormView : View
//    {
//        private float[]? points;

//        //BeatDetector beatDetector;

//        public WaveFormView()
//        {
//            //points = new WavePoint[4];
//            //points[0] = new WavePoint(0, 0);
//            //points[1] = new WavePoint(1 * (Frame.Width / 3), 6);
//            //points[2] = new WavePoint(2 * (Frame.Width / 3), -6);
//            //beatDetector = new BeatDetector(48000);
//        }

//        public float[]? Samples { get; set; }

//        public float ProgressValue { get; set; }

//        public int Center { get; set; }

//        //bool pendingBeat = false;

//        //int remainingBeatFrames = 2;

//        //char[] beatFrames =
//        //{
//        //    '░',
//        //    '▒',
//        //    '█'
//        //};

//        public override void Redraw(Rect bounds)
//        {
//            Move(0, 0);

//            //Driver.AddStr((ProgressValue * Frame.Width).ToString());
//            //points = new float[Frame.Width - 2];
//            //points = new float[3];
//            //points[0] = 0.0f;
//            //points[1] = 1.0f;
//            //points[2] = -1.0f;


//            //DrawFrame(bounds);

//            //for (int x = 0; x < Frame.Width - 2; x++)
//            //{
//            //    double equation = 24 * ((double)x / Frame.Width) + offset;
//            //    double sine = Math.Sin(equation);

//            //    if (x == (Frame.Width - 2) / 2)
//            //    points[x] = (float)sine;

//            //    offset += 0.005f;
//            //}

//            Center = (int)((float)Frame.Height / 2);

//            if (points is null) return;

//            for (int i = 0; i < points.Length; i++)
//            {
//                int x = i * (Frame.Width / points.Length);
//                int pointRelative = (int)(points[i] * (Frame.Height / 2));
//                int y = pointRelative + Center;
//                int yInverse = -pointRelative + Center;

//                Move(x, Clamp(y, 0, Frame.Height));

//                if (i > ProgressValue * Frame.Width)
//                    Driver.SetAttribute(new Terminal.Gui.Attribute(foreground: Color.DarkGray));
//                else
//                    Driver.SetAttribute(SuperView.ColorScheme.Normal);

//                if (y <= Center)
//                {
//                    Driver.AddRune('█');
//                }

//                for (int inv = Math.Min(y, yInverse); inv < Math.Max(y, yInverse); inv++)
//                {
//                    Move(x, Clamp(inv, 0, Frame.Height));
//                    Driver.AddRune('█');
//                }
//            }

//            //if (remainingBeatFrames == 0)
//            //{
//            //    pendingBeat = false;
//            //    remainingBeatFrames = 2;
//            //}
//            //else if (pendingBeat)
//            //{
//            //    remainingBeatFrames--;
//            //}
//        }

//        public override void LayoutSubviews()
//        {
//            if (Frame.Width != points?.Length)
//            {
//                points = new float[Frame.Width];
//            }

//            base.LayoutSubviews();
//        }

//        int Clamp(int value, int min, int max)
//        {
//            if (value < min) value = min;
//            else if (value > max) value = max;

//            return value;
//        }

//        //void GetPoints()
//        //{
//        //    points = new float[Frame.Width];

//        //    if (Samples is not null)
//        //    {
//        //        float[] combinedChannels = new float[Samples.Length / 2];

//        //        int combinedChannelsIndex = 0;
//        //        for (int i = 0; i < Samples.Length; i += 2)
//        //        {
//        //            float average = (Samples[i] + Samples[i + 1]) / 2;
//        //            combinedChannels[combinedChannelsIndex++] = average;
//        //            //beatDetector.ProcessAudio(average);
//        //            //if (beatDetector.BeatPulse) pendingBeat = true;
//        //        }

//        //        int inputLength = combinedChannels.Length;

//        //        for (int i = 0; i < Frame.Width; i++)
//        //        {
//        //            int inputIndex = (int)((float)i / Frame.Width * inputLength);
//        //            points[i] = combinedChannels[inputIndex];
//        //        }
//        //    }
//        //}

//        public void SubmitSamples(float[] samples)
//        {
//            if (points is null) return;

//            //float[] combinedChannels = new float[samples.Length / 2];

//            //int combinedChannelsIndex = 0;
//            //for (int i = 0; i < samples.Length; i += 2)
//            //{
//            //    float average = (samples[i] + samples[i + 1]) / 2;
//            //    combinedChannels[combinedChannelsIndex++] = average * 4f;
//            //    //beatDetector.ProcessAudio(average);
//            //    //if (beatDetector.BeatPulse) pendingBeat = true;
//            //}

//            //int inputLength = combinedChannels.Length;
//            int currentSample = 0;
//            for (int i = 0; i < Frame.Width; i++)
//            {
//                //int inputIndex = (int)((float)i / Frame.Width * inputLength);
//                float average = (Math.Abs(samples[currentSample]) + Math.Abs(samples[currentSample + 1])) / 2;
//                currentSample += 2;

//                points[i] = average * 2f;
//            }
//        }
//    }

//    struct WavePoint
//    {
//        public float X;
//        public float Y;

//        public WavePoint(float x, float y)
//        {
//            X = x;
//            Y = y;
//        }
//    }
//}
