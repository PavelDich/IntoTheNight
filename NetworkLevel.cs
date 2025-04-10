using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Minicop.Game.GravityRave;
using Mirror;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using Zenject;

namespace Minicop.Game.GravityRave
{
    public class NetworkLevel : NetworkBehaviour
    {

        [Inject]
        public DiContainer _diContainer;
        [Inject]
        public NetworkManager _networkManager;
        [SerializeField]
        private static NetworkIdentity _localConnection;
        [SerializeField]
        private Transform[] _spawns;
        public static NetworkIdentity LocalConnection
        {
            get
            {
                return _localConnection;
            }
            set
            {
                if (_localConnection != null) return;
                _localConnection = value;
                Debug.Log("Local Player Find");
            }
        }
        public SyncList<NetworkIdentity> Players = new SyncList<NetworkIdentity>();
        public UnityEvent OnLocalConnectionLeave = new UnityEvent();
        public UnityEvent OnLocalConnectionEnter = new UnityEvent();
        public UnityEvent OnConnectionEnter = new UnityEvent();
        public UnityEvent OnConnectionLeave = new UnityEvent();
        public UnityEvent<NetworkIdentity> OnPlayerSpawned = new UnityEvent<NetworkIdentity>();
        public UnityEvent<NetworkIdentity> OnPlayerRemove = new UnityEvent<NetworkIdentity>();
        [SyncVar]
        public int RoomId = 0;
        public bool IsRoom = false;
        public NetworkIdentity Player;

        private void Start()
        {
            NetworkManager.OnPlayerDisconnect.AddListener(SrvRemovePlayer);
        }
        [Client]
        public void SpawnPlayer()
        {
            if (IsRoom && NetworkClient.active)
            {
                CmdSpawnPlayer(LocalConnection, RoomId);
            }
        }
        [Command(requiresAuthority = false)]
        private void CmdSpawnPlayer(NetworkIdentity owner, int id)
        {
            Vector3 spawn = _spawns[Random.Range(0, _spawns.Length - 1)].position;
            NetworkIdentity go = _diContainer.InstantiatePrefab(Player, spawn, Quaternion.identity, null).GetComponent<NetworkIdentity>();
            SceneManager.MoveGameObjectToScene(go.gameObject, _networkManager.ActiveRooms[id]);

            NetworkServer.Spawn(go.gameObject, owner.connectionToClient);
            Players.Add(go);
        }
        [TargetRpc]
        private void RpcSpawnPlayer(NetworkConnection conn, NetworkIdentity player)
        {
            OnPlayerSpawned.Invoke(player);
        }

        private void OnEnable()
        {
            StartCoroutine(WaitSpawnPlayer());
            OnLocalConnectionEnter.Invoke();
        }

        public IEnumerator WaitSpawnPlayer()
        {
            yield return new WaitForEndOfFrame();
            FindPlayers();
            SpawnPlayer();
        }
        public void FindPlayers()
        {
            if (!NetworkClient.active) return;
            CmdFindPlayers();
        }
        [Command(requiresAuthority = false)]
        public void CmdFindPlayers()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in players)
            {
                NetworkIdentity networkIdentity = p.GetComponent<NetworkIdentity>();
                RpcPlayerEnter(networkIdentity.connectionToClient);
                RpcFindLocalPlayers(networkIdentity.connectionToClient, networkIdentity);
            }
        }

        [TargetRpc]
        public void RpcPlayerEnter(NetworkConnectionToClient conn)
        {
            OnConnectionEnter.Invoke();
        }
        [TargetRpc]
        public void RpcFindLocalPlayers(NetworkConnectionToClient conn, NetworkIdentity localPlayer)
        {
            LocalConnection = localPlayer;
        }

        [Server]
        void SrvRemovePlayer()
        {
            StartCoroutine(Remove());
            IEnumerator Remove()
            {
                yield return new WaitForEndOfFrame();
                Players.RemoveAll(p => p == null);
            }
        }


        public void Open(int id)
        {
            CmdOpen(LocalConnection, id);
        }
        [Command(requiresAuthority = false)]
        public void CmdOpen(NetworkIdentity networkIdentity, int id)
        {
            FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"LoadRoom ({id})");
            _networkManager.ConnectToRoom(networkIdentity, id);
        }

        public void LeaveRoom()
        {
            FindPlayers();
            OnLocalConnectionLeave.Invoke();
            CmdLeaveRoom(LocalConnection);
        }
        [Command(requiresAuthority = false)]
        public void CmdLeaveRoom(NetworkIdentity networkIdentity)
        {
            SrvRemovePlayer(networkIdentity);
            _networkManager.LeaveOfRoom(networkIdentity);
        }

        [Server]
        public void SrvRemovePlayer(NetworkIdentity networkIdentity)
        {
            NetworkIdentity player = Players.Find(player => player.connectionToClient == networkIdentity.connectionToClient);
            Players.Remove(player);
            Destroy(player.gameObject);
            NetworkServer.Destroy(player.gameObject);
        }


        public void OnAccept()
        {
            CmdOnAccept(LocalConnection);
        }
        [Command(requiresAuthority = false)]
        public void CmdOnAccept(NetworkIdentity networkIdentity)
        {
            DatabaseHandler.OnAccept.Invoke(networkIdentity);
        }

        public void Quit()
        {
            Application.Quit();
        }
        public int id;
        public bool _isReady = false;
    }
}