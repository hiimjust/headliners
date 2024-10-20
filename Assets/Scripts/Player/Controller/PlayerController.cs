using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerController : MonoBehaviourPunCallbacks, IPlayer
{
    #region PHOTON_VIEW
    PhotonView PV;
    #endregion

    #region CONSTANT
    private int AIM_ANIMATOR_LAYER = 1;
    #endregion

    #region OTHER_PLAYER_SCRIPTS
    [SerializeField] private Player player;
    [SerializeField] private PlayerUI playerUI;
    private PlayerManager playerManager;
    private StarterAssetsInputs starterAssetsInputs;
    private ThirdPersonController thirdPersonController;
    private Animator animator;
    #endregion 

    #region PLAYER_CAMERA
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    #endregion

    #region PLAYER_RIG
    [SerializeField] private Rig aimRig;
    private float aimRigWeight;
    #endregion

    #region WEAPON(MOVE TO OTHER SCRIPT LATER)
    [SerializeField] private float currentHitDelay;
    [SerializeField] private float hitRate = 0.1f;
    [SerializeField] private Transform vfxHitGreen;
    [SerializeField] private Transform vfxHitRed;
    [SerializeField] private AudioClip gunSound;
    [SerializeField] private AudioClip gunReloadSound;
    private AudioSource _audioSource;
    #endregion

    [Header("CHECK GUN")]
    [SerializeField] private WeaponHolder weaponHolder;
    [SerializeField] private Gun currentGun;
    [SerializeField] private int currentGunMaxAmmo;

    private Transform hitTransform;
    private Vector3 mouseWorldPosition;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();
        _audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        InvokeRepeating("InZoneCheck", 0.0f, ZoneDamage.tickRate);
    }

    private void Update()
    {
        currentGun = weaponHolder.currentGun;
        PlayerAimController();
        PlayerShootController();
        PlayerGunReload();
    }

    private void FixedUpdate()
    {

    }

    #region PLAYER_AIM_SYSTEM
    public void PlayerAimController()
    {
        DetectCurrentAimPosition();

        PlayerAim();
        AimRigLerp();
    }

    public void PlayerAim()
    {
        if (starterAssetsInputs.aim)
        {
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensitivity(this.aimSensitivity);
            thirdPersonController.SetRotateOnMove(false);
            animator.SetLayerWeight(AIM_ANIMATOR_LAYER, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
            aimRigWeight = 1f;
        }
        else
        {
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensitivity(this.normalSensitivity);
            thirdPersonController.SetRotateOnMove(true);
            animator.SetLayerWeight(AIM_ANIMATOR_LAYER, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
            aimRigWeight = 0f;
        }
        aimVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(aimVirtualCamera.m_Lens.FieldOfView, starterAssetsInputs.aim ? currentGun.gunData.aimFov : 40f, Time.deltaTime * 10f);
    }

    private void DetectCurrentAimPosition()
    {
        mouseWorldPosition = Vector3.zero;

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            mouseWorldPosition = raycastHit.point;
            hitTransform = raycastHit.transform;
        }
    }

    private void AimRigLerp()
    {
        aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigWeight, Time.deltaTime * 20f);
    }
    #endregion

    #region PLAYER_SHOOT_SYSTEM
    public void PlayerShootController()
    {
        DetectCurrentAimPosition();

        PlayerShoot();
        AimRigLerp();
    }

    public void PlayerShoot()
    {
        if (Input.GetKey(KeyCode.Mouse0) && starterAssetsInputs.enabled == true && weaponHolder.currentGun.Ammos > 0)
        {
            if (currentHitDelay > 0f)
            {
                currentHitDelay -= Time.deltaTime;
            }
            else
            {
                currentHitDelay = weaponHolder.currentGun.gunData.hitRate;
                weaponHolder.currentGun.Ammos--;
                _audioSource.clip = weaponHolder.currentGun.gunData.gunSound;
                _audioSource.Play();
                if (hitTransform != null)
                {
                    if (hitTransform.gameObject.name.Contains("Player"))
                    {
                        Player targetedPlayer = hitTransform.GetComponent<Player>();
                        Instantiate(vfxHitGreen, mouseWorldPosition, Quaternion.identity);
                        hitTransform.gameObject.GetComponent<IPlayer>()?.TakeDamage(weaponHolder.currentGun.gunData.damage, PV.Owner.NickName);
                    }
                    else
                    {
                        Instantiate(vfxHitRed, mouseWorldPosition, Quaternion.identity);
                    }
                }
            }
            if (!starterAssetsInputs.aim)
            {
                animator.SetLayerWeight(AIM_ANIMATOR_LAYER, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
                Vector3 worldAimTarget = mouseWorldPosition;
                worldAimTarget.y = transform.position.y;
                Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
                aimRigWeight = 1f;
            }
        }
        else if (starterAssetsInputs.enabled == true)
        {
            if (currentHitDelay > 0)
            {
                currentHitDelay -= Time.deltaTime;
            }
            else
            {
                currentHitDelay = 0f;
            }
            if (!starterAssetsInputs.aim)
            {
                animator.SetLayerWeight(AIM_ANIMATOR_LAYER, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
                aimRigWeight = 0f;
            }
        }
    }
    #endregion

    #region PLAYER_GUN_RELOAD
    private void PlayerGunReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && currentGun.Ammos < currentGun.gunData.ammos && !currentGun.Reloading)
        {
            currentGun.Reload();
        }
    }
    #endregion

    #region PLAYER_ATTACK_SYSTEM
    public void TakeDamage(float damage, string shooterName)
    {
        PV.RPC("TakeDamageEvent", RpcTarget.AllViaServer, damage, shooterName);
    }

    [PunRPC]
    private void TakeDamageEvent(float damage, string shooterName)
    {
        if (!PV.IsMine)
            return;

        if (damage < player.Shield)
        {
            player.Shield -= damage;
        }
        else
        {
            player.Health -= damage - player.Shield;
            player.Shield = 0;
        }

        if (!player.IsAlive())
        {
            EliminatePlayer(shooterName);
        }
    }

    private void EliminatePlayer(string shooterName)
    {
        playerManager.EliminatePlayer(shooterName);
    }
    #endregion

    #region SAFE_ZONE_CHECKING
    public void InZoneCheck()
    {
        if (!player.InZone)
        {
            PV.RPC("TakeHealthDamageEvent", RpcTarget.AllViaServer, ZoneDamage.tickDamage);
        }
    }

    [PunRPC]
    private void TakeHealthDamageEvent(float damage)
    {
        if (!PV.IsMine)
            return;

        player.Health -= damage;

        if (!player.IsAlive())
        {
            EliminatePlayer("SAFE ZONE");
        }
    }
    #endregion

    #region GUN
    private void SetCurrentGun()
    {
        currentGun = weaponHolder.currentGun;
        Debug.Log(currentGun);
    }

    public Gun GetCurrentGun()
    {
        return weaponHolder.currentGun;
    }
    #endregion
}
