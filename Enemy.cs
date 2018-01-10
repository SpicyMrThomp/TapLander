using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : BaseCharacter {
    public AudioClip attractedSound;

    protected GameObject player;
    protected float timer = 1, countdown = 0;
    protected float rate = .0125f;

    public Vector3 topLeft, topRight, bottomLeft, bottomRight;
    protected float GizmoRadius = .2f;

    public bool moveTowardsPlayer = false;
    public bool proximityBasedBasic = false;
    public bool proximityBasedSquare = false;
    public float range;
    protected Vector3 origin;

    protected Material mat;
    protected bool canPlaySound = true;
    public bool foundPlayer = false;

    protected Animator anim;

    // Use this for initialization
    protected virtual void Start ()
    {
        this.Starting();
	}

    protected override void Starting()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        origin = this.gameObject.transform.position;

        anim = this.gameObject.GetComponent<Animator>();

        base.Starting();
    }
	
	// Update is called once per frame
	protected virtual void FixedUpdate ()
    {
        UpdateDamageFrames();
	}

    protected virtual void UpdateMovement()
    {
        if(moveTowardsPlayer)
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }
            else if (proximityBasedBasic)
            {
                if (Vector2.Distance(player.transform.position, origin) <= range)
                {
                    MoveTowardsPosition(this.gameObject, player.transform.position, currentSpeed);
                }
                else if (Vector2.Distance(this.gameObject.transform.position, origin) > .05f)
                    MoveTowardsPosition(this.gameObject, origin, currentSpeed);
            }
            else if(proximityBasedSquare)
            {
                if ((player.transform.position.x > topLeft.x && player.transform.position.x < topRight.x &&
                    player.transform.position.x > bottomLeft.x && player.transform.position.x < bottomRight.x) &&
                    (player.transform.position.y < topLeft.y && player.transform.position.y < topRight.y &&
                    player.transform.position.y > bottomLeft.y && player.transform.position.y > bottomRight.y))
                {
                    MoveTowardsPosition(this.gameObject, player.transform.position, currentSpeed);
                    foundPlayer = true;
                }
                else
                    foundPlayer = false;
            }
            else
            {
                MoveTowardsPosition(this.gameObject, player.transform.position, currentSpeed);
            }
        }
    }

    protected void UpdateDamageFrames()
    {
        if(countdown > 0)
        {
            countdown -= rate;
            this.gameObject.GetComponent<Collider2D>().enabled = false;
            if(mat != null)
                mat.color = new Vector4(mat.color.r, mat.color.g, mat.color.b, Time.time - Mathf.Floor(Time.time));
            currentSpeed = 0;
        }
        else
        {
            countdown = 0;
            this.gameObject.GetComponent<Collider2D>().enabled = true;
            if (mat != null)
                mat.color = new Vector4(mat.color.r, mat.color.g, mat.color.b, 1);
            currentSpeed = speed;
        }
    }

    protected virtual void MoveTowardsPosition(GameObject obj, Vector3 targetPos, float moveSpeed)
    {
        obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    public virtual void DamageMe(int dam)
    {
        if(!invincible)
        {
            SetHealth(health - dam);
            SoundManager.instance.PlaySoundEffect(hitSound);

            if (health <= 0)
            {
                this.gameObject.SetActive(false);
                return;
            }

            StartDamageFrames();
        }
    }

    protected void StartDamageFrames()
    {
        countdown = timer;
    }

    protected virtual void CheckFace()
    {
        if (Vector3.Distance(player.transform.position, this.gameObject.transform.position) > range)
        {
            anim.SetBool("closeToPlayer", false);
            anim.SetBool("Hit", false);

            if(attractedSound != null && SoundManager.instance.SoundPlaying(attractedSound))
            {
                SoundManager.instance.StopCurrentSound(attractedSound);
            }
            canPlaySound = true;
        }
        else
        {
            anim.SetBool("closeToPlayer", true);
            anim.SetBool("Hit", false);

            if (attractedSound != null && !SoundManager.instance.SoundPlaying(attractedSound) && canPlaySound)
            {
                SoundManager.instance.PlaySoundEffect(attractedSound);
                canPlaySound = false;
            }
        }

        if (countdown > 0 || health == 0)
        {
            anim.SetBool("closeToPlayer", false);
            anim.SetBool("Hit", true);
        }
        else if(countdown == 0)
        {
            anim.SetBool("Hit", false);
        }
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if(proximityBasedSquare)
        {
            Gizmos.DrawWireSphere(topLeft, GizmoRadius);
            Gizmos.DrawWireSphere(topRight, GizmoRadius);
            Gizmos.DrawWireSphere(bottomLeft, GizmoRadius);
            Gizmos.DrawWireSphere(bottomRight, GizmoRadius);
        }
    }
}
