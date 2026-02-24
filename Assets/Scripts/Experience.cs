using UnityEngine;

// Этот скрипт можно прикрепить к любому персонажу,
// у которого есть уровни и опыт
public class Experience : MonoBehaviour
{
    [Header("Текущие значения")]
    public int level = 1;           // текущий уровень
    public int currentXP = 0;       // сколько опыта уже есть
    public int xpToNextLevel = 100; // сколько нужно до следующего уровня

    [Header("Настройки")]
    public float xpMultiplier = 1.2f; // на сколько растёт потребность в опыте

    // Добавить опыт
    public void AddXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"✨ Получено {amount} опыта! Всего: {currentXP}/{xpToNextLevel}");
        
        // Проверяем, не повысился ли уровень
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    // Повышение уровня
    void LevelUp()
    {
        // Забираем потраченный опыт
        currentXP -= xpToNextLevel;
        level++;
        
        // Считаем, сколько нужно для следующего уровня
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpMultiplier);
        
        Debug.Log($"🎉 УРОВЕНЬ ПОВЫШЕН! Теперь {level} уровень!");
        Debug.Log($"🎯 До следующего уровня: {xpToNextLevel} опыта");
        
        // Здесь можно добавить вызов события, чтобы другие скрипты знали о повышении уровня
        // Например, чтобы воин увеличил характеристики
    }

    // Сколько процентов до следующего уровня (для красивого отображения)
    public float GetProgressPercent()
    {
        return (float)currentXP / xpToNextLevel;
    }

    // Краткая информация для UI
    public string GetStatsLine()
    {
        return $"Уровень: {level} | Опыт: {currentXP}/{xpToNextLevel}";
    }
}