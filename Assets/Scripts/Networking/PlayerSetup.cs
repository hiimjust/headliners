using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPunCallbacks, IPunObservable
{
    #region VARIABLES
    [Header("Player Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera[] vCams;

    [Header("Player Components")]
    public Behaviour[] playerComponents;
    #endregion

    private void Start() {
        var isMine = photonView.IsMine;
        if (isMine) {
            //Enable Player's components & vCams
            foreach (var component in playerComponents) {
                component.enabled = true;
            }
            foreach (CinemachineVirtualCamera vCam in vCams) {
                vCam.gameObject.SetActive(true);
            }
        } else {
            //Disable other players' components & vCams
            foreach (var component in playerComponents) {
                component.enabled = false;
            }
            foreach (CinemachineVirtualCamera vCam in vCams) {
                vCam.gameObject.SetActive(false);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        } else {
            transform.SetPositionAndRotation((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
        }
    }
}
