using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region PHOTON_VIEW
    private PhotonView PV;
    #endregion

    [Header("Player")]
    [SerializeField] private Player player;

    [Header("Player Compass")]
    [SerializeField] private RawImage compassImage;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private TextMeshProUGUI compassDirectionText;

    [Header("Player Health System")]
    private Slider healthImage;
    private Slider shieldImage;

    [Header("Player Nickname")]
    [SerializeField] private TextMeshProUGUI playerNicknameText;

    [Header("Player Gun Data")]
    [SerializeField] private Gun currentGun;
    [SerializeField] private TextMeshProUGUI currentGunNameText;
    [SerializeField] private TextMeshProUGUI currentGunAmmoText;

    [Header("Restricted Safe Zone Notification")]
    [SerializeField] private TextMeshProUGUI safeZoneNotification;

    [Header("Kill Notification")]
    [SerializeField] private TextMeshProUGUI killNotification;

    [Header("Remaining Players")]
    [SerializeField] private TextMeshProUGUI remainingPlayersText;

    [Header("Death - Win Panels")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Map Panel")]
    [SerializeField] private GameObject mapPanel;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        currentGun = player.GetComponent<PlayerController>()?.GetCurrentGun();
        GetPlayerHUDImages();
        UpdatePlayerHealth();
        UpdatePlayerShield();
        UpdateGunData();
        
        playerNicknameText.text = PhotonNetwork.LocalPlayer.NickName;
    }

    private void Update()
    {
        PlayerCompassManager();
        UpdatePlayerHealth();
        UpdatePlayerShield();
        UpdateGunData();
        MapPanelHandle();
    }

    //Get Player's HUD components
    private void GetPlayerHUDImages()
    {
        healthImage = transform.Find("Player_HealthHUD").Find("HealthBar").GetComponent<Slider>();
        shieldImage = transform.Find("Player_HealthHUD").Find("ShieldBar").GetComponent<Slider>();
    }

    //Player Compass: Display Player's current direction on the game's HUD
    #region PLAYER_COMPASS
    private void PlayerCompassManager()
    {
        Vector3 forward = playerTransform.transform.forward;

        forward.y = 0;

        float headingAngle = Quaternion.LookRotation(forward).eulerAngles.y;
        headingAngle = 5 * (Mathf.RoundToInt(headingAngle / 5.0f));

        int displayAngle;
        displayAngle = Mathf.RoundToInt(headingAngle);

        compassImage.uvRect = new Rect((float)headingAngle / 360, 0, 1, 1);

        switch (displayAngle)
        {
            case 0:
                compassDirectionText.text = "N";
                break;
            case 360:
                compassDirectionText.text = "N";
                break;
            case 45:
                compassDirectionText.text = "NE";
                break;
            case 90:
                compassDirectionText.text = "E";
                break;
            case 135:
                compassDirectionText.text = "SE";
                break;
            case 180:
                compassDirectionText.text = "S";
                break;
            case 225:
                compassDirectionText.text = "SW";
                break;
            case 270:
                compassDirectionText.text = "W";
                break;
            case 315:
                compassDirectionText.text = "NW";
                break;
            default:
                compassDirectionText.text = headingAngle.ToString();
                break;
        }
    }
    #endregion

    //Player Health HUD: Display Player's Health on game's HUD
    #region PLAYER_HEALTH_HUD
    private void UpdatePlayerHealth()
    {
        healthImage.value = player.Health / Player.HEALTH_MAX;
    }


    private void UpdatePlayerShield()
    {
        shieldImage.value = player.Shield / Player.SHIELD_MAX;
    }

    private void MapPanelHandle() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            mapPanel.SetActive(!mapPanel.activeSelf);
        }
    }

    private void UpdateGunData()
    {
        //If null set no gun
        if (player.GetComponent<PlayerController>()?.GetCurrentGun() == null)
        {
            currentGunNameText.text = "No Gun";
            currentGunAmmoText.text = "0/0";
            return;
        }

        currentGunNameText.text = player.GetComponent<PlayerController>()?.GetCurrentGun()?.gunData.gunName.ToString();
        currentGunAmmoText.text = player.GetComponent<PlayerController>()?.GetCurrentGun()?.Ammos.ToString() + "/" + player.GetComponent<PlayerController>()?.GetCurrentGun()?.gunData.ammos.ToString();
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == SafeZoneManager.ShrinkingCountdownEvent)
        {
            object[] data = (object[])photonEvent.CustomData;

            int countdownValue = (int)data[0];
            if (countdownValue != 1) 
            {
                safeZoneNotification.text = "SAFE ZONE RESTRICT IN " + countdownValue + " SECONDS";
            } else if (countdownValue == 1) 
            {
                safeZoneNotification.text = "SAFE ZONE RESTRICT IN " + countdownValue + " SECOND";
                StartCoroutine(ShowSafeZoneNotification());
            }
        }

        if (eventCode == PlayerManager.PlayerInstructionEvent) 
        {
            object[] data = (object[])photonEvent.CustomData;

            StartCoroutine(ShowInstructionNotification());
        }
        

        if (eventCode == PlayerManager.PlayerEliminatedEvent) 
        {
            object[] data = (object[])photonEvent.CustomData;

            string shooter = (string)data[0];
            string eliminated = (string)data[1];
            Debug.Log(shooter + " X " + eliminated);

            StartCoroutine(ShowKillNotification(shooter, eliminated));  
        }

        if (eventCode == PlayerManager.RemainingPlayers) {
            object[] data = (object[])photonEvent.CustomData;

            int remainingPlayers = (int)data[0];
            remainingPlayersText.text = remainingPlayers.ToString();
        }
    }

    IEnumerator ShowSafeZoneNotification() 
    {
        yield return new WaitForSeconds(1);
        safeZoneNotification.text = "SAFE ZONE RESTRICTED";
        yield return new WaitForSeconds(5);
        safeZoneNotification.text = "";
    }

    IEnumerator ShowKillNotification(string shooter, string eliminated)
    {
        killNotification.text = shooter + "  X  " + eliminated;
        yield return new WaitForSeconds(5);
        killNotification.text = "";
    }

    IEnumerator ShowInstructionNotification()
    {
        safeZoneNotification.text = "USE W, A, S, D TO MOVE YOUR CHARACTER";
        yield return new WaitForSeconds(5);
        safeZoneNotification.text = "USE LEFT MOUSE BUTTON TO SHOOT, RIGHT MOUSE BUTTON TO AIM";
        yield return new WaitForSeconds(5);
        safeZoneNotification.text = "USE SHIFT TO SPRINT, SPACE TO JUMP";
        yield return new WaitForSeconds(5);
        safeZoneNotification.text = "USE SCROLLWHEEL TO SWITCH WEAPONS";
        yield return new WaitForSeconds(5);
        safeZoneNotification.text = "USE R TO RELOAD";
        yield return new WaitForSeconds(5);
        safeZoneNotification.text = "";
    }
    #endregion
}
