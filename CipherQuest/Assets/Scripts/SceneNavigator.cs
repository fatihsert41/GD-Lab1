using UnityEngine;
using UnityEngine.SceneManagement;  // SceneManager sınıfı buradan gelir

// Oyuncuyu File > Build Profiles > Scene List'teki sahneler arasında taşır.
// Boş bir "SceneNavigator" nesnesine eklenir; metotları düğmelerin
// On Click () listesinden çağrılır.
public class SceneNavigator : MonoBehaviour
{
    // StartMenu her zaman listenin başında (index 0).
    // Çıplak bir 0 yerine adı olan sabit: ne olduğu okununca anlaşılır.
    const int StartSceneIndex = 0;

    // Şu anki sahneden bir sonrakini yükler.
    // PUBLIC olmalı: düğmenin On Click () listesi yalnızca public metotları gösterir.
    public void LoadNextScene()
    {
        // Şu an hangi sahnedeyiz, listede kaçıncı sırada?
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Güvenlik ağı: son sahnenin "sonraki"si yok, başa dön.
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = StartSceneIndex;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    // PLAY AGAIN: nerede olursak olalım ilk sahneye dön.
    public void LoadStartScene()
    {
        SceneManager.LoadScene(StartSceneIndex);
    }
}