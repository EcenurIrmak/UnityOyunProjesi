AI-Powered 3D Runner & Yerel LLM Destekli Etkileşimli Oyun Projesi
Proje Adı: AI-Powered 3D Runner & Local LLM NPC Companion

Ekip Üyeleri:

Marjana Ruzyyeva (231307001)

Ecenur Irmak (241307089)

Tarih:5.11.2026

Üniversite / Bölüm: Kocaeli Üniversitesi, Teknoloji Fakültesi, Bilişim Sistemleri Mühendisliği

1. Giriş: Problemin Tanımı, Amaç ve Genel Oyun Senaryosu
Problemin Tanımı
Geleneksel 3D sonsuz koşu (runner) türündeki video oyunları, genellikle tekdüze ilerleyen mekanikler, statik harita tasarımları ve önceden kodlanmış (hardcoded) tek tip NPC diyalogları üzerine kuruludur. Bu durum, oyuncuların oyun dünyasıyla kurduğu etkileşimi kısa sürede sığlaştırmakta ve tekrara düşen, öngörülebilir bir deneyim yaratmaktadır. Oyuncunun oyun içerisindeki başarıları, topladığı kaynaklar veya gösterdiği performans ile oyunun sonlandığı nokta arasındaki bağ genellikle kısıtlıdır. Ayrıca yapay zeka entegrasyonu barındırmayan geleneksel projelerde NPC karakterler yalnızca pasif birer görsel unsur veya sabit metin taşıyıcısı olarak kalmakta, oyuncuya dinamik, akıcı ve bağlamsal geri bildirimler sunamamaktadır.

Amaç
Bu projenin temel amacı; nesne yönelimli programlama (OOP) prensiplerine tam bağlı, modüler, genişletilebilir ve sürdürülebilir bir Unity 3D mimarisi kurarak proaktif bir oyun döngüsü tasarlamaktır. Proje kapsamında, prosedürel olarak üretilen yollar üzerinde ilerleyen bir karakterin dinamik engellerle ve hareketli araçlarla mücadelesi, belirli bir kaynak toplama hedefi (30 adet enerji taşı) ile sınırlandırılmıştır. Hedefe ulaşıldığında oyun alanı evrilmekte ve tamamen yerel makinede koşan bir Büyük Dil Modeli (LLM - Ollama) altyapısıyla entegre edilen akıllı bir NPC, oyuncunun performansına özel dinamik, tekrara düşmeyen diyaloglar üretmektedir. Böylece klasik arcade mekanikleri ile modern üretken yapay zeka (Generative AI) teknolojileri hibrit bir yapıda başarılı bir şekilde buluşturulmaktadır.

Genel Oyun Senaryosu
Oyuncu, başlangıçta sonsuz döngü mantığıyla dinamik olarak üretilen yollar (CreateRoad) üzerinde ilerler. Arkadan gelen ve rastgele spawn noktalarından türeyen araç engellerine (CarSpawner ve CarMover) karşı dikkatli olmak zorundadır. Bu kaçış ve ilerleyiş sırasında yola yerleştirilen enerji taşlarını toplayarak stoneCount değerini artırır. Taş sayısı tam olarak 30 adede ulaştığında (stoneLimit = 30), sistem otomatik olarak yeni yol üretimini durdurur; araba üretimi ani 
bir kesinti yaratmamak ve oynanış akışını bozmamak adına asenkron bir gecikmeyle (StopCarsAfterDelay) 5 saniye daha devam edip güvenle sonlandırılır. Bu sürecin sonunda son platform (CreatePlatform) sahneye dahil olur ve bu platformun üzerindeki NPCSpawnPoint koordinatlarında gizlenen NPC karakter aktifleşir. Aktifleşen NPC, yerel LLM üzerinden oyuncuyu karşılayarak oyun içi eylemlerine dayalı özelleştirilmiş, akıcı ve anlamlı bir diyalog oturumu başlatır.

2. Otonom AI ve Karar Mekanizması
Oyunun otonom akışı ve spawner mekanizmaları, GameManager ve CarSpawner sınıfları arasındaki asenkron coroutine yapıları ile anlık Update/FixedUpdate döngüleri üzerinden yönetilir. Oyuncunun hareketine göre carSpawn pozisyonu dinamik olarak güncellenirken, taş sınırının denetlenmesi oyunun evre geçişlerini belirler. Aşağıdaki Mermaid akış diyagramı sistemin karar ve durum mekanizmasını ayrıntılı olarak ortaya koymaktadır:

<img width="1947" height="8045" alt="Ollama NPC Dialogue Game Flow-2026-11-05-183228" src="https://github.com/user-attachments/assets/c1bca5da-bbe4-423c-979e-9a8e8aa80b98" />

