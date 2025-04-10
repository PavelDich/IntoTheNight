using System.Collections;
using System;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using Unity.Mathematics;

namespace Minicop.Game.GravityRave
{
    [Serializable]
    public class Shooter : NetworkBehaviour, IData
    {
        #region System
        [field: SerializeField]
        public Transform _transform { get; private set; }
        [field: SerializeField]
        public Transform _gun { get; private set; }
        [field: SerializeField]
        public Transform _shootPoint { get; private set; }
        [field: SerializeField]
        public NetworkIdentity _networkIdentity { get; private set; }


        protected void Start()
        {
            _transform ??= GetComponent<Transform>();
            _gun ??= GetComponent<Transform>();
            _shootPoint ??= GetComponent<Transform>();
            _networkIdentity ??= GetComponentInParent<NetworkIdentity>();
        }
        #endregion



        #region Damage
        public float Damage
        {
            get { return _data.Damage; }
            set { _data.Damage = value; }
        }
        public float ShotsPerMinute
        {
            get { return _data.ShotsPerMinute; }
            set { _data.ShotsPerMinute = value; }
        }
        public bool IsAutomatic
        {
            get { return _data.IsAutomatic; }
            set { _data.IsAutomatic = value; }
        }
        public LayerMask DamagebleLayer
        {
            get { return _data.DamagebleLayer; }
            set { _data.DamagebleLayer = value; }
        }
        public int ShotCount
        {
            get { return _data.ShotCount; }
            set { _data.ShotCount = value; }
        }
        public float Distance
        {
            get { return _data.Distance; }
            set { _data.Distance = value; }
        }

        [field: SerializeField]
        public UnityEvent<Vector3, Vector3> OnShoot { get; private set; } = new UnityEvent<Vector3, Vector3>();
        [field: SerializeField]
        public UnityEvent<Vector3, Vector3> OnHit { get; private set; } = new UnityEvent<Vector3, Vector3>();

        [SerializeField, HideInInspector]
        private bool IsShots = false;
        [SerializeField, HideInInspector]
        private bool IsReady = false;



        public void StopAttack()
        {
            //if (!isOwned) return;
            if (IsAutomatic)
                IsReady = false;
        }

