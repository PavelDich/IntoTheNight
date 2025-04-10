using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Minicop.Game.GravityRave
{
    public class DayCycle : NetworkBehaviour
    {
        [SerializeField]
        private Light _sun;


        public static UnityEvent<float> OnWorldSunState = new UnityEvent<float>();
        public UnityEvent<float> OnSunState = new UnityEvent<float>();
        public float DayTime;
        public const float MaxTime = 180;
        [SyncVar(hook = nameof(SyncTime))]
        public float DiffTime;
        public static float WorldDiffTime;
        private void SyncTime(float oldValue, float newValue)
        {
            _sun.transform.rotation = Quaternion.AngleAxis(DiffTime - 90, Vector3.right);
            _sun.intensity = math.abs(DiffTime / MaxTime);
            OnWorldSunState.Invoke(math.abs(DiffTime / MaxTime));
            OnSunState.Invoke(math.abs(DiffTime / MaxTime));
        }

        private void Update()
        {
            if (isServer)
            {
                if (WorldDiffTime > MaxTime) WorldDiffTime = -MaxTime;
                WorldDiffTime += (Time.deltaTime * MaxTime) / DayTime;
                DiffTime = WorldDiffTime;
            }
        }
    }
}