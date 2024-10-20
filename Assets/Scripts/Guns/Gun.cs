using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Gun Data")]
    [SerializeField] public GunScriptableObject gunData;

    [Header("Gun Stats")]
    [SerializeField] private float damage;
    [SerializeField] private int ammos;
    public int Ammos {
        get { return ammos; }
        set { ammos = value; }
    }
    [SerializeField] private int firedAmmos;

    [Header("Gun Actions Stats")]
    [SerializeField] private bool reloading; 
    public bool Reloading {
        get { return reloading; }
        set { reloading = value; }
    }

    [SerializeField] private bool ready; 
    public bool Ready {
        get { return ready; }
        set { ready = value; }
    }
    [SerializeField] private bool shooting; 
    public bool Shooting {
        get { return shooting; }
        set { shooting = value; }
    }

    [Header("Gun Sounds")]
    [SerializeField] private AudioClip gunReloadSound;
    private AudioSource _audioSource;

    private void Awake() {
        damage = gunData.damage;
        ammos = gunData.ammos;
        Ready = true;

        _audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        InputManager();
    }
    private void InputManager()
    {
        Shooting = Input.GetKey(KeyCode.Mouse0);

        if (Ready && Shooting && !reloading && ammos > 0){
            firedAmmos = gunData.ammosPerTap;
            Shoot();
        }
    }

    #region SHOOTING
    public void Shoot() {
        Ready = false;
        
        firedAmmos--;
        
        Invoke("ResetShot", gunData.hitRate);

    }

    private void ResetShot() {
        Ready = true;
    }
    #endregion

    #region RELOADING
    public void Reload() {
        Reloading = true;
        Invoke("ReloadFinished", gunData.reloadTime);
        _audioSource.clip = gunData.gunReloadSound;
        _audioSource.Play();
    }

    private void ReloadFinished() {
        ammos = gunData.ammos;
        Reloading = false;
    }
    #endregion
}
