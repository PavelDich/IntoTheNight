using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minicop.Game.GravityRave
{
    public static class Config
    {
        public static DataStruct Data = new DataStruct
        {
            IpAdress = "213.176.246.76",
            PortAdress = "49001",
            MySQLHost = "213.176.246.76",
            MySQLDatabase = "records",
            MySQLPort = "3307",
            MySQLCharset = "utf8mb4",
            MySQLUser = "root",
            MySQLPassword = "угадай",
            MySQLClientLogin = "PavelDich",
            MySQLIsActive = true,
        };
        [System.Serializable]
        public struct DataStruct
        {
            public string IpAdress;
            public string PortAdress;
            public bool MySQLIsActive;
            public string MySQLHost;
            public string MySQLDatabase;
            public string MySQLPort;
            public string MySQLCharset;
            public string MySQLUser;
            public string MySQLPassword;
            public string MySQLClientUser;
            public string MySQLClientPassword;
            public string MySQLClientLogin;
            public string CustomConnectionString;
        }
    }
}