using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Minicop.Game.GravityRave
{
    public class Item : NetworkBehaviour
    {
        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private Collider _collider;
        [HideInInspector]
        public int Id;
        public bool IsGrabed
        {
            get { return _data.IsGrabed; }
            set
            { if (isServer) _data.IsGrabed = value; }
        }

        public string Name;
        public Sprite ItemSprite;

        [SerializeField]
        public Type[] Types;
        [Serializable]
        public enum Type
        {
            Null,
            Ammo,
            Resurces,
            Gun,
            Boots,
            Pants,
            Coat,
            Hat,
            Cloth,
        }

        public int Amount
        {
            get { return _data.Amount; }
            set { _data.Amount = value; }
        }
        public int MaxStack;



        private void Start()
        {
            _rigidbody ??= GetComponent<Rigidbody>();
            _collider ??= GetComponent<Collider>();
        }


        public UnityEvent[] TypeUse;

        public void Use(int id)
        {
            if (TypeUse.Length > id)
                TypeUse[id].Invoke();
        }

        public void Drop()
        {
            IsGrabed = false;
            SyncData();
            CmdDrop();
        }
        [Command(requiresAuthority = false)]
        public void CmdDrop()
        {
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;
            _collider.isTrigger = false;
        }
        public void Grab()
        {
            IsGrabed = true;
            SyncData();
            CmdGrab();
        }
        [Command(requiresAuthority = false)]
        public void CmdGrab()
        {
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
            _collider.isTrigger = true;
        }




        [SerializeField]
        public IData[] Class;
        public List<object> ItemData = new List<object>();



        public List<object> GetItemData()
        {
            Class = gameObject.GetComponents<IData>();
            foreach (IData data in Class)
                ItemData.Add(data.GetData());
            return ItemData;
        }
        public void SetItemData(List<object> data)
        {
            if (data == null) return;
            Class = gameObject.GetComponents<IData>();
            for (int i = 0; i < data.Count; i++)
                Class[i].SetData(data[i]);
        }


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
            public bool IsGrabed;
            public int Amount;
        }
        #endregion


        public object GetData()
        {
            return _data;
        }
        public void SetData(object data)
        {
            _data = (Item.Data)data;
        }
    }

    public static class ItemSerializer
    {
        public static void Write(this NetworkWriter writer, Item.Data item)
        {
            writer.WriteBool(item.IsGrabed);
            writer.WriteInt(item.Amount);
        }

        public static Item.Data Read(this NetworkReader reader)
        {
            return new Item.Data
            {
                IsGrabed = reader.ReadBool(),
                Amount = reader.ReadInt(),
            };
        }
    }

    public interface IData
    {
        public object GetData();
        public void SetData(object data);
    }
}