using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Minicop.Game.GravityRave
{
    public class Cloth : MonoBehaviour
    {
        //[SerializeField]
        //private Storage _storage;

        [SerializeField]
        private List<Variant> _variants;
        [Serializable]
        public struct Variant
        {
            public GameObject Visual;
            public Item ClothItem;
        }

        private void Start()
        {
            foreach (Variant variant in _variants)
            {
                variant.Visual.SetActive(false);
            }
        }

        public void UpdateCloth(int slotId, Item item)
        {
            foreach (Variant variant in _variants)
            {
                if (variant.ClothItem.Id == item.Id)
                    variant.Visual.SetActive(true);
            }
        }
        public void UpdateCloth(int slotId, StorageSlot slot)
        {
            foreach (Variant variant in _variants)
            {
                Debug.Log($"{variant.ClothItem.Id} == {slot.ItemId}");
                if (variant.ClothItem.Id == slot.ItemId)
                    variant.Visual.SetActive(true);
            }
        }

        public void ClearCloth(int slotId, StorageSlot slot)
        {
            foreach (Variant variant in _variants)
            {
                variant.Visual.SetActive(false);
            }
        }
    }
}