using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ZoneDamage : MonoBehaviourPunCallbacks
{
    private PhotonView PV;

    [Header("Zone Damage Parameters")]
    [SerializeField] public const float tickRate = 2.0f;
    [SerializeField] public const float tickDamage = 20f;

    private void Awake() {
        PV = GetComponent<PhotonView>();
    }

    private void OnTriggerExit(Collider other) {
        if (other.transform.gameObject.GetComponent<Player>()) {
            other.transform.gameObject.GetComponent<Player>().InZone = false;
        } 
    }

    private void OnTriggerEnter(Collider other) {
        if (other.transform.gameObject.GetComponent<Player>()) {
            other.transform.gameObject.GetComponent<Player>().InZone = true;
        }
    }
}
