using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mirror;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minicop.Game.GravityRave
{
    public class StorageSlot : MonoBehaviour, IPointerUpHandler
    {
        [SerializeField]
        private GraphicRaycaster _rayCaster;


        public Storage Storage;
        public int Id;
        public bool IsEmpty
        {
            get { return _data.IsEmpty; }
            set { _data.IsEmpty = value; }
        }
        public int ItemId
        {
            get { return _data.ItemId; }
            set { _data.ItemId = value; }
        }
        public int Amount
        {
            get { return _data.Amount; }
            set { _data.Amount = value; }
        }
        public int MaxStack
        {
            get { return _data.MaxStack; }
            set { _data.MaxStack = value; }
        }
        public string ItemName
        {
            get { return _data.ItemName; }
            set { _data.ItemName = value; }
        }



        [SerializeField]
        public Item.Type[] Types;
        public Sprite DefaultIcon;
        public TMP_Text AmountText;
        public Image Icon;

        public UnityEvent<int> OnSlotClick = new UnityEvent<int>();
        public UnityEvent<int, StorageSlot> OnSlotSwap = new UnityEvent<int, StorageSlot>();


        private void Start()
        {
            _rayCaster = GetComponentInParent<GraphicRaycaster>();
        }

        public void Init()
        {
            Id = -1;
            IsEmpty = true;
            Clear();
        }

        public void CloneSlot(StorageSlot slot)
        {
            SetData((StorageSlot.Data)slot.GetData());
            Icon.sprite = slot.Icon.sprite;
            AmountText.text = slot.AmountText.text;
            Types = slot.Types;
        }

        public void SetItem(Item item)
        {
            IsEmpty = false;
            ItemId = item.Id;
            MaxStack = item.MaxStack;
            int count = math.min(MaxStack - Amount, item.Amount);
            AddAmount(count);
            Icon.sprite = item.ItemSprite;
            Types = item.Types;
        }

        public void AddAmount(int amount)
        {
            Amount += amount;
            AmountText.text = Amount.ToString();
            if (Amount <= 0) Clear();
        }

        public void Clear()
        {
            IsEmpty = true;
            ItemId = -1;
            Amount = 0;
            AmountText.text = "";
            Icon.sprite = DefaultIcon;
            Types = null;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StorageSlot slot = GetSlotUnderMouse(eventData);
            if (slot)
                if (slot != this)
                    OnSlotSwap.Invoke(Id, slot);
                else
                    OnSlotClick.Invoke(Id);
        }

        StorageSlot GetSlotUnderMouse(PointerEventData eventData)
        {

            eventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();

            _rayCaster.Raycast(eventData, results);

            foreach (RaycastResult i in results)
            {
                if (i.gameObject.TryGetComponent(out StorageSlot slot))
                    return slot;
            }
            return null;
        }

        public object GetData()
        {
            return _data;
        }
        public void SetData(Data data)
        {
            _data = data;
        }
        [SerializeField]
        private Data _data = new Data();
        [Serializable]
        public struct Data
        {
            public bool IsEmpty;
            public int ItemId;
            public int Amount;
            public int MaxStack;
            public string ItemName;
        }
    }
}