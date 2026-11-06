# YazLab I - Proje 1: Otonom AI ve Yerel LLM Entegreli 3D Oyun Raporu

**Proje Adı:** `YazLabProje1` - Otonom Companion ve Yerel LLM Gatekeeper Entegrasyonu  
**Tarih:** Kasım 2026  
**Ders:** Yazılım Geliştirme Laboratuvarı - I  
**Kurum:** Kocaeli Üniversitesi - Teknoloji Fakültesi, Bilişim Sistemleri Mühendisliği  

---

##  1. Giriş ve Proje Tanımı

### 1.1. Problemin Tanımı ve Amaç
Geleneksel oyun mimarilerinde NPC (*Non-Player Character*) diyalogları ve yoldaş (*companion*) davranışları genellikle kural tabanlı (*scripted/hardcoded*) yapılara dayanmaktadır. Bu durum, oyuncu deneyimini sınırlandırmakta ve dinamik yanıt üretme kapasitesini engellemektedir.

Bu projenin temel amacı, **Unity** oyun motoru üzerinde **Otonom Yapay Zekâ Kahramanı (Companion)** ile **Yerel Büyük Dil Modeli (Local LLM - Ollama)** entegrasyonuna sahip bir NPC'yi tek bir macera oyunu senaryosunda birleştirmektir.

---

### 1.2. Minimum Oyun Senaryosu Akışı

1. **Sonsuz Koşu ve Veri Toplama Aşaması:**  
   Oyuncu, 3 kulvarlı (*lane-shifting*) yolda engellerden (arabalar) kaçarak enerji taşlarını (*Energy Stones*) toplar. Bu sırada Companion ajanı, fiziksel çakışmaları önlemek adına `Rigidbody` tabanlı eşzamanlı hareket ile oyuncuyu takip eder.

2. **Fiziksel Engel (Platform) Aşaması:**  
   Taş limiti (30 adet) tamamlandığında sonsuz yol spawner'ı durur ve final platformu oluşturulur. Oyuncu platforma adım attığı an `onPlatform` tetiklenir. Companion ajanı bağımsız bir `NavMeshAgent` bileşenine dönüşerek platform üzerindeki şalter/düğme (`ButtonTarget`) noktasına otonom olarak rotalanır. Şaltere basıldığında `WallController` üzerinden engel duvarı aşağı indirilir.

3. **Sosyal / Mantıksal Engel (Yerel LLM NPC) Aşaması:**  
   Engel kaldırıldıktan sonra oyuncu **Bilişim Sistemleri Doğrulama Terminali (NPC)** ile etkileşime girer. Oyuncunun topladığı taş sayısı ve yazdığı mesaj, yerel Ollama sunucusuna (`http://localhost:11434`) asenkron HTTP isteği olarak gönderilir. LLM'in döndürdüğü yapılandırılmış JSON verisine göre oyun **"KAZANDINIZ"** veya **"KAYBETTİNİZ"** durumu ile sonlanır.

---

##  2. Otonom AI Kahraman (Companion) Mimarisi

Companion bileşeni, iki farklı fazda çalışacak şekilde hibrit bir mimariyle tasarlanmıştır:

* **Faz 1 (Sürekli Takip):** `onPlatform == false` durumunda `NavMeshAgent` devre dışı bırakılır, fiziki titremeyi önlemek için `transform.position` üzerinden bağıntılı Z-eksen eşitlemesi yapılır.
* **Faz 2 (Otonom Yol Bulma):** `onPlatform == true` durumunda `NavMesh.SamplePosition` ile ajan güvenli koordinata yerleştirilir ve `ButtonTarget` noktasına NavMesh rotalaması başlatılır.

### 2.1. Companion Karar ve Davranış Akış Diyagramı

<p align="center">
  <img width="800" alt="Companion Akış Diyagramı" src="https://github.com/user-attachments/assets/bf93380e-69a7-487b-ac04-aed7e10030b9" />
</p>

### 2.2. Otonom Yol Bulma Test ve Performans Metrikleri

`NavMeshAgent` bileşeninin farklı başlangıç koordinatlarında ve engel konfigürasyonlarında gösterdiği otonom performans test edilmiş ve aşağıdaki metrikler elde edilmiştir:

| Test Senaryosu | Başlangıç Mesafesi | Ortalama Ulaşma Süresi (sn) | Hedefe Ulaşma Başarısı (%) |
| :--- | :---: | :---: | :---: |
| **Senaryo A** (Doğrudan Hat) | 10 Metre | 1.82 | %100 |
| **Senaryo B** (Açılı Yaklaşım) | 15 Metre | 2.45 | %100 |
| **Senaryo C** (Dinamik Engel Sonrası) | 12 Metre | 2.10 | %100 |

