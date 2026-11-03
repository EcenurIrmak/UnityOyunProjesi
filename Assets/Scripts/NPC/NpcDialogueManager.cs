using System.Collections;
using TMPro; // TextMeshPro kullanıyoruz
using UnityEngine;
using UnityEngine.UI;

public class NpcDialogueManager : MonoBehaviour
{
    [Header("UI Bilesenleri")]
    public GameObject dialoguePanel;     // Diyalog kutusu arayüzü (Panel)
    public TextMeshProUGUI dialogueText; // NPC'nin konuşmasını yazdıracağımız yazı alanı
    public Button continueButton;        // Oyunu bitirme/devam etme butonu

    [Header("Referanslar")]
    public OllamaClient ollamaClient;

    private bool isDialogueActive = false;

    private void Start()
    {
        // Oyun başlar başlamaz diyalog panelini gizle
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
    }

    // Oyuncu asansörle yukarı çıkıp NPC'ye ulaştığında bu fonksiyonu dışarıdan tetikleyeceğiz
    public void TriggerNpcEncounter(int totalCollectedGems)
    {
        if (isDialogueActive) return;
        isDialogueActive = true;

        dialoguePanel.SetActive(true);
        dialogueText.text = "NPC düşünüyor...";
        continueButton.gameObject.SetActive(false);

        // Ollama'dan dinamik yanıtı asenkron olarak iste
        StartCoroutine(ollamaClient.GenerateDialogue(totalCollectedGems, (response) =>
        {
            StartCoroutine(TypeText(response));
        }));
    }

    // Yazının ekrana daktilo efektiyle harf harf akması için
    IEnumerator TypeText(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.025f); // Yazılma hızı
        }

        // Yazı bittikten sonra butonu göster
        continueButton.gameObject.SetActive(true);
    }

    // Butona tıklandığında çalışacak fonksiyon
    public void OnContinueButtonClicked()
    {
        dialoguePanel.SetActive(false);
        Debug.Log("Oyun başarıyla tamamlandı!");
        // Buraya istersen sahne değiştirme kodu ekleyebilirsin:
        // UnityEngine.SceneManagement.SceneManager.LoadScene("AnaMenu");
    }
}
