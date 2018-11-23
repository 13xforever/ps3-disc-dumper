using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ps3DiscDumper.Utils
{
    internal class IOEx
    {
        public static IEnumerable<string> GetFilepaths(string rootPath, string patternMatch, SearchOption searchOption)
        {
            var foundFiles = Enumerable.Empty<string>();
            try
            {
                foundFiles = foundFiles.Concat(Directory.EnumerateFiles(rootPath, patternMatch));
            }
            catch (Exception e) when (e is UnauthorizedAccessException || e is PathTooLongException)
            {
                Console.WriteLine($"{rootPath}: {e.Message}");
            }

            if (searchOption == SearchOption.AllDirectories)
            {
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
                        catch (Exception e) when (e is UnauthorizedAccessException || e is PathTooLongException)
                        {
                            Console.WriteLine($"{dir}: {e.Message}");
                        }
                    }
                }
                catch (Exception e) when (e is UnauthorizedAccessException || e is PathTooLongException)
                {
                    Console.WriteLine($"{rootPath}: {e.Message}");
                }
            }
            return foundFiles;
        }
    }
}
