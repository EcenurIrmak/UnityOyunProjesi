using System.Collections;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] GameObject[] carPrefabs;
    [SerializeField] GameObject[] stonePrefabs;

    [SerializeField] private float minSpawnTime = 0.5f;
    [SerializeField] private float maxSpawnTime = 3f;
    public int stoneCount = 0;



    

    [SerializeField] CharacterController characterController;

    void Start()
    {
        StartCoroutine(SpawnCars());
        StartCoroutine(SpawnStones());
    }
    IEnumerator SpawnCars()
    {
        while (characterController.isAlive)
        {
            // rastgele süre belirleme
            float randomTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(randomTime);

            // rastgele ref. noktası seçme
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];

            //araba üretmek
            Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length)], spawnPoint.position, spawnPoint.rotation);
        }
    }
    IEnumerator SpawnStones()
    {
        while (characterController.isAlive)
        {
            // rastgele süre belirleme
            float randomTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(randomTime);

            // rastgele ref. noktası seçme
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];

            //enerji taşı üretmek            
            Instantiate(stonePrefabs[Random.Range(0, stonePrefabs.Length)], spawnPoint.position + new Vector3(0,0.1f,0) , spawnPoint.rotation);
            stoneCount++;
        }
    }
}
