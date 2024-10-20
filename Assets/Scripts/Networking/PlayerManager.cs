using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.IO;

public class PlayerManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
	[Header("Player")]
	private PhotonView PV;
	private GameObject player;

	[Space]
	[Header("Panels")]
	[SerializeField] private GameObject deathPanel;
	[SerializeField] private GameObject victoryPanel;

	[Space]
	[Header("Players Counter")]
	private int maxLives = 2;
	private int currentLives = 2;
	private int totalPlayers;

	[Space]
	[Header("Events")]
	public const byte PlayerEliminatedEvent = 2;
	public const byte TotalPlayersMinus = 3;
	public const byte RemainingPlayers = 4;
	public const byte PlayerInstructionEvent = 5;

	[Space]
	[Header("Spawning Points")]
	[SerializeField] private Vector3[] spawningPoints;

	void Awake()
	{
		PV = GetComponent<PhotonView>();

		totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

		spawningPoints = new Vector3[15];

		spawningPoints[0] = new Vector3(84.8951645f, 0.366567254f, 41.6773872f);
		spawningPoints[1] = new Vector3(24.5218544f, 2.08200049f, -80.715889f);
		spawningPoints[2] = new Vector3(24.7294407f, 5.42613697f, -71.450264f);
		spawningPoints[3] = new Vector3(-5.79234314f, 2.08238554f, -73.4004517f);
		spawningPoints[4] = new Vector3(1.22195315f, 1.61894941f, -7.16643906f);
		spawningPoints[5] = new Vector3(94.9620132f, 2.08238649f, -88.9266815f);
		spawningPoints[6] = new Vector3(50.8018684f, 2.08199883f, 2.24908042f);
		spawningPoints[7] = new Vector3(85.5477142f, 2.08199978f, 2.28121877f);
		spawningPoints[8] = new Vector3(6.99571753f, 1.61933613f, -39.1401711f);
		spawningPoints[9] = new Vector3(9.24862099f, 0.0823359489f, 42.5732918f);
		spawningPoints[10] = new Vector3(108.222168f, 0.549999952f, 23.7277946f);
		spawningPoints[11] = new Vector3(101.083488f, 0.550000429f, -9.30860043f);
		spawningPoints[12] = new Vector3(48.6344109f, 2.0813365f, -19.4646301f);
		spawningPoints[13] = new Vector3(93.8250885f, 2.08238578f, -33.9187355f);
		spawningPoints[14] = new Vector3(70.815506f, 5.42616272f, -66.2944031f);
	}

	void Start()
	{
		if (PV.IsMine)
		{
			InstantiatePlayer();

			object[] content = new object[] { };
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
			PhotonNetwork.RaiseEvent(PlayerInstructionEvent, content, raiseEventOptions, SendOptions.SendReliable);
		}

		if (PV.IsMine && PhotonNetwork.IsMasterClient)
		{
			InstantiateSafeZone();
		}
	}

	private void Update()
	{
		if ((totalPlayers == 1 && currentLives > 0) || PhotonNetwork.CurrentRoom.PlayerCount == 1)
		{
			ShowWinPanel();
		}

		object[] content = new object[] { totalPlayers };
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
		PhotonNetwork.RaiseEvent(RemainingPlayers, content, raiseEventOptions, SendOptions.SendReliable);
	}

	private void InstantiatePlayer()
	{
		Vector3 spawn;
		if (currentLives == maxLives)
		{
			int randPoint = Random.Range(0, spawningPoints.Length);
			spawn = spawningPoints[randPoint];
		}
		else
		{
			do
			{
				int randPoint = Random.Range(0, spawningPoints.Length);
				spawn = spawningPoints[randPoint];
			} while (Vector3.Distance(spawn, SafeZoneManager.CurrentCenter) > SafeZoneManager.CurrentRadius);
		}
		player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), new Vector3(spawn.x, 7f, spawn.z), Quaternion.identity, 0, new object[] { PV.ViewID });
	}

	private void InstantiateSafeZone()
	{
		PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "SafeZone"), new Vector3(37.7000008f, 0f, -28.8999996f), Quaternion.identity);
	}

	public void EliminatePlayer(string shooterName)
	{
		if (currentLives > 0)
		{
			currentLives--;

			if (currentLives <= 0)
			{
				ShowDeathPanel();
				PhotonNetwork.Destroy(player.transform.Find("PlayerArmature").gameObject);

				object[] content = new object[] { totalPlayers - 1 };
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
				PhotonNetwork.RaiseEvent(TotalPlayersMinus, content, raiseEventOptions, SendOptions.SendReliable);
			}
			else
			{
				string shooter = shooterName;
				string eliminator = PhotonNetwork.LocalPlayer.NickName;

				object[] content = new object[] { shooter, eliminator, currentLives };
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
				PhotonNetwork.RaiseEvent(PlayerEliminatedEvent, content, raiseEventOptions, SendOptions.SendReliable);

				PhotonNetwork.Destroy(player);

				InstantiatePlayer();
			}
		}
	}

	public void OnEvent(EventData photonEvent)
	{
		if (photonEvent.Code == TotalPlayersMinus)
		{
			object[] data = (object[])photonEvent.CustomData;
			totalPlayers = (int)data[0];
			Debug.Log("TOTAL PLAYER = " + totalPlayers);
		}
	}

	#region SHOW_PANEL
	private void ShowDeathPanel()
	{
		player.transform.Find("Player_UI").Find("Player_Panel").Find("DeathPanel").gameObject.SetActive(true);
		Cursor.lockState = CursorLockMode.None;
	}

	private void ShowWinPanel()
	{
		player.transform.Find("Player_UI").Find("Player_Panel").Find("WinPanel").gameObject.SetActive(true);
		Cursor.lockState = CursorLockMode.None;
	}
	#endregion

	#region PHOTON_EVENT

	#endregion
}
