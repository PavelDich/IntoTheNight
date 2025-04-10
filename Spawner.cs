using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Random = UnityEngine.Random;

namespace Minicop.Game.GravityRave
{
    public class Spawner : NetworkBehaviour
    {
        [SerializeField]
        private Transform _spawnPoint;
        public float Radius;
        [Inject]
        public DiContainer _diContainer;
        public bool IsStartSpawn = true;
        [SerializeField]
        private Variant[] Variants;
        [Serializable]
        public struct Variant
        {
            public NetworkIdentity Object;
            public int MaxCount;
            public int MinCount;
        }


        private void Start()
        {
            if (isServer && IsStartSpawn) SrvSpawn();
        }
        public void Spawn()
        {
            CmdSpawn();
            [Command(requiresAuthority = false)]
            void CmdSpawn()
            {
                SrvSpawn();
            }
        }
        [Server]
        public void SrvSpawn()
        {
            for (int i = 0; i < Variants.Length; i++)
            {
                int gen = Random.Range(Variants[i].MinCount, Variants[i].MaxCount);
                for (int j = 0; j < gen; j++)
                {
                    Vector3 spawn = RandomNavSphere(_spawnPoint.position, Radius);
                    GameObject go = _diContainer.InstantiatePrefab(Variants[i].Object, spawn, Quaternion.identity, _spawnPoint);
                    go.transform.SetParent(null);
                    NetworkServer.Spawn(go);
                }
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