using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ps3DiscDumper.Utils;

internal class IOEx
{
    public static IEnumerable<string> GetFilepaths(string rootPath, string patternMatch, SearchOption searchOption)
    {
        IEnumerable<string> foundFiles = [];
        try
        {
            foundFiles = foundFiles.Concat(Directory.EnumerateFiles(rootPath, patternMatch));
        }
        catch (Exception e) when (e is UnauthorizedAccessException or PathTooLongException)
        {
            Console.WriteLine($"{rootPath}: {e.Message}");
        }

        if (searchOption is not SearchOption.AllDirectories)
            return foundFiles;
        
        try
        {
            var subDirs = Directory.EnumerateDirectories(rootPath);
            foreach (var dir in subDirs)
            {
                try
                {
                    var newFiles = GetFilepaths(dir, patternMatch, searchOption);
                    foundFiles = foundFiles.Concat(newFiles);
                }
                catch (Exception e) when (e is UnauthorizedAccessException or PathTooLongException)
                {
                    Console.WriteLine($"{dir}: {e.Message}");
                }
            }
        }
        catch (Exception e) when (e is UnauthorizedAccessException or PathTooLongException)
        {
            Console.WriteLine($"{rootPath}: {e.Message}");
        }
        return foundFiles;
    }

    public static IEnumerable<string> GetFilepaths(string rootPath, string patternMatch, int maxLevel)
        => GetFilepaths(rootPath, patternMatch, 0, maxLevel);

    private static IEnumerable<string> GetFilepaths(string rootPath, string patternMatch, int currentLevel, int maxLevel)
    {
        IEnumerable<string> foundFiles = [];
        try
        {
            foundFiles = foundFiles.Concat(Directory.EnumerateFiles(rootPath, patternMatch));
        }
        catch (Exception e) when (e is UnauthorizedAccessException or PathTooLongException)
        {
            Console.WriteLine($"{rootPath}: {e.Message}");
        }

        if (currentLevel >= maxLevel)
            return foundFiles;
        
        try
        {
            var subDirs = Directory.EnumerateDirectories(rootPath);
            foreach (var dir in subDirs)
            {
                try
                {
                    var newFiles = GetFilepaths(dir, patternMatch, currentLevel+1, maxLevel);
                    foundFiles = foundFiles.Concat(newFiles);
                }
                catch (Exception e) when (e is UnauthorizedAccessException or PathTooLongException)
                {
                    Console.WriteLine($"{dir}: {e.Message}");
                }
            }
        }
        catch (Exception e) when (e is UnauthorizedAccessException or PathTooLongException)
        {
            Console.WriteLine($"{rootPath}: {e.Message}");
        }
        return foundFiles;
    }
}