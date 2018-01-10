using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapLander : Player
{
    protected bool canBeGrounded = true;
    protected float distToGround;
    protected Vector3 topGizPos, bottomGixPos;

    // Use this for initialization
	protected override void Start ()
    {
        base.Starting();

        ResetGravity();
        playerOrigin = this.gameObject.transform.position;
        gravityDirection = Physics.gravity;

        topGizPos = TopBurner.transform.localPosition;
        bottomGixPos = BottomBurner.transform.localPosition;

        BottomBurner.SetActive(false);
        TopBurner.SetActive(false);
        distToGround = this.gameObject.GetComponent<Collider2D>().bounds.extents.y;
    }
	
	// Update is called once per frame
	protected override void Update ()
    {
        if (currentState != PlayerState.Ice && charged)
        {
            direction = originalGravityDirection;
            playerCenter = this.gameObject.transform.TransformPoint(this.gameObject.transform.localPosition);
            playerVert = this.gameObject.transform.TransformPoint(this.gameObject.transform.localPosition.x, this.gameObject.transform.localPosition.y + 1, this.gameObject.transform.localPosition.z);

            gravityDirection = playerVert - playerCenter;
            gravityDirection *= gravityFlipped;
        }
        else
        {
            gravityDirection = originalGravityDirection;
        }
        Physics2D.gravity = gravityDirection;

        GroundedCheck();

        base.Update();
    }

    protected void GroundedCheck()
    {
        if ((Physics2D.OverlapCircle(TopBurner.transform.position, radius, groundMask) ||
            Physics2D.OverlapCircle(BottomBurner.transform.position, radius, groundMask)) &&
            canBeGrounded)
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }
    }

    protected override void FixedUpdate()
    {
        //BatteryUse();
        base.FixedUpdate();
    }

    public void ResetGravity()
    {
        rb.gravityScale = gravity;
        gravityFlipped = -1;
    }

    public override void SwitchState(PlayerState state)
    {
        lastState = currentState;
        currentState = state;
        switch (currentState)
        {
            case PlayerState.Ice:
                ResetGravity();
                break;
        }
        base.SwitchState(state);
    }

    protected override void TouchInput(Touch touch)
    {
        float touchBrakingTime = brakingTime - .1f;
        switch (touch.phase)
        {
            case TouchPhase.Stationary:
                brakingCountdown += brakeRate;
                if (brakingCountdown >= touchBrakingTime)
                {
                    Brakes();
                    braking = true;
                }
                break;

            case TouchPhase.Ended:
                if (brakingCountdown < touchBrakingTime)
                    FlipGravity();
                brakingCountdown = 0;

                BottomBurner.transform.localScale = flameScale;
                TopBurner.transform.localScale = flameScale;
                braking = false;

                LevelManager.instance.AddTaps(1);
                break;
            default:
                break;
        }
    }

    protected override void ParseInput()
    {
        Vector3 someDirection = transform.InverseTransformDirection(this.gameObject.transform.right);
        Vector2 rightDirection = ((this.gameObject.transform.position + someDirection) - this.gameObject.transform.position);
        Vector2 leftDirection = ((this.gameObject.transform.position - someDirection) - this.gameObject.transform.position);
        if (currentState == PlayerState.Ice || !charged)
            return;

        if (Input.GetButton("Jump"))
        {
            brakingCountdown += brakeRate;
            if (brakingCountdown >= brakingTime)
            {
                Brakes();
                braking = true;
            }
        }
        if (Input.GetButtonUp("Jump"))
        {
            if (brakingCountdown < brakingTime)
            {
                FlipGravity();
            }
                
            brakingCountdown = 0;

            BottomBurner.transform.localScale = flameScale;
            TopBurner.transform.localScale = flameScale;
            braking = false;

            LevelManager.instance.AddTaps(1);
        }

        foreach (Touch touch in Input.touches)
        {
            if (Input.touchCount > 0)
            {
                //Try adding colliders to the new UI. 
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.gameObject.transform.position.z)), Vector2.zero);
                if (hit)
                {
                    if (hit.collider.gameObject.layer != UILayer)
                    {
                        TouchInput(touch);

                        //Horizontal Movement
                        //if (touch.fingerId == 0 && IsGrounded())
                        //{
                        //    if (touch.position.x > Screen.width / 2)
                        //    {
                        //        MoveVelocityInDirection(rightDirection);
                        //    }
                        //    else if (touch.position.x < Screen.width / 2)
                        //    {
                        //        MoveVelocityInDirection(leftDirection);
                        //    }
                        //}
                        //else if(touch.fingerId == 1 && IsGrounded())
                        //{
                        //    TouchInput(touch);
                        //}
                        //else if(!IsGrounded())
                        //{
                        //    TouchInput(touch);
                        //}
                    } 
                }
            }
        }
    }

    private void MoveVelocityInDirection(Vector2 dir)
    {
        if (customPlayerVelocity.magnitude <= speed)
        {
            customPlayerVelocity += dir * .25f;
            customPlayerVelocity = transform.InverseTransformDirection(customPlayerVelocity);
            rb.velocity = customPlayerVelocity;
        }
        else
        {
            customPlayerVelocity = dir * speed;
            customPlayerVelocity = transform.InverseTransformDirection(customPlayerVelocity);
            rb.velocity = customPlayerVelocity;
        }
    }

    protected void Brakes()
    {
        if (rb.velocity.magnitude > .05f)
        {
            rb.velocity *= .9f;
            Debug.Log(rb.velocity);
        }
        if (Mathf.Abs(rb.angularVelocity) > .05f)
        {
            rb.angularVelocity *= .9f;
            Debug.Log(rb.angularVelocity);
        }

        if (BottomBurner.transform.localScale.y > .1f || TopBurner.transform.localScale.y > .1f)
        {
            BottomBurner.transform.localScale = new Vector3(BottomBurner.transform.localScale.x, BottomBurner.transform.localScale.y * .9f, BottomBurner.transform.localScale.z);
            TopBurner.transform.localScale = new Vector3(TopBurner.transform.localScale.x, TopBurner.transform.localScale.y * .9f, TopBurner.transform.localScale.z);
        }
        //Debug.Log(rb.angularVelocity);
    }

    public bool IsBraking()
    {
        return braking;
    }

    public override void AssignBurner()
    {
        if (gravityFlipped == -1 && !TopBurner.activeInHierarchy)
        {
            TopBurner.SetActive(true);
            BottomBurner.SetActive(false);
        }
        else if (gravityFlipped == 1 && !BottomBurner.activeInHierarchy)
        {
            TopBurner.SetActive(false);
            BottomBurner.SetActive(true);
        }

        if(grounded)
        {
            TopBurner.SetActive(false);
            BottomBurner.SetActive(false);
        }
    }

    public Vector3 GetGravDirection()
    {
        return gravityDirection;
    }

    public void FlipGravity(bool t = true)
    {
        gravityFlipped *= -1;
        touched = t;
        StartCoroutine(AirBuffer());
    }

    protected IEnumerator AirBuffer()
    {
        canBeGrounded = false;
        yield return new WaitForSecondsRealtime(.5f);
        canBeGrounded = true;
    }

    public override void SetOnTreadmill(bool iot)
    {
        base.SetOnTreadmill(iot);
    }

    public override bool GetOnTreadmill()
    {
        return base.GetOnTreadmill();
    }

    protected override void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(TopBurner.transform.position, radius);
        Gizmos.DrawWireSphere(BottomBurner.transform.position, radius);

        base.OnDrawGizmos();
    }
}
