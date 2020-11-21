// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using Microsoft.Win32;

namespace PerformanceCalculator
{
    public class OsuPathUtils
    {
        public static string GetOsuPath()
        {
            try
            {
                using RegistryKey key = Registry.ClassesRoot.OpenSubKey("osu\\shell\\open\\command");
                object o = key?.GetValue("");

                if (o == null || !(o is string pathString)) return "";

                pathString = pathString.Split(' ')[0].Trim('"');
                var pathSlices = pathString.Split(Path.DirectorySeparatorChar);
                Array.Resize(ref pathSlices, pathSlices.Length - 1);
                var finalOsuPath = string.Join(Path.DirectorySeparatorChar, pathSlices);
                return finalOsuPath;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetOsuDbPath()
        {
            return Path.Combine(GetOsuPath(), "osu!.db");
        }

        public static string GetOsuScoresDbPath()
        {
            return Path.Combine(GetOsuPath(), "scores.db");
        }

        public static string GetSongsFolderPath()
        {
            return Path.Combine(GetOsuPath(), "Songs");
        }
    }
}
