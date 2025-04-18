using Common;
using Common.BankFiles;
using PrismPlayer.Players;
using Terminal.Gui;

namespace PrismPlayer.Views
{
    class MainView : Toplevel
    {
        private readonly ListView _fileListView;
        private readonly View _playerContainer;
        private readonly Timer _updateTimer;

        private string _lastPath = "";

        private BankFile? _bankFile;
        private Player? _player;
        private PlayerView? _playerView;

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

            _playerContainer = new()
            {
                X = Pos.Right(_fileListView) - 1,
                Y = 1,
                Width = Dim.Fill(7),
                Height = Dim.Fill(),
                CanFocus = true,
                BorderStyle = LineStyle.Rounded,
                SuperViewRendersLineCanvas = true
            };

            if (_playerContainer.Margin is not null)
                _playerContainer.Margin.Thickness = new Thickness(4);

            //_playerFrame.Padding = new Thickness(4);

            var vuMeterFrame = new FrameView()
            {
                Y = 1,
                Width = 7,
                BorderStyle = LineStyle.RoundedDashed
            };

            var vuMeter = new PeakMeterView()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            vuMeterFrame.Add(vuMeter);
            vuMeterFrame.Height = Dim.Fill();
            vuMeterFrame.X = Pos.Right(_playerContainer);
            //vuMeterFrame.BorderStyle = LineStyle.None;

            _fileListView.OpenSelectedItem += (s, e) =>
            {
                SelectItem(e.Item);
            };

            var menu = new MenuBar
            {
                Menus =
                [
                    new MenuBarItem("File",
                    [
                        new MenuItem("Open bank file...", "", () =>
                        {
                            OpenDialog openDialog = new()
                            {
                                Title = "Open bank file...",
                                Text = "Select a Yawaraka Engine (Soft Engine) or Katana Engine bank file"
                            };
                            if (_lastPath != string.Empty) openDialog.Path  = _lastPath;

                            Application.Run(openDialog);

                            string bankFilePath = openDialog.FilePaths[0].ToString() ?? "";
                            if (!openDialog.Canceled && File.Exists(bankFilePath))
                            {
                                InitializePlayer(bankFilePath);
                            }
                            _lastPath = openDialog.Path.ToString() ?? "";
                        }),
                        null!,
                        new MenuItem("Quit", "", () =>
                        {
                            _player?.Stop();
                            _player?.Dispose();
                            Application.RequestStop();
                        })
                    ])
                ]
            };

            Add(menu, _fileListView, _playerContainer, vuMeterFrame);

            _updateTimer = new Timer((s) =>
            {
                if (_player is null || _playerView is null) return;

                Application.Invoke(() =>
                {
                    _playerView.UpdateInfo();

                    vuMeter.LeftValue = _player.LeftSampleAverage * 4.0f;
                    vuMeter.RightValue = _player.RightSampleAverage * 4.0f;

                    //if (_player.MonitorWaveStream?.MonitoredSampleProvider.Buffer is not null)
                    //{
                    //    //waveForm.SubmitSamples(player.MonitorWaveStream.MonitoredSampleProvider.Buffer);
                    //    //waveForm.Samples = player.Monitor.MonitorSampleProvider.Buffer;
                    //}

                    //waveForm.ProgressValue = trackProgress.Value;
                    //vuMeter.Update();
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

            bankPlayer.SetBankFileInfo(selected);
            bankPlayer.Play();

            _playerView?.UpdateInfoOneShot(selected.Name);
        }

        private void InitializePlayer(string path)
        {
            try
            {
                _bankFile = BankFileDetector.DetectBankFile(File.OpenRead(path));

                if (_bankFile is null)
                {
                    MessageBox.Query(
                        "Not a valid bank file",
                        "\n Please select a valid Yawaraka Engine (Soft Engine) \n or Katana Engine bank file. \n", "OK");
                    return;
                }
            }
            catch (Exception e)
            {
                MessageBox.ErrorQuery("Error", e.Message, "OK");
                return;
            }
            _fileListView.SetSource([.. _bankFile.Subfiles]);
            _player?.Dispose();
            _player = new BankPlayer(_bankFile);
            _playerView = new PlayerView(_player)
            {
                Width = Dim.Fill(),
                Height = Dim.Fill()
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
            _playerContainer.ClearViewport();
            _playerContainer.Add(_playerView);
            _updateTimer.Change(0, 33);
        }
    }
}
