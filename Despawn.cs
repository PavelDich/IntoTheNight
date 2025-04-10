using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Minicop.Game.GravityRave
{
    public class Despawn : NetworkBehaviour
    {
        public void Kill()
        {
            CmdKill();
            [Command(requiresAuthority = false)]
            void CmdKill()
            {
                SrvKill();
            }
        }
        [Server]
        public void SrvKill()
        {
            NetworkServer.Destroy(gameObject);
            Destroy(gameObject);
        }
    }
}