using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace IrdLibraryClient;

public static class ApiConfig
{
    public static readonly CancellationTokenSource Cts = new();
}