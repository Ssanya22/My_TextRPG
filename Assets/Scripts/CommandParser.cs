using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class CommandParser
{
    // ====== СЛОВАРИ СИНОНИМОВ ======
    private static Dictionary<string, string[]> actionSynonyms = new Dictionary<string, string[]>
    {
        // Боевые действия
        ["attack"] = new[] {
            "атаковать", "бить", "ударить", "напасть", "вмазать", "побить", "замочить",
            "атака", "бей", "убей", "ударь", "в атаку", "в бой", "нападаю",
            "атакую", "бью", "лупи", "мочи", "уничтожить"
        },

        // Лечение
        ["heal"] = new[] {
            "лечиться", "выпить", "зелье", "исцелиться", "подлечиться", "хп",
            "полечиться", "здоровье", "хил", "хилиться", "восстановить", "лечусь"
        },

        // ====== КОМАНДЫ ДЛЯ NPC ======
        ["whoshere"] = new[] { "кто здесь", "кто тут", "кто рядом", "кто есть", "кто в локации" },
        ["talk"] = new[] { "поговорить", "говорить", "спросить", "побеседовать", "поговорить с", "спросить у" },

        // Статистика (без "кто")
        ["stats"] = new[] {
            "статы", "характеристики", "статистика", "инфо",
            "мой уровень", "мой опыт", "показать статы", "статус", "информация",
            "stats", "stat", "инфа", "мои параметры", "параметры", "характеристики персонажа",
            "покажи статистику", "открой статистику", "посмотреть статы", "свои параметры",
            "что умею", "какой я", "мои скиллы", "мои навыки", "прогресс"
        },

        // Навыки
        ["skills"] = new[] {
            "навыки", "скиллы", "мои навыки", "прокачка",
            "что я умею", "способности", "умения"
        },

        // Улучшение навыков
        ["upgrade"] = new[] {
            "улучшить", "прокачать", "повысить", "развить",
            "вложить очки", "потратить очки", "skill up"
        },

        // Работа с врагами
        ["enemies"] = new[] {
            "враги", "противники", "монстры", "список врагов", "кто рядом",
            "показать врагов", "какие враги", "что тут есть", "кого вижу"
        },

        ["enemy_info"] = new[] {
            "враг", "противник", "инфо о враге", "показать врага",
            "статус врага", "что за враг", "посмотреть врага"
        },

        // Репутация
        ["reputation"] = new[] { "репутация", "отношение", "слава", "уважение" },

        // Выбор цели
        ["select"] = new[] {
            "выбрать", "цель", "навестись на", "атакую", "бью", "целюсь в",
            "переключиться на", "нацелиться на"
        },

        // Воскрешение
        ["resurrect"] = new[] {
            "воскреснуть", "ожить", "возродиться", "начать заново",
            "продолжить", "рестарт", "воскрешение"
        },

        ["skill"] = new[] {
            "улучшить", "прокачать", "скилл", "skill", "повысить",
            "атаку", "защиту", "крит"
        },

        // ====== КОМАНДЫ НАВИГАЦИИ ======
        ["map"] = new[] { "карта", "карту", "показать карту", "мир", "карта мира" },
        ["location"] = new[] { "где я", "локация", "место", "где нахожусь", "текущее место" },
        ["travel"] = new[] {
            "пойти", "идти", "отправиться", "направиться", "двигаться",
            "пойти в", "идти в", "пойти к", "идти к"
        },
    };

    // Словарь для распознавания чисел
    private static Dictionary<string, int> numberWords = new Dictionary<string, int>
    {
        ["первого"] = 1,
        ["первый"] = 1,
        ["1"] = 1,
        ["один"] = 1,
        ["второго"] = 2,
        ["второй"] = 2,
        ["2"] = 2,
        ["два"] = 2,
        ["третьего"] = 3,
        ["третий"] = 3,
        ["3"] = 3,
        ["три"] = 3,
        ["четвертого"] = 4,
        ["четвертый"] = 4,
        ["4"] = 4,
        ["четыре"] = 4,
        ["пятого"] = 5,
        ["пятый"] = 5,
        ["5"] = 5,
        ["пять"] = 5,
        ["последнего"] = -1,
        ["последний"] = -1
    };

    // ====== МЕТОД ПАРСИНГА ======
    public static ParsedCommand Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new ParsedCommand { Action = null, Target = null, TargetIndex = -1 };

        string lower = input.ToLowerInvariant();
        var result = new ParsedCommand();
        result.RawInput = input;

        result.TargetIndex = FindTargetIndex(lower);
        result.Action = FindAction(lower);
        result.Target = FindTarget(lower, result.TargetIndex);
        result.IsCurrentTarget = IsCurrentTargetReference(lower);

        // ====== СПЕЦИАЛЬНАЯ ОБРАБОТКА ДЛЯ TRAVEL ======
        if (result.Action == "travel" && string.IsNullOrEmpty(result.Target))
        {
            string remaining = lower;
            foreach (var synonym in actionSynonyms["travel"])
            {
                if (remaining.Contains(synonym))
                {
                    int idx = remaining.IndexOf(synonym) + synonym.Length;
                    remaining = remaining.Substring(idx).Trim();
                    break;
                }
            }
            remaining = remaining.Replace("в ", "").Replace("во ", "").Replace("на ", "").Replace("к ", "").Trim();
            if (!string.IsNullOrEmpty(remaining))
            {
                result.Target = remaining;
            }
        }
        // ====== СПЕЦИАЛЬНАЯ ОБРАБОТКА ДЛЯ TRAVEL ======
        if (result.Action == "travel" && string.IsNullOrEmpty(result.Target))
        {
            // ... существующий код для travel ...
        }

        // ====== СПЕЦИАЛЬНАЯ ОБРАБОТКА ДЛЯ TALK ======
        if (result.Action == "talk" && string.IsNullOrEmpty(result.Target))
        {
            string remaining = lower;
            foreach (var synonym in actionSynonyms["talk"])
            {
                if (remaining.Contains(synonym))
                {
                    int idx = remaining.IndexOf(synonym) + synonym.Length;
                    remaining = remaining.Substring(idx).Trim();
                    break;
                }
            }
            remaining = remaining.Replace("с ", "").Replace("к ", "").Replace("у ", "").Trim();
            if (!string.IsNullOrEmpty(remaining))
            {
                result.Target = remaining;
            }
        }

        return result;
    }

    private static string FindAction(string text)
    {
        foreach (var pair in actionSynonyms)
        {
            foreach (var synonym in pair.Value)
            {
                if (text.Contains(synonym))
                    return pair.Key;
                if (IsSimilar(text, synonym))
                    return pair.Key;
            }
        }
        return null;
    }

    private static int FindTargetIndex(string text)
    {
        foreach (var pair in numberWords)
        {
            if (text.Contains(pair.Key))
                return pair.Value;
        }
        var match = Regex.Match(text, @"\b(\d+)\b");
        if (match.Success)
            return int.Parse(match.Value);
        return -1;
    }

    private static string FindTarget(string text, int index)
    {
        if (index > 0)
            return "enemy";

        if (text.Contains("этого") || text.Contains("этот") || text.Contains("его") ||
            text.Contains("него") || text.Contains("данного") || text.Contains("текущего") ||
            text.Contains("выбранного") || text.Contains("текущий"))
        {
            return "current";
        }

        if (text.Contains("сил") || text.Contains("strength"))
            return "strength";
        if (text.Contains("ловк") || text.Contains("dexterity"))
            return "dexterity";
        if (text.Contains("телослож") || text.Contains("constitution") || text.Contains("вынослив"))
            return "constitution";
        if (text.Contains("бой") || text.Contains("combat") || text.Contains("боев"))
            return "combat";
        if (text.Contains("торговл") || text.Contains("trading"))
            return "trading";
        if (text.Contains("скрытн") || text.Contains("stealth"))
            return "stealth";

        if (text.Contains("гоблин") || text.Contains("враг") || text.Contains("противник") || text.Contains("монстр"))
            return "enemy";

        if (text.Contains("себя") || text.Contains("собой") || text.Contains("сам") || text.Contains("свой"))
            return "self";

        return null;
    }

    private static bool IsCurrentTargetReference(string text)
    {
        return text.Contains("этого") || text.Contains("этот") || text.Contains("его") ||
               text.Contains("него") || text.Contains("данного") || text.Contains("текущего") ||
               text.Contains("выбранного") || text.Contains("текущий") || text.Contains("того") || text.Contains("того же");
    }

    private static bool IsSimilar(string text, string word)
    {
        if (word.Length < 4) return false;
        string baseWord = word.Substring(0, 4);
        return text.Contains(baseWord);
    }
}

// КЛАСС РЕЗУЛЬТАТА
public class ParsedCommand
{
    public string Action;
    public string Target;
    public int TargetIndex;
    public bool IsCurrentTarget;
    public string RawInput;

    public override string ToString()
    {
        return $"Action: {Action}, Target: {Target}, Index: {TargetIndex}, IsCurrent: {IsCurrentTarget}";
    }
}