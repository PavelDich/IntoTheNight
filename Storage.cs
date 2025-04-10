using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using Zenject;
using UnityEngine.Events;
using Unity.Mathematics;
using ModestTree;

namespace Minicop.Game.GravityRave
{
    public class Storage : NetworkBehaviour
    {
        [Inject]
        public DiContainer _diContainer;
        public List<List<object>> ItemData = new List<List<object>>();
        [SerializeField]
        private StorageSlot _slotPrefab;
        [SerializeField]
        private Transform _slotsParent;
        [SerializeField]
        private Transform _dropPoint;
        public int CountSlots;
        [HideInInspector]
        public List<StorageSlot> Slots = new List<StorageSlot>();
        [SerializeField]
        private ItemInitializer _item;
        [SerializeField]
        public Item.Type StorageType = Item.Type.Null;
        public UnityEvent<Item> OnItemDrop = new UnityEvent<Item>();
        public UnityEvent<int, Item> OnItemSet = new UnityEvent<int, Item>();
        public UnityEvent<int, StorageSlot> OnSlotSwap = new UnityEvent<int, StorageSlot>();
        public UnityEvent<int, StorageSlot> OnSlotClear = new UnityEvent<int, StorageSlot>();

        private void Start()
        {
            for (int i = 0; i < CountSlots; i++)
            {
                StorageSlot slot = Instantiate(_slotPrefab, _slotsParent);
                slot.Init();
                slot.Id = i;
                slot.Storage = this;
                slot.OnSlotClick.AddListener(DropItem);
                slot.OnSlotSwap.AddListener(SwapItem);
                Slots.Add(slot);
            }

            ItemData = new List<List<object>>(new List<object>[Slots.Count]);
            if (isServer) foreach (StorageSlot slot in Slots) SyncSlots.Add(new StorageSlot.Data());
        }
        public override void OnStartClient()
        {
            SyncSlots.OnChange += OnSlotsChanged;
        }

        public override void OnStopClient()
        {
            SyncSlots.OnChange -= OnSlotsChanged;
        }




