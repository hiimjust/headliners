using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    [SerializeField] TMP_InputField nickNameInputField;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject startGameButton;

    void Awake() {
        Instance = this;
    }

    private const string MENU_TITLE = "Title";
    private const string MENU_LOADING = "Loading";
    private const string MENU_ROOM = "Room";
    private const string MENU_FINDROOM = "FindRoom";
    private const string PLAYER_STRING = "Player";

    private const int GAME_MAP_01 = 1;
    

    private void Start()
    {
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        // MenuManager.Instance.OpenMenu(MENU_TITLE);
        Debug.Log("Joined Lobby");
    }

    public void NickNameChanged() {
        PhotonNetwork.NickName = nickNameInputField.text;
        MenuManager.Instance.OpenMenu(MENU_TITLE);
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        // MenuManager.Instance.OpenMenu(MENU_LOADING);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("New Room was created");
        Debug.Log("Created " + PhotonNetwork.CurrentRoom.Name);
    }


    public void JoinRoom(RoomInfo info) {
        PhotonNetwork.JoinRoom(info.Name);
        // MenuManager.Instance.OpenMenu(MENU_LOADING);
    }

    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu(MENU_ROOM);
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent) {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count(); i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        MenuManager.Instance.OpenMenu("Error");
    }

    public void LeaveRoom() {
        Debug.Log("You have left " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LeaveRoom();
        // MenuManager.Instance.OpenMenu(MENU_LOADING);
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu(MENU_TITLE);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(Transform trans in roomListContent) {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList) continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }

    public void StartGame() {
        PhotonNetwork.LoadLevel(GAME_MAP_01);
    }
}
