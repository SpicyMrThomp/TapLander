using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {
    public float speed = 250;
    private float delay;
    public float initialDelay;
    protected bool canRotate = true;

    protected Vector3 rotateDirection;
    protected float directionModifier = 1;
    protected float speedModifier = 1;

    protected virtual void Start()
    {
        StartCoroutine(StartRotate(this.gameObject));
        rotateDirection = new Vector3(0, 0, speed);
    }

    public IEnumerator StartRotate(GameObject obj)
    {
        yield return new WaitForSeconds(initialDelay);
        StartCoroutine(rotateObject(obj));
    }

    public IEnumerator rotateObject(GameObject obj)
    {
        while (true)
        {
            if (!Manager.instance.IsPaused() && canRotate)
            {
                obj.transform.Rotate(rotateDirection * Time.fixedDeltaTime * directionModifier * speedModifier);
            }
            if (delay != 0)
                yield return new WaitForSeconds(delay);
            else
                yield return new WaitForFixedUpdate();
        }
    }

}
