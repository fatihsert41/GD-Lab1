# Kurulum: her şey D: diskine

C: diskinde yer az, bu yüzden Unity Hub, Unity Editor, indirmeler, önbellekler ve projeler **D:**'ye gider.
D:'de en az **20 GB** boş yer olsun (Unity Editor ~7 GB, indirme ~3 GB, proje + Library ~1–2 GB).

Önerilen klasör düzeni:

```
D:\Unity\
├─ Hub\              Unity Hub programı
├─ Editors\          Unity Editor sürümleri (6000.3.25f1 buraya)
├─ Downloads\        Hub'ın indirdiği kurulum dosyaları
└─ Cache\            Paket, GI ve Asset Store önbellekleri
D:\Dev\
├─ Git\              Git for Windows
├─ VSCode\           Visual Studio Code
└─ GameDev\
   └─ GD-Lab1\       bu depo (klon)
      ├─ docs\
      └─ CipherQuest\   Unity projesi (Unity Hub oluşturur)
```

> **Neden proje depo içinde bir alt klasör?** Unity Hub, dolu bir klasöre proje oluşturmaz. Önce depoyu klonlarız,
> sonra Hub'a “konum: `D:\Dev\GameDev\GD-Lab1`, ad: `CipherQuest`” deriz. Böylece belgeler (`docs/`) ile Unity
> projesi aynı depoda, karışmadan durur.

## 1. Git for Windows

1. https://git-scm.com/download/win → 64-bit installer.
2. Kurulumda **Select Destination Location** → `D:\Dev\Git`.
3. Varsayılan editör sorusunda **Visual Studio Code**'u seç (VS Code'u önce kurarsan listede çıkar; sonra da değiştirilebilir).
4. Kalan seçenekler varsayılan. Kurulumdan sonra PowerShell'de:

```powershell
git --version
git config --global user.name  "Fatih Sert"
git config --global user.email "fatihseert@gmail.com"
git config --global init.defaultBranch main
git config --global core.autocrlf true
```

## 2. Visual Studio Code (kod editörü)

1. https://code.visualstudio.com → **User Installer** (x64). User installer yönetici izni istemez ve konumu seçmene izin verir.
2. Konum: `D:\Dev\VSCode`.
3. VS Code'da Extensions (`Ctrl+Shift+X`) → **Unity** (Microsoft) eklentisini kur. Bu, **C# Dev Kit** ve **C#** eklentilerini de getirir.
4. C# Dev Kit, **.NET SDK** ister; eklenti eksikse kurmayı önerir. .NET SDK `C:\Program Files\dotnet`'e kurulur
   (~700 MB) ve konumu değiştirilemez; C:'de tek kalacak parça bu.

> Alternatif: **JetBrains Rider** öğrenciler / ticari olmayan kullanım için ücretsiz ve tamamen D:'ye kurulabilir,
> ama daha ağır. **Visual Studio Community** önerilmez: büyük kısmı C:'ye kurulur.

## 3. Unity Hub

1. https://unity.com/download → Unity Hub for Windows.
2. Kurulumda **Destination Folder** → `D:\Unity\Hub`.
3. Hub'ı aç, Unity hesabınla giriş yap, **Personal** lisansını etkinleştir (ücretsiz).
4. **Ayarlar (sol alttaki dişli) → Installs**:
   - **Installs location** → `D:\Unity\Editors`
   - **Downloads location** → `D:\Unity\Downloads`

   Bunu **Editor'ü kurmadan önce** yap; sonradan değiştirmek kurulu sürümleri taşımaz.

## 4. Unity Editor 6000.3.25f1

1. Hub → **Installs → Install Editor**. Listede **6000.3.25f1** yoksa **Archive** sekmesinden ya da
   https://unity.com/releases/editor/archive üzerinden “Unity 6.3” → 6000.3.25f1 → **Install with Hub**.
2. Modül seçimi:
   - ☐ **Microsoft Visual Studio Community** → **işaretini kaldır** (C:'ye kurulur, VS Code kullanıyoruz).
   - ☑ **Windows Build Support (Mono)** genelde editörle birlikte gelir; Part 10'daki .exe için yeterli.
   - ☐ Android/iOS/WebGL ve **Documentation** gerekmez (yer kazanmak için kapalı kalsın).
3. Kurulumun bittiğini **Installs** listesinde gör.

## 5. Unity önbelleklerini D:'ye al

Herhangi bir proje açıkken **Edit → Preferences** (ilk projeyi oluşturduktan sonra yap):

- **GI Cache → Cache Folder Location** → `D:\Unity\Cache\GiCache`
- **Package Manager → Packages → Cache location** → `D:\Unity\Cache\Packages`
- **Package Manager → My Assets (Asset Store) → Cache location** → `D:\Unity\Cache\AssetStore`
- **External Tools → External Script Editor** → **Visual Studio Code**

## 6. Depoyu D:'ye klonla

```powershell
mkdir D:\Dev\GameDev
cd D:\Dev\GameDev
git clone https://github.com/fatihsert41/GD-Lab1.git
cd GD-Lab1
```

GitHub şifre sorarsa: tarayıcıda açılan pencereden giriş yap (Git Credential Manager, Git ile gelir).

## 7. Unity projesini oluştur (Part 1.1)

1. Hub → **Projects → New project**.
2. Üstte editör sürümü **6000.3.25f1**.
3. Şablon: **Universal 2D**.
4. **Project name:** `CipherQuest` — **Location:** `D:\Dev\GameDev\GD-Lab1`.
5. Unity Cloud / Version Control seçenekleri **kapalı** (git kullanıyoruz). **Create project**.
6. Proje açılınca **Edit → Project Settings → Editor** içinde kontrol et (Unity 6'da varsayılan böyledir):
   - **Version Control → Mode:** Visible Meta Files
   - **Asset Serialization → Mode:** Force Text

Ardından PowerShell'de:

```powershell
cd D:\Dev\GameDev\GD-Lab1
git status
```

`Library/`, `Temp/`, `Logs/` listede **görünmemeli** (`.gitignore` sayesinde); `CipherQuest/Assets`,
`CipherQuest/Packages`, `CipherQuest/ProjectSettings` görünmeli.

## Kontrol

Ne kurulu, ne eksik görmek için depo klasöründe:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\check-setup.ps1
```

## Sık sorunlar

| Belirti | Çözüm |
| ------- | ----- |
| C: yine doluyor | `%LOCALAPPDATA%\Unity\cache` ve `%TEMP%` klasörlerini kontrol et; 5. adımı yaptın mı? |
| Hub 6000.3.25f1'i göstermiyor | Archive sayfasından “Install with Hub” |
| Proje yolu hatası / yavaş import | Yol OneDrive altında veya Türkçe karakterli olmasın (`D:\Dev\GameDev\…` uygun) |
| VS Code'da IntelliSense yok | Unity'de External Script Editor = VS Code, sonra **Assets → Open C# Project** |
