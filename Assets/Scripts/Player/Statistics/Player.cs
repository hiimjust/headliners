using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviourPunCallbacks
{
    public const float HEALTH_MAX = 100;
    public const float HEALTH_MIN = 0;
    
    public const float SHIELD_MAX = 200;
    public const float SHIELD_MIN = 0;

    [SerializeField] private float _health;
    public float Health {
        get { return _health; }
        set {
            this._health = Mathf.Clamp(value, HEALTH_MIN, HEALTH_MAX);
        }
    }

    [SerializeField] private float _shield;
    [SerializeField] public float Shield {
        get { return _shield; }
        set { _shield = value; }    
    }

    [SerializeField] private bool _inZone;
    [SerializeField] public bool InZone {
        get { return _inZone; }
        set { _inZone = value; }
    } 

    public bool IsAlive() {
        return (Health > 0);
    }

    private void Awake() {
        this.InZone = true;
    }
}
