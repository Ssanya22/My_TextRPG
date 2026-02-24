using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    public GameObject goblinPrefab;      // ← public чтобы видеть в инспекторе
    public float spawnInterval = 10f;     // ← public
    public int maxEnemies = 5;            // ← public

    private int currentEnemyCount = 0;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
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
            
            UIManager ui = FindObjectOfType<UIManager>();
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
}