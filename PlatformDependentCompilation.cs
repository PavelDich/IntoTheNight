using System.Collections;
using System.Collections.Generic;
using System.Net;
using kcp2k;
using UnityEngine;
using Zenject;

namespace Minicop.Game.GravityRave
{
    public class PlatformDependentCompilation : MonoBehaviour
    {
        [Inject]
        public NetworkManager _networkManager;
        [Inject]
        public KcpTransport _kcpTransport;


        private void Start()
        {
            JSONController.Load(ref Config.Data, "", "Config");
#if UNITY_EDITOR
            CreateServer();
            ConnectServer();
#endif
#if DEVELOPMENT_BUILD
            CreateServer();
#elif !UNITY_EDITOR
            ConnectServer();
#endif
        }

        private void CreateServer()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            try
            {
                FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Server at {Config.Data.IpAdress}: {Config.Data.PortAdress} creating...");
                ushort.TryParse(Config.Data.PortAdress, out _kcpTransport.port);
                Create();
            }
            catch
            {
                FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.error, $"Port {Config.Data.PortAdress} denied");
            }
#endif
        }
        public void ConnectServer()
        {
            //#if UNITY_EDITOR
            StartCoroutine(WaitConnect());
            //#endif
        }

        public IEnumerator WaitConnect()
        {
            while (true)
            {
                FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Wait to connect ({Config.Data.IpAdress}:{Config.Data.PortAdress})");
                ushort.TryParse(Config.Data.PortAdress, out _kcpTransport.port);
                _networkManager.Connect(Config.Data.IpAdress);
                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator GetIpAdress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    //if (_field.IpAdress != null) Config.Data.IpAdress = ip.ToString();
                    yield return null;
                }
                yield return null;
            }
        }

        public void Create()
        {
            _networkManager.Create();
        }


        public void Disconnect()
        {
            _networkManager.Disconnect();
        }
        public void Exit()
        {
            Application.Quit();
        }
    }
}