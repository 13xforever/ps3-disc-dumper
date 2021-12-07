PS3 Disc Dumper
===============
This is a small utility to make decrypted copies of the PS3 game discs, suitable for use in emulators.

It does require a [compatible blu-ray drive](https://rpcs3.net/quickstart) and existence of a matching [disc key](http://www.psdevwiki.com/ps3/Bluray_disc#IRD_file) to work.

Requirements
============
* Compatible blu-ray drive
* Disc must have decryption key, either in redump database or in the IRD Library
* For binary release you might need to install .NET Core prerequisites
  * See `Supported OS versions` and `.NET Core dependencies` sections in [documentation](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore31)
* For source release you will need to have [.NET 6.0 SDK](https://www.microsoft.com/net/download) installed on your machine

How to use
==========
1. Put `ps3-disc-dumper` executable in the folder you want your dumps to be in (GUI version has configurable settings)
2. Insert a PS3 disc in the compatible drive
3. On Linux open terminal in the folder where you put the binary
    1. `$ chmod +x ps3-disc-dumper` to make it executable
    2. Mount the disc (either through file manager or manually `$ mount` it to `/media/...`)
4. Start the dumper
5. Wait for it to complete

By default all files will be copied in the folder where the dumper was started from (`.\[BLUS12345] Game Title\`).

You can pass an optional parameter with the path if you want to dump in a custom location. This is mostly useful when ran from sources with `$ dotnet run`.

If you have custom key or IRD file, you can put it in local cache (`.\ird\` on Windows, `~/.config/ps3-disc-dumper/ird/` on Linux).

Logs can be found in `/logs/` on Windows or in `~/.config/ps3-disc-dumper/logs/` on Linux.

Versions
========
There are different ways to get the dumper work:
* precompiled binaries
  * make it executable on Linux, then simply run it
* running from sources:
  * `$ git clone https://github.com/13xforever/ps3-disc-dumper.git`
  * `$ cd UI.Console`
  * `$ dotnet run`
* Alternative: Linux build via Docker
  * run `docker-compose up`
  * the executable should then be available at `./UI.Console/bin/Release/net6.0/linux-x64/ps3-disc-dumper`