using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [SerializeField] private TMP_Text mainLog;
    [SerializeField] private TMP_InputField inputFieldTMP;
    [SerializeField] private InputField inputFieldLegacy;
    [SerializeField] private TMP_Text statsText;

    [Header("Игровая логика")]
    [SerializeField] private Character character;  // ← ИЗМЕНЕНО (было Warrior)

    [Header("Боевая система")]
    [SerializeField] private List<Enemy> enemies = new List<Enemy>();
    private Enemy currentEnemy;

    [Header("Опции")]
    [Tooltip("Максимум строк в логе (0 = без ограничения)")]
    [SerializeField] private int maxLogLines = 500;

    [Header("Настройки игры")]
    public bool hardcoreMode = false; // true = перманентная смерть
    private bool modeSelected = false; // выбран ли режим
    private bool isDead = false; // жив ли игрок 
    private ScrollRect _scrollRect;
    private RectTransform _contentRect;
    private RectTransform _logRect;

    [SerializeField] private float logContentPadding = 16f;

    private void OnEnable()
    {
        if (inputFieldTMP != null)
        {
            inputFieldTMP.onSubmit.AddListener(OnTmpSubmit);
            inputFieldTMP.onEndEdit.AddListener(OnTmpEndEdit);
        }

        if (inputFieldLegacy != null)
        {
            inputFieldLegacy.onEndEdit.AddListener(OnLegacyEndEdit);
        }
    }

    private void OnDisable()
    {
        if (inputFieldTMP != null)
        {
            inputFieldTMP.onSubmit.RemoveListener(OnTmpSubmit);
            inputFieldTMP.onEndEdit.RemoveListener(OnTmpEndEdit);
        }

        if (inputFieldLegacy != null)
        {
            inputFieldLegacy.onEndEdit.RemoveListener(OnLegacyEndEdit);
        }
    }

    private void Start()
    {
        if (mainLog == null) Debug.LogWarning("UIManager: не назначен Main Log.");
        if (inputFieldTMP == null && inputFieldLegacy == null) Debug.LogWarning("UIManager: не назначен Input Field (TMP или Legacy).");
        if (statsText == null) Debug.LogWarning("UIManager: не назначен Stats Text.");
        if (character == null) Debug.LogWarning("UIManager: не назначен Character."); // ← ИЗМЕНЕНО

        _scrollRect = mainLog != null ? mainLog.GetComponentInParent<ScrollRect>() : null;
        _contentRect = _scrollRect != null ? _scrollRect.content : null;
        _logRect = mainLog != null ? mainLog.rectTransform : null;

        if (mainLog != null) mainLog.text = "";
        AppendLog("Добро пожаловать в текстовую RPG!");

        // Вместо неё теперь выбор режима
        AppendLog("═══════════════════════════════════");
        AppendLog("⚔️  ВЫБЕРИ РЕЖИМ ИГРЫ  ⚔️");
        AppendLog("1. Обычный — при смерти можно воскреснуть (потеря 25% опыта)");
        AppendLog("2. HARDCORE — одна жизнь ☠️");
        AppendLog("Напиши 'обычный' или 'хардкор' для выбора");
        AppendLog("═══════════════════════════════════");

        RefreshStats();

        if (inputFieldTMP != null) inputFieldTMP.ActivateInputField();
        if (inputFieldLegacy != null) inputFieldLegacy.ActivateInputField();

        modeSelected = false;
    }

    private void Update() { }

    private string GetInputText()
    {
        if (inputFieldTMP != null) return inputFieldTMP.text ?? "";
        if (inputFieldLegacy != null) return inputFieldLegacy.text ?? "";
        return "";
    }

    private void SetInputText(string value)
    {
        if (inputFieldTMP != null) inputFieldTMP.text = value;
        else if (inputFieldLegacy != null) inputFieldLegacy.text = value;
    }

    public void SubmitCommand()
    {
        SubmitCommand(GetInputText());
    }

    private void SubmitCommand(string raw)
    {
        string cmd = (raw ?? "").Trim();
        SetInputText("");

        if (string.IsNullOrEmpty(cmd)) return;

        ProcessCommand(cmd);
    }

    public void ProcessCommand(string cmd)
    {
        if (string.IsNullOrEmpty(cmd)) return;
        // ====== ВЫБОР РЕЖИМА ПРИ СТАРТЕ ======
        if (!modeSelected)
        {
            string lower = cmd.ToLowerInvariant();

            if (lower.Contains("обычный") || lower.Contains("normal") || lower == "1")
            {
                hardcoreMode = false;
                modeSelected = true;
                AppendLog("✨ Выбран ОБЫЧНЫЙ режим. Удачи в приключениях!");
                AppendLog("💡 При смерти ты сможешь воскреснуть в таверне.");
                RefreshStats();
                return;
            }
            else if (lower.Contains("хардкор") || lower.Contains("hardcore") || lower == "2")
            {
                hardcoreMode = true;
                modeSelected = true;
                AppendLog("☠️ ВЫБРАН HARDCORE РЕЖИМ! ☠️");
                AppendLog("💀 Одна смерть — конец игры. Будь осторожен!");
                RefreshStats();
                return;
            }
            else
            {
                AppendLog("❌ Сначала выбери режим: 'обычный' или 'хардкор'");
                return;
            }
        }
        if (isDead)
        {
            string lower = cmd.ToLowerInvariant();

            if (lower.Contains("воскреснуть") || lower.Contains("ожить"))
            {
                ResurrectCharacter();  // ← ИЗМЕНЕНО (было ResurrectWarrior)
            }
            else
            {
                AppendLog("⚰️ Ты мёртв. Напиши 'воскреснуть' чтобы продолжить.");
            }
            return;
        }
        // ИСПОЛЬЗУЕМ НОВЫЙ ПАРСЕР
        ParsedCommand parsed = CommandParser.Parse(cmd);
        parsed.RawInput = cmd;

        // Для отладки - покажем что распарсили
        Debug.Log($"Парсинг: {parsed}");

        // Если ничего не распознали
        if (parsed.Action == null)
        {
            AppendLog($"❓ Не совсем понял команду '{cmd}'. Попробуй сказать проще, например: 'атакую гоблина' или 'выпью зелье'.");
            return;
        }

        // ====== ОБРАБОТКА ДЕЙСТВИЙ ======
        switch (parsed.Action)
        {
            // ----- АТАКА -----
            case "attack":
                // Если явно указан номер
                if (parsed.TargetIndex > 0 && parsed.TargetIndex <= enemies.Count)
                {
                    currentEnemy = enemies[parsed.TargetIndex - 1];
                    AppendLog($"👺 Выбран враг: {currentEnemy.enemyName}");
                    PerformAttack();
                }
                // Если сказано "этого", "текущего" и есть выбранный враг
                else if (parsed.IsCurrentTarget && currentEnemy != null)
                {
                    PerformAttack();
                }
                // Если просто "атакую" и есть выбранный враг
                else if (currentEnemy != null && parsed.Target == "enemy")
                {
                    PerformAttack();
                }
                // Если есть выбранный враг (запасной вариант)
                else if (currentEnemy != null)
                {
                    PerformAttack();
                }
                // Если нет врага, но есть индекс
                else if (parsed.TargetIndex > 0)
                {
                    AppendLog($"👺 Враг с номером {parsed.TargetIndex} не найден. Напиши 'враги' для списка.");
                }
                // Если ничего не подошло
                else
                {
                    AppendLog("👺 Кого атаковать? Напиши 'враги' для списка или выбери номер.");
                }
                break;

            // ----- ЛЕЧЕНИЕ -----
            case "heal":
                PerformHeal();
                break;
            // ----- СТАТИСТИКА -----
            case "stats":
                ShowFullStats();  // теперь показывает подробно в лог
                break;
            // -----НАВЫКИ(без полной статистики)
            case "skills":
                ShowSkills();
                break;
            // ----- СПИСОК ВРАГОВ -----
            case "enemies":
                ShowEnemyList();
                break;

            // ----- ИНФО О ВРАГЕ -----
            case "enemy_info":
                if (currentEnemy != null)
                    AppendLog(currentEnemy.GetStats());
                else
                    AppendLog("Нет выбранного врага.");
                break;

            // ----- ЛОКАЦИИ -----
            case "forest":
                GoToLocation("Лес");
                break;

            case "tavern":
                GoToLocation("Таверна");
                break;

            case "location":
                ShowCurrentLocation();
                break;

            case "mountains":
                GoToLocation("Горы");
                break;

            // ----- ВЫБОР ЦЕЛИ -----
            case "select":
                if (parsed.TargetIndex > 0 && parsed.TargetIndex <= enemies.Count)
                {
                    currentEnemy = enemies[parsed.TargetIndex - 1];
                    AppendLog($"👺 Выбран враг: {currentEnemy.enemyName}");
                }
                else
                {
                    AppendLog("Укажи номер врага, например: 'выбрать второго'");
                }
                break;

            case "resurrect":
                if (character.health > 0)  // ← ИЗМЕНЕНО
                {
                    AppendLog("Ты ещё жив! Зачем воскресать?");
                }
                else
                {
                    ResurrectCharacter();  // ← ИЗМЕНЕНО
                }
                break;

            case "skill":
                HandleSkill(parsed);  // ← ИЗМЕНЕНО (используем HandleSkill)
                break;
            case "навыки":
                string skills = $"📊 ТВОИ НАВЫКИ:\n";
                skills += $"⚔️ Бой: {character.combatSkill}\n";
                skills += $"🪙 Торговля: {character.tradingSkill}\n";
                skills += $"👤 Скрытность: {character.stealthSkill}\n";
                skills += $"🔮 Магия: {character.magicSkill}\n";
                skills += $"🔨 Ремесло: {character.craftingSkill}\n";
                skills += $"🤝 Дипломатия: {character.diplomacySkill}";
                AppendLog(skills);
                break;
            default:
                AppendLog($"❓ Не знаю, что делать с командой '{cmd}'. Попробуй по-другому.");
                break;
        }

        RefreshStats();
        ScrollLogToBottom();

    }

    public void AppendLog(string message)
    {
        if (mainLog == null) return;

        bool wasAtBottom = IsNearBottom();

        // Конвертируем эмодзи перед добавлением!
        string convertedMessage = UnicodeConverter.ToUTF32(message);
        mainLog.text += convertedMessage + "\n";

        if (maxLogLines > 0)
        {
            string[] lines = mainLog.text.Split('\n');
            if (lines.Length > maxLogLines)
            {
                mainLog.text = string.Join("\n", lines, lines.Length - maxLogLines, maxLogLines);
            }
        }

        RebuildLogContentHeight();
        if (wasAtBottom) ScrollLogToBottom();
    }

    public void RefreshStats()
    {
        if (statsText == null) return;
        if (character == null)
        {
            statsText.text = "HP: -\n⚡ -\n🧪 -";
            return;
        }

        // Базовая информация на панели
        string hpLine = $"❤️ HP: {character.health}/{character.maxHealth}";

        // Мана (пока заглушка, потом привяжем к магии)
        string manaLine = character.magicSkill > 0 ? $"⚡ Мана: {character.magicSkill * 2}" : "";

        string potionLine = $"🧪 Зелья: {character.healthPotions}";

        string xpLine = "";
        if (character.experience != null)
        {
            xpLine = $"⭐ УР: {character.experience.level} | {character.experience.currentXP}/{character.experience.xpToNextLevel}";
        }

        // Собираем всё в одну строку, пропуская пустое
        statsText.text = $"{hpLine}\n{manaLine}\n{potionLine}\n{xpLine}";
    }

    private void ScrollLogToBottom()
    {
        if (_scrollRect == null) return;

        Canvas.ForceUpdateCanvases();
        _scrollRect.verticalNormalizedPosition = 0f;
    }

    private bool IsNearBottom()
    {
        if (_scrollRect == null) return true;
        return _scrollRect.verticalNormalizedPosition <= 0.001f;
    }

    private void RebuildLogContentHeight()
    {
        if (mainLog == null || _logRect == null) return;

        mainLog.ForceMeshUpdate();
        float preferred = mainLog.preferredHeight;

        _logRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferred);

        if (_contentRect != null)
        {
            _contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferred + logContentPadding);
        }
    }

    public void AddEnemy(Enemy enemy)
    {
        enemies.Add(enemy);
        if (currentEnemy == null)
        {
            currentEnemy = enemy;
            AppendLog($"👺 Автоматически выбран: {enemy.enemyName}");
        }
    }

    public void RemoveEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
        if (currentEnemy == enemy)
        {
            if (enemies.Count > 0)
                currentEnemy = enemies[0];
            else
                currentEnemy = null;
        }
    }

    public bool IsOrcAlive()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy is Orc && enemy.IsAlive())
                return true;
        }
        return false;
    }

    public void SetCharacter(Character c)  // ← ИЗМЕНЕНО (было SetWarrior)
    {
        character = c;
        RefreshStats();
    }

    private void PerformAttack()
    {
        // ====== 1. ПРОВЕРКА: есть ли враг и жив ли он ======
        if (currentEnemy == null)
        {
            AppendLog("👺 Нет выбранного врага. Напиши 'враги' для списка.");
            return;
        }

        if (!currentEnemy.IsAlive())
        {
            AppendLog($"👺 {currentEnemy.enemyName} уже мёртв. Выбери другого.");
            return;
        }

        // ====== 2. ПРОВЕРКА: жив ли персонаж ПЕРЕД атакой ======
        if (character.health <= 0)  // ← ИЗМЕНЕНО
        {
            // Если персонаж уже мёртв — обрабатываем смерть
            HandleDeath();
            return;
        }

        // ====== 3. ПЕРСОНАЖ АТАКУЕТ ======
        string attackResult = character.Attack();  // ← ИЗМЕНЕНО
        AppendLog(attackResult);

        // Рассчитываем урон
        int damage = Random.Range(1, 9) + character.StrengthModifier;  // ← ИЗМЕНЕНО
        string damageResult = currentEnemy.TakeDamage(damage);
        AppendLog(damageResult);

        // ====== 4. ПРОВЕРКА: умер ли враг ПОСЛЕ атаки ======
        if (!currentEnemy.IsAlive())
        {
            // Враг мёртв — победа, опыт, навык
            AppendLog($"✨ Вы победили {currentEnemy.enemyName}!");

            // Увеличиваем навык боя
            character.ImproveSkill("combat", 2);  // ← НОВОЕ

            if (character.experience != null)
            {
                int xpReward = 50;
                character.experience.AddXP(xpReward);
                AppendLog($"✨ Получено {xpReward} опыта!");
            }
        }
        else // ====== 5. ВРАГ ВЫЖИЛ И ОТВЕЧАЕТ ======
        {
            string enemyAttack = currentEnemy.Attack();
            AppendLog(enemyAttack);

            int enemyDamage = Random.Range(1, 7) + currentEnemy.StrengthModifier;
            string playerDamage = character.TakeDamage(enemyDamage);  // ← ИЗМЕНЕНО
            AppendLog(playerDamage);

            // ====== 6. ПРОВЕРКА: не умер ли персонаж ПОСЛЕ контратаки ======
            if (character.health <= 0)  // ← ИЗМЕНЕНО
            {
                // Если персонаж умер от контратаки — обрабатываем смерть
                HandleDeath();
            }
        }
    }

    private void ShowFullStats()
    {
        if (character == null)
        {
            AppendLog("❌ Нет персонажа!");
            return;
        }

        string stats = $"\n═══════ СТАТИСТИКА ═══════\n";
        stats += $"📜 Ты известен как: {character.GetReputationTitle()}\n";
        stats += $"\n❤️ Здоровье: {character.health}/{character.maxHealth}\n";

        // Мана если есть
        if (character.magicSkill > 0)
            stats += $"⚡ Мана: {character.magicSkill * 2}\n";

        stats += $"\n⚔️ БОЕВЫЕ ХАРАКТЕРИСТИКИ:\n";
        stats += $"   Сила: {character.strength} | Ловкость: {character.dexterity} | Телосложение: {character.constitution}\n";

        if (character.intelligence > 10 || character.wisdom > 10 || character.charisma > 10)
        {
            stats += $"\n🧠 ИНТЕЛЛЕКТУАЛЬНЫЕ:\n";
            stats += $"   Интеллект: {character.intelligence} | Мудрость: {character.wisdom} | Харизма: {character.charisma}\n";
        }

        stats += $"\n📊 НАВЫКИ (растут от действий):\n";
        stats += $"   ⚔️ Бой: {character.combatSkill}\n";
        if (character.tradingSkill > 0) stats += $"   🪙 Торговля: {character.tradingSkill}\n";
        if (character.stealthSkill > 0) stats += $"   👤 Скрытность: {character.stealthSkill}\n";
        if (character.magicSkill > 0) stats += $"   🔮 Магия: {character.magicSkill}\n";
        if (character.craftingSkill > 0) stats += $"   🔨 Ремесло: {character.craftingSkill}\n";
        if (character.diplomacySkill > 0) stats += $"   🤝 Дипломатия: {character.diplomacySkill}\n";

        if (character.experience != null)
        {
            stats += $"\n⭐ ПРОГРЕСС:\n";
            stats += $"   Уровень: {character.experience.level}\n";
            stats += $"   Опыт: {character.experience.currentXP}/{character.experience.xpToNextLevel}\n";
        }

        stats += $"\n🧪 Инвентарь: {character.healthPotions} зелий\n";
        stats += $"⚙️ Режим: {(hardcoreMode ? "HARDCORE ☠️" : "Обычный ✨")}\n";
        stats += $"═══════════════════════";

        AppendLog(stats);
    }

    private void ShowSkills()
{
    if (character == null)
    {
        AppendLog("❌ Нет персонажа!");
        return;
    }

    string skills = $"📊 ТВОИ НАВЫКИ:\n";
    skills += $"⚔️ Бой: {character.combatSkill}\n";
    skills += $"🪙 Торговля: {character.tradingSkill}\n";
    skills += $"👤 Скрытность: {character.stealthSkill}\n";
    skills += $"🔮 Магия: {character.magicSkill}\n";
    skills += $"🔨 Ремесло: {character.craftingSkill}\n";
    skills += $"🤝 Дипломатия: {character.diplomacySkill}\n";
    
    if (character.skillPoints > 0)
    {
        skills += $"\n⭐ Очков навыков: {character.skillPoints} (используй 'улучшить')";
    }
    
    AppendLog(skills);
}
    private void HandleDeath()
    {
        isDead = true;

        // 🛑 СНАЧАЛА останавливаем спавн
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }

        // 💀 ПОТОМ сообщения о смерти
        if (hardcoreMode)
        {
            AppendLog("☠️ GAME OVER — Ты умер в хардкорном режиме!");
            AppendLog("🔄 Перезапуск через 3 секунды...");
            StartCoroutine(RestartAfterDelay(3f));
        }
        else
        {
            AppendLog("💀 ТЫ УМЕР! Напиши 'воскреснуть' чтобы продолжить.");
            AppendLog("⚰️ Враги больше не появляются, пока ты мёртв.");
        }
    }

    private void HandleSkill(ParsedCommand parsed)
    {
        if (character == null)  // ← ИЗМЕНЕНО
        {
            AppendLog("❌ Нет персонажа!");
            return;
        }

        // Пока у персонажа нет системы очков навыков, просто показываем сообщение
        AppendLog("📚 Система навыков в разработке. Скоро ты сможешь улучшать характеристики!");

        // Здесь потом будет логика улучшения навыков
    }

    private void PerformHeal()
    {
        if (character.health <= 0)  // ← ИЗМЕНЕНО
        {
            AppendLog("💀 Ты мёртв и не можешь лечиться. Игра окончена?");
            return;
        }

        if (character.healthPotions <= 0)  // ← ИЗМЕНЕНО
        {
            AppendLog("🧪 У тебя нет зелий! Найди или купи.");
            return;
        }

        if (character.health >= character.maxHealth)  // ← ИЗМЕНЕНО
        {
            AppendLog("❤️ У тебя и так полное здоровье!");
            return;
        }

        character.healthPotions--;  // ← ИЗМЕНЕНО
        int healAmount = 10;
        character.health += healAmount;  // ← ИЗМЕНЕНО

        if (character.health > character.maxHealth)  // ← ИЗМЕНЕНО
            character.health = character.maxHealth;

        AppendLog($"🧪 Ты выпил зелье! Осталось зелий: {character.healthPotions}");
        AppendLog($"❤️ Восстановлено {healAmount} HP. Теперь HP: {character.health}/{character.maxHealth}");
    }

    private void ShowEnemyList()
    {
        if (enemies.Count == 0)
        {
            AppendLog("Нет врагов на сцене.");
            return;
        }

        AppendLog($"👺 Враги ({enemies.Count}):");
        for (int i = 0; i < enemies.Count; i++)
        {
            string status = enemies[i].IsAlive() ? "жив" : "мертв";
            AppendLog($"{i + 1}. {enemies[i].enemyName} (HP: {enemies[i].health}) - {status}");
        }
        AppendLog($"Текущий выбран: {(currentEnemy != null ? currentEnemy.enemyName : "никто")}");
    }

    private void GoToLocation(string location)
    {
        LocationManager locMgr = FindFirstObjectByType<LocationManager>();
        if (locMgr != null)
        {
            locMgr.GoToLocation(location);

            // Правильные сообщения для каждой локации
            switch (location)
            {
                case "Лес":
                    AppendLog("🌳 Ты отправляешься в лес...");
                    break;
                case "Горы":
                    AppendLog("🏔️ Ты отправляешься в горы...");
                    break;
                case "Таверна":
                    AppendLog("🏠 Ты возвращаешься в таверну...");
                    break;
                default:
                    AppendLog($"📍 Ты перемещаешься в {location}");
                    break;
            }
        }
        else
        {
            AppendLog("Ошибка: LocationManager не найден!");
        }
    }

    private void ShowCurrentLocation()
    {
        LocationManager locMgr = FindFirstObjectByType<LocationManager>();
        if (locMgr != null)
        {
            string loc = locMgr.GetCurrentLocation();
            string desc = locMgr.GetLocationDescription();
            AppendLog($"📍 Ты в: {loc} — {desc}");
        }
        else
        {
            AppendLog("Ошибка: LocationManager не найден!");
        }
    }

    private void ResurrectCharacter()  // ← ИЗМЕНЕНО (было ResurrectWarrior)
    {
        // 🧠 ЕСЛИ ХАРДКОР — ВОСКРЕШЕНИЯ НЕТ
        if (hardcoreMode)
        {
            AppendLog("☠️ GAME OVER — Ты умер в хардкорном режиме!");
            AppendLog("🔄 Перезапуск через 3 секунды...");
            StartCoroutine(RestartAfterDelay(3f));
            return;
        }

        // 🧠 ЕСЛИ УЖЕ ЖИВ
        if (character.health > 0)  // ← ИЗМЕНЕНО
        {
            AppendLog("Ты ещё жив! Зачем воскресать?");
            return;
        }

        // 🧠 ОБЫЧНЫЙ РЕЖИМ — ВОСКРЕШАЕМ
        isDead = false;

        // Возвращаем в таверну
        GoToLocation("Таверна");

        // Штраф: теряем 25% опыта
        if (character.experience != null)  // ← ИЗМЕНЕНО
        {
            int lostXP = character.experience.currentXP / 4;
            character.experience.currentXP -= lostXP;
            if (character.experience.currentXP < 0)
                character.experience.currentXP = 0;

            AppendLog($"💔 При воскрешении ты потерял {lostXP} опыта!");
        }

        // Восстанавливаем половину здоровья
        character.health = character.maxHealth / 2;  // ← ИЗМЕНЕНО

        // Включаем спавн обратно
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.RestartSpawning();
        }

        AppendLog($"✨ Ты очнулся в таверне с {character.health}/{character.maxHealth} HP.");  // ← ИЗМЕНЕНО
        AppendLog("💡 Будь осторожнее в следующий раз!");

        RefreshStats();
    }

    private void RestartGame()
    {
        isDead = false;

        // Сбрасываем персонажа
        character.health = character.maxHealth;  // ← ИЗМЕНЕНО
        character.healthPotions = 3;  // ← ИЗМЕНЕНО

        if (character.experience != null)
        {
            character.experience.level = 1;
            character.experience.currentXP = 0;
            character.experience.xpToNextLevel = 100;
        }

        // Очищаем врагов
        foreach (Enemy enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
        enemies.Clear();
        currentEnemy = null;

        // Возвращаем в таверну
        GoToLocation("Таверна");

        // Очищаем лог
        if (mainLog != null) mainLog.text = "";

        // Показываем приветствие и выбор режима
        AppendLog("Добро пожаловать в текстовую RPG!");
        AppendLog("═══════════════════════════════════");
        AppendLog("⚔️  ВЫБЕРИ РЕЖИМ ИГРЫ  ⚔️");
        AppendLog("1. Обычный — при смерти можно воскреснуть (потеря 25% опыта)");
        AppendLog("2. HARDCORE — одна жизнь ☠️");
        AppendLog("Напиши 'обычный' или 'хардкор' для выбора");
        AppendLog("═══════════════════════════════════");

        modeSelected = false;
        RefreshStats();
    }

    private System.Collections.IEnumerator RestartAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        RestartGame();
    }

    private void OnTmpSubmit(string text)
    {
        SubmitCommand(text);
        if (inputFieldTMP != null) inputFieldTMP.ActivateInputField();
    }

    private void OnTmpEndEdit(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        SubmitCommand(text);
        if (inputFieldTMP != null) inputFieldTMP.ActivateInputField();
    }

    private void OnLegacyEndEdit(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        SubmitCommand(text);
        if (inputFieldLegacy != null) inputFieldLegacy.ActivateInputField();
    }
}