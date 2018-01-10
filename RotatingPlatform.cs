using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPlatform : Rotate {
    protected GameObject child;
    protected bool attached = false;

    protected override void Start()
    {
        canRotate = false;
        child = new GameObject();
        child.transform.parent = this.transform;

        base.Start();
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if(col.gameObject.tag == "Player")
        {
            TapLander player = col.gameObject.GetComponent<TapLander>();
            if (player.IsGrounded() && player.transform.parent == null)
            {
                Vector3 playerPoint = this.transform.InverseTransformPoint(col.contacts[0].point);
                player.transform.parent = child.transform;
                attached = true;

                if (((playerPoint.x < 0) && (playerPoint.y > 0)) ||
                    ((playerPoint.x > 0) && (playerPoint.y < 0)))
                {
                    directionModifier = 1;
                }
                else
                {
                    directionModifier = -1;
                }

                canRotate = true;
            }
            else if(player.IsGrounded() && player.Touched())
            {
                Disconnect(player.gameObject);
            }
        }
    }

    private void Disconnect(GameObject player)
    {
        player.transform.parent = null;
        attached = false;
        StartCoroutine(SlowDown());
    }

    private IEnumerator SlowDown()
    {
        while(speedModifier > 0 && !attached)
        {
            speedModifier *= .98f;
            if (speedModifier <= .1f)
                speedModifier = 0;
            yield return new WaitForFixedUpdate();
        }

        canRotate = false;
        speedModifier = 1;
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if(col.gameObject.tag == "Player" && col.gameObject.transform.parent == child.transform)
        {
            Disconnect(col.gameObject);
        }
    }


}
