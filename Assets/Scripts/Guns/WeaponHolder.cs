using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class WeaponHolder : MonoBehaviourPunCallbacks
{
    [SerializeField] private int selectedWeapon = 0;
    [SerializeField] public Gun currentGun;

    [SerializeField] private Transform playerCameraTransform;
    
    PhotonView PV;

    void Awake()
	{
		PV = GetComponent<PhotonView>();
	}

    void Start()
    {
        SelectWeapon(0);
    }

    void Update()
    {
        if (!playerCameraTransform) {
            playerCameraTransform = Camera.main.transform;
        }

        int previousSelectedWeapon = selectedWeapon;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f) {
            if (selectedWeapon >= transform.childCount - 1)
                selectedWeapon = 0;
            else
                selectedWeapon++;
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f) {
            if (selectedWeapon <= 0)
                selectedWeapon = transform.childCount - 1;
            else
                selectedWeapon--;
        }

        for (int i = 0; i < transform.childCount; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i)) {
                selectedWeapon = i;
            }
        }

        if (previousSelectedWeapon != selectedWeapon) {
            SelectWeapon(selectedWeapon);
        }
    }


    private void SelectWeapon(int _selectedWeapon) {
        int i = 0;
        foreach (Transform weapon in transform) {
            if (i == _selectedWeapon) {
                weapon.gameObject.SetActive(true);
                currentGun = weapon.gameObject.GetComponent<Gun>();
            } else {
                weapon.gameObject.SetActive(false);
            }
            i++;
        }
    }

    public Gun GetCurrentGun() {
        return currentGun;
    }

    public void SetCurrentGun(Gun gun) {
        currentGun = gun;
    }
}
