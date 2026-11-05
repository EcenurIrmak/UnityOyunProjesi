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


5. Uygulamaya Ait Ekran Görüntüleri, Test Senaryoları ve Sonuçlar
Test Senaryoları ve Doğrulama
Kaynak Sınırı Testi (stoneLimit = 30): Oyuncunun topladığı taş sayısının tam olarak 30 olduğunda yeni yol üretiminin durdurulması ve döngünün kırılması test edildi. (Sonuç: Başarılı, sayaç tam 30'da kilitlendi).

Araba Gecikme Testi (StopCarsAfterDelay): 30. taşın toplanmasından hemen sonra trafiğin aniden kesilmesi yerine, oyuncuya nefes alacak alan tanımak adına arabaların 5 saniye daha spawn olmaya devam edip ardından güvenle durması doğrulandı. (Sonuç: Başarılı).

LLM Dayanıklılık Testi: Ollama kapalı simülasyonda test edilerek istisna yakalama mekanizmalarının oyun akışını kesintiye uğratmadığı kanıtlandı. (Sonuç: Başarılı, yedek diyalog devreye girdi).

Örnek NPC Diyaloğu (Ollama Çıktısı)
NPC Karakter: "Tebrikler gezgin! 30 enerji taşını başarıyla topladın ve bu tehlikeli yolları geride bıraktın. Sınırları aştın, şimdi sistemin gerçek kapısı senin için aralanıyor..."

6. Sonuç ve Tartışma
Başarılar
Modüler OOP Mimarisi: GameManager, CarSpawner, CarMover ve CharacterController sınıfları katı nesne yönelimli ilkelere uygun olarak ayrıştırılmış, kod tekrarı önlenmiştir.

Asenkron Performans Optimizasyonu: Coroutine yapıları ve FixedUpdate kullanımı sayesinde oyun içi kare hızı (FPS) kararlılığı korunmuş, arayüz donmaları engellenmiştir.

Yerel Yapay Zeka Entegrasyonu: İnternet bağımlılığı olmaksızın çalışan Ollama altyapısı ile güvenli ve özgün bir diyalog deneyimi elde edilmiştir.

Sınırlılıklar
Yerel LLM yanıt süresi (inference latency), çalıştırıldığı bilgisayarın işlemci (CPU) ve grafik kartı (GPU) donanım gücüne doğrudan bağımlıdır. Düşük donanımlı test cihazlarında çıkarım sürelerinin milisaniyelik gecikmelere sebep olabileceği gözlemlenmiştir.

Olası Geliştirmeler
Gelecek aşamalarda NPC diyaloglarına duygu analizi (sentiment analysis) katmanları eklenerek oyuncunun topladığı taş sayısına veya kalan canına duyarlı farklı senaryo dallanmaları, puan bonusları veya harita içi avantajlar tetiklenebilir.








