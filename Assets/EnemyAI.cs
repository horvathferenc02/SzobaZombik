using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Pathfinding;

[RequireComponent (typeof (Rigidbody2D))]
[RequireComponent (typeof (Seeker))]
public class EnemyAI : MonoBehaviour
{
    // Célpont
    public Transform target;

    //Út frissítése másodpercenként
    public float updateRate = 2f;

    private Seeker seeker;
    private Rigidbody2D rb;

    //Kiszámolt út
    public Path path;

    //Az AI sebessége másodpercenként
    public float speed = 300f;
    public ForceMode2D fMode;

    [HideInInspector]
    public bool PathIsEnded = false;

    public float nextWaypointDistance = 3;

    // A pont ami felé haladunk
    private int currentWaypoint = 0;

    private bool searchingForPlayer = false;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        if (target == null)
        {
            if (!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }
            return;
        }
        //Új úvonal a célpont felé, vissza adja az "OnPathComplete" metódus eredményét.
        
        StartCoroutine (UpdatePath());
    }

    IEnumerator SearchForPlayer() {
        GameObject sResult = GameObject.FindGameObjectWithTag("Player");
        if (sResult == null)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(SearchForPlayer());
        }
        else {
            target = sResult.transform;
            searchingForPlayer = false;
            StartCoroutine(UpdatePath());
            yield return false;
        }
    }

    IEnumerator UpdatePath() {
        if (target == null)
        {
            if (!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }
            yield return false;
        }

        seeker.StartPath(transform.position, target.position, OnPathComplete);

        yield return new WaitForSeconds(1f / updateRate);
        StartCoroutine(UpdatePath());
    }

    public void OnPathComplete(Path p) {
        Debug.Log("Találtam egy útvonalat!(Hibakeresés: )" + p.error);
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;

        }
    }

    
    void FixedUpdate()
    {
        if (target == null)
        {
            if (!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }
            return;
        }
        if (path == null)
            return;
        if (currentWaypoint >= path.vectorPath.Count)

        {
            if (PathIsEnded)
            {
                return;
            }

            Debug.Log("End of path reached.");
            PathIsEnded = true;
            return;
        }
        PathIsEnded = false;

        //A következő ponthoz

        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        dir *= speed * Time.fixedDeltaTime;

        //Az mozgatása
        rb.AddForce(dir, fMode);

        float dist = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);

        if (dist < nextWaypointDistance)
        {
            currentWaypoint++;
            return;
        }

    }
}