        public void StartAttack()
        {
            //if (!isOwned) return;
            IsReady = IsAutomatic;
            StartCoroutine(AttackWait());
        }
        public IEnumerator AttackWait()
        {
            if (!_isReloaded || AmmoCharg <= 0 || IsShots) yield break;
            IsShots = true;
            AmmoCharg--;
            CmdSyncData(_data);
            Shot();
            yield return new WaitForSeconds(60f / ShotsPerMinute);
            IsShots = false;
            if (IsReady) StartCoroutine(AttackWait());
            yield break;
        }
        private void Shot()
        {
            Vector3 direction = _gun.forward;
            OnShoot.Invoke(_gun.position, _gun.position + direction * Distance);
            for (int i = 0; i < ShotCount; i++)
            {
                PerformRaycast(direction);
            }
        }
        private void PerformRaycast(Vector3 direction)
        {
            direction = IsUseSpread ? direction + CalculateSpread() : direction;
            Ray ray = new Ray(_shootPoint.position, direction);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, Distance, DamagebleLayer))
            {
                if (hitInfo.collider.TryGetComponent(out IDamageble damageble))
                {
                    damageble.Damage(_networkIdentity, Damage);
                }
                else
                {
                    // On IDamageable is not found.
                }
                OnHit.Invoke(_shootPoint.position, hitInfo.point);
            }
            else
            {
                OnHit.Invoke(_shootPoint.position, _gun.position + direction * Distance);
            }
        }
        #endregion



        #region Ammo
        public float ReloadTime
        {
            get { return _data.ReloadTime; }
            set { _data.ReloadTime = value; }
        }
        public bool IsCanReloaded
        {
            get { return _data.IsCanReloaded; }
            set { _data.IsCanReloaded = value; }
        }
        public int Ammo
        {
            get { return _data.Ammo; }
            set { _data.Ammo = math.clamp(value, 0, AmmoMax); }
        }
        public int AmmoMax
        {
            get { return _data.AmmoMax; }
            set
            {
                _data.AmmoMax = value;
                Ammo = Ammo;
            }
        }
        public int AmmoCharg
        {
            get { return _data.AmmoCharg; }
            set { _data.AmmoCharg = math.clamp(value, 0, AmmoChargMax); }
        }
        public int AmmoChargMax
        {
            get { return _data.AmmoChargMax; }
            set
            {
                _data.AmmoChargMax = value;
                AmmoCharg = AmmoCharg;
            }
        }
        public UnityEvent<float, float, float, float> OnAmmoChange = new UnityEvent<float, float, float, float>();
        public UnityEvent<float, float, float, float> OnAmmoChargChange = new UnityEvent<float, float, float, float>();
        public UnityEvent OnReload = new UnityEvent();
        private bool _isReloaded = true;



        public void Reload()
        {
            if (!IsCanReloaded || Ammo <= 0 || !_isReloaded) return;
            StartCoroutine(ReloadWait());
        }
        private IEnumerator ReloadWait()
        {
            OnReload.Invoke();
            _isReloaded = false;
            yield return new WaitForSeconds(ReloadTime);
            int value = AmmoCharg;
            AmmoCharg = Ammo;
            Ammo -= AmmoCharg;
            Ammo += value;
            _isReloaded = true;
            CmdSyncData(_data);
            yield break;
        }

        #endregion



        #region Spread
        private bool IsUseSpread
        {
            get { return _data.IsUseSpread; }
            set { _data.IsUseSpread = value; }
        }
        float SpreadFactor
        {
            get { return _data.SpreadFactor; }
            set { _data.SpreadFactor = value; }
        }
        private Vector3 CalculateSpread()
        {
            return new Vector3
            {
                x = Random.Range(-SpreadFactor, SpreadFactor),
                y = Random.Range(-SpreadFactor, SpreadFactor),
                z = Random.Range(-SpreadFactor, SpreadFactor)
            };
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
            [Space(5)]
            [Header("Damage")]
            [SerializeField, Min(0f)]
            public float Damage;
            public float ShotsPerMinute;
            public bool IsAutomatic;
            public LayerMask DamagebleLayer;
            public int ShotCount;
            public float Distance;
            [Space(5)]
            [Header("Ammo")]
            [SerializeField, Min(0f)]
            public float ReloadTime;
            public bool IsCanReloaded;
            public int Ammo;
            public int AmmoMax;
            public int AmmoCharg;
            public int AmmoChargMax;
            [Space(5)]
            [Header("Spread")]
            public bool IsUseSpread;
            public float SpreadFactor;
        }
        #endregion


        public object GetData()
        {
            return _data;
        }
        public void SetData(object data)
        {
            _data = (Shooter.Data)data;
        }
    }

    public static class ShooterSerializer
    {
        public static void Write(this NetworkWriter writer, Shooter.Data item)
        {
            writer.WriteFloat(item.Damage);
            writer.WriteFloat(item.ShotsPerMinute);
            writer.WriteBool(item.IsAutomatic);
            writer.WriteLayerMask(item.DamagebleLayer);
            writer.WriteInt(item.ShotCount);
            writer.WriteFloat(item.Distance);

            writer.WriteFloat(item.ReloadTime);
            writer.WriteBool(item.IsCanReloaded);
            writer.WriteInt(item.AmmoMax);
            writer.WriteInt(item.Ammo);
            writer.WriteInt(item.AmmoChargMax);
            writer.WriteInt(item.AmmoCharg);

            writer.WriteBool(item.IsUseSpread);
            writer.WriteFloat(item.SpreadFactor);
        }

        public static Shooter.Data Read(this NetworkReader reader)
        {
            return new Shooter.Data
            {
                Damage = reader.ReadFloat(),
                ShotsPerMinute = reader.ReadFloat(),
                IsAutomatic = reader.ReadBool(),
                DamagebleLayer = reader.ReadLayerMask(),
                ShotCount = reader.ReadInt(),
                Distance = reader.ReadFloat(),

                ReloadTime = reader.ReadFloat(),
                IsCanReloaded = reader.ReadBool(),
                AmmoMax = reader.ReadInt(),
                Ammo = reader.ReadInt(),
                AmmoChargMax = reader.ReadInt(),
                AmmoCharg = reader.ReadInt(),

                IsUseSpread = reader.ReadBool(),
                SpreadFactor = reader.ReadFloat(),
            };
        }
    }
}