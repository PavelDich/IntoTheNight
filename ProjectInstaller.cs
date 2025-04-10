using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using kcp2k;
using Mirror.SimpleWeb;

namespace Minicop.Game.GravityRave
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField]
        private NetworkManager _networkManager;
        [SerializeField]
        private KcpTransport _kcpTransport;
        [SerializeField]
        private DatabaseHandler _databaseHandler;
        public override void InstallBindings()
        {
            //Container.Bind<NeedObj>().FromNew().AsSingle();
            //Container.Bind<NetworkManager>().FromComponentInNewPrefab(_networkManager).AsSingle().NonLazy();
            Container.Bind<NetworkManager>().FromComponentInHierarchy(_networkManager).AsSingle().NonLazy();
            Container.Bind<KcpTransport>().FromComponentInHierarchy(_kcpTransport).AsSingle().NonLazy();
            Container.Bind<DatabaseHandler>().FromComponentInHierarchy(_databaseHandler).AsSingle().NonLazy();
            //Container.Bind<MonoInstaller>().FromComponentInHierarchy(this).AsSingle().NonLazy();
        }
    }
}
