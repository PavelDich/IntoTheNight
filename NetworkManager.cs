using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.Events;
using Zenject;
using UnityEngine.Networking.Types;
using Mirror.BouncyCastle.Crypto.Generators;
using Mirror.BouncyCastle.Bcpg.OpenPgp;
using Mirror.Examples.MultipleMatch;
using System.Linq;


namespace Minicop.Game.GravityRave
{
    public class NetworkManager : Mirror.NetworkManager
    {
        [Inject]
        public DiContainer diContainer;
        //[SerializeField]
        //private PlayerInfo _playerInfo;
        public bool playerSpawned;
        public static UnityEvent<NetworkIdentity> OnPlayerConnect = new UnityEvent<NetworkIdentity>();
        public static UnityEvent OnPlayerDisconnect = new UnityEvent();

        public Transform GetRespawn()
        {
            return GetStartPosition();
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
        }
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            OnPlayerDisconnect.Invoke();
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            //if (!playerSpawned) ActivatePlayerSpawn();
        }
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
        }

        public void Create()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            try
            {
                if (!NetworkServer.active)//&&!NetworkClient.isConnected)
                {
                    FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Host started");
                    StartServer();
                }
                else
                {
                    FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.error, $"You'r are client or server active");
                }
            }
            catch
            {
                FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.error, $"Critical error on create");
            }
#endif
        }

        public void Connect(string ipAdress)
        {
            try
            {
                if (!NetworkClient.isConnected && !NetworkServer.active && !string.IsNullOrWhiteSpace(ipAdress))
                {
                    FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Client started");
                    networkAddress = ipAdress;
                    StartClient();
                }
                else
                {
                    FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.error, $"Ip adress incorrect");
                }
            }
            catch
            {
                FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.error, $"Critical error on connect");
            }
        }

        public void Disconnect()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Host stopped");
                NetworkManager.singleton.StopClient();
                NetworkManager.singleton.StopHost();
            }
            else
            {
                FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Client stopped");
                NetworkManager.singleton.StopClient();
            }
        }
        public void LoadScene(int Scane)
        {
            SceneManager.LoadScene(Scane);
        }





        [System.Serializable]
        public struct Room
        {
            public int Count;
            [Scene]
            public string Scene;
        }
        public static UnityEvent<NetworkIdentity> OnSubSceneLoad = new UnityEvent<NetworkIdentity>();
        private int _countActiveScenes = 1;
        [SerializeField]
        public List<Room> RoomScenes = new List<Room>();
        [SerializeField]
        public List<Scene> ActiveRooms = new List<Scene>();
        private bool _isRoomsLaoded = false;



        [SerializeField]
        public Room LobbyScenes = new Room();
        [SerializeField]
        public List<Scene> ActiveLobbys = new List<Scene>();
        private bool _isLobbyLaoded = false;


        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            //base.OnServerAddPlayer(conn);
            StartCoroutine(OnServerAddPlayerDelayed(conn));
        }
        IEnumerator OnServerAddPlayerDelayed(NetworkConnectionToClient conn)
        {
            yield return new WaitForEndOfFrame();
            while (!_isLobbyLaoded || !_isRoomsLaoded)
                yield return null;

            base.OnServerAddPlayer(conn);
            OnPlayerConnect.Invoke(conn.identity);
        }

        public override void OnStartServer()
        {
            StartCoroutine(LoadScenes());
        }

        public IEnumerator LoadScenes()
        {
            StartCoroutine(ServerLoadSubScenes(LobbyScenes, (resultLobbyes, resultIsLobbyLaoded) =>
            {
                this._isLobbyLaoded = resultIsLobbyLaoded;
                this.ActiveLobbys = resultLobbyes;
            }));
            StartCoroutine(ServerLoadSubScenes(RoomScenes, (resultRooms, resultIsRoomsLaoded) =>
            {
                this._isRoomsLaoded = resultIsRoomsLaoded;
                this.ActiveRooms = resultRooms;
            }));


            while (!_isRoomsLaoded) yield return new WaitForEndOfFrame();
            FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"All scenes created");

            for (int i = 0; i < ActiveRooms.Count; i++)
            {
                GameObject[] rootObjects = ActiveRooms[i].GetRootGameObjects();

                NetworkLevel room = rootObjects[0].GetComponent<NetworkLevel>();
                room.id = i;
                FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Set room info id: {ActiveRooms[i].name}");
                yield return new WaitForEndOfFrame();
            }
            FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"All scenes inicialized");
            yield return null;
        }



        [Server]
        IEnumerator ServerLoadSubScenes(List<Room> rooms, System.Action<List<Scene>, bool> subScenes)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            yield return new WaitForEndOfFrame();
            List<Scene> scenes = new List<Scene>();
            for (int index = 0; index < rooms.Count; index++)
            {
                yield return SceneManager.LoadSceneAsync(rooms[index].Scene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });
                Scene newScene = SceneManager.GetSceneAt(index + _countActiveScenes);
                newScene.GetRootGameObjects()[0].GetComponent<NetworkLevel>().RoomId = index;
                scenes.Add(newScene);
                FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Scene {rooms[index].Scene}({index}) created");
            }
            _countActiveScenes += rooms.Count;

            subScenes.Invoke(scenes, true);
