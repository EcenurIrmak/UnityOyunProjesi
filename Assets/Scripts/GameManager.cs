using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] List<GameObject> Roads = new List<GameObject> ();
    [SerializeField] private GameObject[] Platform;
    [SerializeField] private Transform playerPrefab;
    [SerializeField] Transform carSpawn;
    [SerializeField] private Transform npcCharacter;


    float prevPlayerZ;
    private float roadLength = 3.198714f;
    private int count = 5;
    CarSpawner carSpawner;
    public bool platformCreated = false;
    int StoneLimit = 5;

    void Start()
    {
        if (carSpawner == null)
        {
            carSpawner = FindObjectOfType<CarSpawner>();
        }
        // İlk zemini üret
        Instantiate(Roads[0],transform.position, transform.rotation);
        for (int i=0; i< count; i++)
        {
            CreateRoad();
        }
    }
    void Update()
    {
        if (carSpawner == null) return;
        if (carSpawner.stoneCount < StoneLimit)
        {
            if (playerPrefab.position.z > roadLength - 3.198714f * count)
            {
                CreateRoad();
            }
        }
        else if (carSpawner.stoneCount == StoneLimit && !platformCreated)
        {
            CreatePlatform();
            platformCreated = true;
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
    void CreatePlatform()
    {
        Vector3 platformPosition = new Vector3(0, 0, (roadLength + 12.41863f - 3.198714f));

        // 1. Platformu oluştur
        GameObject newPlatform = Instantiate(Platform[0], platformPosition, transform.rotation);
        Debug.Log("Platform başarıyla oluşturuldu.");

        // 2. Platformun içindeki NPCSpawnPoint'i ara
        Transform spawnPoint = newPlatform.transform.Find("NPCSpawnPoint");

        if (spawnPoint != null)
        {
            Debug.Log("NPCSpawnPoint bulundu.");

            // Sahnedeki NPC'yi bulamadıysak kodla bulmayı deneyelim (Güvenceye alalım)
            if (npcCharacter == null)
            {
                GameObject npcObj = GameObject.Find("NPCCharacter"); // Sahnedeki NPC'nin adı
                if (npcObj != null) npcCharacter = npcObj.transform;
            }

            if (npcCharacter != null)
            {
                // 3. NPC'yi spawn noktasına taşı ve aktif et
                npcCharacter.position = spawnPoint.position;
                npcCharacter.rotation = spawnPoint.rotation;

                // Eğer NPC kapalı duruyorsa görünür yap
                npcCharacter.gameObject.SetActive(true);

                Debug.Log("NPC başarıyla spawn noktasına taşındı ve aktif edildi!");
            }
            else
            {
                Debug.LogError("HATA: GameManager'da npcCharacter atanmamış ve sahnede de bulunamadı!");
            }
        }
        else
        {
            Debug.LogError("HATA: Platform prefab'ının içinde 'NPCSpawnPoint' adında boş bir obje bulunamadı! İsmini kontrol edin.");
        }
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
