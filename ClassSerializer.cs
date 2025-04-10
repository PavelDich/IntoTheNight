using System.Collections;
using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Minicop.Game.GravityRave
{
    [System.Serializable]
    public class Parameter
    {
        public void Sync(Parameter parameter)
        {
            Value = parameter.Value;
            Max = parameter.Max;
        }
        [SerializeField]
        public UnityEvent<float, float, float, float> OnParameterChanged = new UnityEvent<float, float, float, float>();
        [SerializeField]
        private float _value;
        public float Value
        {
            get => _value;
            set
            {
                value = Math.Clamp(value, 0, Max);

                OnParameterChanged.Invoke(_value, value, Max, Max);
                _value = value;
            }
        }

        [SerializeField]
        private float _max;
        public float Max
        {
            get { return _max; }
            set
            {
                value = Math.Max(value, 0);

                OnParameterChanged.Invoke(Value, Value, _max, value);
                _max = value;
            }
        }
    }

    public static class ParameterSerializer
    {
        public static void Write(this NetworkWriter writer, Parameter item)
        {
            writer.WriteFloat(item.Max);
            writer.WriteFloat(item.Value);
        }

        public static Parameter Read(this NetworkReader reader)
        {
            return new Parameter
            {
                Max = reader.ReadFloat(),
                Value = reader.ReadFloat(),
            };
        }
    }
}