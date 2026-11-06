using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public enum NPCMood
{
    Mutlu,
    Heyecanli,
    Kizgin,
    Uzguncu,
    Alayci
}

public class NpcDialogueManager : MonoBehaviour
{
    [Header("UI Bilesenleri")]
    public GameObject diyalogPaneli;
    public TextMeshProUGUI diyalogMetni;
    public Button devamButonu;
    public TMP_InputField oyuncuGirdiAlani;

    [Header("Referanslar")]
    public OllamaClient ollamaIstemcisi;

    private bool diyalogAktifMi = false;
    private NPCMood secilenRuhHali = NPCMood.Mutlu;
    private bool kazanmaDurumu = false;
    private string sohbetGecmisi = "";

    private void Start()
    {
        if (diyalogPaneli != null) diyalogPaneli.SetActive(false);
        if (devamButonu != null)
        {
            devamButonu.onClick.AddListener(GonderButonunaTiklandi);
        }
    }

    public void NpcKarsilamasiBaslat(int toplananKristal, NPCMood ruhHali = NPCMood.Mutlu)
    {
        if (diyalogAktifMi) return;
        diyalogAktifMi = true;
        secilenRuhHali = ruhHali;
        sohbetGecmisi = "";

        diyalogPaneli.SetActive(true);
        diyalogMetni.text = "Güvenlik ve doğrulama terminaline ulaştınız. Veri analizi başlatıldı...";

        if (oyuncuGirdiAlani != null)
        {
            oyuncuGirdiAlani.gameObject.SetActive(true);
            oyuncuGirdiAlani.text = "";
        }

        if (devamButonu != null)
        {
            devamButonu.gameObject.SetActive(true);
        }

        StartCoroutine(ollamaIstemcisi.SistemDogrulamaDiyalogUret("Dinamik engelleri ve tuzakları aşarak hedef kristalleri topladık. Sistem analizinin tamamlanmasını bekliyoruz.", secilenRuhHali, (yanit) =>
        {
            kazanmaDurumu = yanit.isWin;
            sohbetGecmisi = "<b>NPC (Terminal):</b> " + yanit.message;
            diyalogMetni.text = sohbetGecmisi;
        }));
    }

    public void GonderButonunaTiklandi()
    {
        // 1. OllamaClient atanmış mı kontrol et
        if (ollamaIstemcisi == null)
        {
            Debug.LogError("NpcDialogueManager: 'ollamaIstemcisi' Inspector üzerinde atanmamış!");
            return;
        }

        string oyuncuMesaji = oyuncuGirdiAlani != null ? oyuncuGirdiAlani.text : "";

        if (!string.IsNullOrEmpty(oyuncuMesaji))
        {
            sohbetGecmisi += "\n\n<b>Operator (Sen):</b> " + oyuncuMesaji;

            // 2. diyalogMetni null kontrolü
            if (diyalogMetni != null)
            {
                diyalogMetni.text = sohbetGecmisi + "\n\n<b>NPC (Terminal):</b> Yanıt işleniyor...";
            }

            if (oyuncuGirdiAlani != null)
            {
                oyuncuGirdiAlani.text = "";
            }

            // Coroutine başlatma 
            StartCoroutine(ollamaIstemcisi.SistemDogrulamaDiyalogUret(oyuncuMesaji, secilenRuhHali, (yanit) =>
            {
                kazanmaDurumu = yanit.isWin;
                sohbetGecmisi += "\n\n<b>NPC (Terminal):</b> " + yanit.message;

                if (diyalogMetni != null)
                {
                    diyalogMetni.text = sohbetGecmisi;
                }
            }));
        }
        else
        {
            Debug.Log("Lütfen bir mesaj girin.");
        }
    }
}