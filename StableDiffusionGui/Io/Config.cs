﻿using StableDiffusionGui.Forms;
using Newtonsoft.Json;
using StableDiffusionGui;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using StableDiffusionGui.Main;
using System.Linq;
using StableDiffusionGui.Os;

namespace StableDiffusionGui.Io
{
    static class Config
    {
        public static bool Ready = false;
        public static string ConfigPath = "";
        private static Dictionary<string, string> _cachedConfig = new Dictionary<string, string>();

        public static void Init()
        {
            ConfigPath = Path.Combine(Paths.GetDataPath(), Constants.Files.Config);
            IoUtils.CreateFileIfNotExists(ConfigPath);
            Reload();
            Ready = true;
        }

        // public static async Task Reset(int retries = 3, SettingsForm settingsForm = null)
        // {
        //     try
        //     {
        //         if (settingsForm != null)
        //             settingsForm.Enabled = false;
        // 
        //         File.Delete(configPath);
        //         await Task.Delay(100);
        //         cachedValues.Clear();
        //         await Task.Delay(100);
        // 
        //         if (settingsForm != null)
        //             settingsForm.Enabled = true;
        //     }
        //     catch (Exception e)
        //     {
        //         retries -= 1;
        //         Logger.Log($"Failed to reset config: {e.Message}. Retrying ({retries} attempts left).", true);
        //         await Task.Delay(500);
        //         await Reset(retries, settingsForm);
        //     }
        // }

        public static void Set(Key key, string value)
        {
            Set(key.ToString(), value);
        }

        public static void Set(string str, string value)
        {
            Reload();
            _cachedConfig[str] = value;
            WriteConfig();
        }

        public static void Set(Dictionary<string, string> keyValuePairs)
        {
            Reload();

            foreach (var entry in keyValuePairs)
                _cachedConfig[entry.Key] = entry.Value;

            WriteConfig();
        }

        private static void WriteConfig()
        {
            SortedDictionary<string, string> cachedValuesSorted = new SortedDictionary<string, string>(_cachedConfig);
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(cachedValuesSorted, Newtonsoft.Json.Formatting.Indented));
        }

        private static void Reload()
        {
            try
            {
                Dictionary<string, string> newDict = new Dictionary<string, string>();
                var deserializedConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ConfigPath));

                if (deserializedConfig == null)
                    deserializedConfig = new Dictionary<string, string>();

                foreach (var entry in deserializedConfig)
                    newDict.Add(entry.Key, entry.Value);

