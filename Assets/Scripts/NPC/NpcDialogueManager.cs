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
    private int mevcutKristalSayisi = 0;
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
        mevcutKristalSayisi = toplananKristal;
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

        
        StartCoroutine(ollamaIstemcisi.SistemDogrulamaDiyalogUret(mevcutKristalSayisi, "Dinamik engelleri ve tuzakları aşarak hedef kristalleri topladık. Sistem analizinin tamamlanmasını bekliyoruz.", secilenRuhHali, (yanit) =>
        {
            kazanmaDurumu = yanit.isWin;
            sohbetGecmisi = "<b>NPC (Terminal):</b> " + yanit.message;
            diyalogMetni.text = sohbetGecmisi;
        }));
    }

    public void GonderButonunaTiklandi()
    {
        string oyuncuMesaji = oyuncuGirdiAlani != null ? oyuncuGirdiAlani.text : "";

        if (!string.IsNullOrEmpty(oyuncuMesaji))
        {
            
            sohbetGecmisi += "\n\n<b>Operator (Sen):</b> " + oyuncuMesaji;
            diyalogMetni.text = sohbetGecmisi + "\n\n<b>NPC (Terminal):</b> Yanıt işleniyor...";
            oyuncuGirdiAlani.text = "";

            // Oyuncunun yazdığı metin Ollama'ya gidip NPC'nin seçilen ruh halindeki cevabı alınıyor
            StartCoroutine(ollamaIstemcisi.SistemDogrulamaDiyalogUret(mevcutKristalSayisi, oyuncuMesaji, secilenRuhHali, (yanit) => {
                kazanmaDurumu = yanit.isWin;

                sohbetGecmisi += "\n\n<b>NPC (Terminal):</b> " + yanit.message;
                diyalogMetni.text = sohbetGecmisi;
            }));
        }
        else
        {
            Debug.Log("Lütfen bir mesaj girin.");
        }
    }
}