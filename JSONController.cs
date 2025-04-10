using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Minicop.Game.GravityRave
{
    public static class JSONController
    {
        public static void Save(object item, string path, string name)
        {
            Directory.CreateDirectory(Application.dataPath + $"/{path}");
            File.WriteAllText(Application.dataPath + $"{path}/{name}.json", ToJson(item));
        }
        public static void Load<T>(ref T item, string path, string name)
        {
            try
            {
                object obj = (object)item;
                FromJson(ref obj, File.ReadAllText(Application.dataPath + $"{path}/{name}.json"));
                item = (T)obj;
            }
            catch { Save(item, path, name); }
        }

        public static string ToJson(object item)
        {
            return JsonUtility.ToJson(item);
        }

        public static void FromJson<T>(ref T item, string json)
        {
            object obj = (object)item;
            JsonUtility.FromJsonOverwrite(json, obj);
            item = (T)obj;
        }
    }
}