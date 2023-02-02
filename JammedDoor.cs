using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JammedDoor : MonoBehaviour
{
    private Rigidbody rigid;
    public Transform player;

    [Header("Waypoints:")]
    public GameObject[] waypoints;
    public int currentpoint = 0;
    
    [Header("Movement:")]
    public float speed;
    float Cuberadius = 1;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        if (Vector3.Distance(waypoints[currentpoint].transform.position, rigid.position) < Cuberadius)
        {
            currentpoint++;
            if (currentpoint >= waypoints.Length)
            {
                currentpoint = 0;
            }
        }
        rigid.position = Vector3.MoveTowards(rigid.position, waypoints[currentpoint].transform.position, Time.fixedDeltaTime * speed);
    }
}
