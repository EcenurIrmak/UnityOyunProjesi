using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    private NpcDialogueManager diyalogYoneticisi;
    private bool tetiklendiMi = false;

    private void Start()
    {
        diyalogYoneticisi = GetComponent<NpcDialogueManager>();

        if (diyalogYoneticisi == null)
        {
            Debug.LogError("HATA: NPC üzerinde NpcDialogueManager bileşeni bulunamadı!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !tetiklendiMi)
        {
            tetiklendiMi = true;

            // Oyuncunun üzerindeki CharacterController'dan gerçek taş/kristal sayısını alıyoruz
            CharacterController oyuncu = other.GetComponent<CharacterController>();
            int oyuncuTaslari = oyuncu != null ? oyuncu.stones : 0;

            if (diyalogYoneticisi != null)
            {
                // Güncel ve hatasız metot adı ile NPC'yi başlatıyoruz
                diyalogYoneticisi.NpcKarsilamasiBaslat(oyuncuTaslari, NPCMood.Mutlu);
            }
            else
            {
                Debug.LogError("NpcDialogueManager bulunamadı!");
            }
        }
    }
}