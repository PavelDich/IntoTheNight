using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minicop.Game.GravityRave
{
    public static class FileDebug
    {
        [SerializeField]
        private static Data _data;
        [System.Serializable]
        public struct Data
        {
            public List<Log> _log;
        }

        [SerializeField]
        private static List<Log> _log = new List<Log>();
        public static void AddLog(float time, object source, Log.Type type, string name)
        {
            switch (type)
            {
                case Log.Type.log:
                    Debug.Log($"{time} : {source} {type} | {name}");
                    break;
                case Log.Type.warning:
                    Debug.LogWarning($"{time} : {source} {type} | {name}");
                    break;
                case Log.Type.error:
                    Debug.LogError($"{time} : {source} {type} | {name}");
                    break;
            }
            _log.Add(new Log { Time = time, Source = source, _type = type, Name = name });
            _data._log = _log;
            JSONController.Save(_data, "", "Logs");
        }
        [System.Serializable]
        public struct Log
        {
            public float Time;
            public object Source;
            public Type _type;
            public enum Type
            {
                log,
                warning,
                error,
            }
            public string Name;
        }
    }
}