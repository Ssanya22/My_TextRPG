using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    public GameObject goblinPrefab;
    public GameObject orcPrefab;
    public GameObject trollPrefab;

    [Header("Настройки времени")]
    public float spawnInterval = 10f;
    public int maxEnemies = 5;

    [Header("Локации")]
    public string currentLocation = "Таверна";

    public bool spawnEnabled = true;

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

        // Здесь можно привязать врагов к конкретным локациям
        if (newLocation == "Гнилые Топи")
        {
            // Опасные мутанты, Келл-ар-Торн
        }
        else if (newLocation == "Орлиный Утёс")
        {
            // Бандиты, контрабандисты
        }
        else if (newLocation == "Соколиный Пик")
        {
            // Волки, разведчики Вальгрим
        }
        else if (newLocation == "Красный Бор")
        {
            // Обычные враги
        }
        else if (newLocation == "Тренировочная поляна")
        {
            // Гоблины для тренировок
            // Здесь враги спавнятся по умолчанию (в методе SpawnRoutine)
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            Debug.Log($"Проверка спавна: spawnEnabled={spawnEnabled}, location={currentLocation}, count={currentEnemyCount}, max={maxEnemies}");

            if (spawnEnabled && currentLocation != "Таверна" && currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        Debug.Log($"Попытка спавна. spawnEnabled={spawnEnabled}, currentLocation={currentLocation}, currentEnemyCount={currentEnemyCount}, maxEnemies={maxEnemies}");
        if (!spawnEnabled) return;

        GameObject newEnemyPrefab = null;
        UIManager ui = FindFirstObjectByType<UIManager>();

        switch (currentLocation)
        {
            case "Лес":
                bool orcExists = false;
                if (ui != null)
                {
                    orcExists = ui.IsOrcAlive();
                }

                if (orcExists)
                {
                    newEnemyPrefab = goblinPrefab;
                }
                else
                {
                    newEnemyPrefab = Random.value < 0.1f ? orcPrefab : goblinPrefab;
                }
                break;

            case "Горы":
                newEnemyPrefab = trollPrefab;
                break;

            case "Тренировочная поляна":  // ← ДОБАВЛЯЕМ
                newEnemyPrefab = goblinPrefab;
                break;

            default:
                return;
        }

        if (newEnemyPrefab == null)
        {
            Debug.LogError($"Префаб для локации {currentLocation} не назначен!");
            return;
        }

        GameObject spawnedEnemy = Instantiate(newEnemyPrefab);
        Enemy enemy = spawnedEnemy.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.enemyName = $"{enemy.enemyName}-{Random.Range(1, 100)}";
            currentEnemyCount++;

            if (ui != null)
            {
                ui.AddEnemy(enemy);

                if (enemy is Orc)
                    ui.AppendLog($"👹 В лесу появился ВОЖДЬ ОРКОВ: {enemy.enemyName}! Другие враги затаились...");
                else
                    ui.AppendLog($"👺 Появился новый враг: {enemy.enemyName}!");
            }
        }
    }

    public void OnEnemyDied()
    {
        currentEnemyCount--;
        if (currentEnemyCount < 0) currentEnemyCount = 0;
    }

    public void StopSpawning()
    {
        spawnEnabled = false;

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