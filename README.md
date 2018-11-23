PS3 Disc Dumper
===============

This is a small utility to make decrypted copies of the PS3 game discs, suitable for use in emulators.

It does require a [compatible blu-ray drive](https://rpcs3.net/quickstart) and a matching [IRD file](http://www.psdevwiki.com/ps3/Bluray_disc#IRD_file) to work.

Versions
========

There are different versions available on the download page for releases, so here's a list:
* `ps3-disc-dumper.net472.exe` - this is .NET version that requires .NET Framework 4.7.2 to run
* `ps3-disc-dumper.exe` - native x64 Windows binary, only requires VC++ 2017 Redistributable to run
* `ps3-disc-dumper` or `ps3-disc-dumper.elf` - native Linux binary, only requires setting the executable bit to run
* `ps3-disc-dumper.gz` or `ps3-disc-dumper.elf.gz` - the same native Linux binary, but gzipped to save on download size, requires unzipping and setting the execution bit afterwards
* running from sources using .NET Core SDK 2.1 or newer requires cloning this repository (or downloading a source package from the Releases section), and .NET Core SDK to be installed

How to use
==========

1. Put `ps3-disc-dumper` executable in the folder you want your dumps to be in
2. Insert a PS3 disc in the compatible drive
3. On Linux open terminal in the folder where you put the binary
    1. `$ gzip -d ps3-disc-dumper.gz` to unpack the binary if needed
    2. `$ chmod +x ps3-disc-dumper` make it executable
    3. Mount the disc (either through file manager or manual `$ mount` to `/media/...`)
3. Start the dumper
4. Wait for it to complete

By default all files will be copied in the folder where the dumper was started from (`.\[BLUS12345] Game Title\`).

You can pass an optional parameter with the path if you want to dump in a custom location. This is mostly useful when ran from sources with `$ dotnet run`.

If you have custom IRD file that is not available in the IRD Library, you can put it in local cache (`.\ird\` on Windows, `~/.config/ps3-disc-dumper/ird/` on Linux).

Logs can be found in `/logs/` on Windows or in `/var/log/ps3-disc-dumper` on Linux.

Requirements
============
* Compatible blu-ray drive
* Disc must have a matching IRD file
* For binary release you might need to install .NET Core prerequisites
  * See `Supported OS versions` and `.NET Core dependencies` sections in [documentation](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore21)
* For source release you will need to have [.NET Core 2.1 SDK](https://www.microsoft.com/net/download) installed on your machine