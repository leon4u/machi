using System;
using System.IO;
using Machi.Core;
using UnityEngine;

namespace Machi
{
    /// <summary>Local JSON save in Application.persistentDataPath. Cloud backup comes later.</summary>
    public static class SaveStore
    {
        static string PathOf => System.IO.Path.Combine(Application.persistentDataPath, "save.json");

        public static long Now => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public static SaveData Load()
        {
            try
            {
                if (!File.Exists(PathOf)) return null;
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(PathOf));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Machi] save unreadable, starting fresh: " + e.Message);
                return null;
            }
        }

        public static void Save(SaveData data)
        {
            var tmp = PathOf + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(data));
            if (File.Exists(PathOf)) File.Delete(PathOf);
            File.Move(tmp, PathOf);
        }

        public static void Delete()
        {
            if (File.Exists(PathOf)) File.Delete(PathOf);
        }
    }
}
