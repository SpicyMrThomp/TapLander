using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazerBeam : MonoBehaviour {
    protected LineRenderer line;
    protected float lineWidth;
    protected Vector3 lineHit;
    protected Collider2D collider;
    protected GameObject player;
    protected GameObject hitBox;
    public float lineDistance = 30;
    public LayerMask mask;
    public LayerMask playerMask;
    protected ParticleSystem ps;
    protected ParticleSystem.EmissionModule emission;
    protected ParticleSystem.ShapeModule shape;
    public bool pulse = false;
    protected bool active = true;

    public bool delayed = false;
    public float breakTimer = 1f;
    public float timer = 1f;
    public float warningTimer = 1f;
    protected string tag = "";
    protected Vector3 linePosition1, linePosition2;
    protected Vector3 defaultHitPosition;

    protected Material mat;
    public Texture open, closed;

    protected bool overPlayer = false;

    protected float stage = 0;

    // Use this for initialization
    protected virtual void Start ()
    {
        StartCoroutine(StartBeam());
    }

    protected IEnumerator StartBeam()
    { 
        if(delayed)
        {
            yield return new WaitForSeconds(timer);
            delayed = false;
        }

        SetUpBeam();

        if (pulse)
            StartCoroutine(MakeBeam(timer));
    }

    protected virtual void SetUpBeam()
    {
        line = GetComponent<LineRenderer>();
        line.enabled = true;
        line.useWorldSpace = true;
        lineWidth = line.startWidth;
        collider = GetComponent<BoxCollider2D>();
        lineHit = this.transform.up;

        ps = this.GetComponent<ParticleSystem>();
        shape = ps.shape;
        emission = ps.emission;

        mat = this.gameObject.GetComponent<MeshRenderer>().material;
        open = mat.mainTexture;
    }
	
	// Update is called once per frame
	protected virtual void Update ()
    {
        if(delayed) return;

        Vector3 origin = this.transform.position + (this.transform.up.normalized * collider.bounds.size.magnitude * .37f);
        RaycastHit2D hit = Physics2D.Raycast(origin, this.transform.up, Mathf.Infinity, mask | playerMask);
        Debug.DrawLine(this.transform.position, hit.point);
        lineHit = hit.point;
        line.SetPosition(0, origin);

        linePosition1 = line.GetPosition(0);
        if (hit)
        {
            tag = hit.collider.tag;

            switch (tag)
            {
                case "Wall":
                    line.SetPosition(1, lineHit);
                    overPlayer = false;
                    break;
                case "Player":
                    if (active)
                        hit.collider.gameObject.GetComponent<Player>().KillPlayer();
                    else
                        overPlayer = true;
                    break;
                case "Hazard":
                    hit.collider.gameObject.SetActive(false);
                    overPlayer = false;
                    break;
                default:
                    break;
            }
        }
        else
        {
            defaultHitPosition = this.gameObject.transform.up * lineDistance;

            if (hitBox != null)
            {
                line.SetPosition(1, defaultHitPosition);
            }
        }

        linePosition2 = line.GetPosition(1);

        if (active)
        {
            if (!line.enabled)
                line.enabled = true;

            if(!emission.enabled)
                emission.enabled = true;
        }

        ShapeModifier();
    }

    protected void ShapeModifier()
    {
        shape.scale = new Vector3(line.bounds.size.x / 4, Vector2.Distance(linePosition1, linePosition2), Vector2.Distance(linePosition1, linePosition1));
        shape.position = new Vector3(0, Vector2.Distance(linePosition1, linePosition2) / 2, 0);
    }

    protected bool GetActive()
    {
        return active;
    }

    protected void SetActive(bool b)
    {
        active = b;

        if(line.enabled && mat.mainTexture != open)
        {
            mat.mainTexture = open;
        }
        else if(!line.enabled && mat.mainTexture != closed)
        {
            mat.mainTexture = closed;
        }
    }

    protected virtual IEnumerator BreakBeam(float seconds, bool goToNext = true)
    {
        stage = 0;
        emission.enabled = false;
        line.enabled = false;
        SetActive(false);
        yield return new WaitForSeconds(seconds);
        if(goToNext)
            StartCoroutine(WarningBeam(warningTimer));
    }

    protected virtual IEnumerator MakeBeam(float seconds, bool goToNext = true)
    {
        stage = 2;
        emission.enabled = true;
        line.enabled = true;
        line.widthMultiplier = lineWidth;
        SetActive(true);
        yield return new WaitForSeconds(seconds);
        if(goToNext)
            StartCoroutine(BreakBeam(breakTimer));
    }

    protected virtual IEnumerator WarningBeam(float seconds, bool goToNext = true)
    {
        stage = 1;
        line.enabled = true;
        line.widthMultiplier = lineWidth * .33f;
        SetActive(false);
        yield return new WaitForSeconds(seconds);
        if(goToNext)
            StartCoroutine(MakeBeam(timer));
    }
}
