using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacter : MonoBehaviour {
    public bool invincible = false;
    public int health = 1;
    public float speed = 0;
    public int damage = 1;
    public AudioClip hitSound;
    public float currentSpeed;

    protected GameObject mainCamera;
    protected CameraControl cameraControl;

    // Use this for initialization
    void Start ()
    {
        Starting();
	}

    protected virtual void Starting()
    {
        currentSpeed = speed;

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null)
            cameraControl = mainCamera.GetComponent<CameraControl>();
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetHealth(int h)
    {
        health = h;
    }

    public int GetDamage()
    {
        return damage;
    }

    public void SetDamage(int d)
    {
        damage = d;
    }

    public bool IsInvincible()
    {
        return invincible;
    }

    public void SetInvincible(bool b)
    {
        invincible = b;
    }
}
