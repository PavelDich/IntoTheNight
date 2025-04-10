using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Minicop.Game.GravityRave
{
    public class ClientServerPriority : NetworkBehaviour
    {
        public List<GameObject> LocalClientObjects;
        public List<GameObject> ClientObjects;
        public List<GameObject> ServerObjects;

        private void Start()
        {
            List<GameObject> gameObjects = LocalClientObjects.Concat(ClientObjects).Concat(ServerObjects).ToList();
            foreach (GameObject go in gameObjects)
                go.SetActive(false);

            if (NetworkClient.active)
                foreach (GameObject go in ClientObjects)
                    go.SetActive(true);
            if (isOwned)
                foreach (GameObject go in LocalClientObjects)
                    go.SetActive(true);
            if (NetworkServer.active)
                foreach (GameObject go in ClientObjects)
                    go.SetActive(true);
        }
    }
}