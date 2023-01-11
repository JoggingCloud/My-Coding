using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class PickDestination : MonoBehaviour
{
    // List Gameobject(s) locations that are opn for an AI to go to. 
    public List<GameObject> destinations = new List<GameObject>();

    // List Gameobject(s) locations that are resevered and taken by another AI Gameobject. 
    public List<GameObject> reserveSpots = new List<GameObject>();
    
    // On play, this access child object points that are tagged "wow" from the empty parent object that holds all the points. 
    void Awake()
    {
       destinations = GameObject.FindGameObjectsWithTag("wow").ToList();
    }

    public GameObject ReserveDestination()
    {
        
        // The random.range lets the computer know that the count could be any number between 0, and the amount of destinations in the scene which could be infinite.
        GameObject destination = destinations[Random.Range(0, destinations.Count)];
        
        // When an AI Gameobject is on a destinsation spot, it adds that gameobject destination to the reserved list in the Inspector 
        reserveSpots.Add(destination);
        // When an AI Gameobject is not on a destination spot, it removes that gameobject destination from the resevered list in the Inspector
        destinations.Remove(destination);
        // Returns the destinations that are occupied and the ones that aren't 
        return destination;
    }

    public void ReleaseDestination(GameObject currentDestination)
    {
        // If the current point is not occupied by an object, then resevered spot will be available & adds that previous reseverved spot to destinations.
        // Null = no object or data being reference in the scene. Basically the game object you thought you have doesn't exist. 
        if (currentDestination != null)
        {
             reserveSpots.Remove(currentDestination);
             destinations.Add(currentDestination);
        }
       
    }
}
