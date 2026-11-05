using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

public class OllamaClient : MonoBehaviour
{
    private string ollamaUrl = "http://localhost:11434/api/generate";
    [SerializeField] private string modelName = "llama3";

    public IEnumerator SistemDogrulamaDiyalogUret(int kristalSayisi, string oyuncuMesaji, NPCMood ruhHali, System.Action<NpcResponseData> yanitAksiyonu)
    {
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
                              "Oyuncunun topladığı kristal sayısına (Hedef: 20 ve üzeri) ve oyuncunun yazdığı mesaja göre simülasyon sonucunu belirle. " +
                              "NPC'nin ruh hali ve kararı şuna dayanmalıdır: " + npcRuhHaliMetni + " " +
                              "Asla 'Benim ruh halim şöyle' gibi meta açıklamalar yapma, doğrudan profesyonel bir sistem terminali gibi konuş. " +
                              "Yanıtını MUTLAKA sadece şu JSON formatında ver, başka hiçbir metin ekleme: " +
                              "{\"message\": \"NPC'nin akademik terminal yanıtı ve KAZANDINIZ veya KAYBETTİNİZ kararı\", \"isWin\": true/false}";

        string kullaniciKomutu = $"Veri Analizi: Oyuncu {kristalSayisi} kristal topladı (Hedef eşik: 20). " +
                                 $"Oyuncunun mesajı: '{oyuncuMesaji}'. " +
                                 $"Eğer kristalSayisi >= 20 ise isWin true olsun ve mutlu/heyecanlı bir terminal tonuyla 'KAZANDINIZ' de. " +
                                 $"Eğer kristalSayisi < 20 ise isWin false olsun ve seçilen ruh haline göre (Kızgın/Üzgün/Alaycı) 'KAYBETTİNİZ' de. " +
                                 $"Cevabını tamamen Türkçe, resmi, teknik ve akademik bir dille yaz.";

        yield return StartCoroutine(JsonIstegiGonder(sistemKomutu, kullaniciKomutu, kristalSayisi, oyuncuMesaji, yanitAksiyonu));
    }

    private IEnumerator JsonIstegiGonder(string sistemKomutu, string kullaniciKomutu, int kristalSayisi, string oyuncuMesaji, System.Action<NpcResponseData> yanitAksiyonu)
    {
        string kacirilmisSistem = sistemKomutu.Replace("\"", "\\\"");
        string kacirilmisKullanici = kullaniciKomutu.Replace("\"", "\\\"");

        string jsonGövdesi = "{" +
            "\"model\": \"" + modelName + "\"," +
            "\"system\": \"" + kacirilmisSistem + "\"," +
            "\"prompt\": \"" + kacirilmisKullanici + "\"," +
            "\"stream\": false," +
            "\"format\": \"json\"" +
        "}";

        byte[] hamGovde = Encoding.UTF8.GetBytes(jsonGövdesi);

        using (UnityWebRequest webIstegi = new UnityWebRequest(ollamaUrl, "POST"))
        {
            webIstegi.uploadHandler = new UploadHandlerRaw(hamGovde);
            webIstegi.downloadHandler = new DownloadHandlerBuffer();
            webIstegi.SetRequestHeader("Content-Type", "application/json");

            yield return webIstegi.SendWebRequest();

            if (webIstegi.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Ollama Bağlantı Hatasi: " + webIstegi.error);
                bool varsayilanKazandi = kristalSayisi >= 20;
                yanitAksiyonu?.Invoke(new NpcResponseData
                {
                    message = varsayilanKazandi ? "Veri doğrulaması başarılı. KAZANDINIZ!" : "Yetersiz veri. KAYBETTİNİZ!",
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
        try
        {
            int yanitIndeksi = hamJson.IndexOf("\"response\":\"");
            if (yanitIndeksi != -1)
            {
                int baslangic = yanitIndeksi + 12;
                int bitis = hamJson.IndexOf("\",\"done\":");
                if (bitis == -1) bitis = hamJson.LastIndexOf("\"");

                if (bitis > baslangic)
                {
                    string icMetin = hamJson.Substring(baslangic, bitis - baslangic);
                    icMetin = System.Text.RegularExpressions.Regex.Unescape(icMetin);

                    NpcResponseData veri = JsonUtility.FromJson<NpcResponseData>(icMetin);
                    if (veri != null && !string.IsNullOrEmpty(veri.message))
                    {
                        return veri;
                    }
                }
            }
        }
        catch (System.Exception hata)
        {
            Debug.LogError("JSON Çözümleme Hatası: " + hata.Message);
        }

        bool varsayilanKazandi = kristalSayisi >= 20;
        return new NpcResponseData
        {
            message = varsayilanKazandi ? "Simülasyon matrisi onaylandı. KAZANDINIZ!" : "Yetersiz parametre girişi. KAYBETTİNİZ!",
            isWin = varsayilanKazandi
        };
    }
}