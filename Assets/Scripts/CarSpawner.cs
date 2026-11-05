using System.Collections;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] GameObject[] carPrefabs;
    [SerializeField] GameObject[] stonePrefabs;

    [SerializeField] private float minSpawnTime = 0.5f;
    [SerializeField] private float maxSpawnTime = 3f;
    [SerializeField] GameManager gameManager;

    public int stoneCount = 0;

    // Taş limiti burada da 30 olarak güncellendi
    private int stoneLimit = 30;
    private bool stopCarSpawning = false;

    [SerializeField] CharacterController characterController;

    void Start()
    {
        StartCoroutine(SpawnCars());
        StartCoroutine(SpawnStones());
    }

    IEnumerator SpawnCars()
    {
        while (characterController.isAlive && !stopCarSpawning)
        {
            float randomTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(randomTime);

            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];

            Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length)], spawnPoint.position, spawnPoint.rotation);
        }
    }

    IEnumerator SpawnStones()
    {
        while (characterController.isAlive && stoneCount < stoneLimit)
        {
            float randomTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(randomTime);

            if (stoneCount >= stoneLimit) break;

            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];

            Instantiate(stonePrefabs[Random.Range(0, stonePrefabs.Length)], spawnPoint.position + new Vector3(0, 0.1f, 0), spawnPoint.rotation);
            stoneCount++;

            // 30. taş üretildiği an araba üretiminin 5 saniye sonra durması için geri sayım başlatılıyor
            if (stoneCount >= stoneLimit)
            {
                StartCoroutine(StopCarsAfterDelay(5f));
            }
        }
    }

    // Arabaların 5 saniye daha üretilip sonra durmasını sağlayan fonksiyon
    IEnumerator StopCarsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        stopCarSpawning = true;
    }
}