3. LLM Entegrasyonu Mimarisi
Sistem, dışa bağımlı bulut servisleri yerine tamamen yerel bilgisayar kaynaklarını kullanan Ollama yerel sunucusuna (http://localhost:11434/api/generate) C# UnityWebRequest / HttpClient yapıları aracılığıyla asenkron HTTP POST istekleri göndererek çalıştırılmaktadır.

Sistem Promptu ve İstek/Yanıt Akışı
NPC karakter, oyun evrenine adapte edilmiş siber-rehber kimliğiyle promptlanır. Oyuncu 30 taşı toplayıp son platforma ayak bastığında, oyun durumu ve oyuncunun başarı verileri JSON payload formatında yerel LLM'e iletilir. Yapılandırılmış çıktı şeması sayesinde modelden gelen yanıtlar güvenle parse edilerek oyun arayüzüne yansıtılır.
<img width="8192" height="2510" alt="Ollama NPC Dialogue Game Flow-2026-11-05-190412" src="https://github.com/user-attachments/assets/7a9fd4fa-7101-4e44-a93f-e6c7b365cede" />

4. Hata Yönetimi ve Dayanıklılık Stratejisi
Canlı sunumlar ve test demoları sırasında oyunun kararlılığını korumak ve NOT-6 kuralında belirtilen çökme risklerini sıfıra indirmek amacıyla kapsamlı hata yönetimi stratejileri entegre edilmiştir:

Yerel Sunucu Erişilemezliği (Ollama Kapalı): Ollama servisinin arka planda çalışmadığı veya ağ zaman aşımı (HttpRequestException) yarattığı senaryolarda oyunun hata verip kapanması engellenmiştir. try-catch blokları ile sarılan istek fonksiyonları, hata durumunda anında önceden tanımlanmış fallback (yedek) diyalog metinlerini devreye sokar.

Geçersiz veya Eksik JSON Yanıtları: LLM sunucusundan boş, bozuk veya şemaya uymayan verilerin dönme ihtimaline karşı katı null check (boş referans denetimi) mekanizmaları kurulmuştur.

Bileşen Güvenliği (Null-Reference Koruması): CarSpawner, GameManager ve npcCharacter referanslarının sahnede eksik olması durumunda FindObjectOfType veya GameObject.Find yedek arama sorgularıyla nesneler güvenli bir şekilde atanır.

5. TEST SENARYOLARI VE SONUÇLAR
Senaryo 1: Prosedürel Yol Üretimi ve Sonsuz Döngü Akış Testi (CreateRoad)Senaryonun Amacı: Oyuncu ilerledikçe GameManager üzerindeki CreateRoad metodunun tetiklenerek yol parçalarının dinamik (prosedürel) olarak sahneye hatasız eklenmesi ve bellek yönetimi doğrulaması.Girdi Verileri: Oyuncu karakterinin $Z$ eksenindeki anlık konum verisi, yol prefabrikleri ve tetikleyici (trigger) alanlar.İşlem Adımları:Oyuncu başlangıç zemininden ileriye doğru koşmaya başlar.Oyuncu yolun sonundaki sınır tetikleyicisine bastığında CreateRoad() metodu çağrılır.Yeni yol parçası bir önceki parçanın uç koordinatına eklenir (Instantiate).Arkada kalan ve oyuncunun görüş alanından çıkan eski yol parçaları bellekten temizlenir (Destroy).Beklenen Çıktı / Sonuç: Yolun takılma veya görsel kırılma olmadan sonsuz döngü mantığıyla akıcı şekilde üretilmesi ve bellek sızıntısının (memory leak) önlenmesi. (Sonuç: Başarılı)

Senaryo 2: Dinamik Araç ve Taş Spawner Mekanizması (CarSpawner & CarMover)Senaryonun Amacı: Oyuncunun hareketine paralel olarak arkadan gelen araç engellerinin ve yoldaki enerji taşlarının asenkron Coroutine yapıları ile doğru noktalarda üretilmesinin doğrulanması.Girdi Verileri: CarSpawner Coroutine döngüsü, rastgele spawn noktaları ($X, Y, Z$ koordinatları), araç hız parametreleri.İşlem Adımları:CarSpawner sınıfı başlatılır ve belirlenen saniye aralıklarında Coroutine çalışır.Oyuncunun önünde ve arkasında araçlar ve enerji taşları rastgele şeritlere spawn edilir.CarMover bileşeni aktifleşerek araçları oyuncunun hareket yönüne doğru belirli bir sabit hızla sürer.Beklenen Çıktı / Sonuç: Araçların ve taşların çakışmadan, oyuncunun ilerleyişini zorlaştıracak ancak imkansız kılmayacak dengeli aralıklarla sahneye üretilmesi. (Sonuç: Başarılı)

Senaryo 3: Kaynak Sınırı ve Trafik Sonlandırma Testi (stoneLimit = 30 & StopCarsAfterDelay)
Senaryonun Amacı: Oyuncunun 30 adet enerji taşını topladığında yol üretiminin durması ve trafiğin güvenli bir şekilde kesilmesinin test edilmesi.

Girdi Verileri: stoneCount sayaç değişkeni, stoneLimit = 30 eşik değeri, 5 saniyelik StopCarsAfterDelay gecikme süresi.

İşlem Adımları:

Oyuncu yoldaki enerji taşlarını toplayarak stoneCount değerini artırır.

enerji taşı toplandığı anda (stoneCount == 30) GameManager yeni yol üretimini (CreateRoad) durdurur.

Trafiğin aniden kesilip oyuncuyu sarsmaması için StopCarsAfterDelay Coroutine'i tetiklenerek araç üretimi 5 saniye daha devam eder ve ardından tamamen kapatılır.

Beklenen Çıktı / Sonuç: Yol üretiminin 30. taşta durması, sayaç kilitlenmesi ve 5 saniyelik tolerans süresinin sonunda araç üretiminin sorunsuz şekilde kapanması. (Sonuç: Başarılı, sayaç tam 30'da kilitlendi)

Senaryo 4: Bitiş Platformu Üretimi ve NPC Doğma Mekanizması (CreatePlatform & NPCSpawnPoint)
Senaryonun Amacı: Araç trafiği durduktan sonra son platformun eklenmesi ve NPC karakterinin belirlenen koordinatta aktifleşmesinin doğrulanması.

Girdi Verileri: Son platform prefabriği, CreatePlatform() metodu çağrısı, platform üzerindeki NPCSpawnPoint Transform koordinatı.

İşlem Adımları:

Trafik durduktan sonra CreatePlatform() çalıştırılır ve son platform sahnenin ucuna bağlanır.

Gizli durumda bekleyen NPC karakteri, platform üzerindeki NPCSpawnPoint pozisyonuna ışınlanır ve SetActive(true) yapılır.

Oyuncu platforma adım attığında oyun içi kamera diyalog açısına geçer.

Beklenen Çıktı / Sonuç: Platformun doğru hizada sahneye eklenmesi, NPC'nin tam doğma noktasında görünür hale gelmesi ve diyalog moduna geçilmesi. (Sonuç: Başarılı)

Senaryo 5: Yerel LLM Entegrasyonu, Asenkron İstek ve Dayanıklılık Testi (Ollama HTTP / Fallback)
Senaryonun Amacı: Ollama yerel LLM sunucusuna gönderilen asenkron HTTP POST isteğinin doğrulanması ve sunucu kapalı olduğunda çökme yaşanmadan yedek (fallback) diyalog sisteminin devreye girmesi.

Girdi Verileri: http://localhost:11434/api/generate adresi, sistem promptu, oyuncunun başarı verisi (JSON formatında) ve simüle edilmiş sunucu kesintisi.

İşlem Adımları:

Oyuncu NPC'nin yanına vardığında C# tarafında hazırlanan JSON payload'ı yerel Ollama API'ye async/await ile gönderilir.

Normal Çalışma: Modelden dönen dinamik yanıt parse edilerek sohbet paneline basılır.

Hata Simülasyonu: Ollama servisi kapatılarak ağ hatası (HttpRequestException) tetiklenir. try-catch bloğu hatayı yakalar.

Beklenen Çıktı / Sonuç: İstek asenkron yapıldığı için oyun kilitlenmez (FPS düşmez). Sunucu açıkken dinamik LLM yanıtı; kapalıyken önceden tanımlanmış yedek diyalog metni ekrana basılır. Oyun çökmez. (Sonuç: Başarılı, yedek diyalog başarıyla devreye girdi)

Örnek NPC Diyaloğu (Ollama Çıktısı):
NPC Karakter: "Tebrikler gezgin! 30 enerji taşını başarıyla topladın ve bu tehlikeli yolları geride bıraktın. Sınırları aştın, şimdi sistemin gerçek kapısı senin için aralanıyor..."

6. SONUÇ VE TARTIŞMA
6.1. Başarılar
Modüler OOP Mimarisi: GameManager, CarSpawner, CarMover ve CharacterController sınıfları katı nesne yönelimli ilkelere uygun olarak ayrıştırılmış, kod tekrarı önlenmiştir.

Asenkron Performans Optimizasyonu: Coroutine yapıları ve FixedUpdate kullanımı sayesinde oyun içi kare hızı (FPS) kararlılığı korunmuş, arayüz donmaları engellenmiştir.

Yerel Yapay Zeka Entegrasyonu: İnternet bağımlılığı olmaksızın çalışan Ollama altyapısı ile güvenli ve özgün bir diyalog deneyimi elde edilmiştir.

6.2. Sınırlılıklar
Yerel LLM yanıt süresi (inference latency), çalıştırıldığı bilgisayarın GPU/CPU donanım gücüne doğrudan bağımlıdır.

6.3. Olası Geliştirmeler
Gelecek aşamalarda NPC diyaloglarına duygu analizi (sentiment analysis) katmanları eklenerek oyuncunun oynayış tarzına göre farklı senaryo dallanmaları oluşturulabilir.

KAYNAKÇA

Unity Technologies. (2022). Unity Documentation - Procedural Generation & NavMesh. https://docs.unity3d.com

Ollama. (2024). Ollama API Reference: Generate a Response. https://github.com/ollama/ollama/blob/main/docs/api.md

Microsoft Docs. (2023). Asynchronous Programming in C# with async/await. https://learn.microsoft.com/en-us/dotnet/csharp/async

