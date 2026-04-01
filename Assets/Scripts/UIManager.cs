using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

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

    [Header("Быстрые действия")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button healButton;
    [SerializeField] private Button statsButton;
    [SerializeField] private Button mapButton;
    [SerializeField] private Button homeButton;

    [Header("Настройки игры")]
    public bool hardcoreMode = false; // true = перманентная смерть
    private bool modeSelected = false; // выбран ли режим
    private bool isDead = false; // жив ли игрок 
    private float detectionTimer = 0f;
    private float detectionInterval = 2f; // проверка каждые 2 секунды
    private ScrollRect _scrollRect;
    private RectTransform _contentRect;
    private RectTransform _logRect;
    private DialogueManager dialogueManager;
    private NPC currentNPC;
    private DialogueNode currentDialogue;



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
        dialogueManager = FindFirstObjectByType<DialogueManager>();

        modeSelected = false;

        // ====== НАСТРОЙКА БЫСТРЫХ КНОПОК ======
        SetupButtons();
    }

    private void Update()
    {
        if (isDead) return;

        detectionTimer += Time.deltaTime;
        if (detectionTimer >= detectionInterval)
        {
            detectionTimer = 0f;
            CheckDetection();
        }
    }

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

        // ====== ЕСЛИ ИДЁТ ДИАЛОГ ======
        if (currentDialogue != null)
        {
            string lower = cmd.ToLowerInvariant();
            if (int.TryParse(lower, out int choiceIndex) && choiceIndex >= 1 && choiceIndex <= currentDialogue.options.Count)
            {
                SelectDialogueOption(choiceIndex - 1);
                return;
            }
            else
            {
                AppendLog("❓ Напиши номер ответа (например, '1')");
                return;
            }
        }
        // ИСПОЛЬЗУЕМ НОВЫЙ ПАРСЕР
        ParsedCommand parsed = CommandParser.Parse(cmd);
        parsed.RawInput = cmd;

        // Для отладки - покажем что распарсили
        Debug.Log($"Парсинг: {parsed}");
        Debug.Log($"🔍 Отладка: Action={parsed.Action}, Target={parsed.Target}");
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
                if (enemies.Count == 0)
                {
                    AppendLog("👺 Нет врагов для атаки!");
                    break;
                }

                // Проверяем, в стелсе ли мы
                if (character.stealthSkill > 30)
                {
                    // Если в стелсе — можно выбрать цель
                    if (parsed.TargetIndex > 0 && parsed.TargetIndex <= enemies.Count)
                    {
                        currentEnemy = enemies[parsed.TargetIndex - 1];
                        if (currentEnemy.IsAlive())
                        {
                            AppendLog($"👺 Выбран враг: {currentEnemy.enemyName}");
                            PerformAttack();
                        }
                        else
                        {
                            AppendLog($"👺 {currentEnemy.enemyName} уже мёртв!");
                        }
                    }
                    else if (currentEnemy != null && currentEnemy.IsAlive())
                    {
                        PerformAttack();
                    }
                    else
                    {
                        AppendLog("👺 Выбери врага командой 'выбрать N'");
                    }
                }
                else
                {
                    // Если не в стелсе — авто-атака первого живого
                    foreach (Enemy e in enemies)
                    {
                        if (e.IsAlive())
                        {
                            currentEnemy = e;
                            AppendLog($"👺 {currentEnemy.enemyName} атакует тебя!");
                            PerformAttack();
                            break;
                        }
                    }
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
            case "upgrade":
                HandleUpgrade(parsed);
                break;
            // ----- СПИСОК ВРАГОВ -----
            case "enemies":
                ShowEnemyList();
                break;
            // ----- РЕПУТАЦИЯ -----
            case "reputation":
                ShowReputation();
                break;
            // ----- ИНФО О ВРАГЕ -----
            case "enemy_info":
                if (currentEnemy != null)
                    AppendLog(currentEnemy.GetStats());
                else
                    AppendLog("Нет выбранного врага.");
                break;

            // ----- ЛОКАЦИИ -----         

            case "tavern":
                GoToLocation("Таверна");
                break;

            case "mountains":
                GoToLocation("Горы");
                break;

            // ----- ВЫБОР ЦЕЛИ -----
            case "select":
                if (character.stealthSkill <= 30)
                {
                    AppendLog("👤 Ты слишком заметен! Враги уже нападают.");
                    break;
                }

                if (parsed.TargetIndex > 0 && parsed.TargetIndex <= enemies.Count)
                {
                    currentEnemy = enemies[parsed.TargetIndex - 1];
                    if (currentEnemy.IsAlive())
                    {
                        AppendLog($"👺 Выбран враг: {currentEnemy.enemyName}");
                    }
                    else
                    {
                        AppendLog($"👺 {currentEnemy.enemyName} уже мёртв!");
                        currentEnemy = null;
                    }
                }
                else
                {
                    AppendLog("Укажи номер врага, например: 'выбрать второго'");
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


            // ====== КОМАНДЫ НАВИГАЦИИ ======
            case "map":
            case "карта":
                ShowMap();
                break;

            case "location":
            case "где я":
                ShowCurrentLocation();
                break;

            case "travel":
            case "пойти":
            case "идти":
                TravelTo(parsed);
                break;
            // ====== КОМАНДЫ ДЛЯ NPC ======
            case "whoshere":
                ShowNPCsHere();
                break;

            case "talk":
                TalkToNPC(parsed);
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
            statsText.text = "HP: - | СИЛ: - | ЛОВ: - | ТЕЛ: -";
            return;
        }

        string stats = $"❤️ HP: {character.health}/{character.maxHealth}\n";  // ← обязательно!
        stats += $"⚔️ СИЛ: {character.strength} | 🏹 ЛОВ: {character.dexterity} | 🛡️ ТЕЛ: {character.constitution}\n";
        stats += $"🧪 Зелья: {character.healthPotions}\n";

        if (character.experience != null)
        {
            stats += $"⭐ УР: {character.experience.level} | {character.experience.currentXP}/{character.experience.xpToNextLevel}";
        }

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

    public bool IsOrcAlive()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy is Orc && enemy.IsAlive())
                return true;
        }
        return false;
    }
    public int GetEnemyCount()
    {
        return enemies.Count;
    }
    public void SetCharacter(Character c)  // ← ИЗМЕНЕНО (было SetWarrior)
    {
        character = c;
        RefreshStats();
    }

    private void PerformAttack()
    {
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

        // Проверка на смерть
        if (character.health <= 0)
        {
            HandleDeath();
            return;
        }

        // Атака
        string attackResult = character.Attack();
        AppendLog(attackResult);

        int damage = UnityEngine.Random.Range(1, 9) + character.StrengthModifier;
        string damageResult = currentEnemy.TakeDamage(damage);
        AppendLog(damageResult);

        if (!currentEnemy.IsAlive())
        {
            AppendLog($"✨ Вы победили {currentEnemy.enemyName}!");
            character.ImproveSkill("combat", 2);

            if (character.experience != null)
            {
                int xpReward = currentEnemy.xpReward;
                character.experience.AddXP(xpReward);
                AppendLog($"✨ Получено {xpReward} опыта!");
            }

            // ====== НАЧИСЛЕНИЕ РЕПУТАЦИИ ======
            WorldMap worldMap = FindFirstObjectByType<WorldMap>();
            if (worldMap != null)
            {
                var currentLocation = worldMap.GetCurrentLocation();
                int currentRep = character.GetReputation(Faction.Veliry);
                int maxRep = currentLocation.maxReputation;

                if (currentRep < maxRep)
                {
                    character.AddReputation(Faction.Veliry, 2);
                    AppendLog($"📈 Репутация с Велирами +2 (всего: {currentRep + 2}/{maxRep})");
                }
                else
                {
                    AppendLog($"📈 Твоя репутация с Велирами достигла предела в этой местности ({maxRep}). Пора искать новые земли.");
                }
            }
        }
        else
        {
            // Враг отвечает
            string enemyAttack = currentEnemy.Attack();
            AppendLog(enemyAttack);

            int enemyDamage = UnityEngine.Random.Range(1, 7) + currentEnemy.StrengthModifier;
            string playerDamage = character.TakeDamage(enemyDamage);
            AppendLog(playerDamage);

            if (character.health <= 0)
            {
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
        stats += $"   (+{character.maxHealth - 10} от телосложения)\n";

        if (character.magicSkill > 0)
            stats += $"⚡ Мана: {character.magicSkill * 2}\n";

        stats += $"\n⚔️ БОЕВЫЕ ХАРАКТЕРИСТИКИ:\n";
        stats += $"   Сила: {character.strength} | +{(character.strength - 10) / 2} к урону\n";
        stats += $"   Ловкость: {character.dexterity} | {character.GetDodgeChance()}% уворота\n";
        stats += $"   Телосложение: {character.constitution} | +{(character.constitution - 10) / 2} HP\n";

        stats += $"\n📊 НАВЫКИ:\n";
        stats += $"   ⚔️ Бой: {character.combatSkill}\n";
        if (character.tradingSkill > 0)
            stats += $"   🪙 Торговля: {character.tradingSkill} | скидка {Mathf.RoundToInt((character.charisma - 10) * 2)}%\n";
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

        // Показываем характеристики и здоровье
        skills += $"\n📈 ХАРАКТЕРИСТИКИ:\n";
        skills += $"   Сила: {character.strength}\n";
        skills += $"   Ловкость: {character.dexterity}\n";
        skills += $"   Телосложение: {character.constitution}\n";
        skills += $"   ❤️ Здоровье: {character.health}/{character.maxHealth}\n";  // ← ДОБАВИЛИ

        if (character.skillPoints > 0)
        {
            skills += $"\n⭐ Очков навыков: {character.skillPoints} (используй 'улучшить силу/бой/телосложение/торговлю')";
        }

        AppendLog(skills);
    }
    private void HandleUpgrade(ParsedCommand parsed)
    {
        if (character == null)
        {
            AppendLog("❌ Нет персонажа!");
            return;
        }

        if (character.skillPoints <= 0)
        {
            AppendLog("❌ У тебя нет очков навыков! Сначала повысь уровень.");
            return;
        }

        if (string.IsNullOrEmpty(parsed.Target))
        {
            AppendLog($"📚 Что можно улучшить (у тебя {character.skillPoints} очков):");
            AppendLog("   • 'улучшить бой' — +2 к навыку боя");
            AppendLog("   • 'улучшить силу' — +1 к силе");
            AppendLog("   • 'улучшить ловкость' — +1 к ловкости");
            AppendLog("   • 'улучшить телосложение' — +1 к здоровью");
            AppendLog("   • 'улучшить торговлю' — +2 к торговле");
            AppendLog("   • 'улучшить скрытность' — +2 к скрытности");
            return;
        }

        // Проверяем, что улучшаем
        string skillToUpgrade = parsed.Target;

        // Пробуем улучшить
        bool success = character.UpgradeSkill(skillToUpgrade);

        if (success)
        {
            AppendLog($"✨ Навык '{skillToUpgrade}' улучшен!");
            RefreshStats();
        }
        else
        {
            AppendLog($"❌ Не удалось улучшить '{skillToUpgrade}'. Попробуй: силу, ловкость, бой, торговлю, скрытность.");
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

    // ====== НОВЫЙ МЕТОД ДЛЯ ПРОВЕРКИ ОБНАРУЖЕНИЯ ======
    private void CheckDetection()
    {
        if (enemies.Count == 0 || character == null) return;

        bool detected = false;

        foreach (Enemy enemy in enemies)
        {
            if (enemy.IsAlive() && character.IsDetectedBy(enemy))
            {
                detected = true;
                if (currentEnemy == null || !currentEnemy.IsAlive())
                {
                    currentEnemy = enemy;
                    AppendLog($"👺 {enemy.enemyName} заметил тебя! Бой начинается!");
                }
                break;
            }
        }

        if (!detected && character.stealthSkill > 30)
        {
            AppendLog("👤 Ты остаёшься незамеченным. Можешь выбрать цель.");
        }
    }

    // ====== МЕТОДЫ ДЛЯ НАВИГАЦИИ ======
    private void ShowMap()
    {
        WorldMap worldMap = FindFirstObjectByType<WorldMap>();
        if (worldMap != null)
            AppendLog(worldMap.GetMapString());
        else
            AppendLog("❌ Карта мира не найдена!");
    }

    private void ShowCurrentLocation()
    {
        WorldMap worldMap = FindFirstObjectByType<WorldMap>();
        if (worldMap == null)
        {
            AppendLog("❌ Карта мира не найдена!");
            return;
        }

        var loc = worldMap.GetCurrentLocation();
        if (loc == null)
        {
            AppendLog("❌ Не удалось определить местоположение");
            return;
        }

        AppendLog($"📍 {loc.name}\n");
        AppendLog(loc.description);

        var destinations = worldMap.GetAvailableDestinations();
        if (destinations.Count > 0)
            AppendLog($"\n🚪 Можно пойти: {string.Join(", ", destinations)}");
    }

    private void TravelTo(ParsedCommand parsed)
    {
        if (string.IsNullOrEmpty(parsed.Target))
        {
            AppendLog("🤔 Куда идти? Например: 'пойти в лес'");
            return;
        }

        WorldMap worldMap = FindFirstObjectByType<WorldMap>();
        if (worldMap == null)
        {
            AppendLog("❌ Карта мира не найдена!");
            return;
        }

        var current = worldMap.GetCurrentLocation();
        if (current == null)
        {
            AppendLog("❌ Не могу определить текущее местоположение");
            return;
        }

        // Убираем предлоги из цели
        string targetName = parsed.Target.ToLower()
            .Replace("в ", "")
            .Replace("во ", "")
            .Replace("на ", "")
            .Replace("к ", "")
            .Trim();

        // Ищем локацию по названию среди доступных
        string targetId = null;
        foreach (var connId in current.connections)
        {
            var loc = worldMap.GetLocation(connId);
            if (loc != null && loc.name.ToLower().Contains(targetName))
            {
                targetId = connId;
                break;
            }
        }

        if (targetId == null)
        {
            AppendLog($"❌ Нет места '{parsed.Target}' рядом с {current.name}");
            AppendLog($"Доступно: {string.Join(", ", worldMap.GetAvailableDestinations())}");
            return;
        }

        var targetLoc = worldMap.GetLocation(targetId);
        // ====== ОТЛАДКА ======
        Debug.Log($"Попытка перейти в {targetLoc.name}, isUnlocked={targetLoc.isUnlocked}, repRequired={targetLoc.reputationRequired}");

        if (!targetLoc.isUnlocked)
        {
            int playerRep = character.GetReputation(targetLoc.faction);
            Debug.Log($"Проверка {targetLoc.name}: репутация={playerRep}, требуется={targetLoc.reputationRequired}");

            if (playerRep >= targetLoc.reputationRequired)
            {
                // Открываем локацию
                targetLoc.isUnlocked = true;
                AppendLog($"✨ Твоя репутация с {targetLoc.faction} достаточна! {targetLoc.name} открыта.");
            }
            else
            {
                AppendLog($"🔒 {targetLoc.name} закрыта. Нужна репутация {targetLoc.reputationRequired} с {targetLoc.faction}. У тебя {playerRep}");
                return;
            }
        }

        if (worldMap.TravelTo(targetId))
        {
            var newLoc = worldMap.GetCurrentLocation();
            AppendLog($"🚶 Ты отправляешься в {newLoc.name}...");
            AppendLog(newLoc.description);

            if (newLoc.isDangerous)
            {
                EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
                if (spawner != null)
                {
                    spawner.UpdateLocation(newLoc.name);
                    AppendLog("⚠️ Вокруг чувствуется опасность...");
                }
            }
        }
        else
        {
            AppendLog($"❌ Не удалось переместиться в {targetLoc.name}");
        }
    }
    private void OnTmpSubmit(string text)
    {
        SubmitCommand(text);
        if (inputFieldTMP != null) inputFieldTMP.ActivateInputField();
    }

    private void ShowReputation()
    {
        string result = "📊 РЕПУТАЦИЯ:\n";
        result += "═══════════════════════\n";

        foreach (Faction faction in Enum.GetValues(typeof(Faction)))
        {
            int rep = character.GetReputation(faction);
            string icon = rep > 0 ? "👍" : (rep < 0 ? "👎" : "🤝");
            result += $"{icon} {faction}: {rep}\n";
        }

        AppendLog(result);
    }

    private void TalkToNPC(ParsedCommand parsed)
    {
        if (string.IsNullOrEmpty(parsed.Target))
        {
            AppendLog("С кем поговорить? Например: 'поговорить с твердиславом'");
            return;
        }

        string npcName = parsed.Target.ToLower();

        // Находим NPC в текущей локации
        WorldMap worldMap = FindFirstObjectByType<WorldMap>();
        if (worldMap == null)
        {
            AppendLog("❌ Карта мира не найдена!");
            return;
        }

        var currentLoc = worldMap.GetCurrentLocation();
        if (currentLoc == null || currentLoc.npcs == null)
        {
            AppendLog("Здесь никого нет.");
            return;
        }

        NPC foundNPC = null;
        foreach (string npcId in currentLoc.npcs)
        {
            NPC npc = dialogueManager.GetNPC(npcId);
            if (npc != null && npc.name.ToLower().Contains(npcName))
            {
                foundNPC = npc;
                break;
            }
        }

        if (foundNPC == null)
        {
            AppendLog($"Здесь нет никого с именем '{parsed.Target}'.");
            return;
        }

        if (dialogueManager == null)
        {
            AppendLog("❌ Система диалогов не найдена!");
            return;
        }

        dialogueManager.StartDialogue(foundNPC);
    }

    // ====== НАСТРОЙКА БЫСТРЫХ КНОПОК ======
    private void SetupButtons()
    {
        if (attackButton != null)
            attackButton.onClick.AddListener(() => SubmitCommand("атаковать"));

        if (healButton != null)
            healButton.onClick.AddListener(() => SubmitCommand("зелье"));

        if (statsButton != null)
            statsButton.onClick.AddListener(() => SubmitCommand("статы"));

        if (mapButton != null)
            mapButton.onClick.AddListener(() => SubmitCommand("карта"));

        if (homeButton != null)
            homeButton.onClick.AddListener(() => SubmitCommand("пойти в таверну"));
    }

    // ====== ДИАЛОГИ ======
    public void StartDialogue(NPC npc, DialogueNode startNode)
    {
        currentNPC = npc;
        currentDialogue = startNode;
        ShowDialogue();
    }

    private void ShowDialogue()
    {
        if (currentDialogue == null) return;

        AppendLog($"\n═══════ {currentNPC.name} ═══════");
        AppendLog(currentDialogue.npcText);

        if (currentDialogue.options != null && currentDialogue.options.Count > 0)
        {
            AppendLog("\nВарианты:");
            for (int i = 0; i < currentDialogue.options.Count; i++)
            {
                var opt = currentDialogue.options[i];
                string req = opt.requiredReputation > 0 ? $" (нужна репутация {opt.requiredReputation})" : "";
                AppendLog($"{i + 1}. {opt.text}{req}");
            }
            AppendLog("\nНапиши номер ответа (например, '1')");
        }
        else
        {
            EndDialogue();
        }
    }

    public void SelectDialogueOption(int index)
    {
        if (currentDialogue == null || currentDialogue.options == null || index >= currentDialogue.options.Count)
        {
            AppendLog("❌ Неверный выбор");
            return;
        }

        var option = currentDialogue.options[index];

        // Проверка репутации
        if (option.requiredReputation > 0 && character.GetReputation(Faction.Veliry) < option.requiredReputation)
        {
            AppendLog($"❌ Нужна репутация {option.requiredReputation} с Велирами");
            ShowDialogue();
            return;
        }

        // Применяем эффект
        if (!string.IsNullOrEmpty(option.effect))
        {
            switch (option.effect)
            {
                case "add_reputation":
                    Faction faction = (Faction)System.Enum.Parse(typeof(Faction), option.target);
                    character.AddReputation(faction, option.amount);
                    break;
                case "open_location":
                    WorldMap worldMap = FindFirstObjectByType<WorldMap>();
                    if (worldMap != null)
                        worldMap.UnlockLocation(option.target);
                    break;
            }
        }

        // Показываем ответ NPC
        if (!string.IsNullOrEmpty(option.responseText))
        {
            AppendLog($"\n{currentNPC.name}: {option.responseText}");
        }

        // Переходим к следующему диалогу или завершаем
        if (!string.IsNullOrEmpty(option.nextDialogueId))
        {
            currentDialogue = dialogueManager.GetDialogue(option.nextDialogueId);
            ShowDialogue();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        AppendLog("\n═══════ КОНЕЦ ДИАЛОГА ═══════");
        currentNPC = null;
        currentDialogue = null;
    }

    // ====== NPC ======
    private void ShowNPCsHere()
    {
        WorldMap worldMap = FindFirstObjectByType<WorldMap>();
        if (worldMap == null)
        {
            AppendLog("❌ Карта мира не найдена!");
            return;
        }

        var currentLoc = worldMap.GetCurrentLocation();
        if (currentLoc == null || currentLoc.npcs == null || currentLoc.npcs.Count == 0)
        {
            AppendLog("Здесь никого нет.");
            return;
        }

        AppendLog($"👥 Кто здесь ({currentLoc.name}):");
        foreach (string npcId in currentLoc.npcs)
        {
            NPC npc = dialogueManager.GetNPC(npcId);
            if (npc != null)
            {
                AppendLog($"   • {npc.name}");
            }
        }
        AppendLog("\nНапиши 'поговорить с [имя]' чтобы начать разговор.");
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