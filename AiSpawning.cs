using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiSpawning : MonoBehaviour
{
    public GameObject RegularZombie, OtherZombie, BigZombie;
    public Transform[] SpawnPoints;

    public float normalSpawnRate = 2f;
    public float slowSpawnRate = 2f;

    public Transform player;
    public List<GameObject> alivelist = new List<GameObject>();
    public int lifetimeSpawns;
    
    public int maxNormalSpawns = 5;
    public int maxBigSpawns = 5;

    private float lastSpawnTime;

    void Update()
    {
        BasicZombie();
        WarZombie();
        BossZombie();
    }

    public void BasicZombie()
    {
        if (player == null) return;

        if (Time.time - lastSpawnTime >= normalSpawnRate && alivelist.Count < maxNormalSpawns)
        {
            var randomSpawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Length - 1)];
            GameObject currentenemy = Instantiate(RegularZombie, randomSpawnPoint.position, Quaternion.identity);
            alivelist.Add(currentenemy);
            lastSpawnTime = Time.time;
            normalSpawnRate *= 0.98f;
            lifetimeSpawns++;
        }
        for (int i = alivelist.Count - 1; i >= 0; i--)
        {
            if (alivelist[i] == null)
            {
                alivelist.RemoveAt(i);
                lastSpawnTime = Time.time;
            }
        }
    }

    public void WarZombie()
    {
        if (player == null) return;

        if (Time.time - lastSpawnTime >= normalSpawnRate && alivelist.Count < maxNormalSpawns)
        {
            var randomSpawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Length - 1)];
            GameObject currentenemy = Instantiate(OtherZombie, randomSpawnPoint.position, Quaternion.identity);
            alivelist.Add(currentenemy);
            lastSpawnTime = Time.time;
            normalSpawnRate *= 0.98f;
            lifetimeSpawns++;
        }
        for (int i = alivelist.Count - 1; i >= 0; i--)
        {
            if (alivelist[i] == null)
            {
                alivelist.RemoveAt(i);
                lastSpawnTime = Time.time;
            }
        }
    }

    public void BossZombie()
    {
        if (player == null) return;

        if (Time.time - lastSpawnTime >= slowSpawnRate && alivelist.Count < maxBigSpawns)
        {
            var randomSpawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Length - 1)];
            GameObject currentenemy = Instantiate(BigZombie, randomSpawnPoint.position, Quaternion.identity);
            alivelist.Add(currentenemy);
            lastSpawnTime = Time.time;
            slowSpawnRate *= 0.98f;
            lifetimeSpawns++;
        }
        for (int i = alivelist.Count - 1; i >= 0; i--)
        {
            if (alivelist[i] == null)
            {
                alivelist.RemoveAt(i);
                lastSpawnTime = Time.time;
            }
        }
    }
}