﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using StableDiffusionGui.Main;

namespace StableDiffusionGui.Io
{
    internal static class Paths
    {
        public static string SessionTimestamp;

        public static void Init()
        {
            var n = DateTime.Now;
            SessionTimestamp = $"{n.Year}-{n.Month.ToString().PadLeft(2, '0')}-{n.Day.ToString().PadLeft(2, '0')}-{n.Hour.ToString().PadLeft(2, '0')}-{n.Minute.ToString().PadLeft(2, '0')}-{n.Second.ToString().PadLeft(2, '0')}";
        }

        public static string GetExe()
        {
            return System.Reflection.Assembly.GetEntryAssembly().GetName().CodeBase.Replace("file:///", "");
        }

        public static string GetExeDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetDataPath()
        {
            string path = Path.Combine(GetExeDir(), "Data");
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetSessionsPath()
        {
            string path = Path.Combine(GetDataPath(), "sessions");
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetLogPath(bool noSession = false)
        {
            string path = Path.Combine(GetDataPath(), "logs", (noSession ? "" : SessionTimestamp));
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetModelsPath()
        {
            string path = Path.Combine(GetDataPath(), "models");
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetSessionDataPath()
        {
            string path = Path.Combine(GetSessionsPath(), SessionTimestamp);
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetBinPath()
        {
            string path = Path.Combine(GetDataPath(), Constants.Dirs.Bins);
            Directory.CreateDirectory(path);
            return path;
        }

        public static List<FileInfo> GetModels(string pattern = "*.ckpt")
        {
            List<FileInfo> list = new List<FileInfo>();

            try
            {
                List<string> mdlFolders = new List<string>() { GetModelsPath() };

                var customModelDirsList = JsonConvert.DeserializeObject<List<string>>(Config.Get(Config.Key.customModelDirs));

                if (customModelDirsList != null)
                    mdlFolders.AddRange(customModelDirsList);

                foreach (string folderPath in mdlFolders)
                    list.AddRange(IoUtils.GetFileInfosSorted(folderPath, false, pattern).ToList());

                return list.Distinct().OrderBy(x => x.Name).ToList();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error getting models: {ex.Message}");
                Logger.Log(ex.StackTrace, true);
            }

            return list;
        }

        public static FileInfo GetModel(string filename, bool anyExtension = false)
        {
            return GetModels().FirstOrDefault(x => x.Name == filename);
        }
    }
}
