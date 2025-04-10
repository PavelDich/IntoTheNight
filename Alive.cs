using System.Collections;
using System.Collections.Generic;
using Minicop.Game.GravityRave;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Minicop.Game.GravityRave
{
    public class Alive : NetworkBehaviour, IDamageble, IData
    {
        #region  System
        private void Start()
        {
            if (!NetworkClient.active) return;
            OnHealthChanged.AddListener(Regenerate);
        }
        #endregion



        #region Health
        [SerializeField]
        public UnityEvent<float, float, float, float> OnHealthChanged = new UnityEvent<float, float, float, float>();
        public UnityEvent OnDeath = new UnityEvent();
        public float HealthMax
        {
            get { return _data.HealthMax; }
            set
            {
                _data.HealthMax = value;
                Health = Health;
            }
        }
        public float Health
        {
            get { return _data.Health; }
            set
            {
                _data.Health = math.clamp(value, 0, HealthMax);
                if (_data.Health == 0) OnDeath.Invoke();
            }
        }

        public void Damage(NetworkIdentity shooter, float damage)
        {
            Health -= damage;
            CmdSyncData(_data);
        }
        #endregion



        #region Regeneration
        public float RegenerationDelay
        {
            get { return _data.RegenerationDelay; }
            set { _data.RegenerationDelay = value; }
        }
        public float RegenerationPerSecond
        {
            get { return _data.RegenerationPerSecond; }
            set { _data.RegenerationPerSecond = value; }
        }
        [SerializeField, HideInInspector]
        private Coroutine ActiveRegenerator;
        public void Regenerate(float oldValue, float newValue, float oldMax, float newMax)
        {
            if (newValue > oldValue) return;
            if (ActiveRegenerator != null)
            {
                StopCoroutine(ActiveRegenerator);
            }
            ActiveRegenerator = StartCoroutine(Start());

            IEnumerator Start()
            {
                yield return new WaitForSeconds(RegenerationDelay);
                while (Health < HealthMax && RegenerationPerSecond != 0)
                {
                    yield return new WaitForEndOfFrame();
                    Health += RegenerationPerSecond * Time.deltaTime;
                    CmdSyncData(_data);
                }
                yield break;
            }
        }
        #endregion



        #region Network
        [SyncVar(hook = (nameof(OnSyncData)))]
        [SerializeField]
        private Data _data = new Data();
        private void OnSyncData(Data oldValue, Data newValue)
        {
            _data = newValue;
        }
        public void SyncData()
        {
            CmdSyncData(_data);
        }
        [Command(requiresAuthority = false)]
        public void CmdSyncData(Data value)
        {
            _data = value;
        }
        [System.Serializable]
        public struct Data
        {
            public float HealthMax;
            public float Health;
            public float RegenerationPerSecond;
            public float RegenerationDelay;
        }
        #endregion



        public object GetData()
        {
            return _data;
        }
        public void SetData(object data)
        {
            _data = (Alive.Data)data;
        }
    }

    public static class AliveSerializer
    {
        public static void Write(this NetworkWriter writer, Alive.Data item)
        {
            writer.WriteFloat(item.Health);
            writer.WriteFloat(item.HealthMax);
            writer.WriteFloat(item.RegenerationDelay);
            writer.WriteFloat(item.RegenerationPerSecond);
        }

        public static Alive.Data Read(this NetworkReader reader)
        {
            return new Alive.Data
            {
                Health = reader.ReadFloat(),
                HealthMax = reader.ReadFloat(),
                RegenerationDelay = reader.ReadFloat(),
                RegenerationPerSecond = reader.ReadFloat(),
            };
        }
    }
}