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
        AppendLog("Введите команду: attack, hit, stats, враг, бей, зелье, где я");
        
        RefreshStats();

        if (inputFieldTMP != null) inputFieldTMP.ActivateInputField();
        if (inputFieldLegacy != null) inputFieldLegacy.ActivateInputField();
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

        string lower = cmd.ToLowerInvariant();
        string result;

        // ====== КОМАНДЫ ВОИНА ======
        if (lower == "attack" || lower == "атака")
        {
            result = warrior != null ? warrior.Attack() : "Нет воина в сцене.";
            AppendLog(result);
        }
        else if (lower == "hit" || lower == "урон" || lower == "damage")
        {
            result = warrior != null ? warrior.TakeDamage() : "Нет воина в сцене.";
            AppendLog(result);
        }
        else if (lower == "stats" || lower == "статы")
        {
            result = warrior != null ? warrior.GetStatsLine() : "Нет воина в сцене.";
            AppendLog(result);
        }

        // ====== КОМАНДА ЛЕЧЕНИЯ ======
        else if (lower == "зелье" || lower == "heal" || lower == "лечиться")
        {
            if (warrior == null)
            {
                AppendLog("Нет воина в сцене.");
            }
            else if (warrior.healthPotions <= 0)
            {
                AppendLog("🧪 У тебя нет зелий! Найди или купи.");
            }
            else if (warrior.health >= warrior.maxHealth)
            {
                AppendLog("❤️ У тебя и так полное здоровье!");
            }
            else
            {
                warrior.healthPotions--;
                
                int healAmount = 10;
                warrior.health += healAmount;
                
                if (warrior.health > warrior.maxHealth)
                    warrior.health = warrior.maxHealth;
                    
                AppendLog($"🧪 Ты выпил зелье! Осталось зелий: {warrior.healthPotions}");
                AppendLog($"❤️ Восстановлено {healAmount} HP. Теперь HP: {warrior.health}/{warrior.maxHealth}");
                RefreshStats();
            }
        }

        // ====== КОМАНДЫ ДЛЯ РАБОТЫ СО ВРАГАМИ ======
        else if (lower == "враг" || lower == "enemy")
        {
            if (currentEnemy != null)
                AppendLog(currentEnemy.GetStats());
            else
                AppendLog("Нет выбранного врага. Введите 'враги' чтобы увидеть список.");
        }
        else if (lower == "враги" || lower == "enemies")
        {
            if (enemies.Count == 0)
            {
                AppendLog("Нет врагов на сцене.");
            }
            else
            {
                AppendLog($"👺 Враги ({enemies.Count}):");
                for (int i = 0; i < enemies.Count; i++)
                {
                    string status = enemies[i].IsAlive() ? "жив" : "мертв";
                    AppendLog($"{i + 1}. {enemies[i].enemyName} (HP: {enemies[i].health}) - {status}");
                }
                AppendLog($"Текущий выбран: {(currentEnemy != null ? currentEnemy.enemyName : "никто")}");
            }
        }
        else if (lower.StartsWith("выбрать "))
        {
            string numStr = lower.Replace("выбрать ", "").Trim();
            if (int.TryParse(numStr, out int index))
            {
                if (index >= 1 && index <= enemies.Count)
                {
                    currentEnemy = enemies[index - 1];
                    AppendLog($"👺 Выбран враг: {currentEnemy.enemyName} (HP: {currentEnemy.health})");
                }
                else
                {
                    AppendLog($"Нет врага с номером {index}. Всего врагов: {enemies.Count}");
                }
            }
            else
            {
                AppendLog("Нужно ввести номер врага. Например: выбрать 2");
            }
        }

        // ====== БОЕВАЯ КОМАНДА ======
        else if (lower == "атаковать врага" || lower == "бей" || lower == "attack enemy")
        {
            if (currentEnemy == null)
            {
                AppendLog("Нет выбранного врага! Сначала выбери: 'враги' потом 'выбрать N'");
            }
            else if (!currentEnemy.IsAlive())
            {
                AppendLog($"👺 Враг {currentEnemy.enemyName} уже повержен!");

                Enemy nextEnemy = null;
                foreach (Enemy e in enemies)
                {
                    if (e.IsAlive())
                    {
                        nextEnemy = e;
                        break;
                    }
                }

                if (nextEnemy != null)
                {
                    currentEnemy = nextEnemy;
                    AppendLog($"Автоматически выбран: {currentEnemy.enemyName}");
                }
            }
            else
            {
                string attackResult = warrior.Attack();
                AppendLog(attackResult);

                int damage = Random.Range(1, 9) + warrior.StrengthModifier;
                string damageResult = currentEnemy.TakeDamage(damage);
                AppendLog(damageResult);

                if (!currentEnemy.IsAlive())
                {
                    AppendLog($"✨ Вы победили {currentEnemy.enemyName}!");

                    if (warrior != null && warrior.experience != null)
                    {
                        int xpReward = 50;
                        warrior.experience.AddXP(xpReward);
                        AppendLog($"✨ Получено {xpReward} опыта!");
                    }
                }
                else
                {
                    string enemyAttack = currentEnemy.Attack();
                    AppendLog(enemyAttack);

                    int enemyDamage = Random.Range(1, 7) + currentEnemy.StrengthModifier;
                    string playerDamage = warrior.TakeDamage(enemyDamage);
                    AppendLog(playerDamage);
                }
            }
        }

        // ====== КОМАНДЫ ЛОКАЦИЙ ======
        else if (lower == "идти лес" || lower == "лес")
        {
            LocationManager locMgr = FindObjectOfType<LocationManager>();
            if (locMgr != null)
            {
                locMgr.GoToLocation("Лес");
                AppendLog("🌳 Ты отправляешься в лес...");
            }
            else
            {
                AppendLog("Ошибка: LocationManager не найден!");
            }
        }
        else if (lower == "идти таверна" || lower == "таверна")
        {
            LocationManager locMgr = FindObjectOfType<LocationManager>();
            if (locMgr != null)
            {
                locMgr.GoToLocation("Таверна");
                AppendLog("🏠 Ты возвращаешься в таверну...");
            }
            else
            {
                AppendLog("Ошибка: LocationManager не найден!");
            }
        }
        else if (lower == "где я" || lower == "локация")
        {
            LocationManager locMgr = FindObjectOfType<LocationManager>();
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

        // ====== НЕИЗВЕСТНАЯ КОМАНДА ======
        else
        {
            AppendLog($"Неизвестная команда: {cmd}");
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

        string stats = $"HP: {warrior.health}/{warrior.maxHealth}\nСИЛ: {warrior.strength}\nЛОВ: {warrior.dexterity}\nЗелья: {warrior.healthPotions}{xpInfo}";
        
        // Конвертируем статистику
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