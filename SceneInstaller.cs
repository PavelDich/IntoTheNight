using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Minicop.Game.GravityRave
{
    public class SceneInstaller : MonoInstaller
    {
        //[SerializeField]
        //private RoundManager _roundManager;
        [SerializeField]
        private NetworkLevel _networkLevel;
        public override void InstallBindings()
        {
            Container.Bind<NetworkLevel>().FromComponentInHierarchy(_networkLevel).AsSingle().NonLazy();
            //Container.Bind<NeedObj>().FromComponentInHierarchy(_needObj).AsSingle();
            //Container.Bind<NetworkManager>().FromComponentInHierarchy(_networkManager).AsSingle();
        }
    }
}