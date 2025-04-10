using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Zenject;

namespace Minicop.Game.GravityRave
{
    public class SoundPlay : NetworkBehaviour
    {
        [Inject]
        private NetworkLevel _networkLevel;
        [SerializeField]
        private AudioSource _audioSource;
        [SerializeField]
        private AudioClip _audioClip;
        private void Start()
        {
            _audioSource ??= GetComponent<AudioSource>();
        }
        public void Play()
        {
            _audioSource.PlayOneShot(_audioClip);
            CmdPlay();
        }
        [Command(requiresAuthority = false)]
        private void CmdPlay()
        {
            foreach (NetworkIdentity player in _networkLevel.Players)
            {
                RpcPlay(player.connectionToClient);
            }
        }
        [TargetRpc]
        private void RpcPlay(NetworkConnectionToClient conn)
        {
            _audioSource.PlayOneShot(_audioClip);
        }
    }
}