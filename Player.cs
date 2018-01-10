using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BaseCharacter
{
    protected ParticleSystem ps;

    protected float batteryRate = .005f;
    public enum PlayerState {Normal, Shield, Ice, Fire};
    protected PlayerState currentState;
    protected PlayerState lastState;

    protected Manager manager;
    protected Rigidbody2D rb;
    protected float oldVelocityMagnitude;
    protected Vector2 customPlayerVelocity = Vector2.zero;

    public float batteryLife = 1;
    protected bool charged = true;

    protected Vector3 playerOrigin;
    protected Vector3 playerCenter;
    protected Vector3 playerVert;
    protected Vector3 oldPosition;

    protected float gravity = 5f;
    protected Vector3 originalGravityDirection;
    protected Vector3 gravityDirection;
    protected Vector3 direction;
    protected float gravityFlipped = -1;

    protected Transform transformMule;
    protected Vector3 distance;

    protected Camera camera;
    public LayerMask UILayer;

    public bool touched = false, touching = false;
    protected float brakingTime = .5f;
    protected float brakingCountdown = 0;
    protected float brakeRate = .05f;
    protected bool braking = false;

    public GameObject BottomBurner, TopBurner, secondaryImage;
    protected Material secondaryImageMat;
    public float radius = .2f;
    public LayerMask groundMask;
    public bool grounded;
    protected bool onTreadmill = false;

    protected Vector3 flameScale;

    protected float countDown = 0;
    protected float firedCountDown = 0;
    protected float shieldCountdown = 0;
    protected float timer = 1;

    protected bool dead = false;
    protected bool fired = false;
    protected bool shielded = false;
    public float firedSpeed = 15;

    public AudioClip splodeSound;
    public AudioClip wallSound;
    public AudioClip goalSound;

    protected Material mat;

    // Use this for initialization
    protected virtual void Start()
    {
        //QualitySettings.vSyncCount = 0;

        Starting();
    }

    protected override void Starting()
    {
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        ps = this.gameObject.GetComponent<ParticleSystem>();
        originalGravityDirection = Physics.gravity;

        distance = new Vector3();
        camera = FindObjectOfType<Camera>();

        flameScale = BottomBurner.transform.localScale;
        //groundMask = LayerMask.NameToLayer("Ground");
        
        secondaryImageMat = secondaryImage.GetComponent<MeshRenderer>().material;
        countDown = timer;
        shieldCountdown = timer;

        oldVelocityMagnitude = rb.velocity.magnitude;
        oldPosition = this.gameObject.transform.position;

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null)
            cameraControl = mainCamera.GetComponent<CameraControl>();

        mat = this.gameObject.GetComponent<MeshRenderer>().material;

        lastState = PlayerState.Normal;
        SwitchState(PlayerState.Normal);
    }

    public virtual void SwitchState(PlayerState state)
    {
        lastState = currentState;
        currentState = state;
        switch(currentState)
        {
            case PlayerState.Normal:
                gravityDirection = originalGravityDirection;
                secondaryImage.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Clear");
                shielded = false;
                invincible = false;
                break;

            case PlayerState.Shield:
                gravityDirection = originalGravityDirection;
                secondaryImage.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/GooBubble");
                shielded = true;
                invincible = false;
                break;

            case PlayerState.Ice:
                //ResetGravity();
                secondaryImage.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/IceTile");
                shielded = true;
                invincible = false;

                StartCoroutine(IceTimer(3));
                break;

            case PlayerState.Fire:
                gravityDirection = originalGravityDirection;
                secondaryImage.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Fire");
                shielded = false;
                invincible = true;
                break;

            default:
                break;
        }
    }

    public PlayerState GetState()
    {
        return currentState;
    }

    protected void BatteryUse()
    {
        if (!IsGrounded())
        {
            batteryLife -= batteryRate;

            if (batteryLife <= 0)
            {
                batteryLife = 0;
                charged = false;
            }
        }
        else if (IsGrounded())
        {
            batteryLife += .025f;

            if (batteryLife >= 1)
            {
                batteryLife = 1;
                charged = true;
            }
        }

        if(CanvasManager.instance.GetBatteryLifeBar() != null)
            CanvasManager.instance.GetBatteryLifeBar().transform.localScale = new Vector3(batteryLife, 1, 1);
    }

    protected IEnumerator IceTimer(float sec)
    {
        yield return new WaitForSecondsRealtime(sec);
        if(currentState == PlayerState.Ice)
        {
            SwitchState(PlayerState.Normal);
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        touched = false;

        if (camera == null)
            camera = FindObjectOfType<Camera>();

        if (!WithinCameraBounds(this.gameObject.transform.position))
        {
            KillPlayer();
        }

        //this.gameObject.transform.rotation = Quaternion.identity;
        AssignBurner();
        UpdateFired();
        DetectInput();

        oldVelocityMagnitude = rb.velocity.magnitude;
    }

    protected virtual void FixedUpdate()
    {
        UpdateDamageFrames();
    }

    protected void LateUpdate()
    {
        if (transformMule != null)
        {
            this.gameObject.transform.position = transformMule.position/* + distance*/;
            Debug.Log(transformMule.position + " " + 2);
        }

        oldPosition = this.gameObject.transform.position;
        UpdateMaterial();
    }

    protected virtual void DetectInput()
    {
        if(!manager.IsPaused())
        {
            ParseInput();
        }
    }

    protected virtual void ParseInput()
    {
        throw new NotImplementedException();
    }

    protected virtual void TouchInput(Touch touch)
    {
        
    }

    public virtual void AssignBurner()
    {
        
    }

    protected bool WithinCameraBounds(Vector3 pos)
    {
        float fudgeRoom = 10f;
        Vector3 screenPos = camera.WorldToScreenPoint(pos);

        if (screenPos.x <= 0 - fudgeRoom || screenPos.x >= camera.pixelRect.width + fudgeRoom)
            return false;
        else if (screenPos.y <= 0 - fudgeRoom || screenPos.y >= camera.pixelRect.height + fudgeRoom)
            return false;
        else return true;
    }

    public void SetCharge(float charge = 1)
    {
        batteryLife = charge;
    }

    protected void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.tag == "Goal")
        {
            if(!dead)
            {
                SoundManager.instance.PlayPlayerSound(goalSound);

                LevelManager.instance.SetTime(Time.time);
                CanvasManager.instance.LoadPostMortemCanvas();
            }
        }

        if (col.gameObject.tag.Contains("Hazard"))
        {
            if (col.gameObject.tag.Contains("Damagable") && col.gameObject.activeInHierarchy)
            {
                if (fired)
                {
                    col.gameObject.SetActive(false);
                }
                else
                    KillPlayer();
            }
            else if (!col.gameObject.tag.Contains("Damagable"))
            {
                KillPlayer();
            }
        }
    }

    protected void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.GetComponent<Enemy>())
        {
            if (fired && !col.gameObject.GetComponent<Enemy>().IsInvincible())
            {
                col.gameObject.GetComponent<Enemy>().DamageMe(GetDamage());
            }
            else if(col.gameObject.GetComponent<Enemy>().GetDamage() < 1)
            {
                col.gameObject.GetComponent<Enemy>().DamageMe(GetDamage());
            }
            else
                KillPlayer();
        }
    }

    protected void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag.Contains("Wall"))
        {
            if(!dead)
            {
                float volume = (VelocitySpoof(oldPosition).normalized.magnitude);
                SoundManager.instance.PlayPlayerSound(wallSound, volume);
            }
        }
    }

    public void KillPlayer()
    {
        if(!dead)
        {
            if (currentState == PlayerState.Normal || currentState == PlayerState.Fire)
            {
                dead = true;
                StartCoroutine("CauseExplosion");

                if (cameraControl != null)
                    cameraControl.StartShakingCamera();
            }
            else if(currentState != PlayerState.Normal && currentState != PlayerState.Fire)
            {
                DisableShield();
            }
        }
    }

    protected void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag == "MovingWall")
        {
            transformMule = null;
            //rb.gravityScale = gravity;
        }

        //if(col.gameObject.tag == "Black Hole")
        //{
        //    inBlackHole = false;
        //    canBeInBlackHole = true;
        //}
    }

    public bool Touched()
    {
        return touched;
    }

    protected GameObject MakeExplosion()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        //obj.transform.parent = this.gameObject.transform;
        obj.transform.localScale = new Vector3(this.gameObject.transform.localScale.x /5, this.gameObject.transform.localScale.y / 5, 0);
        obj.transform.localPosition = new Vector3(this.gameObject.transform.localPosition.x, this.gameObject.transform.localPosition.y, this.gameObject.transform.localPosition.z);
        obj.transform.rotation = this.gameObject.transform.rotation;
        Material tempMat = new Material(Shader.Find("Sprites/Default"));
        //tempMat.mainTexture = splosion;
        tempMat.mainTextureScale = new Vector2(obj.transform.localScale.x, 1);
        obj.GetComponent<MeshRenderer>().material = tempMat;
        obj.SetActive(false);

        return obj;
    }

    protected void UpdateMaterial()
    {
        //splosionObj.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, -1);
    }

    IEnumerator CauseExplosion()
    {
        SoundManager.instance.PlayPlayerSound(splodeSound);
        ps.Play();

        yield return new WaitForSeconds(ps.main.duration);
        Manager.instance.ResetScene();
    }

    protected bool WithinAbsRange(float value, float range)
    {
        bool valid = false;

        if (value <= range && value >= -range)
            valid = true;

        return valid;
    }

    public bool IsGrounded()
    {
        return grounded;
    }

    public bool GetFired()
    {
        return fired;
    }

    public void SetFired(bool f)
    {
        fired = f;
        if (fired)
            firedCountDown = timer;
    }

    protected void FiredTimer()
    {
        if (firedCountDown <= timer && firedCountDown > 0)
        {
            firedCountDown -= .01f;
        }
        else
        {
            firedCountDown = 0;
            fired = false;
        }
    }

    protected void UpdateFired()
    {
        if (rb.velocity.magnitude > firedSpeed && !grounded)
            fired = true;
        if (rb.velocity.magnitude < oldVelocityMagnitude || grounded || braking)
            fired = false;


        if (fired && currentState != PlayerState.Fire)
        {
            SwitchState(PlayerState.Fire);
        }
        else if(!fired && (currentState != PlayerState.Ice && currentState != PlayerState.Shield) && currentState != PlayerState.Normal)
        {
            SwitchState(PlayerState.Normal);
        }

        if(fired)
            secondaryImage.transform.up = rb.velocity * -1;
    }

    public void SetShield(bool b, AudioClip a = null)
    {
        shielded = b;

        if(shielded)
        {
            SwitchState(PlayerState.Shield);
            SoundManager.instance.PlaySoundEffect(a);
        }
        else
        {
            SwitchState(PlayerState.Normal);
            //SoundManager.instance.PlaySoundEffect(hitSound);
        }
    }

    public bool GetShield()
    {
        return shielded;
    }

    //protected void UpdateShield()
    //{
    //    if (shielded)
    //        Shield.SetActive(true);
    //    else
    //        Shield.SetActive(false);
    //}

    protected void DisableShield()
    {
        if (shieldCountdown == timer && shielded)
        {
            shieldCountdown -= .05f;
            SoundManager.instance.PlaySoundEffect(hitSound);
            cameraControl.StartShakingCamera();
        }
    }

    protected void UpdateDamageFrames()
    {
        if (shieldCountdown > 0 && shieldCountdown != timer)
        {
            shieldCountdown -= .05f;
            mat.color = new Vector4(mat.color.r, mat.color.g, mat.color.b, Time.time - Mathf.Floor(Time.time));
            return;
        }
        else if(shieldCountdown <= 0)
        {
            shieldCountdown = timer;
            mat.color = new Vector4(mat.color.r, mat.color.g, mat.color.b, 1);
            SetShield(false, null);
            return;
        }
    }

    protected Vector3 VelocitySpoof(Vector3 old)
    {
        Vector3 newVelocity;

        newVelocity = (this.gameObject.transform.position - old) / Time.fixedDeltaTime;

        return newVelocity;
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
    }

    public virtual void SetOnTreadmill(bool iot)
    {
        onTreadmill = iot;
    }

    public virtual bool GetOnTreadmill()
    {
        return onTreadmill;
    }
}
