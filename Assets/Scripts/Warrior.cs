using UnityEngine;

public class Warrior : MonoBehaviour
{
    public int strength = 16;
    public int constitution = 14;
    public int dexterity = 12;
    public int health;
    [Header("Опыт и уровни")]
    public Experience experience; // компонент с опытом

    /// <summary>Модификатор силы: (сила - 10) / 2</summary>
    public int StrengthModifier => (strength - 10) / 2;
    /// <summary>Модификатор телосложения</summary>
    public int ConstitutionModifier => (constitution - 10) / 2;
    /// <summary>Модификатор ловкости</summary>
    public int DexterityModifier => (dexterity - 10) / 2;

    void Start()
    {
        int conModifier = (constitution - 10) / 2;
        health = 10 + conModifier;
        Debug.Log($"⚔ Воин создан! СИЛ:{strength}, ТЕЛ:{constitution}, ЛОВ:{dexterity}, HP:{health}");
        // Если опыт не назначен, создадим его
    if (experience == null)
        experience = GetComponent<Experience>();
    
    if (experience == null)
        Debug.LogWarning("У воина нет компонента Experience!");
    }

    /// <summary>Выполнить атаку. Возвращает строку с результатом броска.</summary>
    public string Attack()
    {
        int d20 = Random.Range(1, 21);
        int total = d20 + StrengthModifier;
        return $"🗡 Атака: d20={d20} + мод.силы={StrengthModifier} → итог {total}";
    }

    /// <summary>Получить урон. Возвращает строку с результатом.</summary>
    public string TakeDamage(int amount = -1)
    {
        if (amount < 0) amount = Random.Range(2, 9);
        health -= amount;
        if (health < 0) health = 0;
        return $"💥 Получено {amount} урона! Осталось HP: {health}";
    }

    /// <summary>Краткая сводка статов для UI</summary>
    public string GetStatsLine()
    {
        return $"HP: {health} | СИЛ: {strength} | ЛОВ: {dexterity}";
    }
}
    

