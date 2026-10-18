# Yol haritası: Lab 1 — Cipher Quest

CENG462 Game Development, Lab 1. Kaynak: `CENG462_LabGuide_CipherQuest.pdf` (39 sayfa).
Motor: **Unity 6.3 LTS (6000.3.25f1)**, şablon **Universal 2D**. Dil: C#.

## Oyun ne?

Oyuncu 1–1000 arası gizli bir kod tutar. **Codebreaker Fatih** (kılavuzdaki “Evren” yerine kendi adın) tahmin eder,
oyuncu **HIGHER / LOWER / CORRECT** düğmeleriyle cevap verir. Üç sahne:

```
StartMenu (0) ──START──▶ CoreGame (1) ──CORRECT──▶ WinScreen (2)
     ▲                                                  │
     └──────────────────── PLAY AGAIN ──────────────────┘
QUIT (StartMenu'de) oyunu kapatır.
```

## Çalışma düzeni

- Unity işi senin Windows bilgisayarında, **D:** diskinde yapılır (kurulum: [SETUP.md](SETUP.md)).
- Her adım: dal (`feat/…`, `docs/…`) → commit → push → PR → birleştir → `git switch main` + `git pull`.
- Her PR sonunda kılavuzdaki **Checkpoint** kutuları sağlanmış olmalı.
- Ben her adımda ne yapacağını ve **neden** öyle yapıldığını anlatırım; sen Unity'de uygularsın, push'larsın, ben PR'ı incelerim.
- Kodlar önce kendin yazmaya çalışarak (Challenge kutuları), sonra kılavuzdaki çözümle karşılaştırılarak yazılır.

## Adımlar

| PR | Kılavuz | Ne yapacağız | Süre |
| -- | ------- | ------------ | ---- |
| 0 | — | Depo kurulumu: yol haritası, kurulum rehberi, `.gitignore` | — |
| — | — | Bilgisayar kurulumu D:'de (Unity Hub, Unity 6.3, VS Code, Git) — PR yok | ~1 saat (indirme) |
| 1 | Part 1 | Unity projesi, Canvas + Canvas Scaler, anchor'lar, görseller | 30 dk |
| 2 | Part 0 + 2 | Tasarım kararları (`DESIGN.md`), TextMesh Pro, font asset, düğme renkleri, üç sahne | 25 dk |
| 3 | Part 3 + 4 | `SceneNavigator.cs`, Scene List, START → CORRECT → PLAY AGAIN döngüsü | 25 dk |
| 4 | Part 5 | CoreGame ekranı (wireframe, Background, GuessText, AttemptText…) | 20 dk |
| 5 | Part 6 + 7 | Konsol kodunu düğmelere çevirme (`Codebreaker.cs`), tahmini ekranda gösterme | 30 dk |
| 6 | Part 8 + 9 | `Random.Range`, tekrar etmeyen tahmin, deneme sayacı, çelişki yakalama | 30 dk |
| 7 | Part 10 | QUIT düğmesi, cila, Player Settings, Windows build (GitHub Release) | 20 dk |
| 8 | Wrap-up | Yansıma sorularının cevapları (`REFLECTION.md`), isteğe bağlı ekstra görevler | — |

### Durum

- [x] **PR 0** — Depo kurulumu ve yol haritası
- [x] **Kurulum** — D:'de Unity Hub + Unity 6000.3.25f1 (`D:\UnityCache`) + VS Code + Git, depo `D:\Dev\GameDev\GD-Lab1`
- [x] **PR 1 — Part 1:** `CipherQuest` projesi; `StartMenu` sahnesi; Canvas Scaler 1920×1080 Scale With Screen Size; StartButton middle-right, CodebreakerFatih bottom-left
- [ ] **PR 2 — Part 0 + 2:** `DESIGN.md` dolduruldu; StartText (TMP), font asset (`Assets/Fonts`), düğme Color Tint renkleri; `CoreGame` ve `WinScreen` sahneleri
- [ ] **PR 3 — Part 3–4:** Scene List 0/1/2; `SceneNavigator` (`LoadNextScene`, `LoadStartScene`); tam döngü hatasız
- [ ] **PR 4 — Part 5:** CoreGame'de Tablo 3'teki 8 öğe, doğru isim ve parent–child
- [ ] **PR 5 — Part 6–7:** `Codebreaker.cs` (settings/state ayrımı, `OnPressHigher/Lower`), `Gameplay` objesi, ekranda tahmin
- [ ] **PR 6 — Part 8–9:** Rastgele tahmin, uçlar (1 ve 1000) ulaşılabilir, deneme sayacı, çelişki mesajı, Tablo 7 testleri
- [ ] **PR 7 — Part 10:** QuitGame (`#if UNITY_EDITOR`), kamera arka planı, IntroText/WinText, build çalışıyor
- [ ] **PR 8 — Wrap-up:** Tamamlama listesi, yansıma soruları, (isteğe bağlı) ekstra görevler

## Bu labda öğrenilecek kavramlar

| Kavram | Nerede | Kısaca |
| ------ | ------ | ------ |
| Canvas, Canvas Scaler | Part 1 | UI'nın çizildiği yüzey; ekran boyutu değişince ölçeklenir |
| Anchor / pivot | Part 1 | Öğenin ekranın hangi köşesine “yapıştığı” |
| TextMesh Pro, font asset | Part 2 | Keskin metin; font dosyasından üretilen doku atlası |
| Affordance (Color Tint) | Part 2 | Düğmenin “tıklanabilirim” geri bildirimi |
| Scene, build index | Part 3 | Her ekran ayrı sahne; Scene List'teki sırası = index |
| `SceneManager`, `public` metot, On Click () | Part 3–4 | Koddan sahne yükleme; düğmeden metot çağırma |
| `[SerializeField]`, kapsülleme | Part 6 | Private alanı Inspector'da göstermek |
| Settings vs. state | Part 6 | Tasarımcı ayarı ile oyun sırasında değişen değeri ayırmak |
| `TextMeshProUGUI` referansı | Part 7 | Script'ten ekrandaki yazıyı değiştirmek |
| `Random.Range(int,int)` | Part 8 | Alt sınır dahil, üst sınır **hariç** |
| DRY | Part 8 | Aynı hesabı tek yerde tutmak |
| Savunmacı programlama | Part 9 | `min > max` çelişkisini yakalamak, `Mathf.Min/Max` |
| Koşullu derleme | Part 10 | `#if UNITY_EDITOR` |
| Standalone build | Part 10 | Unity olmadan çalışan .exe |