#endif
            yield return null;
        }


        [Server]
        IEnumerator ServerLoadSubScenes(Room room, System.Action<List<Scene>, bool> subScenes)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            yield return new WaitForEndOfFrame();
            List<Scene> scenes = new List<Scene>();
            for (int index = 0; index < room.Count; index++)
            {
                yield return SceneManager.LoadSceneAsync(room.Scene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });
                Scene newScene = SceneManager.GetSceneAt(index + _countActiveScenes);
                newScene.GetRootGameObjects()[0].GetComponent<NetworkLevel>().RoomId = index + _countActiveScenes;
                scenes.Add(newScene);
                FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Scene {room.Scene}({index}) created");
            }
            _countActiveScenes += room.Count;

            subScenes.Invoke(scenes, true);
#endif
            yield return null;
        }





        [Server]
        public void ConnectToRoom(NetworkIdentity networkIdentity, int id)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            OnSubSceneLoad.Invoke(networkIdentity);
            StartCoroutine(RoomConecting(networkIdentity.connectionToClient, id));
#endif
        }
        [Server]
        private IEnumerator RoomConecting(NetworkConnectionToClient conn, int id)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            yield return new WaitForEndOfFrame();
            conn.Send(new SceneMessage { sceneName = RoomScenes[id].Scene, sceneOperation = SceneOperation.LoadAdditive });

            SceneManager.MoveGameObjectToScene(conn.identity.gameObject, ActiveRooms[id]);
            //            ActiveRooms[id].GetRootGameObjects()[0].GetComponent<NetworkLevel>().PlayerConnected(conn.identity);
#endif
            yield return null;
        }
        //#endif
        public override void OnStopServer()
        {
            for (int index = 0; index < RoomScenes.Count; index++)
            {
                NetworkServer.SendToAll(new SceneMessage { sceneName = RoomScenes[index].Scene, sceneOperation = SceneOperation.UnloadAdditive });
            }
            NetworkServer.SendToAll(new SceneMessage { sceneName = LobbyScenes.Scene, sceneOperation = SceneOperation.UnloadAdditive });
            StartCoroutine(ServerUnloadSubScenes());
        }
        IEnumerator ServerUnloadSubScenes()
        {
            for (int index = 0; index < ActiveRooms.Count; index++)
                if (ActiveRooms[index].IsValid())
                    yield return SceneManager.UnloadSceneAsync(ActiveRooms[index]);
            ActiveRooms.Clear();

            yield return Resources.UnloadUnusedAssets();
        }

        public override void OnStopClient()
        {
            if (mode == NetworkManagerMode.Offline)
                StartCoroutine(ClientUnloadSubScenes());
        }
        public void ClientUnloadOfSubScenes()
        {
            StartCoroutine(ClientUnloadSubScenes());
        }
        IEnumerator ClientUnloadSubScenes()
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
                if (SceneManager.GetSceneAt(index) != SceneManager.GetActiveScene())
                    yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(index));
        }


        [Server]
        public void LeaveOfRoom(NetworkIdentity networkIdentity)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            OnSubSceneLoad.Invoke(networkIdentity);
            StartCoroutine(LeaveRoom(networkIdentity.connectionToClient));
#endif
        }
        [Server]
        private IEnumerator LeaveRoom(NetworkConnectionToClient conn)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            yield return new WaitForEndOfFrame();
            conn.Send(new SceneMessage { sceneName = LobbyScenes.Scene, sceneOperation = SceneOperation.LoadAdditive });

            SceneManager.MoveGameObjectToScene(conn.identity.gameObject, ActiveLobbys[0]);
            //ClientUnloadOfSubScenes();
#endif
            yield return null;
        }
    }
}
public struct PosMessage : NetworkMessage
{
    public string PlayerName;
}
