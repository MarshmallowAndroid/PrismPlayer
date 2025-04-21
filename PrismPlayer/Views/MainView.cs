using Common;
using Common.BankFiles;
using PrismPlayer.Players;
using Terminal.Gui;

namespace PrismPlayer.Views
{
    class MainView : Toplevel
    {
        private readonly ListView _fileListView;
        private readonly Timer _updateTimer;

        private string _lastPath = "";

        private BankFile? _bankFile;
        private Player? _player;
        private PlayerView _playerView;

        public MainView()
        {
            _fileListView = new()
            {
                Y = 1,
                Width = 20,
                Height = Dim.Fill(),
                BorderStyle = LineStyle.Rounded,
                SuperViewRendersLineCanvas = true
            };

            _fileListView.VerticalScrollBar.AutoShow = true;

            _playerView = new()
            {
                X = Pos.Right(_fileListView),
                Y = Pos.Center() + 1,
                Width = Dim.Fill(7),
                Height = Dim.Fill(),
                CanFocus = true,
                BorderStyle = LineStyle.Rounded,
                SuperViewRendersLineCanvas = true
            };

            _playerView.PreviousRequested += () =>
            {
                if (_fileListView.SelectedItem - 1 < 0)
                    _fileListView.SelectedItem = _fileListView.Source.Count - 1;
                else
                    _fileListView.SelectedItem--;

                SelectItem(_fileListView.SelectedItem);
            };
            _playerView.NextRequested += () =>
            {
                if (_fileListView.SelectedItem + 1 >= _fileListView.Source.Count)
                    _fileListView.SelectedItem = 0;
                else
                    _fileListView.SelectedItem++;

                SelectItem(_fileListView.SelectedItem);
            };

            //if (_playerView.Margin is not null)
            //    _playerView.Margin.Thickness = new Thickness(4);

            //_playerFrame.Padding = new Thickness(4);

            var vuMeterFrame = new FrameView()
            {
                Y = 1,
                Width = 7,
                BorderStyle = LineStyle.RoundedDashed,
                TabStop = TabBehavior.NoStop
            };

            var vuMeter = new VUMeterView()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            vuMeterFrame.Add(vuMeter);
            vuMeterFrame.Height = Dim.Fill();
            vuMeterFrame.X = Pos.AnchorEnd();
            //vuMeterFrame.BorderStyle = LineStyle.None;

            _fileListView.OpenSelectedItem += (s, e) =>
            {
                SelectItem(e.Item);
            };

            var menu = new MenuBar
            {
                Menus =
                [
                    new MenuBarItem("_File",
                    [
                        new MenuItem("_Open bank file...", "", () =>
                        {
                            FileDialog fileDialog = new()
                            {
                                Title = "Open bank file...",
                                OpenMode = OpenMode.File
                            };
                            if (_lastPath != string.Empty) fileDialog.Path  = _lastPath;

                            Application.Run(fileDialog);

                            if (fileDialog.Path != _lastPath)
                            {
                                string bankFilePath = fileDialog.Path ?? "";

                                if (File.Exists(bankFilePath))
                                    InitializePlayer(bankFilePath);

                                _lastPath = fileDialog.Path ?? "";
                            }
                        }, shortcutKey: Key.O.WithCtrl),
                        new MenuItem("_Export Ogg Vorbis", "", () =>
                        {
                            if (_player is not BankPlayer bankPlayer) return;
                            if (bankPlayer.ExportCurrent(out string fileName, "ogg"))
                            {
                                MessageBox.Query("Export complete", $"\n Exported file as {fileName} \n\n", "OK");
                            }

                        }, shortcutKey: Key.E.WithCtrl),
                        new MenuItem("Export as _WAV", "", () =>
                        {
                            if (_player is not BankPlayer bankPlayer) return;
                            if (bankPlayer.ExportCurrent(out string fileName, "wav"))
                            {
                                MessageBox.Query("Export complete", $"\n Exported file as {fileName} \n\n", "OK");
                            }

                        }, shortcutKey: Key.E.WithCtrl.WithAlt),
                        null!,
                        new MenuItem("_Quit", "", () =>
                        {
                            _player?.Stop();
                            _player?.Dispose();
                            Application.RequestStop();
                        }, shortcutKey: Application.QuitKey)
                    ])
                ]
            };

            Add(menu, _fileListView, _playerView, vuMeterFrame);

            _updateTimer = new Timer((s) =>
            {
                if (_player is null || _playerView is null) return;
                if (_player.PlaybackState != NAudio.Wave.PlaybackState.Playing) return;

                Application.Invoke(() =>
                {
                    _playerView.UpdateInfo();

                    vuMeter.LeftValue = _player.LeftSampleAverage * 4.0f;
                    vuMeter.RightValue = _player.RightSampleAverage * 4.0f;
                });
            }, null, System.Threading.Timeout.Infinite, 16);
        }

        public MainView(string path) : this()
        {
            _lastPath = path;
            InitializePlayer(path);
        }

        private void SelectItem(int index)
        {
            if (_player is not BankPlayer bankPlayer) return;
            if (_bankFile is null) return;

            Subfile selected = _bankFile.Subfiles[index];

            bankPlayer.SetSubfile(selected);
            bankPlayer.Play();

            _playerView?.UpdateInfoOneShot(selected.Name);
        }

        private void InitializePlayer(string path)
        {
            _player?.Dispose();
            _bankFile?.Dispose();
            try
            {
                _bankFile = BankFileDetector.DetectBankFile(File.OpenRead(path));

                if (_bankFile is null)
                {
                    MessageBox.ErrorQuery(
                        "Not a valid bank file",
                        "\n Please select a valid Yawaraka Engine (Soft Engine) \n or Katana Engine bank file. \n\n", "OK");
                    return;
                }
            }
            catch (Exception e)
            {
                MessageBox.ErrorQuery("Error", e.Message, "OK");
                return;
            }
            _fileListView.SetSource([.. _bankFile.Subfiles]);
            _fileListView.SelectedItem = 0;
            _player = new BankPlayer(_bankFile);
            _playerView.Initialize(_player);
            _updateTimer.Change(0, 33);
        }
    }
}
