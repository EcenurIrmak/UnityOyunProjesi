using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaClient : MonoBehaviour
{
    private string ollamaUrl = "http://localhost:11434/api/generate";
    [SerializeField] private string modelName = "llama3"; // Bilgisayarında yüklü model adı (örn: llama3 veya phi3)

    [Serializable]
    private class OllamaRequest
    {
        public string model;
        public string prompt;
        public bool stream;
    }

    [Serializable]
    private class OllamaResponse
    {
        public string response;
        public bool done;
    }

    public IEnumerator GenerateDialogue(int collectedGems, Action<string> onResponseReceived)
    {
        string prompt;

        if (collectedGems >= 20)
        {
            prompt = $"Bir video oyununda kapıyı koruyan eğlenceli bir NPC'sin. Oyuncu 30 üzerinden {collectedGems} adet enerji taşı topladı (hedef en az 20 idi). Oyuncuyu tebrik eden, samimi ve kısa (en fazla 2 tümce) bir Türkçe geçiş izni mesajı yaz.";
        }
        else
        {
            prompt = $"Bir video oyununda kapıyı koruyan esprili bir NPC'sin. Oyuncu 30 üzerinden {collectedGems} adet enerji taşı topladı (hedef 20 idi, yani eksik topladı). Oyuncuya takılan, 'ah be bir tane kaçtı' tarzında esprili bir cümle kurup, ardından 'neyse bu seferlik seni geçiriyorum' diyerek kapıyı açacağını belirten kısa bir Türkçe mesaj yaz.";
        }

        OllamaRequest bodyObj = new OllamaRequest
        {
            model = modelName,
            prompt = prompt,
            stream = false
        };

        string jsonBody = JsonUtility.ToJson(bodyObj);

        using (UnityWebRequest request = new UnityWebRequest(ollamaUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                OllamaResponse res = JsonUtility.FromJson<OllamaResponse>(jsonResponse);
                onResponseReceived?.Invoke(res.response.Trim());
            }
            else
            {
                // Ollama kapalıysa veya yanıt vermezse oyunun donmaması için yedek metin
                string fallback = collectedGems >= 20
                    ? "Harika iş çıkardın, yeterli taşı topladın. Geçebilirsin!"
                    : "Hey, 20'den az taşın var ama neyse, bu seferlik seni idare ediyorum geç bakalım!";
                onResponseReceived?.Invoke(fallback);
            }
        }
    }
}
