using UnityEngine;

// Это конкретный враг — Гоблин
public class Goblin : Enemy
{
    // Это выполняется при запуске игры
    void Start()
    {
        // Настраиваем именно этого гоблина
        enemyName = "Гоблин-разбойник";  
        health = 12;                      
        strength = 14;
        xpReward = 50;                    
    }
}