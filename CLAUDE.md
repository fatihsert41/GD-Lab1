# GD-Lab1 — Claude Code notları

CENG462 Game Development dersi Lab 1: **Cipher Quest** (Unity 6.3 LTS 6000.3.25f1, Universal 2D, C#).
Geliştirici bilgisayar mühendisliği öğrencisi; amaç öğrenmek. İletişim Türkçe.

- Yol haritası: [docs/ROADMAP.md](docs/ROADMAP.md) — adım bitince kutusu işaretlenir.
- Kurulum: [docs/SETUP.md](docs/SETUP.md) — her şey **D:** diskinde (C:'de yer yok).
- Kaynak: hocanın `CENG462_LabGuide_CipherQuest.pdf` kılavuzu; isimler ve yapı ona uyar. “Evren” yerine **Fatih**.

## Çalışma düzeni

- Unity işini kullanıcı kendi Windows bilgisayarında yapar; Claude her adımı anlatır (**Ne? · Neden? · Nasıl?**), PR'ları inceler.
- Kod yazdırırken önce kılavuzdaki Challenge'ı kullanıcının denemesine fırsat ver, sonra çözümü açıkla.
- Her adım: dal → commit (Conventional Commits) → push → PR → kullanıcı birleştirir. Bir adım bitince dur, özet ver.
- Unity projesi `CipherQuest/` alt klasöründe. `Library/`, `Temp/`, `Logs/`, `UserSettings/`, build'ler commit edilmez.
- `.meta` dosyaları her zaman commit edilir (Unity referansları GUID ile tutar).
