using UnityEngine;
using System;
using System.Data;
using System.Text;

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

using MySql.Data;
using Mirror;
using MySql.Data.MySqlClient;
using Zenject;
using UnityEngine.Events;

namespace Minicop.Game.GravityRave
{
	public class DatabaseHandler : MonoBehaviour
	{
		public static UnityEvent<NetworkIdentity> OnAccept = new UnityEvent<NetworkIdentity>();
		[Inject]
		public NetworkManager _networkManager;
		private string connectionString;
		private MySqlConnection conn = null;
		private MySqlCommand cmd = null;
		private MySqlDataReader rdr = null;
		public int Id;

		private MD5 _md5Hash;

		void Start()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (!Config.Data.MySQLIsActive) return;

			DataBaseOpen();
			try
			{
				IDbCommand cmd = conn.CreateCommand();
				string sql =
					"SELECT Email, Password,Login " +
					"FROM user";
				cmd.CommandText = sql;

				IDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					string Email = (string)reader["Email"];
					string Password = (string)reader["Password"];
					string Login = (string)reader["Login"];


					FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"LoadData: {Email}, {Password}, {Login}");
				}
			}
			catch (Exception ex)
			{
				FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.error, $"{ex.ToString()}");
			}
			DataBaseClose();
#endif
		}
		public void OnApplicationQuit()
		{
			DataBaseClose();
		}

		private void DataBaseOpen()
		{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			try
			{
				conn = new MySqlConnection("server=213.176.246.76;database=into_the_night_db;port=3306;user=root;password=donotkys84;");
				//conn = new MySqlConnection("server=localhost;database=into_the_night_bd;port=3306;user=root;password=donotkys84;");
				FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Connecting...");
				conn.Open();
			}
			catch (Exception ex)
			{
				FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.error, $"{ex.ToString()}");
			}
#endif
		}
		public void DataBaseClose()
		{
			if (conn != null)
			{
				if (conn.State.ToString() != "Closed")
				{
					conn.Close();
					FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Connect closed");
				}
				conn.Dispose();
			}
		}

		//#if DEVELOPMENT_BUILD || UNITY_EDITOR
		public void CheckUser(NetworkIdentity networkIdentity, string password, string login)
		{
			try
			{
				DataBaseOpen();
				try
				{
					IDbCommand cmd = conn.CreateCommand();
					string sql =
					$"SELECT Id,Login FROM user WHERE Password = '{password}' AND Login = '{login}';";
					cmd.CommandText = sql;

					IDataReader reader = cmd.ExecuteReader();

					int results = 0;
					while (reader.Read())
					{
						results += (int)reader["Id"];
						Id = (int)reader["Id"];

					}
					if (results == 0)
					{
						FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Email and password incorect");
						return;
					}

					FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Email and password access");
					OnAccept.Invoke(networkIdentity);
					//_networkManager.LeaveOfRoom(networkIdentity);
				}
				catch (Exception ex)
				{
					FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.error, $"{ex.ToString()}");
				}
				DataBaseClose();
			}
			catch (Exception ex)
			{
				FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.error, $"{ex.ToString()}");
			}
		}
		//#endif

		[Server]
		public void SaveUser(NetworkIdentity networkIdentity, string email, string password, string login)
		{
			try
			{
				try
				{
					DataBaseOpen();

					IDbCommand cmd = conn.CreateCommand();
					string sql =
					$"SELECT Id FROM user WHERE Password = '{password}' AND Login = '{login}';";
					cmd.CommandText = sql;

					IDataReader reader = cmd.ExecuteReader();

					int results = 0;
					while (reader.Read())
					{
						results += (int)reader["Id"];
						Id = (int)reader["Id"];
					}
					if (results != 0)
					{
						FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Login busy");
						return;
					}

					FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Login not busy");

					FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"query access = INSERT INTO users(Email, Password,Login) VALUES (?Email, ?Password,?Login);");
					string query = "INSERT INTO user(Email, Password, Login) VALUES (?Email, ?Password,?Login);";

					FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Data inserting...");
					MySqlCommand cmd2 = new MySqlCommand(query, conn);
					cmd2.Parameters.Add("?Email", MySqlDbType.VarChar).Value = email;
					cmd2.Parameters.Add("?Password", MySqlDbType.VarChar).Value = password;
					cmd2.Parameters.Add("?Login", MySqlDbType.VarChar).Value = login;
					cmd2.ExecuteNonQuery();

					DataBaseClose();

					FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.log, $"Player {networkIdentity.netId}");
					OnAccept.Invoke(networkIdentity);
					//_networkManager.LeaveOfRoom(networkIdentity);
				}
				catch (MySqlException ex)
				{
					FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.error, $"Error in adding mysql row. Error: + {ex.Message}");
				}

				return;
				//string sql = $"INSERT INTO users(Email, Password, Login) VALUES({email}, {password}, {login})";
			}
			catch (Exception ex)
			{
				FileDebug.AddLog(Time.time, this, FileDebug.Log.Type.error, $"{ex.ToString()}");
			}
		}
	}

}