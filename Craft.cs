using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace Minicop.Game.GravityRave
{
    public class Craft : MonoBehaviour
    {
        [SerializeField]
        private CraftSlot _slotPrefab;
        [SerializeField]
        private Transform _slotParent;
        public Variant[] Variants;
        [SerializeField]
        private Storage _storage;
        [Serializable]
        public struct Variant
        {
            public int Count;
            public Item CraftItem;
            [SerializeField]
            public NeedItem[] NeedItems;
            [Serializable]
            public struct NeedItem
            {
                public Item ItemType;
                public int Count;
            }
        }

        private void Start()
        {
            for (int i = 0; i < Variants.Length; i++)
            {
                CraftSlot slot = Instantiate(_slotPrefab, _slotParent);
                slot.Id = i;
                slot.OnClick.AddListener(Create);
                slot.Init(Variants[i]);
            }
        }
        public void Create(int id)
        {
            List<StorageSlot> storageSlots = new List<StorageSlot>();
            List<int> needAmount = new List<int>();

            foreach (Variant.NeedItem needItem in Variants[id].NeedItems)
            {
                if (!CheckCountItem(needItem.ItemType.Id, needItem.Count, (List<StorageSlot> storageSlotsResult, List<int> needAmountResult) =>
                {
                    storageSlots = storageSlotsResult;
                    needAmount = needAmountResult;
                })) return;

                //break;
                for (int i = 0; i < storageSlots.Count; i++)
                {
                    storageSlots[i].AddAmount(-needAmount[i]);
                }
            }

            _storage.SetItem(Variants[id].CraftItem, Variants[id].Count);
        }





        public int GetCountItem(int id, System.Action<List<StorageSlot>, List<int>> onSlotsFound)
        {
            List<StorageSlot> slots = new List<StorageSlot>();
            List<int> countInSlots = new List<int>();



            int count = 0;
            for (int i = 0; i < _storage.Slots.Count; i++)
            {
                if (_storage.Slots[i].ItemId == id && !_storage.Slots[i].IsEmpty)
                {
                    slots.Add(_storage.Slots[i]);
                    countInSlots.Add(_storage.Slots[i].Amount);

                    count += _storage.Slots[i].Amount;
                }
            }

            onSlotsFound.Invoke(slots, countInSlots);
            return count;
        }

        public bool CheckCountItem(int id, int needCount, System.Action<List<StorageSlot>, List<int>> onSlotsFound)
        {
            List<StorageSlot> slots = new List<StorageSlot>();
            List<int> countInSlots = new List<int>();



            for (int i = 0; i < _storage.Slots.Count; i++)
            {
                if (_storage.Slots[i].ItemId == id && !_storage.Slots[i].IsEmpty)
                {
                    slots.Add(_storage.Slots[i]);
                    countInSlots.Add(math.min(needCount, _storage.Slots[i].Amount));

                    needCount -= _storage.Slots[i].Amount;
                }
                if (needCount <= 0)
                {
                    onSlotsFound.Invoke(slots, countInSlots);
                    return true;
                }
            }

            onSlotsFound.Invoke(slots, countInSlots);
            return false;
        }
    }
}