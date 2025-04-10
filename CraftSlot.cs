using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minicop.Game.GravityRave
{
    public class CraftSlot : MonoBehaviour, IPointerClickHandler
    {
        public int Id;
        public UnityEvent<int> OnClick = new UnityEvent<int>();
        public CraftSlotResurce _craftResurces;
        public Transform _craftResurcesParent;
        public Transform _crafteResurcesParent;


        public void Init(Craft.Variant variant)
        {
            foreach (Craft.Variant.NeedItem needItem in variant.NeedItems)
            {
                CraftSlotResurce craftebleImage = Instantiate(_craftResurces, _craftResurcesParent);
                craftebleImage.Init(needItem.ItemType, needItem.Count);
            }
            CraftSlotResurce craftedImage = Instantiate(_craftResurces, _crafteResurcesParent);
            craftedImage.Init(variant.CraftItem, variant.Count);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick.Invoke(Id);
        }
    }
}