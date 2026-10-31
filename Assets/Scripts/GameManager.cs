using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] List<GameObject> Roads = new List<GameObject> ();
    [SerializeField] private Transform playerPrefab;
    [SerializeField] Transform carSpawn;
  

    float prevPlayerZ;
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

    private void FixedUpdate()
    {
        //Oyuncunun z pozisyonundaki değişimi
        float deltaZ = playerPrefab.position.z - prevPlayerZ;

        // car spawner z eksenini değişim kadar arttır.
        carSpawn.position += new Vector3(0, 0, deltaZ);

        // bir sonraki kare için z pozisyonunu güncelle
        prevPlayerZ = playerPrefab.position.z;
    }

    void CreateRoad()
    {
        Instantiate(Roads[Random.Range(0,Roads.Count)], transform.forward * roadLength, transform.rotation);
        roadLength += 3.198714f;
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(1);
    }
}
