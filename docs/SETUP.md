# Kurulum: her şey D: diskine

C: diskinde yer az, bu yüzden Unity Hub, Unity Editor, indirmeler, önbellekler ve projeler **D:**'ye gider.
D:'de en az **20 GB** boş yer olsun (Unity Editor ~7 GB, indirme ~3 GB, proje + Library ~1–2 GB).

Önerilen klasör düzeni:

```
D:\Unity\Hub\         Unity Hub programı (kurulum yönetici olarak yapar)
D:\UnityCache\        Unity'nin yazdığı her şey (normal kullanıcı yazabilmeli)
├─ 6000.3.25f1\      Unity Editor (Hub'ın Installs location'ı D:\UnityCache)
├─ Packages\         paket önbelleği
├─ AssetStore\       Asset Store önbelleği
└─ GiCache\          ışık (GI) önbelleği
D:\Dev\
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
2. Kurulumda **Select Destination Location** → D: altında bir klasör (ör. `D:\Git`).
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
2. Konum: D: altında bir klasör. (Zaten C:'de kuruluysa bırak: ~400 MB, taşımaya değmez.)
3. VS Code'da Extensions (`Ctrl+Shift+X`) → **Unity** (Microsoft) eklentisini kur. Bu, **C# Dev Kit** ve **C#** eklentilerini de getirir.
4. C# Dev Kit, **.NET SDK** ister; eklenti eksikse kurmayı önerir. .NET SDK `C:\Program Files\dotnet`'e kurulur
   (~700 MB) ve konumu değiştirilemez; C:'de tek kalacak parça bu.

> Alternatif: **JetBrains Rider** öğrenciler / ticari olmayan kullanım için ücretsiz ve tamamen D:'ye kurulabilir,
> ama daha ağır. **Visual Studio Community** önerilmez: büyük kısmı C:'ye kurulur.

## 2a. D:'de çalışma klasörlerini aç (bir kez, yönetici onayıyla)

Bazı bilgisayarlarda D: kökünde normal kullanıcı klasör açamaz (“erişim reddedildi”). önbellek (`D:\UnityCache`) ve projeler (`D:\Dev`) için klasörleri yönetici olarak açıp yalnızca bunlarda kendi kullanıcına tam yetki ver.
Normal PowerShell'de (UAC penceresine **Evet**):

```powershell
$me = whoami
Start-Process powershell -Verb RunAs -Wait -ArgumentList "-NoProfile -Command New-Item -ItemType Directory -Force D:\UnityCache\Packages,D:\UnityCache\AssetStore,D:\UnityCache\GiCache,D:\UnityCache\Editors,D:\UnityCache\Downloads,D:\Dev\GameDev | Out-Null; icacls D:\UnityCache /grant '${me}:(OI)(CI)F' /T; icacls D:\Dev /grant '${me}:(OI)(CI)F' /T"
```

`icacls … (OI)(CI)F`: içindeki dosya ve alt klasörlere de geçen tam yetki. Kontrol (normal pencerede, hatasız bitmeli):

```powershell
New-Item D:\Dev\GameDev\test.txt -Force; Remove-Item D:\Dev\GameDev\test.txt
```

## 2b. Unity önbelleklerini Hub'dan önce D:'ye yönlendir

Unity, ilk projeyi oluştururken paketleri **global önbelleğe** indirir (varsayılan `C:\Users\<ad>\AppData\Local\Unity\cache`,
1–2 GB ve büyür). Preferences menüsüne ancak proje açıldıktan sonra girilebildiği için, bunu **ortam değişkeniyle** önceden
ayarlıyoruz. PowerShell'de:

```powershell
setx UPM_CACHE_ROOT "D:\UnityCache\Packages"
setx ASSETSTORE_CACHE_PATH "D:\UnityCache\AssetStore"
```

> **Neden `D:\Unity` içinde değil?** `D:\Unity`'yi Hub kurulumu yönetici olarak oluşturur; normal kullanıcı (ve Hub, Unity)
> içine yazamaz → “erişim reddedildi” / Hub'da “Download failed”. Editor, indirmeler ve önbellekler `D:\UnityCache`'te.

`setx` kalıcıdır ama yalnızca **yeni açılan** pencereleri etkiler: Hub açıksa kapatıp yeniden aç.
Kontrol (yeni PowerShell penceresinde): `$env:UPM_CACHE_ROOT` → `D:\UnityCache\Packages`.

## 3. Unity Hub

1. https://unity.com/download → Unity Hub for Windows.
2. Kurulumda **Destination Folder** → `D:\Unity\Hub`.
3. Hub'ı aç, Unity hesabınla giriş yap.
   **Lisans:** ⚙ → **Licenses** → **Add** → **Get a free personal license** (Personal, ücretsiz). Lisans olmadan Editor açılmaz.
4. **Ayarlar (sol alttaki dişli) → Installs**:
   - **Installs location** → `D:\UnityCache` (Editor `D:\UnityCache\6000.3.25f1`'e kurulur)
   - **Downloads location** → `D:\UnityCache` (kurulum dosyası ~4 GB, kurulumdan sonra silinebilir)

   Bunu **Editor'ü kurmadan önce** yap; sonradan değiştirmek kurulu sürümleri taşımaz.

## 4. Unity Editor 6000.3.25f1

1. Hub → **Installs → Install Editor**. Listede **6000.3.25f1** yoksa **Archive** sekmesinden ya da
   https://unity.com/releases/editor/archive üzerinden “Unity 6.3” → 6000.3.25f1 → **Install with Hub**.
2. Modül seçimi:
   - ☐ **Microsoft Visual Studio Community** → **işaretini kaldır** (varsayılan işaretli gelir; ~1,7 GB C:'ye kurulur, VS Code kullanıyoruz). Unutulursa kurulum sonunda açılan Visual Studio Installer'ı iptal et.
   - ☑ **Windows Build Support (Mono)** genelde editörle birlikte gelir; Part 10'daki .exe için yeterli.
   - ☐ Android/iOS/WebGL ve **Documentation** gerekmez (yer kazanmak için kapalı kalsın).
3. Kurulumun bittiğini **Installs** listesinde gör.

## 5. Unity önbelleklerini D:'ye al

Paket ve Asset Store önbellekleri 2b'de ortam değişkeniyle ayarlandı. Kalanlar, ilk proje açıkken **Edit → Preferences**:

- **GI Cache → Cache Folder Location** → `D:\UnityCache\GiCache`
- **Package Manager**: Packages cache location `D:\UnityCache\Packages` görünmeli (2b sayesinde)
- **External Tools → External Script Editor** → **Visual Studio Code**

## 6. Depoyu D:'ye klonla

```powershell
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
| Hub: “Download failed: Something went wrong” | Klasöre yazma izni yok → Installs/Downloads `D:\UnityCache\…` (adım 2a ve 3), Hub'ı tamamen kapatıp aç, ⟳ |
| Hub 6000.3.25f1'i göstermiyor | Archive sayfasından “Install with Hub” |
| Gereksiz modül kuruldu (Web, Documentation) | Hub → Installs → sürümün ⚙ menüsü → modülü kaldır |
| Proje yolu hatası / yavaş import | Yol OneDrive altında veya Türkçe karakterli olmasın (`D:\Dev\GameDev\…` uygun) |
| VS Code'da IntelliSense yok | Unity'de External Script Editor = VS Code, sonra **Assets → Open C# Project** |
