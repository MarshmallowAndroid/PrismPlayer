# PrismPlayer

Audio player built with [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui) for listening to Venus Vacation PRISM and Xtreme Venus Vacation sound banks.

![Player screenshot](Docs/Player.png)

---

## System requirements
- [.NET Runtime 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or higher. Get the SDK instead if you intend to build the project.
- [Windows Terminal](https://github.com/microsoft/terminal) is recommended.
  The regular Windows Console Host does not support some glyphs and compatibility with
  other terminals and platforms have not been tested.

  ---

## Usage

### Running from the command line
#### PowerShell
```
.\PrismPlayer.exe <path_to_bank_file>
```
#### Command Prompt
```
PrismPlayer <path_to_bank_file>
```
If no arguments are passed, the program will still launch but you will
have to use the `File > Open bank file...` menu in the interface.
![File select screenshot](Docs/FileSelect.png)

---

## Controls
Both keyboard and mouse input are supported.

- `Alt`+`F`: Open the File menu.
- `Tab` or `Right Arrow`: Focus the next control.
- `Shift`+`Tab` or `Left Arrow`: Focus the previous control.
- `Shift` and `Left Arrow` or `Right Arrow`: Change the track bar value.
- `Ctrl`+`E`: Export Ogg file.
- `Ctrl`+`Shift`+`E`: Export as WAV file.

---

## Building

Clone the repository recursively.
```
git clone --recurse-submodules https://github.com/MarshmallowAndroid/PrismPlayer.git
```

Open `PrismPlayer.sln`, then build `PrismPlayer`.

---

## Libraries
- [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui) - TUI library
- [NAudio](https://github.com/naudio/NAudio) - Audio library
- [VorbisPizza](https://github.com/TechPizzaDev/VorbisPizza) - Fork of the [NVorbis](https://github.com/NVorbis/NVorbis) Ogg Vorbis decoder with updated implementation
- [NAudio.Vorbis](https://github.com/naudio/Vorbis) - Wrapper for NVorbis slightly modified to work with VorbisPizza

---

## Todo

- [ ] Export support
- [ ] Improved keyboard shortcuts