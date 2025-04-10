using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using Zenject;

namespace Minicop.Game.GravityRave
{
    public class Regestration : NetworkBehaviour
    {
        [Inject]
        public NetworkManager _networkManager;
        [Inject]
        public NetworkLevel _networkLevel;
        [Inject]
        public DatabaseHandler _databaseHandler;


        public GameObject UI;
        public TMP_InputField RegistrtionEmail;
        public TMP_InputField RegistrtionPassword;
        public TMP_InputField RegistrtionLogin;
        public TMP_InputField EnterLogin;
        public TMP_InputField EnterPassword;

        public void Registration()
        {
            Config.Data.MySQLClientUser = RegistrtionEmail.text;
            Config.Data.MySQLClientPassword = RegistrtionPassword.text;
            Config.Data.MySQLClientLogin = RegistrtionLogin.text;
            JSONController.Save(Config.Data, "", "Config");

            CmdRegistration(NetworkLevel.LocalConnection, RegistrtionEmail.text, RegistrtionPassword.text, RegistrtionLogin.text);
        }
        [Command(requiresAuthority = false)]
        void CmdRegistration(NetworkIdentity networkIdentity, string email, string password, string login)
        {
            _databaseHandler.SaveUser(networkIdentity, email, password, login);
        }
        public void Enter()
        {
            Config.Data.MySQLClientPassword = EnterPassword.text;
            Config.Data.MySQLClientLogin = EnterLogin.text;
            JSONController.Save(Config.Data, "", "Config");

            CmdEnter(NetworkLevel.LocalConnection, EnterPassword.text, EnterLogin.text);
        }
        [Command(requiresAuthority = false)]
        void CmdEnter(NetworkIdentity networkIdentity, string password, string login)
        {
            _databaseHandler.CheckUser(networkIdentity, password, login);
        }

        private void Start()
        {
            RegistrtionEmail.text = Config.Data.MySQLClientUser;
            RegistrtionPassword.text = Config.Data.MySQLClientPassword;
            RegistrtionLogin.text = Config.Data.MySQLClientLogin;
            EnterPassword.text = Config.Data.MySQLClientPassword;
            EnterLogin.text = Config.Data.MySQLClientLogin;
            DatabaseHandler.OnAccept.AddListener(Open);
            //NetworkManager.OnSubSceneLoad.AddListener(Open);
        }

        [Server]
        public void Open(NetworkIdentity networkIdentity)
        {
            RpcOpen(networkIdentity.connectionToClient);
            _networkManager.LeaveOfRoom(networkIdentity);
        }
        [TargetRpc]
        public void RpcOpen(NetworkConnectionToClient conn)
        {
            UI.SetActive(false);
        }
    }
}