# Lab 4 — Glimmer Grotto, part 3

Oyuncu girdisi, koşma, yön çevirme ve zıplama. Proje: [`GlimmerGrotto/`](../../GlimmerGrotto/) (Lab 3'ün devamı).

Menü **Glimmer Grotto → 3. Lab 4'ü kur** (önce **1. Lab 2'yi kur** ve **2. Lab 3'ü kur** çalışmış olmalı).
Lab 3 aracını sonradan yeniden çalıştırırsan Gravity Scale 1'e döner; o zaman bunu da tekrar çalıştır.

## Neler yapıldı

| Part | Ne | Nerede |
| ---- | -- | ------ |
| 0 | Active Input Handling: **Input System Package (New)**; `InputSystem_Actions` proje genelindeki actions | Project Settings, `Assets/Settings` |
| 1 | `Fatih (Player)`: **Player Input** (InputSystem_Actions, Default Map **Player**, Behavior **Send Messages**) | prefab |
| 1–6 | `PlayerMovement.cs` (kılavuzun son hali), Player Input'un altında | `Assets/Scripts` |
| 2 | Run Speed **10**; `linearVelocity.y` fiziğe bırakılır | PlayerMovement |
| 3 | `FlipSprite`: Scale X = ±1, yalnızca hareket ederken | PlayerMovement |
| 4 | `isRunning` koddan (`StringToHash` ile), `IsMovingHorizontally()` | PlayerMovement |
| 5 | Jump Speed **25**, Rigidbody 2D Gravity Scale **8**, Collision Detection **Continuous** | prefab |
| 6 | **Ground** katmanı (User Layer 6), **Platforms** bu katmanda; Layer Collision Matrix tamamen işaretli (değişmedi) | TagManager, Level1 |

Kılavuzdaki gibi kod yorumlarında **Evren** yerine **Fatih** yazar. `OnMove` içindeki `Debug.Log` satırı
yorum olarak duruyor: başındaki `//` silinirse Part 1'deki gibi girdiyi Console'a yazar.

### Kodun akışı

```
Klavye / gamepad ──▶ InputSystem_Actions (Move, Jump) ──▶ Player Input (Send Messages)
     OnMove(value) → moveInput'a yaz (durum: her karede Update kullanır)
     OnJump(value) → yerde mi? (IsTouchingLayers(groundMask)) → evet: y hızı = jumpSpeed (olay: hemen)
Update ─▶ Run()        x hızı = moveInput.x × runSpeed, y hızı = fizikteki değer; isRunning'i ayarla
       └▶ FlipSprite() hareket ediyorsa Scale X = Sign(x hızı)
```

## Deneme

1. Unity'de projeyi aç, derlemenin bitmesini bekle, menüden **Glimmer Grotto → 3. Lab 4'ü kur**.
2. **Play**'e bas ve **Game görünümüne bir kez tıkla** (tuşlar ancak o zaman oyuna gider).
3. **A / D** veya **oklar**: Fatih koşar, yüzü koştuğu yöne döner, bıraktığında o yöne bakmaya devam eder ve idle oynar.
4. **W / S**: hiçbir şey olmaz. Kenardan koşup düşünce normal düşer ve iner.
5. **Space**: yaklaşık 4 karo zıplar (Play'de ~3,7), 2/3 saniyede iner; 3 karoluk basamağa çıkar.
   Havadayken Space'e tekrar basmak işe yaramaz.
6. Console boş olmalı (hata ya da uyarı yok).

## Tamamlama listesi

- [x] Fatih'te Player Input (InputSystem_Actions, Send Messages) ve PlayerMovement; prefab'a uygulandı
- [x] A/D, oklar veya gamepad çubuğu Run Speed ile koşturur; yerçekimiyle düşer ve iner
- [x] Koştuğu yöne bakar, durunca yönünü korur; scale her zaman (±1, 1, 1)
- [x] Hareket ederken Running, dururken Idling
- [x] Jump Speed 25, Gravity Scale 8 ile 3 karonun biraz üstüne zıplar; Collision Detection Continuous
- [x] Platforms Ground katmanında; Fatih yalnızca ona dokunurken zıplar
- [ ] Unity'de doğrulama: aracı çalıştır, Play ile dene, sahneyi ve prefab'ı commit et

## Yansıma soruları

**1. PlayerMovement neden Move değerini bir alanda saklıyor da Jump'ı OnJump içinde hemen uyguluyor?**
Move bir **durum**dur: oyuncu D'yi iki saniye basılı tutar, `OnMove` yalnızca değer değiştiğinde (başta bir kez)
gelir. Fatih'in iki saniye boyunca koşması için değeri saklayıp her karede `Update` içinde kullanmak gerekir.
Jump bir **olay**dır: bir basış, bir zıplama. Saklanacak ya da her karede yapılacak bir şey yok; geldiği anda uygulanır.

**2. Run'da yeni hızın y kısmı neden `body.linearVelocity.y` olmalı? `0f` ile tam olarak ne ters gitti?**
y hızı fiziğin işidir: yerçekimi her fizik adımında (0,02 s) onu yaklaşık −0,2 birim/sn azaltır, iniş sıfırlar, zıplama
artırır. `0f` yazınca Update her karede bu birikimi siler; yerçekimi hiç hız biriktiremez ve Fatih düşmek yerine
saniyede ~0,2 birim yavaşça süzülür (zıplama da bir sonraki karede silinirdi). Mevcut değeri korumak, hızın adım adım
büyümesine, yani gerçek düşmeye izin verir.

**3. `Mathf.Sign(0)` ne döndürür, FlipSprite'ın ilk sürümünde nasıl hataya yol açtı?**
**1** döndürür. Fatih durunca x hızı 0 olur, scale 1'e ayarlanır ve sola koşarken durduğunda hep sağa döner.
Çözüm: yalnızca hareket ederken (`IsMovingHorizontally()`) scale'i değiştirmek.

**4. `Mathf.Abs(x) > Mathf.Epsilon` x'in sıfıra yakın olup olmadığını mı, sıfır olmadığını mı test eder? Ne zaman başka bir şey gerekir?**
**Sıfır olmadığını.** Epsilon en küçük pozitif float'tır (~1,4 × 10⁻⁴⁵); 0,000001 bile ondan çok büyüktür. Bizim için
doğru, çünkü tuş basılı değilken hız tam 0'dır. Değer hesaplamadan ya da fizikten geliyorsa (sürtünmeyle yavaşlayan hız,
kayan nokta hataları) "neredeyse sıfır" gerekir: `Mathf.Abs(x) > 0.01f` gibi bir tolerans; iki float'ın eşitliği için
`Mathf.Approximately(a, b)`.

**5. Tasarımcı Fatih'in daha yükseğe zıplamasını ama havada aynı süre kalmasını istiyor. Hangi sayılar hangi yöne değişir?**
Formüller: yükselme süresi `t = v / (9,81 × s)`, yükseklik `h = v² / (2 × 9,81 × s)`. Süre aynı kalsın diye `v / s` sabit
kalmalı, yani **Jump Speed ve Gravity Scale aynı oranda artırılır**. İkisi k katına çıkınca t değişmez, h ise k katına
çıkar (v² k², s k kat). Örnek: 25 / 8 → 50 / 16: süre yine 0,32 s, yükseklik ~4'ten ~8 karoya.

**6. Sorting layer ile layer arasındaki fark nedir? Her birinin bir kullanımını ver.**
**Sorting layer** çizim sırasıdır: neyin neyin önünde görüneceği (Background arkada, Player önde). **Layer** bir grup
etiketidir; fizik (Layer Collision Matrix: hangi katmanlar çarpışır), kameralar (Culling Mask: hangi katmanlar çizilir)
ve sorgular (`IsTouchingLayers(groundMask)`: Fatih Ground'a dokunuyor mu?) onu kullanır.

**7. `Animator.StringToHash` ve `LayerMask.GetMask`'i her karede ya da her basışta değil de bir kez çağırmak neden daha iyi?**
İkisi de bir metni sayıya çevirmek için arama yapar ve sonuç oyun boyunca değişmez. Bir kez hesaplayıp saklamak her
karedeki (SetBool) ya da her basıştaki (OnJump) gereksiz işi kaldırır. Ayrıca ad kodda **tek bir yerde** yazılır;
yazım hatası olursa düzeltilecek tek satır vardır.

## Hocaya gösterme listesi

Lab sonunda "göster" denebilecek şeyler ve nasıl gösterileceği. Hepsini bir kez kendin dene.

| Ne istenebilir | Nasıl gösterilir |
| -------------- | ---------------- |
| **Input Actions asset** | Project'te `Assets/Settings/InputSystem_Actions` çift tık → **Player** map → **Move** (Value, Vector2; WASD + oklar + sol çubuk), **Jump** (Button; Space + gamepad güney tuşu). Hiçbir şey değiştirmeden kapat. |
| **Player Input bileşeni** | Fatih'i seç → Player Input: Actions = InputSystem_Actions, Default Map = Player, Behavior = **Send Messages**; altındaki listede **OnMove, OnJump**. Sarı uyarı: proje genelindeki asset tek ve paylaşımlı, Player + UI map'leri birlikte açık; bu labda zararı yok. |
| **Girdiyi Console'da yazdır** | `PlayerMovement.cs` → `OnMove` içindeki `// Debug.Log(moveInput);` satırının başındaki `//`'yi sil, kaydet, Play → Game'e tıkla → tuşlar. D = (1, 0), bırakınca (0, 0), W+D = (0.71, 0.71). Sonra `//`'yi geri koy. |
| **Koşma hızı** | Play'deyken Fatih → PlayerMovement → **Run Speed**'i değiştir: etkisi anında. Durunca değer geri gelir. |
| **Neden `linearVelocity.y`** | Kenardan koşup düşür: normal düşer. "0f yazsaydık yerçekimi her karede silinir, yavaş süzülürdü." |
| **Yön çevirme** | Play'de sola koş, dur: Transform → Scale **(−1, 1, 1)** kalır; sağa koş: (1, 1, 1). Hiç arada bir değer olmaz. |
| **Koşma animasyonu** | **Window → Animation → Animator**'ı Game'in yanına koy, Play: koşarken Running, dururken Idling yanar. |
| **Zıplama ayarı** | Fatih → Rigidbody 2D: **Gravity Scale 8**, **Collision Detection Continuous**; PlayerMovement: **Jump Speed 25**. Formül: h = 25² / (2 × 9,81 × 8) ≈ 4 karo, Play'de ~3,7 (fizik 0,02 s adımlarla ilerler). |
| **Global yerçekimi nerede** | **Edit → Project Settings → Physics 2D → General Settings → Gravity (0, −9,81)**. "Physics" sayfası 3D içindir. Biz yalnızca Fatih'in Gravity Scale'ini değiştirdik. |
| **Ground katmanı** | **Platforms**'u seç → Inspector'ın üstünde **Layer: Ground**. Tags and Layers'ta **User Layer 6 = Ground**. |
| **Layer Collision Matrix** | **Project Settings → Physics 2D → Layer Collision Matrix**: Ground'un satırı/sütunu var, her kutu işaretli. İşareti kaldırılan iki katman birbirinin içinden geçer. |
| **Culling Mask (isteğe bağlı)** | Main Camera → Rendering → **Culling Mask**: Nothing → yalnızca arka plan rengi; Everything → mağara geri gelir. |
| **Havada zıplayamama** | Play'de Space'e hızlı hızlı bas: bir kez zıplar, inmeden tekrar zıplamaz. Kod: `OnJump` başındaki guard clause `if (!bodyCollider.IsTouchingLayers(groundMask)) return;` |
| **Bilinen sınır** | Havada bir duvara değerken tekrar zıplayabilir (duvar da Ground) ve duvara yapışabilir (sürtünme). Çözüm: Extra challenge 1 (ayak collider'ı) ve 2 (Friction 0 malzeme). |
| **Active Input Handling** | **Project Settings → Player → Other Settings → Active Input Handling = Input System Package (New)**. Eski `Input.GetAxis` bu ayarda çalışmaz. |
| **Prefab'a uygulandı mı** | Sahnede Fatih'i seç → üstte **Overrides**: değişiklik yok (her şey prefab'da). |
| **Kodun parçaları** | `[SerializeField]` alanlar (runSpeed, jumpSpeed), `Start`'ta önbelleğe alınan referanslar (body, animator, bodyCollider, groundMask), `IsRunningHash`, `IsMovingHorizontally()` (tek test tek yerde), `+=` değil `=` ile zıplama (hep aynı yükseklik). |
