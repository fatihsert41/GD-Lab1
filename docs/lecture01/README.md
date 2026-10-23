# Lecture 01 — FirstScene (Activity 1 + 2)

Unity 6.3 LTS (6000.3.25f1), Universal 2D. Proje klasörü: [`FirstScene/`](../../FirstScene/) (slayttaki proje adı).

## Slayttaki istekler

| Slayt | İstek | Projede |
| ----- | ----- | ------- |
| Create a Universal 2D project | Ad **FirstScene** | `FirstScene/`, Player Settings → Product Name `FirstScene` |
| Activity 1 | En az üç sprite şekli | Square, Circle, Triangle (`Assets/Sprites`); karakter slayttaki örnek canavarın düzeninde |
| Activity 1 | Renk, ölçek, dönüş | Turkuaz kafa, turuncu boynuzlar, mercan kollar/bacaklar; kollar ±42°, kaşlar ±17°, üst dişler 180° |
| Activity 1 | Hangi parça önde | Order in Layer 0 (boynuz, kol, parmak, bacak) → 1 (kafa, el, ayak) → 2 ağız → 3 ağız örtüsü, dil → 4 dişler, göz çerçevesi → 5 göz → 6 göz bebeği → 7 parıltı → 8 kaşlar |
| Save the Scene | Ad **FirstScene**, karakter kamerada | `Assets/Scenes/FirstScene.unity`, Scene List'te 0 |
| Your first C# file | `PrintToConsole.cs`, **Assets** klasöründe, dosya ve sınıf adı aynı | `Assets/PrintToConsole.cs` |
| Script must be attached | Kafa nesnesinin adı **Head**, script onda | `Character › Head` → PrintToConsole bileşeni |
| Activity 2 | Karakterin söyleyeceği kendi cümlen | `Grrr! I am Fatih's first character.` |

## Hiyerarşi

```
Main Camera            (arka plan slayttaki koyu lacivert)
Global Light 2D
Character
  LeftHorn, RightHorn, LeftArm, RightArm, LeftFinger1-3, RightFinger1-3,
  LeftLeg, RightLeg, Head (PrintToConsole), LeftHand, RightHand, LeftFoot, RightFoot,
  Mouth, MouthCover, Tongue, Tooth1-6,
  LeftEyeOutline, RightEyeOutline, LeftEye, RightEye, LeftPupil, RightPupil,
  LeftShine, RightShine, LeftBrow, RightBrow
```

Ağız yarım daire: koyu bir elipsin üst yarısını kafa renginde bir kare (MouthCover, daha yüksek Order in Layer) örter.

## Hocaya gösterme (Before you leave)

1. Unity Hub → **Add ▾ → Add project from disk** → `GD-Lab1\FirstScene`. İlk açılış birkaç dakika sürer (paketler içe aktarılır).
2. Project penceresinde **Assets › Scenes › FirstScene**'e çift tıkla → karakter Scene ve Game görünümünde.
3. **Play** → Console: `Grrr! I am Fatih's first character.`
