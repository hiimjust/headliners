using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ReloadGame : MonoBehaviourPunCallbacks
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Reload();
        }
    }

    public void Reload() {
        Debug.Log("Reload Game");
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left Room");
        Cursor.lockState = CursorLockMode.None;
        PhotonNetwork.Destroy(RoomManager.Instance.gameObject);
        PhotonNetwork.LoadLevel(0);
        MenuManager.Instance.OpenMenu("Title");
    }
}
