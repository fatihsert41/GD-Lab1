# CENG462 Game Development — Lab'lar

| Lab | Proje | Belge |
| --- | ----- | ----- |
| 1 — Cipher Quest | [`CipherQuest/`](CipherQuest/) | bu sayfa, [docs/REFLECTION.md](docs/REFLECTION.md) |
| 2 — Glimmer Grotto, part 1 (tilemap, rule tile) | [`GlimmerGrotto/`](GlimmerGrotto/) | [docs/lab2/README.md](docs/lab2/README.md) |
| 3 — Glimmer Grotto, part 2 (animasyon, çarpışma) | [`GlimmerGrotto/`](GlimmerGrotto/) | [docs/lab3/README.md](docs/lab3/README.md) |

---

# Lab 1 — Cipher Quest

CENG462 Game Development (Fall 2026–2027), Lab 1. Unity 6.3 LTS (6000.3.25f1), Universal 2D.

Oyuncu 1–1000 arası gizli bir kod tutar; **Codebreaker Fatih** tahmin eder, oyuncu **HIGHER / LOWER / CORRECT** ile cevap verir.

```
StartMenu (0) ──START──▶ CoreGame (1) ──CORRECT──▶ WinScreen (2)
     ▲   QUIT → oyunu kapatır                           │
     └──────────────────── PLAY AGAIN ──────────────────┘
```

## Neler yapıldı

| Part | Konu | Nerede |
| ---- | ---- | ------ |
| 1 | Canvas Scaler (1920×1080, Scale With Screen Size), anchor'lar | Tüm sahneler |
| 2 | TextMesh Pro yazılar, Color Tint düğmeler, üç sahne | `Assets/Scenes/` |
| 3–4 | `SceneNavigator`: `LoadNextScene` (current + 1, son sahnede başa sarar), `LoadStartScene` | `Assets/Scripts/SceneNavigator.cs` |
| 5 | CoreGame arayüzü: Background, HeadingText, DialogueText, GuessBackground › GuessText, AttemptText, HIGHER/CORRECT/LOWER | CoreGame |
| 6–7 | Konsol kodu düğmelere: settings (`lowestCode`, `highestCode`) / state (`min`, `max`, `guess`), `OnPressHigher/Lower` | `Assets/Scripts/Codebreaker.cs` |
| 8 | `Random.Range(min, max + 1)`, tahmin elenir (`guess ± 1`), DRY | Codebreaker |
| 9 | Deneme sayacı, çelişki (`min > max`) yakalama, `[Header]`, `Mathf.Min/Max` | Codebreaker |
| 10 | `QuitGame` (`#if UNITY_EDITOR`), IntroText, WinText, kamera `#101820`, Player Settings, Windows build | StartMenu, WinScreen |

Kurulum aracı: `Assets/Editor/CipherQuestSetup.cs` (menü **Cipher Quest**). Part 5–10'daki sahne işlerini
(nesneler, konumlar, renkler, On Click bağlantıları, Inspector referansları, Scene List, Player Settings) koddan yapar
ve Windows build alır. Yalnızca Editor'de çalışır, oyuna girmez.

## Çalıştırma

1. Unity Hub → `CipherQuest` projesini aç (Unity 6000.3.25f1).
2. İlk kez: menü **Cipher Quest → 1. Sahneleri kur (Part 5-10)**.
3. **StartMenu** açıkken **Play**.
4. Build: **Cipher Quest → 2. Windows build al** → `Builds/CipherQuest/Cipher Quest.exe`.
   Paylaşırken klasörün tamamını zip'le (`.exe` tek başına çalışmaz).

## Belgeler

| Belge | İçerik |
| ----- | ------ |
| [docs/REFLECTION.md](docs/REFLECTION.md) | Tamamlama listesi, test planı, değerlendirme sorularının cevapları |
| [docs/ROADMAP.md](docs/ROADMAP.md) | Adımlar ve öğrenilen kavramlar |
| [docs/SETUP.md](docs/SETUP.md) | D: diskine kurulum |
| [docs/DESIGN.md](docs/DESIGN.md) | Part 0: tema, renkler |
