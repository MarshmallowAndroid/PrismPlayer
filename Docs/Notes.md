# Notes

Games tested:
- Venus Vacation PRISM - DEAD OR ALIVE Xtreme -
- DEAD OR ALIVE Xtreme Venus Vacation
- DEAD OR ALIVE 6
- DEAD OR ALIVE 5 Last Round

It must be noted that the tested games are developed by Team Ninja.
I'm unsure whether games developed by the other teams under KOEI TECMO will work
as each team seems to package their files differently.[^1]

---

Subfiles will have a different representation depending on the type of file.
- Katana Engine (Venus Vacation PRISM, DOA6, etc.) subfiles will display as an internal identifier.
- Yawaraka (Soft) Engine (Xtreme Venus Vacation, DOA5LR, etc.) subfiles will display as offsets in the file.

---

VVP and DOA6 use the same name and relative path for their BGM sound bank, `data\0x272c6efb.file`.
Must be a Katana Engine thing.

The BGM sound bank for XVV is the second largest file in the `DX11_global_data\COMMON`
directory, `b3c5a6c36e6b29127ab013fffe1d3d784198af2fc3e01f5274762f71fcedc8ea`.

DOA5LR's sound banks mostly work, but some subfiles point to a header that references several KOVS streams,
which I might try to support.
Its files in the `DATA` and `DATA2` folder seem to consist entirely of sound banks.

[^1]: [Publisher · KOEI TECMO GAMES CO., LTD. · SteamDB](https://steamdb.info/publisher/KOEI+TECMO+GAMES+CO.%2C+LTD./)