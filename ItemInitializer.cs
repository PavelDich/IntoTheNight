using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Zenject;

namespace Minicop.Game.GravityRave
{
    public class ItemInitializer : NetworkBehaviour
    {
        [Inject]
        public DiContainer _diContainer;
        [Inject]
        public NetworkLevel _networkLevel;
        public static Item[] Items;
        [SerializeField]
        private Item[] _items;

        protected override void OnValidate()
        {
            base.OnValidate();
            Items = _items;
            for (int i = 0; i < Items.Length; i++)
            {
                Items[i].Id = i;
            }
        }

        [Server]
        public Item Spawn(int id, List<object> data)
        {
            GameObject go = _diContainer.InstantiatePrefab(Items[id].gameObject, transform);
            Item item = go.GetComponent<Item>();
            item.SetItemData(data);
            go.transform.SetParent(null);

            NetworkServer.Spawn(go);
            Destroy(gameObject);
            return item;
        }
    }
}