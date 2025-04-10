using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

namespace Minicop.Game.GravityRave
{
    public class NavMeshWalker : NetworkBehaviour
    {
        [SerializeField]
        private NavMeshAgent _pathfinder;

        public void MoveTarget(Vector3 target)
        {
            CmdMoveTarget(target);
            [Command(requiresAuthority = false)]
            void CmdMoveTarget(Vector3 target)
            {
                _pathfinder.SetDestination(target);
                //transform.LookAt(target);
            }
        }


        public static Vector3 RandomNavSphere(Vector3 origin, float distance)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
            randomDirection += origin;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, distance, -1);
            return navHit.position;
        }
    }
}