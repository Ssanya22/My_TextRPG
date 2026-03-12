using UnityEngine;

public class Experience : MonoBehaviour
{
    [Header("Текущие значения")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Настройки")]
    public float xpMultiplier = 1.2f;

    public void AddXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"✨ Получено {amount} опыта! Всего: {currentXP}/{xpToNextLevel}");
        
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpMultiplier);
        
        // Даём очко навыка персонажу
        Character character = GetComponent<Character>();  // ← ИЗМЕНЕНО (было Warrior)
        if (character != null)
        {
            character.ImproveSkill("combat", 1);  // можно давать выбор, пока просто бой
        }
        
        Debug.Log($"🎉 УРОВЕНЬ ПОВЫШЕН! Теперь {level} уровень! До следующего: {xpToNextLevel} опыта");
    }

    public float GetProgressPercent()
    {
        return (float)currentXP / xpToNextLevel;
    }

    public string GetStatsLine()
    {
        return $"Уровень: {level} | Опыт: {currentXP}/{xpToNextLevel}";
    }
}