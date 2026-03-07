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
    [SerializeField] private Warrior warrior;

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
        if (warrior == null) Debug.LogWarning("UIManager: не назначен Warrior.");

        _scrollRect = mainLog != null ? mainLog.GetComponentInParent<ScrollRect>() : null;
        _contentRect = _scrollRect != null ? _scrollRect.content : null;
        _logRect = mainLog != null ? mainLog.rectTransform : null;

        if (mainLog != null) mainLog.text = "";
        AppendLog("Добро пожаловать в текстовую RPG!");

        // ЭТУ СТРОКУ УДАЛИЛИ:
        // AppendLog("Введите команду: attack, hit, stats, враг, бей, зелье, где я");

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
                ResurrectWarrior();
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
                AppendLog(warrior.GetStatsLine());
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
                if (warrior.health > 0)
                {
                    AppendLog("Ты ещё жив! Зачем воскресать?");
                }
                else
                {
                    ResurrectWarrior();
                }
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
        if (warrior == null)
        {
            statsText.text = "HP: -\nСИЛ: -\nЛОВ: -\nЗелья: -";
            return;
        }

        string xpInfo = "";
        if (warrior.experience != null)
        {
            xpInfo = $"\nУР: {warrior.experience.level}\nОПЫТ: {warrior.experience.currentXP}/{warrior.experience.xpToNextLevel}";
        }

        string modeText = "";
        if (modeSelected)
        {
            modeText = hardcoreMode ? "\n☠️ HARDCORE" : "\n✨ Обычный";
        }
        else
        {
            modeText = "\n⚔️ Выбери режим";
        }

        string stats = $"HP: {warrior.health}/{warrior.maxHealth}\nСИЛ: {warrior.strength}\nЛОВ: {warrior.dexterity}\nЗелья: {warrior.healthPotions}{xpInfo}{modeText}";

        statsText.text = UnicodeConverter.ToUTF32(stats);
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

    public void SetWarrior(Warrior w)
    {
        warrior = w;
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

        // ====== 2. ПРОВЕРКА: жив ли воин ПЕРЕД атакой ======
        if (warrior.health <= 0)
        {
            // Если воин уже мёртв — обрабатываем смерть
            HandleDeath();
            return;
        }

        // ====== 3. ВОИН АТАКУЕТ ======
        string attackResult = warrior.Attack();
        AppendLog(attackResult);

        // Рассчитываем урон воина
        int damage = Random.Range(1, 9) + warrior.StrengthModifier;
        string damageResult = currentEnemy.TakeDamage(damage);
        AppendLog(damageResult);

        // ====== 4. ПРОВЕРКА: умер ли враг ПОСЛЕ атаки ======
        if (!currentEnemy.IsAlive())
        {
            // Враг мёртв — победа, опыт, возможно новый враг
            AppendLog($"✨ Вы победили {currentEnemy.enemyName}!");

            if (warrior.experience != null)
            {
                int xpReward = 50;
                warrior.experience.AddXP(xpReward);
                AppendLog($"✨ Получено {xpReward} опыта!");
            }

            // Враг удаляется из списка (можно добавить позже)
        }
        else // ====== 5. ВРАГ ВЫЖИЛ И ОТВЕЧАЕТ ======
        {
            string enemyAttack = currentEnemy.Attack();
            AppendLog(enemyAttack);

            int enemyDamage = Random.Range(1, 7) + currentEnemy.StrengthModifier;
            string playerDamage = warrior.TakeDamage(enemyDamage);
            AppendLog(playerDamage);

            // ====== 6. ПРОВЕРКА: не умер ли воин ПОСЛЕ контратаки ======
            if (warrior.health <= 0)
            {
                // Если воин умер от контратаки — обрабатываем смерть
                HandleDeath();
            }
        }
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

    private void PerformHeal()
    {
        if (warrior.health <= 0)
        {
            AppendLog("💀 Ты мёртв и не можешь лечиться. Игра окончена?");
            return;
        }

        if (warrior.healthPotions <= 0)
        {
            AppendLog("🧪 У тебя нет зелий! Найди или купи.");
            return;
        }

        if (warrior.health >= warrior.maxHealth)
        {
            AppendLog("❤️ У тебя и так полное здоровье!");
            return;
        }

        warrior.healthPotions--;
        int healAmount = 10;
        warrior.health += healAmount;

        if (warrior.health > warrior.maxHealth)
            warrior.health = warrior.maxHealth;

        AppendLog($"🧪 Ты выпил зелье! Осталось зелий: {warrior.healthPotions}");
        AppendLog($"❤️ Восстановлено {healAmount} HP. Теперь HP: {warrior.health}/{warrior.maxHealth}");
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
            AppendLog(location == "Лес" ? "🌳 Ты отправляешься в лес..." : "🏠 Ты возвращаешься в таверну...");
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

    private void ResurrectWarrior()
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
        if (warrior.health > 0)
        {
            AppendLog("Ты ещё жив! Зачем воскресать?");
            return;
        }

        // 🧠 ОБЫЧНЫЙ РЕЖИМ — ВОСКРЕШАЕМ
        isDead = false;  // ← сбрасываем флаг смерти

        // Возвращаем в таверну
        GoToLocation("Таверна");

        // Штраф: теряем 25% опыта
        if (warrior.experience != null)
        {
            int lostXP = warrior.experience.currentXP / 4;
            warrior.experience.currentXP -= lostXP;
            if (warrior.experience.currentXP < 0)
                warrior.experience.currentXP = 0;

            AppendLog($"💔 При воскрешении ты потерял {lostXP} опыта!");
        }

        // Восстанавливаем половину здоровья
        warrior.health = warrior.maxHealth / 2;

        // Включаем спавн обратно
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.RestartSpawning();
        }

        AppendLog($"✨ Ты очнулся в таверне с {warrior.health}/{warrior.maxHealth} HP.");
        AppendLog("💡 Будь осторожнее в следующий раз!");

        RefreshStats();
    }

    private void RestartGame()
    {
        isDead = false;
        // Сбрасываем воина
        warrior.health = warrior.maxHealth;
        warrior.healthPotions = 3;
        if (warrior.experience != null)
        {
            warrior.experience.level = 1;
            warrior.experience.currentXP = 0;
            warrior.experience.xpToNextLevel = 100;
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
        yield return new WaitForSeconds(seconds);  // ждём указанное количество секунд
        RestartGame();  // вызываем перезапуск
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