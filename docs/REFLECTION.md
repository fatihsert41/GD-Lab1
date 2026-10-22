# Cipher Quest: Wrap-up

## Tamamlama listesi (kılavuz sf. 31–32)

- [x] Scene List: StartMenu (0), CoreGame (1), WinScreen (2)
- [x] UI anchor'lı; 16:9, 16:10, 4:3 ve 5:4'te yerinde kalıyor
- [x] Her düğme fareyle üstüne gelince ve basınca renk değiştiriyor (Color Tint)
- [x] START → CoreGame, CORRECT → WinScreen, PLAY AGAIN → StartMenu, QUIT oyunu kapatıyor
- [x] Tahmin ekranda; HIGHER / LOWER güncelliyor; tahmin tekrar etmiyor; 1 ve 1000'e ulaşılabiliyor
- [x] Deneme sayacı çalışıyor ve her turda sıfırlanıyor
- [x] Çelişkili cevaplar yakalanıyor, mesajla yeni tur başlıyor
- [x] Her GameObject anlamlı adlı; script, görsel ve sahneler kendi klasörlerinde
- [x] Codebreaker kendi adımla (Fatih)
- [x] Standalone build Unity olmadan çalışıyor

## Test planı (Tablo 7)

| Test | Nasıl | Beklenen |
| ---- | ----- | -------- |
| Dürüst tur | Bir kod tut, tam tur oyna | Attempt 1, 2, 3… ve hiçbir tahmin tekrar etmiyor |
| Uçlar | Highest Code = 10; hep HIGHER, sonra hep LOWER | Önce 10'a, sonra 1'e ulaşıyor |
| Çelişki | Aralık 1–10; tahmin 10'ken HIGHER (ya da 1'de LOWER) | Çelişki mesajı ve Attempt 1 ile yeni tur |
| Tek değer | Lowest = Highest = 7 | İlk tahmin 7; HIGHER mesajla yeni tur başlatır |
| Ters ayar | Lowest = 1000, Highest = 1 | Oyun normal şekilde 1–1000'de oynanıyor |
| Eksik referans | Attempt Text yuvasını boşalt, Play | Console'da NullReferenceException; çift tıklayınca satıra gidiyor |

Testlerden sonra aralığı 1–1000'e geri al.

## Değerlendirme soruları (Reflection questions)

**1. `LoadNextScene()` neden sabit sayılar yerine `currentSceneIndex + 1` kullanıyor? Sabit index ne zaman kabul edilebilir?**
Sahne listesine araya yeni bir sahne (ör. Tutorial) eklenince bütün numaralar kayar; sabit numara yazan düğme sessizce yanlış sahneyi açar. "Bir sonraki" demek listenin şekli ne olursa olsun doğru çalışır. Sabit index, yeri hiç değişmeyecek bir sahne için kabul edilebilir: StartMenu her zaman 0'da olduğu için `LoadStartScene()` 0'ı yükler, hem de adı olan bir sabitle (`StartSceneIndex`).

**2. Nokta anchor ile stretch anchor farkı nedir? GuessText için hangisini kullandın, neden?**
Nokta anchor'da dört tutamaç tek noktadadır: öğe o noktaya sabit uzaklıkta durur ve boyutu değişmez. Stretch anchor'da tutamaçlar ayrıdır: her kenar kendi tutamacına sabit uzaklıkta kalır, öğe parent'ı ile birlikte büyüyüp küçülür. GuessText için stretch-stretch (kenar boşlukları 0) kullandık; böylece yazı GuessBackground kutusunu her zaman tam doldurur ve kutu taşınınca ya da boyutlanınca onunla birlikte gider.

**3. `OnPressHigher()` neden public olmak zorunda, `NextGuess()` neden private kalabiliyor?**
Düğmenin On Click () listesi sınıfın dışından çağırır ve yalnızca public metotları gösterir. `NextGuess()`'i yalnızca Codebreaker kendi içinden çağırır; dışarıya açmaya gerek yok. Gerekenden fazlasını açmamak kapsüllemedir (encapsulation).

**4. `Random.Range`'in tam sayı sürümü bu oyunda neden `max + 1` istiyor? Olmasa ne olurdu?**
`Random.Range(int, int)` ilk değeri dahil eder, ikinciyi hariç tutar. `Random.Range(min, max)` hiçbir zaman `max`'ı döndürmez; hep HIGHER diyen oyuncu için Fatih 1000'e asla ulaşamazdı (pencere 1000–1000 olduğunda bile). `max + 1` yazınca `max` da seçilebilir olur.

**5. Konsol sürümü `max = max + 1` satırına ihtiyaç duyuyordu. Part 8'de neden silebildik?**
Konsol sürümünde HIGHER `min = guess` yapıyor ve tahmin ortadan `(max + min) / 2` ile alınıyordu. Tam sayı bölmesi aşağı yuvarladığı için 999–1000 aralığında tahmin hep 999 kalıyor, 1000'e ulaşılamıyordu; `max + 1` hilesi bunu düzeltiyordu. Part 8'de HIGHER `min = guess + 1`, LOWER `max = guess - 1` yapıyor ve `Random.Range(min, max + 1)` kullanıyoruz. Artık `min` ve `max` tam olarak hâlâ mümkün olan kodları anlatıyor, hileye gerek kalmadı.

**6. `min > max` oyuncunun cevapları hakkında ne söyler?**
Pencere boş: hiçbir kod oyuncunun verdiği tüm cevaplara aynı anda uymuyor. Yani oyuncu bir yerde yanlış düğmeye bastı (ya da Fatih'i deniyor). Oyun bunu yakalayıp "Your clues contradict each other!" mesajıyla yeni tur başlatıyor. Yanlış tıklama hemen yakalanmasa bile, sonraki dürüst cevaplar pencereyi gerçek koddan uzaklaştırarak daraltmaya devam ettiği için eninde sonunda pencere boşalır.

**7. `lowestCode` ve `highestCode`'u `min` ve `max`'tan ayrı tutmakla ne kazandık?**
Ayarlar (settings) ile durum (state) ayrıldı. `lowestCode` / `highestCode` tasarımcının Inspector'da girdiği değerlerdir ve kod onları hiç değiştirmez; `min` / `max` oyun sırasında daralan çalışma değerleridir. Her yeni tur ayarları state'e kopyalayarak başlar, yani sabit sayıları koda tekrar yazmaya gerek kalmaz ve Inspector'daki değerler ilk tıklamada kaybolmaz. `Mathf.Min/Max` ile ters girilmiş ayarlar da güvenle düzeltilir.
