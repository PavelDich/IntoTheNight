using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minicop.Game.GravityRave
{
    public class Hotbar : NetworkBehaviour
    {
        [SerializeField]
        private Transform _hand;
        [SerializeField]
        public float GrabDistance;
        [SerializeField]
        public float GrabRadius;
        [SerializeField]
        public Transform _grabPoint;
        [SerializeField]
        public LayerMask ItemLayer;



        [SyncVar(hook = nameof(SyncItemInHand))]
        private NetworkIdentity ItemInHand;
        private void SyncItemInHand(NetworkIdentity oldValue, NetworkIdentity newValue)
        {

        }



        public UnityEvent<Item> OnTake = new UnityEvent<Item>();



        private void Update()
        {
            if (isServer)
                Hold();
        }



        public void Use(int id)
        {
            if (!ItemInHand) return;
            ItemInHand.GetComponent<Item>().Use(id);
        }



        public void Grab(Item item)
        {
            if (ItemInHand || !item) return;
            item.Grab();
            CmdGrab(item.GetComponent<NetworkIdentity>());
            [Command(requiresAuthority = false)]
            void CmdGrab(NetworkIdentity value)
            {
                if (!value) return;
                ItemInHand = value;
            }
        }

        public void Hold()
        {
            if (!ItemInHand) return;
            ItemInHand.transform.position = _hand.position;
            ItemInHand.transform.rotation = _hand.rotation;
        }

        public void Drop()
        {
            if (!ItemInHand) return;
            ItemInHand.GetComponent<Item>().Drop();
            CmdDrop();
            [Command(requiresAuthority = false)]
            void CmdDrop()
            {
                if (!ItemInHand) return;
                ItemInHand = null;
            }
        }

        public void Take()
        {
            if (ItemInHand == null)
            {
                if (Physics.SphereCast(_grabPoint.position + Vector3.forward, GrabRadius, _grabPoint.forward,
                        out RaycastHit sphereCastHit, GrabDistance, ItemLayer))
                    if (sphereCastHit.collider.TryGetComponent(out Item item))
                        if (!item.IsGrabed)
                            OnTake.Invoke(item);
            }
            else
            {
                OnTake.Invoke(ItemInHand.GetComponent<Item>());
            }
        }
    }
}