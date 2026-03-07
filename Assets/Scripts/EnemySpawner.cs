using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    public GameObject goblinPrefab;      // ← public чтобы видеть в инспекторе
    public float spawnInterval = 10f;     // ← public
    public int maxEnemies = 5;            // ← public
    public bool spawnEnabled = true; // можно ли спавнить врагов

    [Header("Локации")]
    public string currentLocation = "Таверна";  // текущая локация

    private int currentEnemyCount = 0;
    private Coroutine spawnCoroutine;

    void Start()
    {
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void UpdateLocation(string newLocation)
    {
        currentLocation = newLocation;
        Debug.Log($"Спавнер обновил локацию: {currentLocation}");
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Добавляем проверку на spawnEnabled
            if (spawnEnabled && currentLocation == "Лес" && currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        // 🚨 НЕМЕДЛЕННАЯ ПРОВЕРКА — если спавн отключён, выходим
        if (!spawnEnabled) return;
        if (goblinPrefab == null)
        {
            Debug.LogError("Goblin Prefab не назначен в SpawnManager!");
            return;
        }

        GameObject newGoblin = Instantiate(goblinPrefab);
        Goblin goblin = newGoblin.GetComponent<Goblin>();

        if (goblin != null)
        {
            goblin.enemyName = $"Гоблин-{Random.Range(1, 100)}";
            currentEnemyCount++;

            UIManager ui = FindFirstObjectByType<UIManager>();
            if (ui != null)
            {
                ui.AddEnemy(goblin);
                ui.AppendLog($"👺 Появился новый враг: {goblin.enemyName}!");
            }
        }
    }

    public void OnEnemyDied()
    {
        currentEnemyCount--;
        if (currentEnemyCount < 0) currentEnemyCount = 0;
    }

    // ====== НОВЫЙ МЕТОД ======
    public void StopSpawning()
    {
        spawnEnabled = false;

        // Если корутина ещё работает — останавливаем её принудительно
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        Debug.Log("🛑 Спавн врагов полностью остановлен.");
    }
    public void RestartSpawning()
    {
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
        spawnEnabled = true;
        Debug.Log("▶️ Спавн врагов возобновлён.");
    }
}