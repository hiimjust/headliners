using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GunScriptableObject", menuName = "ScriptableObject/GunData")]
public class GunScriptableObject : ScriptableObject
{
    [Space]
    [Header("Gun Information")]
    public string gunName;
    public float damage;
    public int ammos;
    public int ammosPerTap;

    [Space]
    [Header("Gun Stats")]
    public float hitRate;
    public float reloadTime;

    [Space]
    [Header("Gun Field of View")]
    public float aimFov;

    [Space]
    [Header("Gun VFX")]
    public Transform vfx;

    [Space]
    [Header("Gun SFX")]
    public AudioClip gunSound;
    public AudioClip gunReloadSound;
}
