PS3 Disc Dumper
===============

This is a small utility to make decrypted copies of the PS3 game discs, suitable for use in emulators.

It does require a [compatible blu-ray drive](https://rpcs3.net/quickstart) and a matching [IRD file](http://www.psdevwiki.com/ps3/Bluray_disc#IRD_file) to work.

How to use
==========

1. Insert a PS3 disc in the compatible drive
2. Start the dumper
3. Wait for it to complete

By default all files will be copied in the folder where the dumper was started from (`.\[BLUS12345] Game Title\`).

You can pass an optional parameter with the path if you want to dump in a custom location.

If you have custom IRD file that is not available in the IRD Library, you can put it in local cache (`.\ird\`)

Requirements
============
* Compatible blu-ray drive
* Disc must have a matching IRD file
* For binary release you might need to install .NET Core prerequisites
  * See `Supported OS versions` and `.NET Core dependencies` sections in [documentation](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore21)
* For source release you will need to have [.NET Core 2.1 SDK](https://www.microsoft.com/net/download) installed on your machine