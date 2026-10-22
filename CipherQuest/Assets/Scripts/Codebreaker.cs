using UnityEngine;
using TMPro;  // TextMesh Pro türleri, ör. TextMeshProUGUI

// ------------------------------------------------------------------
// Cipher Quest - Codebreaker
// Oyuncu gizli bir kod tutar. Codebreaker Fatih, kodu içermesi gereken
// bir arama penceresi [min, max] tutar ve bu pencerenin içinden tahmin eder.
// Her HIGHER / LOWER cevabı pencereyi daraltır.
// CoreGame sahnesindeki "Gameplay" nesnesine eklenir.
// ------------------------------------------------------------------
public class Codebreaker : MonoBehaviour
{
    // ---- Ayarlar: Inspector'da düzenlenir, kod tarafından asla değiştirilmez ----
    [Header("Code range")]
    [SerializeField] int lowestCode = 1;      // izin verilen en küçük kod
    [SerializeField] int highestCode = 1000;  // izin verilen en büyük kod

    // ---- Referanslar: yazı nesnelerini bu yuvalara sürükle ----
    [Header("UI references")]
    [SerializeField] TextMeshProUGUI guessText;
    [SerializeField] TextMeshProUGUI dialogueText;  // tahminin üstündeki satır
    [SerializeField] TextMeshProUGUI attemptText;   // "Attempt 3"

    // ---- Durum (state): oyun sırasında değişen çalışma değerleri ----
    int min;       // kod bundan küçük olamaz
    int max;       // kod bundan büyük olamaz
    int guess;     // Fatih'in şu anki tahmini
    int attempts;  // bu turda yapılan tahmin sayısı

    void Start()
    {
        StartNewRound();
    }

    // Ayarları state'e kopyalar ve ilk tahmini yapar.
    void StartNewRound()
    {
        // Mathf.Min / Mathf.Max: Inspector'da iki ayar ters yazılsa bile min <= max kalır.
        // Bu olmasaydı pencere baştan boş olur, StartNewRound ve NextGuess birbirini
        // sonsuza kadar çağırır ve oyun stack overflow ile çökerdi.
        min = Mathf.Min(lowestCode, highestCode);
        max = Mathf.Max(lowestCode, highestCode);
        attempts = 0;
        NextGuess();  // tahmin etme işi TEK bir yerde (DRY)
    }

    // HIGHER düğmesi: gizli kod tahminden büyük.
    public void OnPressHigher()
    {
        min = guess + 1;  // tahminin kendisi de elendi
        NextGuess();
    }

    // LOWER düğmesi: gizli kod tahminden küçük.
    public void OnPressLower()
    {
        max = guess - 1;  // tahminin kendisi de elendi
        NextGuess();
    }

    // Arama penceresinin içinden rastgele bir kod seçer ve ekranda gösterir.
    void NextGuess()
    {
        // Boş pencere (min > max): hiçbir kod tüm cevaplara uymuyor,
        // oyuncu bir yerde yanlış düğmeye bastı. Baştan başla.
        if (min > max)
        {
            StartNewRound();
            dialogueText.text = "Your clues contradict each other!\n" +
                                "Rebooting... Is your secret code...";
            return;  // gerisini atla: StartNewRound zaten tahmin yaptı
        }

        // Random.Range(int, int): ilk değer DAHİL, ikinci değer HARİÇ.
        // max + 1 yazınca max'ın kendisi de seçilebilir olur.
        guess = Random.Range(min, max + 1);
        attempts++;  // attempts = attempts + 1 ile aynı

        guessText.text = guess.ToString();  // int -> string, ekranda
        attemptText.text = "Attempt " + attempts;
        dialogueText.text = "Is your secret code...";
    }
}
