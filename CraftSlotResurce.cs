using System.Collections;
using System.Collections.Generic;
using Minicop.Game.GravityRave;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Minicop.Game.GravityRave
{
    public class CraftSlotResurce : MonoBehaviour
    {
        public Image _itemImage;
        public TMP_Text _itemCount;
        public void Init(Item item, int count)
        {
            _itemImage.sprite = item.ItemSprite;
            _itemCount.text = count.ToString();
        }
    }
}