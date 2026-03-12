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
    public int skillPoints = 0;       // очки навыков для улучшения
    [Header("Текущее состояние")]
    public int health;
    public int healthPotions = 3;
    
    [Header("Опыт и уровни")]
    public Experience experience;
    
    [Header("Гильдии")]
    public List<string> guilds = new List<string>(); // названия гильдий, где состоит
    
    // Свойства для совместимости со старым кодом
    public int maxHealth => 10 + (constitution - 10) / 2;
    public int StrengthModifier => (strength - 10) / 2;
    
    void Start()
    {
        health = maxHealth;
        Debug.Log($"👤 Персонаж создан! Имя: {characterName}");
        
        if (experience == null)
            experience = GetComponent<Experience>();
    }
    
    // ====== МЕТОДЫ ДЛЯ НАВЫКОВ ======
    
    // Увеличить навык от действия
    public void ImproveSkill(string skillName, int amount = 1)
    {
        switch(skillName.ToLower())
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
    
    // Получить название на основе навыков (кто ты в глазах мира)
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
            
        switch(topSkill.Key)
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
    
    // Полная статистика для UI
    public string GetStatsLine()
    {
        return $"HP: {health}/{maxHealth} | СИЛ: {strength} | ЛОВ: {dexterity} | Зелья: {healthPotions}";
    }
    
    public string GetSkillsLine()
    {
        return $"⚔️ {combatSkill} | 🪙 {tradingSkill} | 👤 {stealthSkill} | 🔮 {magicSkill} | 🔨 {craftingSkill} | 🤝 {diplomacySkill}";
    }
    
    // ====== БОЕВЫЕ МЕТОДЫ (оставляем как есть) ======
    
    public string Attack()
    {
        int d20 = Random.Range(1, 21);
        int total = d20 + StrengthModifier + (combatSkill / 10); // бонус от навыка боя
        
        // Шанс крита зависит от навыка
        bool isCrit = Random.Range(0, 100) < (combatSkill / 2);
        int critMultiplier = isCrit ? 2 : 1;
        
        total *= critMultiplier;
        
        string critText = isCrit ? "🔥 КРИТ! " : "";
        return $"{critText}⚔️ Атака: d20={d20} + мод.силы={StrengthModifier} + навык={combatSkill/10} = {total}";
    }
    
    public string TakeDamage(int amount)
    {
        // Защита зависит от навыка боя (воин лучше защищается)
        int reduction = combatSkill / 20;
        int reduced = Mathf.Max(1, amount - reduction);
        
        health -= reduced;
        if (health < 0) health = 0;
        
        return $"🛡️ Получено {reduced} урона (было {amount}, защита снизила на {reduction}). Осталось HP: {health}";
    }
}