        public void TakeItem(int slotId, Item item)
        {
            if (StorageType != Item.Type.Null)
                if (!item.Types.Contains(StorageType)) return;

            if (Slots[slotId].IsEmpty || (Slots[slotId].ItemId == item.Id && Slots[slotId].Amount < Slots[slotId].MaxStack))
            {
                Slots[slotId].SetItem(item);
                CmdSyncSlots(slotId, SlotToData(Slots[slotId]));
                CmdTakeItem(slotId, item.GetComponent<NetworkIdentity>());
                OnItemSet.Invoke(slotId, item);
            }
        }
        public void TakeItem(Item item)
        {
            if (StorageType != Item.Type.Null)
                if (!item.Types.Contains(StorageType)) return;

            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].IsEmpty || (Slots[i].ItemId == item.Id && Slots[i].Amount < Slots[i].MaxStack))
                {
                    Slots[i].SetItem(item);
                    CmdSyncSlots(i, SlotToData(Slots[i]));
                    CmdTakeItem(i, item.GetComponent<NetworkIdentity>());
                    OnItemSet.Invoke(i, item);
                    break;
                }
                else continue;
            }
        }
        [Command(requiresAuthority = false)]
        void CmdTakeItem(int slotId, NetworkIdentity networkIdentity)
        {
            ItemData[slotId] = networkIdentity.GetComponent<Item>().GetItemData();
            Destroy(networkIdentity.gameObject);
            NetworkServer.Destroy(networkIdentity.gameObject);
        }



        public void SetItem(Item item, int count)
        {
            if (StorageType != Item.Type.Null)
                if (!item.Types.Contains(StorageType)) return;

            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].IsEmpty || (Slots[i].ItemId == item.Id && Slots[i].Amount < Slots[i].MaxStack))
                {
                    Slots[i].SetItem(item);
                    Slots[i].AddAmount(count - item.Amount);
                    CmdSyncSlots(i, SlotToData(Slots[i]));
                    //CmdSetItem(i, item.GetComponent<NetworkIdentity>());
                    OnItemSet.Invoke(i, item);
                    break;
                }
                else continue;
            }
        }
        [Command(requiresAuthority = false)]
        void CmdSetItem(int slotId, NetworkIdentity networkIdentity)
        {
            //ItemData[slotId] = networkIdentity.GetComponent<Item>().GetItemData();
        }


        public void DropItem(int slotId)
        {
            if (Slots[slotId].IsEmpty) return;

            CmdDropItem(GetComponent<NetworkIdentity>(), slotId);
        }
        [Command(requiresAuthority = false)]
        private void CmdDropItem(NetworkIdentity conn, int slotId)
        {
            GameObject go = _diContainer.InstantiatePrefab(_item.gameObject, _dropPoint);
            go.transform.SetParent(null);
            Item item = go.GetComponent<ItemInitializer>().Spawn(SyncSlots[slotId].ItemId, ItemData[slotId]);
            NetworkServer.Spawn(go, conn.connectionToClient);
            RpcDropItem(conn.connectionToClient, item.GetComponent<NetworkIdentity>(), slotId);
        }
        [TargetRpc]
        private void RpcDropItem(NetworkConnectionToClient conn, NetworkIdentity itemNetId, int slotId)
        {
            Item item = itemNetId.GetComponent<Item>();
            item.Amount = SyncSlots[slotId].Amount;
            item.SyncData();
            Slots[slotId].Clear();
            CmdSyncSlots(slotId, SlotToData(Slots[slotId]));

            OnSlotClear.Invoke(slotId, Slots[slotId]);
            OnItemDrop.Invoke(item.GetComponent<Item>());
        }


        public void SwapItem(int slotId, StorageSlot targetSlot)
        {
            if (Slots[slotId].IsEmpty) return;
            if (!targetSlot.IsEmpty) return;

            if (targetSlot.Storage.StorageType != Item.Type.Null)
                if (!Slots[slotId].Types.Contains(targetSlot.Storage.StorageType)) return;

            targetSlot.CloneSlot(Slots[slotId]);
            Slots[slotId].Clear();

            CmdSyncSlots(slotId, SlotToData(Slots[slotId]));
            targetSlot.Storage.CmdSyncSlots(targetSlot.Id, SlotToData(targetSlot));

            OnSlotClear.Invoke(slotId, Slots[slotId]);
            targetSlot.Storage.OnSlotSwap.Invoke(targetSlot.Id, targetSlot);

            CmdSwapItem(slotId, targetSlot.Id, targetSlot.Storage.GetComponent<NetworkIdentity>());
        }
        [Command(requiresAuthority = false)]
        private void CmdSwapItem(int slotId, int targetSlotId, NetworkIdentity targetSlotStorage)
        {
            targetSlotStorage.GetComponent<Storage>().ItemData[targetSlotId] = ItemData[slotId];
        }






        #region Network
        public readonly SyncList<StorageSlot.Data> SyncSlots = new SyncList<StorageSlot.Data>();
        [Command(requiresAuthority = false)]
        public void CmdSyncSlots(int slotId, StorageSlot.Data slotData)
        {
            SyncSlots[slotId] = new StorageSlot.Data()
            {
                ItemId = slotData.ItemId,
                IsEmpty = slotData.IsEmpty,
                Amount = slotData.Amount,
                MaxStack = slotData.MaxStack,
                ItemName = slotData.ItemName,
            };
        }
        public StorageSlot.Data SlotToData(StorageSlot slot)
        {
            return new StorageSlot.Data
            {
                ItemId = slot.ItemId,
                IsEmpty = slot.IsEmpty,
                Amount = slot.Amount,
                MaxStack = slot.MaxStack,
                ItemName = slot.ItemName,
            };
        }
        void OnSlotsChanged(SyncList<StorageSlot.Data>.Operation op, int index, StorageSlot.Data value)
        {
            switch (op)
            {
                case SyncList<StorageSlot.Data>.Operation.OP_ADD:
                    break;

                case SyncList<StorageSlot.Data>.Operation.OP_INSERT:
                    break;

                case SyncList<StorageSlot.Data>.Operation.OP_SET:
                    break;

                case SyncList<StorageSlot.Data>.Operation.OP_REMOVEAT:
                    break;

                case SyncList<StorageSlot.Data>.Operation.OP_CLEAR:
                    break;
            }
        }


        private StorageSlot.Data _slot = new StorageSlot.Data();
        #endregion
    }

    public static class StorageSerializer
    {
        public static void Write(this NetworkWriter writer, StorageSlot.Data item)
        {
            writer.WriteInt(item.ItemId);
            writer.WriteBool(item.IsEmpty);
            writer.WriteInt(item.Amount);
            writer.WriteInt(item.MaxStack);
            writer.WriteString(item.ItemName);
        }

        public static StorageSlot.Data Read(this NetworkReader reader)
        {
            return new StorageSlot.Data
            {
                ItemId = reader.ReadInt(),
                IsEmpty = reader.ReadBool(),
                Amount = reader.ReadInt(),
                MaxStack = reader.ReadInt(),
                ItemName = reader.ReadString(),
            };
        }
    }
}