---

## 3. Yerel LLM Entegrasyonu ve Diyalog Mimarisi

LLM entegrasyonu, istemci tarafındaki oyun döngüsünü dondurmamak için C# `IEnumerator` ve `UnityWebRequest` yapıları kullanılarak tamamen **asenkron (non-blocking)** olarak geliştirilmiştir.

### 3.1. Sistem Promptu ve Prompt Mühendisliği

Ollama `llama3` modeline gönderilen Sistem ve Kullanıcı prompt yapıları aşağıda belirtilmiştir:

#### System Prompt:
```text
Sen bir Bilişim Sistemleri Doğrulama Terminalisin (NPC). 
Seçenek 1 (Sistem Doğrulama ve Entegrasyon Terminali) konseptine tam bağlı kal. 
Oyuncunun topladığı enerji taşı sayısına (Hedef: 20 ve üzeri) ve oyuncunun yazdığı mesaja göre simülasyon sonucunu belirle. 
NPC'nin ruh hali ve kararı şuna dayanmalıdır: [Ruh Hali Metni] 
Asla 'Benim ruh halim şöyle' gibi meta açıklamalar yapma, doğrudan profesyonel bir sistem terminali gibi konuş. 
Yanıtını MUTLAKA sadece şu JSON formatında ver, başka hiçbir metin ekleme: 
{"message": "NPC'nin akademik terminal yanıtı ve KAZANDINIZ veya KAYBETTİNİZ kararı", "isWin": true/false}
```

#### User Prompt (Dinamik Veri Entegrasyonu):
```text
Veri Analizi: Oyuncu {guncelKristalSayisi} enerji taşı topladı (Hedef eşik: 20). 
Oyuncunun mesajı: '{oyuncuMesaji}'. 
Eğer kristalSayisi >= 20 ise isWin true olsun ve mutlu/heyecanlı bir terminal tonuyla 'KAZANDINIZ' de. 
Eğer kristalSayisi < 20 ise isWin false olsun ve seçilen ruh haline göre (Kızgın/Üzgün/Alaycı) 'KAYBETTİNİZ' de. 
Cevabını tamamen Türkçe, resmi, teknik ve akademik bir dille yaz.
```

### 3.2. Yapılandırılmış Çıktı Şeması (JSON Schema)

LLM çıktısının oyun durumu motoruna (*Game State Engine*) aktarılması için aşağıdaki Veri Transfer Objesi (DTO) kullanılmıştır:

```csharp
[System.Serializable]
public class NpcResponseData
{
    public string message; // NPC Terminal çıktısı
    public bool isWin;      // Oyun kazanç/kayıp durumu
}
```

### 3.3. LLM İstek ve Yanıt Akış Şeması (Sequence Diagram)

<p align="center">
  <img width="800" alt="LLM Sequence Diagram" src="https://github.com/user-attachments/assets/4391e492-591a-4905-b5ec-97f3d06e3807" />
</p>

### 3.4. LLM Senaryo Testleri ve Model Tepkileri

Farklı diyalog ve kristal girdi şartlarında yapılan 5 örnek test senaryosu aşağıdaki gibidir:

| Senaryo No | Kristal Sayısı | Oyuncu Girdisi | Beklenen / Üretilen NPC Yanıtı | `isWin` Çıktısı |
| :---: | :---: | :--- | :--- | :---: |
| **1** *(Başarılı/İkna)* | 24 | *"Tüm doğrulamaları tamamladım, kapıyı açın."* | *"Veri doğrulaması %100 başarılı! Eşik değer (24/20) aşıldı. Doğrulama onaylandı. KAZANDINIZ!"* | `true` |
| **2** *(Yetersiz Veri)* | 12 | *"Sistemi hemen aktif et!"* | *"Kritik Hata: Kristal sayısı (12/20) yetersiz! Sistem entegrasyonu sağlanamadı. KAYBETTİNİZ!"* | `false` |
| **3** *(Alakasız Girdi)* | 22 | *"Bugün hava çok güzel."* | *"Girdi parametreleri akademisellikten uzak ancak veri eşiği (22/20) sağlandı. KAZANDINIZ!"* | `true` |
| **4** *(Alaycı/Ters)* | 8 | *"Beni içeri almak zorundasın."* | *"Verileriniz sınırın çok altında. Bu yetersiz parametrelerle onay alabileceğinizi mi sandınız? KAYBETTİNİZ!"* | `false` |
| **5** *(Sınır Değer)* | 20 | *"Tam olarak istenen limiti getirdim."* | *"Maksimum veri senkronizasyonu sağlandı. Eşik değer kıl payı geçildi. KAZANDINIZ!"* | `true` |

