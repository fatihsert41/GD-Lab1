# Tasarım (Part 0)

Unity'ye dokunmadan önce oyunun **nasıl hissettireceğine** karar veriyoruz. Kılavuzdaki Tablo 1 örnek; aşağıdaki
“Benim versiyonum” sütununu sen doldur (Challenge 1). Emin değilsen Cipher Quest değerleriyle başla, sonra değiştir.

| Konu | Cipher Quest (kılavuz) | Benim versiyonum |
| ---- | ---------------------- | ---------------- |
| Oyuncu deneyimi | Gerilim (“kodumu çözecek mi?”), çözünce gülme | |
| Temel mekanik | Oyuncu kod tutar, bilgisayar tahmin eder, oyuncu HIGHER / LOWER / CORRECT der | (aynı) |
| Oyun döngüsü | Tahmin → geri bildirim → daha dar tahmin → … → çözüldü → tekrar oyna | (aynı) |
| **Tema** | Casus / hacker terminali | |
| **Ses tonu** | Biraz dramatik film hacker'ı: “Accessing your brain… is your code 742?” | |
| **Görseller** | Kapüşonlu, güneş gözlüklü Codebreaker; kazanınca kutlama | Codebreaker **Fatih** |
| **Stil ve renkler** | Koyu terminal arka planı, neon yeşil rakamlar, amber ve mavi düğmeler | |
| **Font** | Share Tech Mono veya VT323 (Google Fonts, SIL OFL) | |

## Renkler (kılavuzdaki değerler)

| Kullanım | HEX |
| -------- | --- |
| Kamera arka planı | `#101820` |
| Panel (Background) | `#1C2A38` |
| GuessBackground | `#0E2A1E` |
| GuessText (neon yeşil) | `#3DFF8C` |
| START düğmesi (Normal / Highlighted / Pressed) | `#F2A541` / `#D98E2B` / `#B8741C` |
| HIGHER / LOWER düğmeleri | `#2E86AB` |
| CORRECT düğmesi | `#3DDC84` |
| QUIT düğmesi | `#E4572E` |

## Oyun akışı

```
StartMenu (0) ──START──▶ CoreGame (1) ──CORRECT──▶ WinScreen (2)
     ▲                                                  │
     └──────────────────── PLAY AGAIN ──────────────────┘
```

> Not: Türkçe karakter (ç, ğ, ı, İ, ö, ş, ü) kullanacaksan fontun bunları içermeli ve font asset'i
> **Custom Characters** ile üretilmeli (Part 2.2, “Squares instead of letters”). İlk sürümde metinleri İngilizce
> tutmak en kolayı.