                _cachedConfig = newDict; // Use temp dict and only copy it back if no exception was thrown
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to reload config! {e.Message}", true);
            }
        }

        // Get using fixed key
        public static string Get(Key key, string defaultVal)
        {
            WriteIfDoesntExist(key.ToString(), defaultVal);
            return Get(key);
        }

        // Get using string
        public static string Get(string key, string defaultVal)
        {
            WriteIfDoesntExist(key, defaultVal);
            return Get(key);
        }

        public static string Get(Key key, Type type = Type.String)
        {
            return Get(key.ToString(), type);
        }

        public static string Get(string key, Type type = Type.String)
        {
            string keyStr = key.ToString();

            try
            {
                if (_cachedConfig.ContainsKey(keyStr))
                    return _cachedConfig[keyStr];

                return WriteDefaultValIfExists(key.ToString(), type);
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to get {keyStr.Wrap()} from config! {e.Message}");
            }

            return null;
        }

        #region Get Bool

        public static bool GetBool(Key key)
        {
            return Get(key, Type.Bool).GetBool();
        }

        public static bool GetBool(Key key, bool defaultVal = false)
        {
            WriteIfDoesntExist(key.ToString(), (defaultVal ? "True" : "False"));
            return Get(key, Type.Bool).GetBool();
        }

        public static bool GetBool(string key)
        {
            return Get(key, Type.Bool).GetBool();
        }

        public static bool GetBool(string key, bool defaultVal)
        {
            WriteIfDoesntExist(key.ToString(), (defaultVal ? "True" : "False"));
            return bool.Parse(Get(key, Type.Bool));
        }

        #endregion

        #region Get Int

        public static int GetInt(Key key)
        {
            return Get(key, Type.Int).GetInt();
        }

        public static int GetInt(Key key, int defaultVal)
        {
            WriteIfDoesntExist(key.ToString(), defaultVal.ToString());
            return GetInt(key);
        }

        public static int GetInt(string key)
        {
            return Get(key, Type.Int).GetInt();
        }

        public static int GetInt(string key, int defaultVal)
        {
            WriteIfDoesntExist(key.ToString(), defaultVal.ToString());
            return GetInt(key);
        }

        #endregion

        #region Get Float

        public static float GetFloat(Key key)
        {
            return Get(key, Type.Float).GetFloat();
        }

        public static float GetFloat(Key key, float defaultVal)
        {
            WriteIfDoesntExist(key.ToString(), defaultVal.ToStringDot());
            return Get(key, Type.Float).GetFloat();
        }

        public static float GetFloat(string key)
        {
            return Get(key, Type.Float).GetFloat();
        }

        public static float GetFloat(string key, float defaultVal)
        {
            WriteIfDoesntExist(key.ToString(), defaultVal.ToStringDot());
            return Get(key, Type.Float).GetFloat();
        }

        public static string GetFloatString(Key key)
        {
            return Get(key, Type.Float).Replace(",", ".");
        }

        public static string GetFloatString(string key)
        {
            return Get(key, Type.Float).Replace(",", ".");
        }

        #endregion

        static void WriteIfDoesntExist(string key, string val)
        {
            if (_cachedConfig.ContainsKey(key.ToString()))
                return;

            Set(key, val);
        }

        public enum Type { String, Int, Float, Bool }
        private static string WriteDefaultValIfExists(string keyStr, Type type)
        {
            Key key;

            try
            {
                key = (Key)Enum.Parse(typeof(Key), keyStr);
            }
            catch
            {
                key = Key.none;
            }

            if (key == Key.checkboxMultiPromptsSameSeed) return WriteDefault(key, "True");
            if (key == Key.sliderInitStrength) return WriteDefault(key, "10");
            if (key == Key.sliderResW) return WriteDefault(key, "512");
            if (key == Key.sliderResH) return WriteDefault(key, "512");
            if (key == Key.sliderSteps) return WriteDefault(key, "25");
            if (key == Key.sliderScale) return WriteDefault(key, "9");
            if (key == Key.textboxOutPath) return WriteDefault(key, Path.Combine(Paths.GetExeDir(), "Images"));
            if (key == Key.upDownIterations) return WriteDefault(key, "5");
            if (key == Key.comboxSdModel) return WriteDefault(key, Paths.GetModels().Select(x => x.Name).FirstOrDefault());
            if (key == Key.checkboxEnableHistory) return WriteDefault(key, true.ToString());
            if (key == Key.sliderCodeformerFidelity) return WriteDefault(key, "0.6");
            if (keyStr == "checkboxFullPrecision") return WriteDefault(key, (GpuUtils.CachedGpus.Count > 0 && GpuUtils.CachedGpus[0].FullName.Contains(" GTX 16")).ToString());
            if (keyStr.MatchesWildcard("checkbox*InFilename")) return WriteDefault(key, true.ToString());

            if (key == Key.none)
                return WriteDefault(keyStr, "");
            else
                return WriteDefault(key, "");
        }

        private static string WriteDefault(Key key, string def)
        {
            Set(key, def);
            return def;
        }

        private static string WriteDefault(string key, string def)
        {
            Set(key, def);
            return def;
        }

        public enum Key
        {
            none,
            cmdDebugMode,
            checkboxMultiPromptsSameSeed,
            comboxSampler,
            sliderInitStrength,
            sliderResW,
            sliderResH,
            sliderSteps,
            sliderScale,
            textboxOutPath,
            upDownIterations,
            comboxSdModel,
            lowMemTurbo,
            checkboxEnableHistory,
            sliderCodeformerFidelity,
            customModelDirs,
        }
    }
}
