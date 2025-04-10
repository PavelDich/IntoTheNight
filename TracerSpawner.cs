using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Mirror;
using UnityEngine;
using Zenject;

namespace Minicop.Game.GravityRave
{
    public class TracerSpawner : NetworkBehaviour
    {
        [SerializeField]
        [Inject]
        private NetworkLevel _networkLevel;


        [SerializeField]
        private Tracer _tracer;
        public void Spawn(Vector3 startPosition, Vector3 endPosition)
        {
            CmdSpawn(startPosition, endPosition);
            [Command(requiresAuthority = false)]
            void CmdSpawn(Vector3 startPosition, Vector3 endPosition)
            {
                foreach (NetworkIdentity player in _networkLevel.Players)
                {
                    RpcSpawn(player.connectionToClient, startPosition, endPosition);
                }
            }
            [TargetRpc]
            void RpcSpawn(NetworkConnectionToClient conn, Vector3 startPosition, Vector3 endPosition)
            {
                Tracer tracer = Instantiate(_tracer, transform.position, transform.rotation);
                tracer.Init(startPosition, endPosition);
            }
        }
    }
}