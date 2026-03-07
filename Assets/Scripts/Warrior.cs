using UnityEngine;

public class Warrior : MonoBehaviour
{
    [Header("Основные характеристики")]
    public int strength = 16;
    public int constitution = 14;
    public int dexterity = 12;
    public int health;

    [Header("Опыт и уровни")]
    public Experience experience;

    [Header("Инвентарь")]
    public int healthPotions = 3;

    [Header("Скиллы (прокачка)")]
    public int skillPoints = 0;           // очки навыков за уровень
    public int attackBonus = 0;           // дополнительный урон
    public int defenseBonus = 0;          // снижение получаемого урона
    public int critChance = 0;             // шанс крита (0–100)

    // ====== СВОЙСТВА ======
    public int maxHealth => 10 + (constitution - 10) / 2;
    public int StrengthModifier => (strength - 10) / 2;

    // ====== СТАНДАРТНЫЕ МЕТОДЫ ======
    void Start()
    {
        health = maxHealth;
        Debug.Log($"⚔️ Воин создан! СИЛ:{strength}, ТЕЛ:{constitution}, ЛОВ:{dexterity}, HP:{health}/{maxHealth}");

        if (experience == null)
            experience = GetComponent<Experience>();

        if (experience == null)
            Debug.LogWarning("У воина нет компонента Experience!");
    }

    // ====== БОЕВЫЕ МЕТОДЫ ======
    public string Attack()
    {
        int d20 = Random.Range(1, 21);

        // Проверка на критический удар
        bool isCrit = Random.Range(0, 100) < critChance;
        int critMultiplier = isCrit ? 2 : 1;

        // Расчёт урона с учётом бонуса атаки
        int total = (d20 + StrengthModifier + attackBonus) * critMultiplier;

        string critText = isCrit ? "🔥 КРИТ! " : "";
        return $"{critText}⚔️ Атака: d20={d20} + мод.силы={StrengthModifier} + бонус={attackBonus} = {total}";
    }

    public string TakeDamage(int amount)
    {
        int reduced = Mathf.Max(1, amount - defenseBonus); // минимум 1 урон
        health -= reduced;
        if (health < 0) health = 0;

        return $"🛡️ Получено {reduced} урона (было {amount}, защита снизила на {defenseBonus}). Осталось HP: {health}";
    }

    // ====== ПРОКАЧКА СКИЛЛОВ ======
    public void LevelUpSkill()
    {
        skillPoints++;
        Debug.Log($"📈 Получено очко навыка! Всего: {skillPoints}");
    }

    public bool SpendSkillPoint(string skillName)
    {
        if (skillPoints <= 0)
        {
            Debug.Log("❌ Нет очков навыков!");
            return false;
        }

        switch (skillName.ToLower())
        {
            case "атака":
            case "attack":
                attackBonus += 2;
                Debug.Log($"⚔️ Бонус атаки увеличен! Теперь: {attackBonus}");
                break;

            case "защита":
            case "defense":
                defenseBonus += 1;
                Debug.Log($"🛡️ Бонус защиты увеличен! Теперь: {defenseBonus}");
                break;

            case "крит":
            case "crit":
                critChance += 5;
                if (critChance > 50) critChance = 50;
                Debug.Log($"🔥 Шанс крита увеличен! Теперь: {critChance}%");
                break;

            default:
                Debug.Log($"❌ Неизвестный навык: {skillName}");
                return false;
        }

        skillPoints--;
        return true;
    }

    // ====== ИНФОРМАЦИЯ ======
    public string GetStatsLine()
    {
        return $"HP: {health}/{maxHealth} | СИЛ: {strength} | ЛОВ: {dexterity} | Зелья: {healthPotions} | Очки навыков: {skillPoints}";
    }

    public string GetSkillInfo()
    {
        return $"⚔️ Атака +{attackBonus}  🛡️ Защита -{defenseBonus} урона  🔥 Крит {critChance}%";
    }
}