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
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (spawnEnabled && currentLocation != "Таверна" && currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
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