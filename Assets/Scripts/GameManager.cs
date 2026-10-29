using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] List<GameObject> Roads = new List<GameObject> ();
    [SerializeField] private Transform playerPrefab;
    private float roadLength = 3.198714f;
    private int count = 5;

    void Start()
    {
        // İlk zemini üret
        Instantiate(Roads[0],transform.position, transform.rotation);
        for (int i=0; i< count; i++)
        {
            CreateRoad();
        }
    }
    void Update()
    {
        if (playerPrefab.position.z > roadLength - 3.198714f*count)
        {
            CreateRoad();
        }
    }

    void CreateRoad()
    {
        Instantiate(Roads[Random.Range(0,Roads.Count)], transform.forward * roadLength, transform.rotation);
        roadLength += 3.198714f;
    }
}
