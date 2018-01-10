using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreadMill : MonoBehaviour {

    private float speed = 6f;
    public float rate = .1f;
    private Vector2 rightDirection;
    private Vector2 leftDirection;
    private Vector2 rightTreadDirection = Vector2.right;
    private Vector2 leftTreadDirection = Vector2.left;
    public bool opposite = false;
    private GameObject player;
    private GameObject topTreads;
    private Material topMat;
    private Material bottomMat;
    private GameObject bottomTreads;
    public Texture treads;
    private Rigidbody2D playerRB;

    private void Start()
    {
        CheckForSwitch();

        topTreads = MakeTreads(1);
        bottomTreads = MakeTreads(-1);
        topMat = topTreads.GetComponent<MeshRenderer>().material;
        bottomMat = bottomTreads.GetComponent<MeshRenderer>().material;

        StartCoroutine(MoveTreads());
    }

    private void FixedUpdate()
    {
        CheckForSwitch();
    }

    private IEnumerator MoveTreads()
    {
        while(true)
        {
            topMat.mainTextureOffset += (leftTreadDirection * Time.fixedDeltaTime);
            bottomMat.mainTextureOffset += (rightTreadDirection * Time.fixedDeltaTime);

            yield return new WaitForFixedUpdate();
        }
    }

    private void CheckForSwitch()
    {
        if (!opposite)
        {
            rightDirection = new Vector2(transform.right.x, transform.right.y);
            leftDirection = new Vector2(transform.right.x, transform.right.y) * -1;

            rightTreadDirection = Vector2.right;
            leftTreadDirection = Vector2.left;
        }
        else
        {
            leftDirection = new Vector2(transform.right.x, transform.right.y);
            rightDirection = new Vector2(transform.right.x, transform.right.y) * -1;

            leftTreadDirection = Vector2.right;
            rightTreadDirection = Vector2.left;
        }
    }

    public void ChangeGears()
    {
        if (opposite)
            opposite = false;
        else
            opposite = true;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            player = col.gameObject;
            playerRB = player.GetComponent<Rigidbody2D>();
            player.GetComponent<TapLander>().SetOnTreadmill(true);
        }
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        float treadVelocity = speed/* * Time.deltaTime*/;

        if (player != null && player == col.gameObject)
        {
            if (Mathf.Abs(playerRB.velocity.magnitude) <= speed)
            {
                if(OnTop(player.transform.position))
                {
                    playerRB.velocity += rate * rightDirection;

                }
                else if(OnBottom(player.transform.position))
                {
                    playerRB.velocity += rate * leftDirection;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject == player)
        {
            player.GetComponent<TapLander>().SetOnTreadmill(false);
            player = null;
            playerRB = null;
        }
    }

    private GameObject MakeTreads(float position)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.transform.parent = this.gameObject.transform;

        obj.transform.localScale = new Vector3(1, this.gameObject.transform.localScale.y / 4, 0);
        obj.transform.localPosition = new Vector3(0, (.5f * position), .5f);
        obj.transform.localRotation = Quaternion.identity;
        Material tempMat = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
        tempMat.mainTexture = treads;
        tempMat.mainTextureScale = new Vector2(this.transform.localScale.x, 1);
        obj.GetComponent<MeshRenderer>().material = tempMat;
        obj.layer = LayerMask.NameToLayer("Ground");

        return obj;
    }

    private bool OnTop(Vector3 playerPos)
    {
        if ((Vector3.Distance(playerPos, topTreads.transform.position)) < (Vector3.Distance(playerPos, bottomTreads.transform.position)))
            return true;
        else
            return false;
    }

    private bool OnBottom(Vector3 playerPos)
    {
        if ((Vector3.Distance(playerPos, topTreads.transform.position)) > (Vector3.Distance(playerPos, bottomTreads.transform.position)))
            return true;
        else
            return false;
    }
}