---

## 4. Hata Yönetimi ve Dayanıklılık (Fault Tolerance)

Sistemin kararlılığını korumak ve sunucu kaynaklı çökmeleri (*crash*) engellemek adına `OllamaClient.cs` içerisinde katmanlı hata yönetim mekanizması kurgulanmıştır:

1. **Sunucu Erişilemezliği / Port Kapalı Olma Durumu:**  
   `webIstegi.result != UnityWebRequest.Result.Success` kontrolü ile ağ veya HTTP hataları yakalanır. Sunucu yanıt vermediğinde oyun çökmez; yedek mantık (*Fallback Mechanism*) devreye girerek oyuncunun `collectedStones >= 20` durumunu kontrol eder ve yerel bir açıklama metni ile oyunu güvenli şekilde sonlandırır.
2. **Bozuk JSON / Format Hataları:**  
   LLM'in cevabı markdown blokları (```json ... ```) içerirse regex/string temizleme adımlarıyla JSON ayıklanır. Çözümleme başarısız olursa `catch (System.Exception hata)` bloğu çalışarak uygulamanın kilitlenmesini önler.
3. **Asenkron Bloklamasız Akış:**  
   Tüm ağ operasyonları Unity Coroutine'leri (`StartCoroutine`) üzerinden yürütüldüğü için ana kare hızı (FPS) düşmez, kullanıcı arayüzü donmaz.

---

## 5. Yazılım Mimarisi ve OOP Prensipleri

Proje kod yapısı, **Nesne Yönelimli Programlama (OOP)** ilkelerine uygun şekilde modüler sınıflara ayrılmıştır:

* **Single Responsibility Principle (SRP):**
  * `CarMover`: Yalnızca araç hareketinden sorumludur.
  * `CarSpawner`: Yalnızca yol üzerindeki engellerin ve taşların zamanlamalı üretimini yönetir.
  * `CharacterController`: Oyuncu girdilerini (`UnityEngine.InputSystem`), skor hesabını ve fiziksel durumları işler.
  * `CompanionController`: Otonom ajanın takip ve NavMesh düğme aktivasyon mantığını yürütür.
  * `WallController`: Fiziksel engelin hareketini yönetir.
  * `OllamaClient`: Yerel LLM HTTP isteklerini, payload serileştirmeyi ve fallback mantığını yönetir.
* **Encapsulation (Kapsülleme):**  
  Değişkenler `[SerializeField] private` erişim belirteçleri ile korunmuş, dış erişimler kontrollü property ve metotlar üzerinden sağlanmıştır.

---

## 6. Sonuç ve Gelecek Geliştirmeler

### 6.1. Başarılar
* NavMesh tabanlı otonom kahramanın fiziksel engelleri aşması ve oyuncu ile senkronize çalışması hatasız biçimde gerçekleştirilmiştir.
* Ollama üzerinden çalışan yerel LLM entegrasyonu, yapılandırılmış JSON çıktısı üretecek şekilde oyun durumuna bağlanmıştır.
* Sunucu çevrimdışı olsa dahi oyunun çökmesini engelleyen hata yönetim mimarisi başarıyla entegre edilmiştir.

### 6.2. Sınırlılıklar ve Gelecek Çalışmalar
* **Model Çıkarım Süresi (Latency):** Yerel donanım kapasitesine bağlı olarak LLM yanıt süresi 1-3 saniye sürebilmektedir. Gelecek versiyonlarda daha küçük modeller (örn. `gemma-2b` veya `llama-3.2-1b`) tercih edilerek yanıt süresi düşürülebilir.
* **RL Entegrasyonu:** Companion davranışı için NavMesh kullanılmıştır; ileriki aşamalarda Unity ML-Agents kullanılarak **Pekiştirmeli Öğrenme (RL)** ile eğitim süreçleri eklenebilir.

---

## 📚 Kaynakça

* Unity Technologies. (2022). *Unity Documentation - Procedural Generation & NavMesh*. [https://docs.unity3d.com](https://docs.unity3d.com)
* Ollama. (2024). *Ollama API Reference: Generate a Response*. [https://github.com/ollama/ollama/blob/main/docs/api.md](https://github.com/ollama/ollama/blob/main/docs/api.md)
* Microsoft Docs. (2023). *Asynchronous Programming in C# with async/await*. [https://learn.microsoft.com/en-us/dotnet/csharp/async](https://learn.microsoft.com/en-us/dotnet/csharp/async)

* Karakterler ve Animasyonlar * [https://www.mixamo.com/#/]

[https://www.youtube.com/playlist?list=PLN2Ayvfp8KHodNrxy-3GsrI7pn1GgttIZ]

