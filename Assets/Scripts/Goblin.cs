using UnityEngine;

// Это конкретный враг — Гоблин
public class Goblin : Enemy
{
    // Это выполняется при запуске игры
    void Start()
    {
        // Настраиваем именно этого гоблина
        enemyName = "Гоблин-разбойник";  // даём имя
        health = 12;                      // побольше жизней
        strength = 14;                    // посильнее
    }
}