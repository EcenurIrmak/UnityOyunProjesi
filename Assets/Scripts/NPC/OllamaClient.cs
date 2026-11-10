using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

public class OllamaClient : MonoBehaviour
{
    private string ollamaUrl = "http://localhost:11434/api/generate";
    [SerializeField] private string modelName = "llama3";    
    public CharacterController characterController; // Sahneden sürükleyip bırakabilirsiniz

    private void Start()
    {
        if (characterController == null)
        {
            characterController = FindObjectOfType<CharacterController>();
        }
    }

    public IEnumerator SistemDogrulamaDiyalogUret(string oyuncuMesaji, NPCMood ruhHali, System.Action<NpcResponseData> yanitAksiyonu)
    {        
        int guncelKristalSayisi = characterController != null ? characterController.collectedStones : 0;

        string npcRuhHaliMetni = ruhHali switch
        {
            NPCMood.Mutlu => "Veri doğrulaması %100 başarılı! Eşik değer aşıldı. Tebrikler, KAZANDINIZ!",
            NPCMood.Heyecanli => "Mükemmel performans! Maksimum veri senkronizasyonu sağlandı. Harika iş, KAZANDINIZ!",
            NPCMood.Kizgin => "Kritik hata: Kristal sayısı yetersiz! Sistem gereksinimlerini karşılayamadınız. KAYBETTİNİZ!",
            NPCMood.Uzguncu => "Üzgünüm, hesaplanan değerler hedef aralığın altında kaldı. Başarısız oldunuz, KAYBETTİNİZ!",
            NPCMood.Alayci => "Verileriniz sınırın altında. Bu performansla onay alabileceğinizi mi sandınız? KAYBETTİNİZ!",
            _ => "Sistem analizi tamamlandı."
        };

        string sistemKomutu = "Sen bir Bilişim Sistemleri Doğrulama Terminalisin (NPC). " +
                              "Seçenek 1 (Sistem Doğrulama ve Entegrasyon Terminali) konseptine tam bağlı kal. " +
                              "Oyuncunun topladığı enerji taşı sayısına (Hedef: 20 ve üzeri) ve oyuncunun yazdığı mesaja göre simülasyon sonucunu belirle. " +
                              "NPC'nin ruh hali ve kararı şuna dayanmalıdır: " + npcRuhHaliMetni + " " +
                              "Asla 'Benim ruh halim şöyle' gibi meta açıklamalar yapma, doğrudan profesyonel bir sistem terminali gibi konuş. " +
                              "Yanıtını MUTLAKA sadece şu JSON formatında ver, başka hiçbir metin ekleme: " +
                              "{\"message\": \"NPC'nin akademik terminal yanıtı ve KAZANDINIZ veya KAYBETTİNİZ kararı\", \"isWin\": true/false}";

        string kullaniciKomutu = $"Veri Analizi: Oyuncu {guncelKristalSayisi} enerji taşı topladı (Hedef eşik: 20). " +
                                 $"Oyuncunun mesajı: '{oyuncuMesaji}'. " +
                                 $"Eğer kristalSayisi >= 20 ise isWin true olsun ve mutlu/heyecanlı bir terminal tonuyla 'KAZANDINIZ' de. " +
                                 $"Eğer kristalSayisi < 20 ise isWin false olsun ve seçilen ruh haline göre (Kızgın/Üzgün/Alaycı) 'KAYBETTİNİZ' de. " +
                                 $"Cevabını tamamen Türkçe, resmi, teknik ve akademik bir dille yaz.";

        yield return StartCoroutine(JsonIstegiGonder(sistemKomutu, kullaniciKomutu, guncelKristalSayisi, oyuncuMesaji, yanitAksiyonu));
    }

    private IEnumerator JsonIstegiGonder(string sistemKomutu, string kullaniciKomutu, int kristalSayisi, string oyuncuMesaji, System.Action<NpcResponseData> yanitAksiyonu)
    {
        OllamaPayload payload = new OllamaPayload
        {
            model = modelName,
            system = sistemKomutu,
            prompt = kullaniciKomutu,
            stream = false,
            format = "json"
        };

        string jsonGövdesi = JsonUtility.ToJson(payload);
        byte[] hamGovde = Encoding.UTF8.GetBytes(jsonGövdesi);

        using (UnityWebRequest webIstegi = new UnityWebRequest(ollamaUrl, "POST"))
        {
            webIstegi.uploadHandler = new UploadHandlerRaw(hamGovde);
            webIstegi.downloadHandler = new DownloadHandlerBuffer();
            webIstegi.SetRequestHeader("Content-Type", "application/json");

            yield return webIstegi.SendWebRequest();

            if (webIstegi.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Ollama Sunucusuna Erişilemedi veya Hata Alındı (Hata Yönetimi Devrede): " + webIstegi.error + " | Yanıt: " + webIstegi.downloadHandler.text);

                bool varsayilanKazandi = kristalSayisi >= 20;

                string guvenliMesaj = varsayilanKazandi
                    ? "Sistem terminali geçici olarak çevrimdışı, ancak veri eşiği (>=20) sağlandı. Doğrulama başarılı, KAZANDINIZ!"
                    : "Sistem terminali yanıt vermiyor ve enerji taşı sayısı yetersiz. Eşik altına kalındı, KAYBETTİNİZ!";

                yanitAksiyonu?.Invoke(new NpcResponseData
                {
                    message = guvenliMesaj,
                    isWin = varsayilanKazandi
                });
            }
            else
            {
                string jsonYaniti = webIstegi.downloadHandler.text;
                NpcResponseData veri = JsonCoz(jsonYaniti, kristalSayisi);
                yanitAksiyonu?.Invoke(veri);
            }
        }
    }

    private NpcResponseData JsonCoz(string hamJson, int kristalSayisi)
    {
        bool varsayilanKazandi = kristalSayisi >= 20;
        try
        {
            OllamaRootResponse root = JsonUtility.FromJson<OllamaRootResponse>(hamJson);
            if (root != null && !string.IsNullOrEmpty(root.response))
            {
                string temizJson = root.response.Trim();
                if (temizJson.StartsWith("```json"))
                {
                    temizJson = temizJson.Substring(7);
                }
                else if (temizJson.StartsWith("```"))
                {
                    temizJson = temizJson.Substring(3);
                }
                if (temizJson.EndsWith("```"))
                {
                    temizJson = temizJson.Substring(0, temizJson.Length - 3);
                }
                temizJson = temizJson.Trim();

                NpcResponseData veri = JsonUtility.FromJson<NpcResponseData>(temizJson);
                if (veri != null && !string.IsNullOrEmpty(veri.message))
                {
                    return veri;
                }
            }
        }
        catch (System.Exception hata)
        {
            Debug.LogWarning("JSON Çözümleme Hatası: " + hata.Message);
        }

        return new NpcResponseData
        {
            message = varsayilanKazandi ? "Simülasyon matrisi onaylandı. KAZANDINIZ!" : "Yetersiz parametre girişi. KAYBETTİNİZ!",
            isWin = varsayilanKazandi
        };
    }
}

[System.Serializable]
public class OllamaPayload
{
    public string model;
    public string system;
    public string prompt;
    public bool stream;
    public string format;
}

[System.Serializable]
public class OllamaRootResponse
{
    public string response;
}