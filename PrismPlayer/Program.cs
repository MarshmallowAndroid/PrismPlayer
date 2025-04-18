using NAudio.Wave;
using PrismPlayer.Views;
using System.Numerics;
using System.Security.Cryptography;
using Terminal.Gui;

namespace PrismPlayer
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();

            MainView mainView;

            if (args.Length > 0 && File.Exists(args[0]))
            {
                mainView = new MainView(args[0]);
            }
            else
                mainView = new();

            ThemeManager.Instance.Theme = "Dark";
            mainView.ColorScheme = Colors.ColorSchemes["Base"];
            ConfigurationManager.Apply();

            Application.Run(mainView, (e) =>
            {
                MessageBox.ErrorQuery("Exception", $"\n {e.Message} \n\n", "OK");
                return true;
            });

            Application.Shutdown();
        }
    }
}
