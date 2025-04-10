using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minicop.Game.GravityRave
{
    public class Chest : MonoBehaviour
    {
        public GameObject ChestUi;
        public LayerMask _playerLayer;
        private void OnTriggerEnter(Collider col)
        {
            if ((~_playerLayer & (1 << col.gameObject.layer)) == 0)
            {
                ChestUi.SetActive(true);
            }
        }
        private void OnTriggerExit(Collider col)
        {
            if ((~_playerLayer & (1 << col.gameObject.layer)) == 0)
            {
                ChestUi.SetActive(false);
            }
        }
    }
}