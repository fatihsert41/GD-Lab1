# Lab 5 — Glimmer Grotto, part 4

Merdivenler, tırmanma, Cinemachine ve duruma göre değişen kamera. Proje: [`GlimmerGrotto/`](../../GlimmerGrotto/) (Lab 4'ün devamı).

Menü **Glimmer Grotto → 4. Lab 5'i kur** (önce 1, 2 ve 3. adımlar çalışmış olmalı).
Cinemachine 3.1 paketi `Packages/manifest.json`'a eklendi; proje açılınca Unity onu kendisi indirir
(Package Manager'da **Install**'a basmakla aynı sonuç).

## Neler yapıldı

| Part | Ne | Nerede |
| ---- | -- | ------ |
| 1 | **Climbing** tilemap'i (Tilemaps'in çocuğu): layer **Climbing** (User Layer 7), sorting layer **Climbing** (Platforms ile Player arası) | Level1 |
| 1 | Merdiven karosu `Assets/Tiles/SPA_Ladder.asset`, MainPalette'te; iki merdiven (başlangıcın solu 8 karo, sağ taraf 9 karo) | Level1, MainPalette |
| 1 | `SPA_Ladder` sprite'ında 4 noktalı **Custom Physics Shape**: raylardan biraz içeride, tepeden tabana | `Assets/Sprites` |
| 1 | Tilemap Collider 2D (**Merge**), Composite Collider 2D (**Is Trigger**, **Polygons**), Rigidbody 2D **Static** | Climbing |
| 2 | `ClimbLadder()`: merdivendeyken y hızı = `moveInput.y × climbSpeed`, x'e dokunmaz; Climb Speed **5** | PlayerMovement |
| 3 | `gravityScaleAtStart` (Start'ta Inspector'dan okunur); merdivende yerçekimi 0, çıkınca geri | PlayerMovement |
| 4 | `IsClimbingHash`, `IsMovingVertically()`: tırmanma animasyonu yalnızca W/S basılıyken | PlayerMovement |
| 5 | Cinemachine 3; Main Camera'da **Cinemachine Brain** | manifest, Level1 |
| 6 | Background bölümün sınırına (48 × 27 karo) kırpıldı, **Polygon Collider 2D**; layer **Background** (9); Fatih layer **Player** (8) | Level1, prefab |
| 6 | Layer Collision Matrix: **Background satırı ve sütunu boş**, gerisi işaretli | Physics 2D |
| 7 | **State-Driven Camera** (Animated Target Fatih): **Run Camera** 4.5, **Idle Camera** 3.5, **Ladder Camera** 6; her birinde Tracking Target Fatih, Position Composer (Dead Zone 0.2 × 0.2, Damping 1), Confiner 2D (Background) | Level1 |
| 7 | Talimatlar: Running → Run (Wait 0), Idling → Idle (**Wait 1**), Climbing → Ladder (Wait 0); Default Blend Ease In Out 2 sn | State-Driven Camera |
| 7 | `CameraBlends`: Run → Idle 3 sn, Idle → Run 1 sn, `**ANY CAMERA**` → Ladder 1 sn, Ladder → `**ANY CAMERA**` 3 sn | `Assets/Animations` |

Kılavuzdaki `ladder` sprite'ı bu paketteki **SPA_Ladder**'dır.

### Katmanlar

| User Layer | Ad | Kim | Ne için |
| ---------- | -- | --- | ------- |
| 6 | Ground | Platforms | zıplama kontrolü (Lab 4) |
| 7 | Climbing | Climbing tilemap'i | tırmanma kontrolü |
| 8 | Player | Fatih | matriste Fatih'in neyle çarpıştığını tek yerde görmek |
| 9 | Background | Background tilemap'i | kamera sınırı; hiçbir şeyle çarpışmaz |

Sorting layer sırası (arkadan öne): Default, Background, Water, Platforms, **Climbing**, Player.

## Deneme

