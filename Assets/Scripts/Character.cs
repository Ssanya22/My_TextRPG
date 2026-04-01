using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Character : MonoBehaviour
{
    [Header("Базовая информация")]
    public string characterName = "Герой";

    [Header("Характеристики (базовые)")]
    public int strength = 10;
    public int constitution = 10;
    public int dexterity = 10;
    public int intelligence = 10;
    public int wisdom = 10;
    public int charisma = 10;

    [Header("Навыки (растут от действий)")]
    public int combatSkill = 0;      // бой
    public int tradingSkill = 0;     // торговля
    public int stealthSkill = 0;     // скрытность
    public int magicSkill = 0;       // магия
    public int craftingSkill = 0;    // ремесло
    public int diplomacySkill = 0;   // дипломатия
    public int skillPoints = 0;      // очки навыков для улучшения

    [Header("Текущее состояние")]
    public int health;
    public int healthPotions = 3;

    [Header("Опыт и уровни")]
    public Experience experience;

    [Header("Гильдии")]
    public List<string> guilds = new List<string>(); // названия гильдий, где состоит

    [Header("Репутация")]
    public Dictionary<Faction, int> reputation = new Dictionary<Faction, int>();

    // Свойства
    public int maxHealth => 10 + (constitution - 10) / 2;
    public int StrengthModifier => (strength - 10) / 2;
    public int DexterityModifier => (dexterity - 10) / 2;
    public int IntelligenceModifier => (intelligence - 10) / 2;
    public int CharismaModifier => (charisma - 10) / 2;

    void Start()
    {
        health = maxHealth;
        Debug.Log($"👤 Персонаж создан! Имя: {characterName}");

        if (experience == null)
            experience = GetComponent<Experience>();

        // ====== ИНИЦИАЛИЗАЦИЯ РЕПУТАЦИИ ======
        foreach (Faction f in System.Enum.GetValues(typeof(Faction)))
        {
            reputation[f] = 0;
        }

        // Стартовая репутация с Велирами
        reputation[Faction.Veliry] = 0;
        Debug.Log($"📈 Начальная репутация с Велирами: 0");
    }

    // ====== МЕТОДЫ ДЛЯ РЕПУТАЦИИ ======

    public void AddReputation(Faction faction, int amount)
    {
        if (!reputation.ContainsKey(faction))
            reputation[faction] = 0;

        reputation[faction] += amount;
        reputation[faction] = Mathf.Clamp(reputation[faction], -100, 100);

        Debug.Log($"📈 Репутация с {faction}: {reputation[faction]} ({(amount > 0 ? "+" : "")}{amount})");
    }

    public int GetReputation(Faction faction)
    {
        return reputation.ContainsKey(faction) ? reputation[faction] : 0;
    }

    public string GetReputationString()
    {
        string result = "";
        foreach (var pair in reputation)
        {
            string icon = pair.Value > 0 ? "👍" : (pair.Value < 0 ? "👎" : "🤝");
            result += $"{icon} {pair.Key}: {pair.Value}\n";
        }
        return result;
    }

    // ====== МЕТОДЫ ДЛЯ НАВЫКОВ ======

    public void ImproveSkill(string skillName, int amount = 1)
    {
        switch (skillName.ToLower())
        {
            case "combat":
            case "бой":
                combatSkill += amount;
                Debug.Log($"⚔️ Навык боя увеличен! Теперь: {combatSkill}");
                break;

            case "trading":
            case "торговля":
                tradingSkill += amount;
                Debug.Log($"🪙 Навык торговли увеличен! Теперь: {tradingSkill}");
                break;

            case "stealth":
            case "скрытность":
                stealthSkill += amount;
                Debug.Log($"👤 Навык скрытности увеличен! Теперь: {stealthSkill}");
                break;

            case "magic":
            case "магия":
                magicSkill += amount;
                Debug.Log($"🔮 Навык магии увеличен! Теперь: {magicSkill}");
                break;

            case "crafting":
            case "ремесло":
                craftingSkill += amount;
                Debug.Log($"🔨 Навык ремесла увеличен! Теперь: {craftingSkill}");
                break;

            case "diplomacy":
            case "дипломатия":
                diplomacySkill += amount;
                Debug.Log($"🤝 Навык дипломатии увеличен! Теперь: {diplomacySkill}");
                break;
        }
    }

    public bool UpgradeSkill(string skillName)
    {
        if (skillPoints <= 0)
        {
            Debug.Log("❌ Нет очков навыков!");
            return false;
        }

        switch (skillName.ToLower())
        {
            case "бой":
            case "combat":
                combatSkill += 2;
                break;

            case "сила":
            case "strength":
                strength += 1;
                break;

            case "ловкость":
            case "dexterity":
                dexterity += 1;
                break;

            case "телосложение":
            case "constitution":
            case "выносливость":
                constitution += 1;
                health = maxHealth;
                break;

            case "торговля":
            case "trading":
                tradingSkill += 2;
                break;

            case "скрытность":
            case "stealth":
                stealthSkill += 2;
                break;

            default:
                Debug.Log($"❌ Неизвестный навык: {skillName}");
                return false;
        }

        skillPoints--;
        Debug.Log($"✨ Навык '{skillName}' улучшен! Осталось очков: {skillPoints}");
        return true;
    }

    // ====== ПОЛУЧЕНИЕ ТИТУЛА ======

    public string GetReputationTitle()
    {
        var skills = new Dictionary<string, int>
        {
            {"combat", combatSkill},
            {"trading", tradingSkill},
            {"stealth", stealthSkill},
            {"magic", magicSkill},
            {"crafting", craftingSkill},
            {"diplomacy", diplomacySkill}
        };

        var topSkill = skills.OrderByDescending(x => x.Value).First();

        if (topSkill.Value < 10)
            return "новичок";

        switch (topSkill.Key)
        {
            case "combat" when combatSkill > 50:
                return "легендарный воин";
            case "combat":
                return "воин";

            case "trading" when tradingSkill > 50:
                return "богатый купец";
            case "trading":
                return "торговец";

            case "stealth" when stealthSkill > 50:
                return "тень";
            case "stealth":
                return "вор";

            case "magic" when magicSkill > 50:
                return "архимаг";
            case "magic":
                return "маг";

            case "crafting" when craftingSkill > 50:
                return "мастер-ремесленник";
            case "crafting":
                return "ремесленник";

            case "diplomacy" when diplomacySkill > 50:
                return "посол";
            case "diplomacy":
                return "дипломат";

            default:
                return "авантюрист";
        }
    }

    // ====== БОЕВЫЕ МЕТОДЫ ======

    public int GetDodgeChance()
    {
        return 5 + Mathf.Max(0, (dexterity - 10) / 2);
    }

    public string Attack()
    {
        int d20 = Random.Range(1, 21);
        int combatBonus = combatSkill / 10;
        int strengthBonus = (strength - 10) / 2;
        int total = d20 + combatBonus + strengthBonus;

        bool isCrit = Random.Range(0, 100) < (combatSkill / 2);
        if (isCrit) total *= 2;

        string critText = isCrit ? "🔥 КРИТ! " : "";
        return $"{critText}⚔️ Атака: d20={d20} + навык={combatBonus} + сила={strengthBonus} = {total}";
    }

    public string TakeDamage(int amount)
    {
        if (Random.Range(0, 100) < GetDodgeChance())
        {
            return "💨 Ты увернулся от атаки!";
        }

        int reduction = combatSkill / 20;
        int reduced = Mathf.Max(1, amount - reduction);

        health -= reduced;
        if (health < 0) health = 0;

        return $"🛡️ Получено {reduced} урона (было {amount}, защита снизила на {reduction}). Осталось HP: {health}";
    }

    // ====== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ======

    public string GetStatsLine()
    {
        return $"HP: {health}/{maxHealth} | СИЛ: {strength} | ЛОВ: {dexterity} | Зелья: {healthPotions}";
    }

    public string GetSkillsLine()
    {
        return $"⚔️ {combatSkill} | 🪙 {tradingSkill} | 👤 {stealthSkill} | 🔮 {magicSkill} | 🔨 {craftingSkill} | 🤝 {diplomacySkill}";
    }

    public int GetPriceModifier(int basePrice)
    {
        float charismaBonus = 1.0f - ((charisma - 10) * 0.02f);
        return Mathf.RoundToInt(basePrice * charismaBonus);
    }

    public bool IsDetectedBy(Enemy enemy)
    {
        // Базовый шанс обнаружения 30%
        float baseChance = 0.3f;

        // Скрытность уменьшает шанс
        float stealthFactor = Mathf.Max(0, 1 - (stealthSkill / 100f));

        // Сила врага увеличивает шанс
        float enemyFactor = enemy.strength / 20f;

        float detectionChance = baseChance * enemyFactor * stealthFactor;

        // Учитываем количество врагов
        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null)
        {
            int enemyCount = ui.GetEnemyCount();
            detectionChance *= (1 + (enemyCount * 0.1f));
        }

        return UnityEngine.Random.value < detectionChance;
    }
}