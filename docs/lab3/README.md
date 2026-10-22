# Lab 3 — Glimmer Grotto, part 2

Sprite animasyonu, Animator Controller ve 2D çarpışma. Proje: [`GlimmerGrotto/`](../../GlimmerGrotto/) (Lab 2'nin devamı).

Menü **Glimmer Grotto → 2. Lab 3'ü kur** (önce **1. Lab 2'yi kur** çalışmış olmalı).

## Neler yapıldı

| Part | Ne | Nerede |
| ---- | -- | ------ |
| 0 | Sprite paketi Lab 2'de kılavuzdaki Tablo 1 ayarlarıyla içe aktarıldı ve kesildi | `Assets/Sprites` |
| 1 | `Fatih (Player)`: Sprite Renderer (`SPA_Player_Bronze_5`, sorting layer **Player**, listenin en altında), Animator | Level1 |
| 1 | `Idling` klibi (kare 5–8, Loop Time), `Player` Animator Controller, varsayılan durum Idling | `Assets/Animations` |
| 2 | `Running` (19–28, hız 2) ve `Climbing` (14–15, hız 0,5) klipleri | `Assets/Animations` |
| 2 | Bool parametreler `isRunning`, `isClimbing`; 4 geçiş, Has Exit Time kapalı, Exit Time 0, Transition Duration 0 | Player.controller |
| 3 | Platforms: Tilemap Collider 2D (**Composite Operation: Merge**), Composite Collider 2D (Outlines), Rigidbody 2D **Static** | Level1 |
| 3 | Oyuncu: Capsule Collider 2D (0,5 × 0,875, gövdeye oturur), Rigidbody 2D Dynamic, Gravity 1, **Freeze Rotation Z** | Level1 |
| 3 | Prefab `Assets/Prefabs/Fatih (Player).prefab`; sahnedeki nesne onun örneği (mavi) | `Assets/Prefabs` |

Kılavuzdaki **player_purple** bu paketteki mor ceketli **SPA_Player_Bronze**'dur; kare numaraları aynıdır
(shoot 0–4, idle 5–8, roll 9–13, climb 14–15, jump 16–18, run 19–28).

Global Light 2D, yeni **Player** katmanını da aydınlatacak şekilde güncellenir (aksi halde URP 2D'de oyuncu siyah görünür).

### Player controller

```
Entry ──▶ Idling (varsayılan, hız 1)
            │  isRunning = true  ──▶ Running (hız 2)  ── isRunning = false ──▶ Idling
            │  isClimbing = true ──▶ Climbing (hız 0,5) ── isClimbing = false ──▶ Idling
```

## Deneme

1. **Play**: Fatih soldaki zeminin biraz üstünde başlar, düşer, yere basar, dik durur ve idle oynar.
2. **Window → Animation → Animator** → Parameters: **isRunning** kutusunu işaretle → yerinde koşar; kaldır → idle.
   **isClimbing** → tırmanır. İkisi birden işaretlenirse makine ilk bulduğu geçişe gider; hangisinin seçileceği
   sonraki lab'ın hareket kodunun işi.

## Tamamlama listesi

- [x] Sprite paketi Tablo 1 ayarlarıyla; ilk idle karesi 5
- [x] Fatih (Player): Sprite Renderer (Player katmanı), Animator (Player controller), Capsule Collider 2D, Rigidbody 2D (Freeze Rotation Z)
- [x] Idling, Running, Climbing döngüde; Running hız 2, Climbing hız 0,5
- [x] isRunning ve isClimbing durumları anında değiştirir; Idling varsayılan durum
- [x] Platforms'ta statik Rigidbody üzerinde birleştirilmiş (Merge) composite collider
- [x] Fatih düşer, yere basar, dik durur ve idle oynar; prefab bütün bu değişiklikleri içerir
- [x] Sahne kaydedildi

## Değerlendirme soruları

**1. Animator bileşeni ile Animator Controller varlığı arasındaki fark nedir? İkinci bir düşman türü için hangisini kopyalardın?**
Animator, GameObject'in üzerindeki bileşendir; animasyonları o nesnede oynatır ve hangi kurallara uyacağını
Controller yuvasından öğrenir. Animator Controller ise Project penceresindeki varlıktır: durumlar, geçişler ve
parametrelerden oluşan durum makinesi, yani kurallar. Farklı davranan ikinci bir düşman için **controller'ı**
kopyalarım (ya da Animator Override Controller kullanırım); Animator bileşeni her nesnede zaten ayrı bir kopyadır.

**2. Her durumu birbirine bağlamak yerine neden Idling'i merkez yaptık?**
Her durumun yalnızca bir girişi ve bir çıkışı olur: durum sayısı arttıkça geçiş sayısı doğrusal büyür (her yeni durum
için 2 ok), her şeyi her şeye bağlamak ise karesel büyür ve yönetilemez. Ayrıca mantık sadeleşir: koşmayı bırakan
oyuncu önce Idling'e döner, oradan tırmanmaya geçer. Running'den Climbing'e doğrudan bir yol gerekmez.

**3. Has Exit Time ne yapar, bir oyun onu ne zaman açık ister?**
Geçişin, koşul sağlansa bile mevcut klibin belirli bir oranı (Exit Time, ör. 0,75 = %75) oynanana kadar beklemesini
sağlar. Piksel sanatı platformer'ında tepki anında olmalı, bu yüzden kapalı. Açık olması, bir animasyonun yarıda
kesilmemesi gereken durumlarda istenir: saldırı ya da iniş animasyonunun bitip sonra idle'a dönmesi, bir kapı açılma
animasyonunun tamamlanması gibi.

**4. Composite Collider 2D neden Rigidbody 2D ister, zemin için neden Static doğru gövde türü?**
Composite collider, aynı Rigidbody 2D'ye bağlı collider'ları birleştirir; hangi şekillerin bir arada olduğunu
belirleyen şey o ortak gövdedir, bu yüzden Rigidbody 2D zorunludur (Unity ekleyince kendisi de ekler). Zemin hiç
hareket etmez; Static gövde yerçekiminden ve çarpışmalardan etkilenmez, fizik motoruna neredeyse hiç maliyet
çıkarmaz. Dynamic kalsaydı zemin yerçekimiyle düşerdi.

**5. Karo tabanlı zeminde kapsül collider neden kutu collider'dan daha iyi hissettirir?**
Bölüm 1 birimlik karolardan oluşur; oyuncu sürekli karo sınırlarının üzerinden geçer. Kutunun keskin köşesi en küçük
bir sınıra ya da basamağa takılır. Kapsülün altı yuvarlaktır; sınırlar ve küçük çıkıntılar takılınan bir engel değil,
üzerinden kayılan bir tümsek olur.

**6. Prefab override'larını uygulamayı unutsaydın 2. bölümde ne ters giderdi?**
Collider, Rigidbody ve constraint yalnızca 1. bölümdeki örneğe eklenmiş olurdu; prefab varlığı bunları bilmezdi.
2. bölüme prefab'dan konan yeni Fatih'in collider'ı ve gövdesi olmazdı: düşmez ya da zeminin içinden geçer.
Bu araçta prefab, bütün bileşenler eklendikten sonra kaydedildiği için override kalmaz.
