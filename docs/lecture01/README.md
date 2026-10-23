# Lecture 01 — FirstScene (Activity 1 + 2)

Unity 6.3 LTS (6000.3.25f1), Universal 2D. Proje klasörü: [`FirstScene/`](../../FirstScene/) (slayttaki proje adı).

## Slayttaki istekler

| Slayt | İstek | Projede |
| ----- | ----- | ------- |
| Create a Universal 2D project | Ad **FirstScene** | `FirstScene/`, Player Settings → Product Name `FirstScene` |
| Activity 1 | En az üç sprite şekli | Square, Circle, Triangle (`Assets/Sprites`) |
| Activity 1 | Renk, ölçek, dönüş | Her parçanın rengi ve ölçeği farklı; kollar ±20°, göğüs üçgeni 180° |
| Activity 1 | Hangi parça önde | Order in Layer: 0 arkada (anten, boyun, kollar, bacaklar), 1 gövde/kafa/eller/ayaklar, 2 gözler/ağız/göğüs ışığı |
| Save the Scene | Ad **FirstScene**, karakter kamerada | `Assets/Scenes/FirstScene.unity`, Scene List'te 0 |
| Your first C# file | `PrintToConsole.cs`, **Assets** klasöründe, dosya ve sınıf adı aynı | `Assets/PrintToConsole.cs` |
| Script must be attached | Kafa nesnesinin adı **Head**, script onda | `Robot › Head` → PrintToConsole bileşeni |
| Activity 2 | Karakterin söyleyeceği kendi cümlen | `Beep boop! I am Fatih's robot.` |

## Hiyerarşi

```
Main Camera
Global Light 2D
Robot
  Antenna, AntennaTip, Head (PrintToConsole), LeftEye, RightEye, Mouth,
  Neck, Body, ChestLight, LeftArm, RightArm, LeftHand, RightHand,
  LeftLeg, RightLeg, LeftFoot, RightFoot
```

## Hocaya gösterme (Before you leave)

1. Unity Hub → **Add ▾ → Add project from disk** → `GD-Lab1\FirstScene`. İlk açılış birkaç dakika sürer (paketler içe aktarılır).
2. Project penceresinde **Assets › Scenes › FirstScene**'e çift tıkla → robot Scene ve Game görünümünde.
3. **Play** → Console: `Beep boop! I am Fatih's robot.`
