using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAdvance : MonoBehaviour
{
    public NavMeshAgent enemy;
    public Transform target;
    public float maxRange;
    public float waitTime = 3f;
    private float startMovement;
    public float dotProduct;

    // An Array range of min and max values of how fast the AI can walk when adjusted in inspector
    [Range(0, 100)] public float speed;

    // An array of min and max of the direction the AI can travel. Can be adjusted in the inspector
    [Range(1, 1000)] public float walkRadius;

    public void Start()
    {
        // On the first frame, the AI is informed of what gameoject is the player
        target = GameObject.Find("FPS").transform;

        // If AI does not equal a numeric value then AI can go to random position on the NavMesh 
        if (enemy != null)
        {
            //Speed of AI movement is determined in the scene 
            enemy.speed = speed;
            enemy.SetDestination(RandomNavMeshLocation());
        }
    }

    //Vector3 calculates random position on the NavMesh
    public Vector3 RandomNavMeshLocation()
    {
        Vector3 lastPosition = Vector3.zero;

        // Randome position is assigned inside the given space where the AI can walk on 
        Vector3 randomPosition = Random.insideUnitSphere * walkRadius;

        // A new random position is constantly made by the AI as it transforms its positon on the NavMesh
        randomPosition += transform.position;
        
        // "out" a;;pws a function to return two values
        // If AI has reached destination "Hit" within the raycasting of the SamplePosition, then return previous postion as it records last postion it hit 
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, walkRadius, 1))
        {
            lastPosition = hit.position;
        }
        return lastPosition;
    }

    // Checking if player is in sight within a certain range and distance.
    public void Update()
    {
        // Current distance between AI and player
        float currentDistance = Vector3.Distance(transform.position, target.position);

        // If current distance between AI and player is less than maxRange of sight given to AI
        if (currentDistance < maxRange)
        {
            // If player is in sight and is in front of the AI field of view then AI will chase after the player. If not then the AI will go back to free roaming.
            if (isInFront() && isLineOfSight())
            {
                enemy.SetDestination(target.position);
            }
        }
        else if (Time.time - startMovement >= waitTime)
        {
            startMovement = Time.time;
            enemy.SetDestination(RandomNavMeshLocation());
        }
    }


    bool isInFront()
    {
        // Getting the direction of the player
        Vector3 directionOfPlayer = (target.position - transform.position).normalized;

        // Getting the angle of where the player is from the forward face of the player and the direction of player  
        float angle = Vector3.Angle(transform.forward, directionOfPlayer);

        // If the player is within the FOV then the line wil appear meaning its true that the enemy can see the player in front. 
        if (Mathf.Abs(angle) > 90 && Mathf.Abs(angle) < 270)
        {
            return true;
        }
        // Else return false 
        return false;
    }

    bool isLineOfSight()
    {
        // Raycast is checking to see if there's anything between the AI and the targeted player
        RaycastHit _hit;
        Vector3 directionOfPlayer = (target.position - transform.position).normalized;

        // Checking for a hit from this AI positon to the direction of the player then its going to go out 
        if (Physics.Raycast(transform.position, directionOfPlayer, out _hit, 50000f))
        { 
            if (_hit.transform.name == "FPS")
            {
                return true;
            }
        }
        // Else return false 
        return false;
    }
}