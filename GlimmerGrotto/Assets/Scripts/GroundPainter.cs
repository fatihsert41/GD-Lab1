using UnityEngine;
using UnityEngine.Tilemaps;   // Tilemap ve TileBase bu ad alanında

// GroundPainter, bir tilemap'e koddan inişli çıkışlı zemin boyar.
// GrottoRuleTile ile boyar; hangi sprite'ın görüneceğine yine rule tile karar verir:
// çimen üstler, köşeler, yanlar ve dolgu kendiliğinden gelir.
//
// Kullanım: bileşeni Platforms tilemap'ine ekle, "What to paint" altındaki iki yuvayı
// doldur, bileşenin More menüsünü (başlığındaki üç nokta) aç ve "Paint Ground" seç.
// Seed'i değiştirip tekrar boyarsan yeni bir zemin çıkar. "Clear Ground" alanı boşaltır.
public class GroundPainter : MonoBehaviour
{
    [Header("What to paint")]
    [Tooltip("The tilemap that receives the tiles, usually Platforms.")]
    [SerializeField] Tilemap targetTilemap;
    [Tooltip("The tile to paint. Use the rule tile so that the edges look right.")]
    [SerializeField] TileBase groundTile;

    [Header("Where to paint (cell coordinates)")]
    [SerializeField] int firstColumn = -15;       // en soldaki sütunun x'i
    [SerializeField, Min(1)] int columns = 30;    // kaç sütun boyanacak
    [SerializeField] int bottomRow = -6;          // zemin bu satıra kadar dolu

    [Header("Shape of the surface")]
    [SerializeField] int startHeight = -3;        // ilk sütunun yüzey satırı
    [SerializeField] int lowestSurface = -4;      // yüzey bu satırın altına inmez
    [SerializeField] int highestSurface = 0;      // ...ve bunun üstüne çıkmaz
    // [Min(1)] Inspector'da 0 yazılmasını engeller: 0 sütunluk bir düzlük x'i hiç
    // ilerletmez, while döngüsü bitmez ve Editor donardı.
    [SerializeField, Min(1)] int minFlatRun = 2;  // en kısa düzlük (sütun)
    [SerializeField, Min(1)] int maxFlatRun = 5;  // en uzun düzlük (sütun)

    [Header("Pits")]
    [Range(0f, 1f)]
    [SerializeField] float pitChance = 0.25f;     // bir düzlükten sonra çukur olma olasılığı
    [SerializeField, Min(1)] int pitWidth = 2;    // çukur genişliği (sütun)

    [Header("Randomness")]
    [Tooltip("The same seed always gives the same ground.")]
    [SerializeField] int seed = 462;

    // Reset, bileşen eklenince (ya da menüden Reset seçilince) çalışır.
    // Bu GameObject'te bir Tilemap varsa hedef olarak hemen onu kullanırız.
    void Reset()
    {
        targetTilemap = GetComponent<Tilemap>();
    }

    [ContextMenu("Paint Ground")]
    void PaintGround()
    {
        if (!IsReady()) return;
        RecordUndo("Paint Ground");
        ClearArea();                                // boş bir şeritten başla

        Random.InitState(seed);                     // aynı seed, aynı sayılar, aynı zemin
        // Start Height izin verilen satırların dışındaysa başlangıcı da sınırla.
        int surface = Mathf.Clamp(startHeight, lowestSurface, highestSurface);
        int x = firstColumn;
        int lastColumn = firstColumn + columns - 1;

        while (x <= lastColumn)
        {
            // 1. Bir düzlük. Random.Range'in int sürümü ikinci değeri hiç döndürmez,
            //    bu yüzden 1 ekliyoruz (Cipher Quest'teki max + 1 dersi).
            int run = Random.Range(minFlatRun, maxFlatRun + 1);
            for (int i = 0; i < run && x <= lastColumn; i++)
            {
                FillColumn(x, surface);
                x++;
            }

            // 2. Bazen bir çukur: birkaç sütun atla, ama en sonda asla.
            if (Random.value < pitChance && x + pitWidth < lastColumn)
            {
                x += pitWidth;
            }

            // 3. Bir basamak çık, in ya da düz kal: Random.Range(-1, 2) -1, 0 veya 1 verir.
            surface += Random.Range(-1, 2);
            surface = Mathf.Clamp(surface, lowestSurface, highestSurface);
        }
    }

    [ContextMenu("Clear Ground")]
    void ClearGround()
    {
        if (!IsReady()) return;
        RecordUndo("Clear Ground");
        ClearArea();
    }

    // Bir sütunu en alt satırdan yüzey satırına kadar boyar.
    void FillColumn(int x, int surfaceRow)
    {
        for (int y = bottomRow; y <= surfaceRow; y++)
        {
            targetTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
        }
    }

    // Boyayıcının bütün alanını boşaltır. SetTile ile null bir hücreyi siler.
    void ClearArea()
    {
        for (int x = firstColumn; x < firstColumn + columns; x++)
        {
            for (int y = bottomRow; y <= highestSurface; y++)
            {
                targetTilemap.SetTile(new Vector3Int(x, y, 0), null);
            }
        }
    }

    bool IsReady()
    {
        if (targetTilemap != null && groundTile != null) return true;
        Debug.LogWarning("GroundPainter: fill in Target Tilemap and Ground Tile.", this);
        return false;
    }

    void RecordUndo(string actionName)
    {
#if UNITY_EDITOR
        // Ctrl+Z boyamayı geri alabilsin ve sahne değişmiş olarak işaretlensin.
        UnityEditor.Undo.RegisterCompleteObjectUndo(targetTilemap, actionName);
#endif
    }
}