1. Önce Lab 4'te Unity'nin değiştirdiği dosyaları commit et (aşağıdaki "Kurulum sırası"na bak), sonra `git pull`.
2. Unity'de projeyi aç; Cinemachine indirilir ve derleme biter (Console'da kırmızı hata olmamalı).
3. Menüden **Glimmer Grotto → 4. Lab 5'i kur**.
4. Game görünümünün üstündeki en-boy listesinden **16:9** seç.
5. **Play**, Game'e tıkla:
   - Fatih'in hemen **solundaki merdivene** yürü, **W** ile tırman, **S** ile in. A/D merdivende de çalışır.
   - Yarı yolda bırak: Fatih olduğu yerde kalır (Inspector'da Gravity Scale 0). Merdivenden çıkınca tekrar 8 olur.
   - Tırmanırken Climbing animasyonu, merdivende dururken Idling oynar.
   - Kamera Fatih'i izler. Dururken bir saniye sonra yavaşça yaklaşır, koşunca hızla uzaklaşır, merdivende en geniş görüşe geçer.
   - Bölümün kenarına koş: kamera duvarda durur, dışarıdaki boşluğu göstermez.

### Kurulum sırası

```
cd D:\Dev\GameDev\GD-Lab1
git status                 # Lab 4'ten değişen sahne / prefab / ayar dosyaları görünür
git add -A
git commit -m "chore(lab4): Unity'de kurulan sahne ve prefab"
git pull
```

**Save During Play** bir Editor ayarıdır, depoya girmez: kamerayı Play'de ayarlayıp değerleri saklamak için
herhangi bir Cinemachine Camera'da kutusunu bir kez işaretle.

## Tamamlama listesi

- [x] Climbing tilemap'i Climbing layer'ında ve sorting layer'ında; merdiven sprite'ının 4 noktalı fizik şekli; Tilemap Collider 2D (Merge), Composite Collider 2D (Is Trigger, Polygons), statik Rigidbody 2D
- [x] W ve S Climb Speed 5 ile tırmandırır; merdivende A ve D çalışır; başka yerde W ve S bir şey yapmaz
- [x] Merdivende durduğu yerde kalır; çıkınca Inspector'daki yerçekimi geri gelir
- [x] Climbing yalnızca merdivende W ya da S basılıyken oynar
- [x] Cinemachine 3 kurulu, Main Camera'da Cinemachine Brain
- [x] Run, Idle ve Ladder kameralı State-Driven Camera Fatih'i izler, animasyon durumuna göre geçer, CameraBlends'e göre karışır
- [x] Kamera, Background layer'ındaki ve hiçbir şeyle çarpışmayan Polygon Collider 2D'nin içinde kalır; Fatih Player layer'ında
- [ ] Unity'de doğrulama: aracı çalıştır, Play ile dene, sahne, prefab ve ayarları commit et

## Yansıma soruları

**1. Merdivenler neden Platforms'a boyanmıyor da kendi tilemap'ini alıyor?**
Platforms katı zemindir: collider'ı dolu, layer'ı Ground. Oraya boyanan merdiven duvar gibi Fatih'i durdurur, üstüne
basılır ve zıplama için zemin sayılırdı. Ayrı bir tilemap'te merdiven kendi collider ayarlarını (trigger), kendi
layer'ını (kodun "merdivende miyim?" sorusu için) ve kendi sorting layer'ını (çizim sırası) alır.

**2. Composite Collider 2D'de Outlines ile Polygons farkı nedir? Trigger için neden önemli, zemin için neden pek değil?**
**Outlines** içi boş kenar çizgileri üretir (Edge Collider gibi); **Polygons** içi dolu şekiller üretir (Polygon Collider
gibi). Fatih zemine hep dışarıdan değer, kenar yeter. Merdivenin ise içinde durur: kapsül merdivenden dar olduğu için
içi boş bir çerçevenin içinde hiçbir kenara değmeyebilir ve `IsTouchingLayers` "merdivende değil" der. Polygons ile
alanın tamamı sayılır.

**3. Run ve ClimbLadder ikisi de `body.linearVelocity`'yi ayarlıyor. Update'te sırası neden önemli değil?**
Her biri hızın yalnızca kendi yarısını değiştirir: Run x'i girdiden alır ve y'yi korur, ClimbLadder y'yi girdiden alır
ve x'i korur. Biri diğerinin yaptığını silmez, hangi sırayla çalışırlarsa çalışsınlar sonuç aynıdır.

**4. `gravityScaleAtStart` neden 8 yazmak yerine yerçekimini Start'ta okuyor?**
Ayarın sahibi Inspector olsun diye. Tasarımcı Gravity Scale'i 3 yaparsa kod merdivenden çıkınca 3'ü geri verir;
kodda 8 yazsaydı Inspector'daki değer her merdivenden sonra ezilirdi ve sayı iki yerde tutulmuş olurdu.

**5. `animator.SetBool(IsClimbingHash, false)` return'den önce değil de sonra gelseydi ne olurdu?**
Satıra hiç ulaşılmazdı (derleyici "unreachable code" uyarısı verir). `isClimbing` merdivenden çıkınca da true
kalır ve Fatih yerde, havada, duvar dibinde tırmanma animasyonu oynardı.

**6. Main Camera, Cinemachine Brain ve Cinemachine Camera ayrı ayrı ne yapar?**
**Main Camera** görüntüyü çizen tek gerçek kameradır. **Cinemachine Brain** onun üzerindedir: hangi Cinemachine
Camera'nın canlı olduğuna karar verir, Main Camera'yı ona göre konumlar ve çekimler arasında yumuşak geçiş (blend)
yapar. **Cinemachine Camera** hiçbir şey çizmez; bir çekimi tarif eder: lens (boyut), neyi izleyeceği, nasıl izleyeceği.

**7. Polygon Collider 2D Fatih'i neden itti? Hiçbir şeyle çarpışmayan bir layer neden Is Trigger'dan daha iyi çözüm?**
Fizik motoru için Polygon Collider 2D katı bir nesnedir. İkisi de Default layer'ındaydı ve Default, Default ile
çarpışır; Fatih şeklin içinde kaldığı için motor onu en kısa yoldan dışarı itti. Trigger da fiziğe katılmaya devam
eder: Fatih oyun boyunca onun içinde olur, `OnTriggerEnter2D` gibi mesajlar onu bildirir, geniş bir layer maskesiyle
yapılan temas kontrolü onu sayar. Ayrı layer'da ve matriste her kutu boşken hiçbir collider ona değmez; Confiner yalnızca
şeklini okur.

**8. Wait, Min ve özel blend neyi kontrol eder? Her biri için bir oyun durumu söyle.**
- **Wait:** kameraya geçmeden önce kaç saniye beklenecek. İki koşu arasındaki kısa duraklamada kamera boşuna yaklaşmaz.
- **Min:** seçilen kamera en az kaç saniye canlı kalacak. Oyuncu tuşlara hızlı hızlı basınca kamera ileri geri titremez.
- **Özel blend:** belirli bir kamera çifti arasındaki geçişin süresi ve biçimi. Dinlenirken yavaş ve sakin yaklaşma,
  koşmaya başlayınca hızlı uzaklaşma (yoksa oyuncu kadrajdan çıkar); ışınlanmada Cut.

## Hocaya gösterme listesi

| Ne istenebilir | Nasıl gösterilir |
| -------------- | ---------------- |
| **Merdiven tilemap'i** | Hierarchy → Tilemaps → **Climbing**'i seç: üstte **Layer = Climbing**; Tilemap Renderer → **Sorting Layer = Climbing**. "Layer kod için, sorting layer çizim için; adları aynı ama birbirinden bağımsız." |
| **Merdiven karosu ve palet** | **Window → 2D → Tile Palette** → MainPalette: merdiven karosu sağda. Project'te `Assets/Tiles/SPA_Ladder`. |
| **Fizik şekli** | Project'te `Assets/Sprites/SPA_Ladder` → **Open Sprite Editor** → sol üst menü **Custom Physics Shape**: 4 noktalı dikdörtgen, raylardan biraz içeride, tepeden tabana. "Pikselden üretilen şeklin basamak aralarında delikleri vardı; Fatih merdivenin her yerinde merdivende sayılsın diye kendim çizdim." Kapatırken değişiklik yapma. |
| **Merdivenin collider'ları** | Climbing seçiliyken: Tilemap Collider 2D (**Composite Operation: Merge**), Composite Collider 2D (**Is Trigger** işaretli, **Geometry Type: Polygons**), Rigidbody 2D (**Static**). Scene'de her merdivende tek yeşil dikdörtgen. |
| **Neden Is Trigger / Static / Polygons** | Trigger: içinden geçilir ama temas sayılır. Static: Dynamic olsa yerçekimiyle düşerdi. Polygons: kapsül merdivenin tam ortasındayken de sayılsın. |
| **Tırmanma kodu** | `PlayerMovement` → `ClimbLadder()`: merdivene değmiyorsa yerçekimini geri ver, `isClimbing` false, çık. Değiyorsa y hızı = `moveInput.y × climbSpeed`, yerçekimi 0, animasyon yalnızca dikey hareket varken. |
| **Yerçekimi kapanıyor** | Play'de Fatih'i seç, merdivende yarı yolda dur: Rigidbody 2D → **Gravity Scale 0**; merdivenden çık: **8**. |
| **Tırmanma animasyonu** | Animator penceresi açık, Play: W basılıyken Climbing, merdivende dururken Idling, yanlara giderken Running. |
| **Merdivenden zıplanmıyor** | "Bilerek: merdivenin dibinde OnJump hızı 25 yapar ama aynı karede ClimbLadder onu `moveInput.y × 5` ile değiştirir. Merdivenden zıplanmaz, inilir." |
| **Cinemachine kurulu** | **Window → Package Management → Package Manager** → In Project: **Cinemachine 3.1**. Main Camera'yı seç: **Cinemachine Brain** bileşeni. "Main Camera'nın Transform'una artık Brain karar veriyor." |
| **Kamera ayarları** | Hierarchy en üstte **State-Driven Camera** → **Run Camera**: Lens **Orthographic Size 4.5** (9 karo yükseklik), **Tracking Target = Fatih (Player)**, Position Control **Position Composer**: Screen Position (0, 0), **Dead Zone** açık 0.2 × 0.2, Damping 1, 1, 1. Play'de Run Camera seçiliyken Game'de kılavuz çizgileri görünür. |
| **Dead Zone / Damping** | Play'de Run Camera'da Damping X'i 0 yap: kamera yatayda yapışır; Dead Zone'u kapat: her zıplamada dünya sallanır. Geri al. |
| **Kamera bölümün dışına çıkmıyor** | Play'de en sola koş: kamera duvarda durur, Fatih ortadan kayar. **Background** tilemap'i → **Polygon Collider 2D** (bölümün dört köşesi), her kamerada **Cinemachine Confiner 2D → Bounding Shape 2D = Background**. |
| **Neden Background layer'ı** | **Project Settings → Physics 2D → Layer Collision Matrix**: Background satırı ve sütunu boş. "Polygon fizik için katı bir nesne; Fatih içinde kaldığı için onu itiyordu. Hiçbir şeyle çarpışmayan bir layer'a koydum." Fatih'in **Layer = Player**. |
| **Duruma göre kamera** | State-Driven Camera'yı seç: **Animated Target = Fatih**; Instructions: Running → Run Camera, Idling → Idle Camera (**Wait 1**), Climbing → Ladder Camera. Play: durunca 1 sn sonra yaklaşır (3.5), koşunca uzaklaşır (4.5), merdivende en geniş (6). |
| **Solo** | Idle Camera → **Solo**: oyun dışında da Game görünümü o kameradan gösterir; başka bir şey seçince kapanır. |
| **Blend'ler** | State-Driven Camera → Default Blend **Ease In Out 2 sn**; Custom Blends **CameraBlends** (`Assets/Animations`): Run → Idle 3 sn, Idle → Run 1 sn, **\*\*ANY CAMERA\*\*** → Ladder 1 sn, Ladder → **\*\*ANY CAMERA\*\*** 3 sn. "Kurallar kamera adına göre; kamerayı yeniden adlandırırsam kural çalışmaz." |
| **Merdivende dururken kamera yaklaşıyor** | "Merdivende dururken isClimbing false olduğu için Animator Idling'e döner; State-Driven Camera merdiveni bilmez, yalnızca animasyon durumunu görür." |
| **Orthographic Size** | "Görünüşün yarı yüksekliği (dünya birimi). 4.5 → 9 karo; 16:9'da genişlik 16 karo." |
