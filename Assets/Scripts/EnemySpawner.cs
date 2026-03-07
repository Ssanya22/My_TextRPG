using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    public GameObject goblinPrefab;      // префаб обычного гоблина
    public GameObject orcPrefab;         // префаб орка
    public GameObject trollPrefab;       // префаб тролля

    [Header("Настройки времени")]
    public float spawnInterval = 10f;     // интервал между спавнами (сек)
    public int maxEnemies = 5;            // максимум врагов одновременно

    [Header("Локации")]
    public string currentLocation = "Таверна";  // текущая локация

    // Флаг для включения/выключения спавна
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

            // Проверяем, можно ли спавнить
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

        // ====== ВЫБОР ВРАГА В ЗАВИСИМОСТИ ОТ ЛОКАЦИИ ======
        switch (currentLocation)
        {
            case "Лес":
                // Проверяем, есть ли уже живой орк через метод UIManager
                bool orcExists = false;
                if (ui != null)
                {
                    orcExists = ui.IsOrcAlive();
                }

                if (orcExists)
                {
                    // Если орк уже есть — спавним только гоблинов
                    newEnemyPrefab = goblinPrefab;
                }
                else
                {
                    // Если орка нет — 10% на орка, 90% на гоблина
                    newEnemyPrefab = Random.value < 0.1f ? orcPrefab : goblinPrefab;
                }
                break;

            case "Горы":
                newEnemyPrefab = trollPrefab;
                break;

            case "Таверна":
                return;

            default:
                Debug.LogWarning($"Неизвестная локация: {currentLocation}");
                return;
        }

        // Проверка наличия префаба
        if (newEnemyPrefab == null)
        {
            Debug.LogError($"Префаб для локации {currentLocation} не назначен!");
            return;
        }

        // Создаём врага
        GameObject spawnedEnemy = Instantiate(newEnemyPrefab);
        Enemy enemy = spawnedEnemy.GetComponent<Enemy>();

        if (enemy != null)
        {
            // Даём врагу случайное имя
            enemy.enemyName = $"{enemy.enemyName}-{Random.Range(1, 100)}";
            
            // Увеличиваем счётчик
            currentEnemyCount++;

            // Добавляем в UIManager
            if (ui != null)
            {
                ui.AddEnemy(enemy);
                
                // Особое сообщение для орка-вождя
                if (enemy is Orc)
                    ui.AppendLog($"👹 В лесу появился ВОЖДЬ ОРКОВ: {enemy.enemyName}! Другие враги затаились...");
                else
                    ui.AppendLog($"👺 Появился новый враг: {enemy.enemyName}!");
            }
        }
    }

    // ====== ВЫЗЫВАЕТСЯ, КОГДА ВРАГ УМИРАЕТ ======
    public void OnEnemyDied()
    {
        currentEnemyCount--;
        if (currentEnemyCount < 0) currentEnemyCount = 0;
    }

    // ====== ПОЛНАЯ ОСТАНОВКА СПАВНА ======
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

    // ====== ВОЗОБНОВЛЕНИЕ СПАВНА ======
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