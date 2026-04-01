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

        // Статистика
        ["stats"] = new[] {
    "статы", "характеристики", "статистика", "инфо", "кто я", "что я могу",
    "мой уровень", "мой опыт", "показать статы", "статус", "информация",
    "stats", "stat", "инфа", "мои параметры", "параметры", "характеристики персонажа",
    "покажи статистику", "открой статистику", "посмотреть статы", "свои параметры",
    "что умею", "какой я", "мои скиллы", "мои навыки", "прогресс"
},
        // Навыки (отдельно от статистики)
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
            "враги", "противники", "монстры", "кто тут", "список врагов", "кто рядом",
            "показать врагов", "какие враги", "что тут есть", "кого вижу"
        },

        ["enemy_info"] = new[] {
            "враг", "противник", "инфо о враге", "кто это", "показать врага",
            "статус врага", "что за враг", "посмотреть врага"
        },

        // Перемещение

        ["location"] = new[] {
            "где я", "локация", "место", "где нахожусь", "текущее место",
            "где это", "что за место", "где я сейчас"
        },

        ["reputation"] = new[] { "репутация", "отношение", "слава", "уважение" },

        // Выбор цели
        ["select"] = new[] {
            "выбрать", "цель", "навестись на", "атакую", "бью", "целюсь в",
            "переключиться на", "нацелиться на"
        },
        // Воскреснуть после смерти
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

    // ====== НОВЫЙ УЛУЧШЕННЫЙ МЕТОД ПАРСИНГА ======
    public static ParsedCommand Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new ParsedCommand { Action = null, Target = null, TargetIndex = -1 };

        string lower = input.ToLowerInvariant();
        var result = new ParsedCommand();
        result.RawInput = input;

        // 1. Сначала ищем числовой индекс
        result.TargetIndex = FindTargetIndex(lower);

        // 2. Ищем действие
        result.Action = FindAction(lower);

        // 3. Ищем цель с учётом контекста
        result.Target = FindTarget(lower, result.TargetIndex);

        // 4. Определяем конкретику (этот, текущий, выбранный)
        result.IsCurrentTarget = IsCurrentTargetReference(lower);

        // ====== 5. СПЕЦИАЛЬНАЯ ОБРАБОТКА ДЛЯ TRAVEL ======
        if (result.Action == "travel" && string.IsNullOrEmpty(result.Target))
        {
            // Убираем само действие из строки
            string remaining = lower;

            // Пробуем найти и удалить синоним из travel
            foreach (var synonym in actionSynonyms["travel"])
            {
                if (remaining.Contains(synonym))
                {
                    int idx = remaining.IndexOf(synonym) + synonym.Length;
                    remaining = remaining.Substring(idx).Trim();
                    break;
                }
            }

            // Убираем предлоги в начале
            remaining = remaining.Replace("в ", "").Replace("во ", "").Replace("на ", "").Replace("к ", "").Trim();

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
                {
                    return pair.Key;
                }

                // Проверяем похожие слова (для окончаний)
                if (IsSimilar(text, synonym))
                {
                    return pair.Key;
                }
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

        // Ищем просто числа в тексте
        var match = Regex.Match(text, @"\b(\d+)\b");
        if (match.Success)
            return int.Parse(match.Value);

        return -1;
    }
    private static string FindTarget(string text, int index)
    {
        // Если есть числовой индекс, значит цель точно враг
        if (index > 0)
            return "enemy";

        // Проверяем указатели на текущего врага
        if (text.Contains("этого") || text.Contains("этот") ||
            text.Contains("его") || text.Contains("него") ||
            text.Contains("данного") || text.Contains("текущего") ||
            text.Contains("выбранного") || text.Contains("текущий"))
        {
            return "current";
        }

        // ====== НОВЫЕ ЦЕЛИ ДЛЯ УЛУЧШЕНИЙ ======
        if (text.Contains("сил") || text.Contains("strength"))
            return "strength";

        if (text.Contains("ловк") || text.Contains("dexterity"))
            return "dexterity";
        if (text.Contains("телослож") || text.Contains("constitution") || text.Contains("вынослив"))
            return "constitution";  // ← ДОБАВИЛИ

        if (text.Contains("бой") || text.Contains("combat") || text.Contains("боев"))
            return "combat";

        if (text.Contains("торговл") || text.Contains("trading"))
            return "trading";

        if (text.Contains("скрытн") || text.Contains("stealth"))
            return "stealth";

        // Общие слова для врага
        if (text.Contains("гоблин") || text.Contains("враг") ||
            text.Contains("противник") || text.Contains("монстр"))
        {
            return "enemy";
        }

        // Лечение себя
        if (text.Contains("себя") || text.Contains("собой") ||
            text.Contains("сам") || text.Contains("свой"))
        {
            return "self";
        }

        return null;
    }

    private static bool IsCurrentTargetReference(string text)
    {
        return text.Contains("этого") || text.Contains("этот") ||
               text.Contains("его") || text.Contains("него") ||
               text.Contains("данного") || text.Contains("текущего") ||
               text.Contains("выбранного") || text.Contains("текущий") ||
               text.Contains("того") || text.Contains("того же");
    }

    private static bool IsSimilar(string text, string word)
    {
        if (word.Length < 4) return false;

        // Берем основу слова (первые 4 буквы)
        string baseWord = word.Substring(0, 4);
        return text.Contains(baseWord);
    }
}

// УЛУЧШЕННЫЙ КЛАСС РЕЗУЛЬТАТА
public class ParsedCommand
{
    public string Action;           // attack, heal, stats, etc
    public string Target;           // enemy, self, current, null
    public int TargetIndex;         // 1, 2, 3, etc
    public bool IsCurrentTarget;    // ссылка на текущего врага ("этого")
    public string RawInput;          // исходный текст

    public override string ToString()
    {
        return $"Action: {Action}, Target: {Target}, Index: {TargetIndex}, IsCurrent: {IsCurrentTarget}";
    }
}