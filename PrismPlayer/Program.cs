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
            //Application.Init();
            ////Application.HeightAsBuffer = true;

            //SetupMainViews();

            //if (args.Length > 0 && File.Exists(args[0]))
            //{
            //    SetupBankPlayer(args[0], fileListView);

            //    if (args.Length > 1)
            //    {
            //        //var bankPlayer = player as BankPlayer;

            //        foreach (var bankFileInfo in bankFile.BankFileInfos)
            //        {
            //            if (bankFileInfo.ToString().Equals(args[1]))
            //            {
            //                fileListView.SelectedItem = fileListView.Source.ToList().IndexOf(bankFileInfo);
            //                BeginPlayback(bankFileInfo);
            //            }
            //        }
            //    }
            //}

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
