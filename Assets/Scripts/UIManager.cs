using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Управляет UI текстовой RPG: лог, поле ввода команд, панель статистики.
/// Enter отправляет команду. Команды: attack, hit, stats, враг, бей.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [SerializeField] private TMP_Text mainLog;
    [SerializeField] private TMP_InputField inputFieldTMP;
    [SerializeField] private InputField inputFieldLegacy;
    [SerializeField] private TMP_Text statsText;

    [Header("Игровая логика")]
    [SerializeField] private Warrior warrior;

    [Header("Опции")]
    [Tooltip("Максимум строк в логе (0 = без ограничения)")]
    [SerializeField] private int maxLogLines = 500;
    [Header("Боевая система")]
    [SerializeField] private List<Enemy> enemies = new List<Enemy>();

    private Enemy currentEnemy;
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

        // Очищаем лог от начального текста (если был задан вручную) и добавляем приветствие
        if (mainLog != null) mainLog.text = "";
        AppendLog("Добро пожаловать в текстовую RPG!");
        AppendLog("Введите команду: attack, hit, stats, враг, бей");

        RefreshStats();

        // Чтобы можно было сразу печатать команды после запуска
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

    /// <summary>Отправить команду из поля ввода (вызывается по Enter или кнопке).</summary>
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

    /// <summary>Обработка одной команды.</summary>

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
                AppendLog($"Враг {currentEnemy.enemyName} уже повержен!");

                // Автоматически выбираем следующего живого врага
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
                    AppendLog($"👺 Автоматически выбран: {currentEnemy.enemyName}");
                }
            }
            else
            {
                // Воин атакует
                string attackResult = warrior.Attack();
                AppendLog(attackResult);

                // Враг получает урон
                int damage = Random.Range(1, 9) + warrior.StrengthModifier;
                string damageResult = currentEnemy.TakeDamage(damage);
                AppendLog(damageResult);

                // Если враг умер после удара
                if (!currentEnemy.IsAlive())
                {
                    AppendLog($"✨ Вы победили {currentEnemy.enemyName}!");

                    // Добавляем опыт за победу
                    if (warrior != null && warrior.experience != null)
                    {
                        int xpReward = 50;
                        warrior.experience.AddXP(xpReward);
                        AppendLog($"✨ Получено {xpReward} опыта!");
                    }

                    // Удаляем врага из списка (но объект пока оставляем)
                    // Можно будет потом добавить красивое исчезновение
                }
                else
                {
                    // Если враг ещё жив, он отвечает
                    string enemyAttack = currentEnemy.Attack();
                    AppendLog(enemyAttack);

                    // Воин получает урон
                    int enemyDamage = Random.Range(1, 7) + currentEnemy.StrengthModifier;
                    string playerDamage = warrior.TakeDamage(enemyDamage);
                    AppendLog(playerDamage);
                }
            }
        }

        // ====== НЕИЗВЕСТНАЯ КОМАНДА ======
        else
        {
            AppendLog($"Неизвестная команда: {cmd}");
        }

        // Обновляем статистику и прокручиваем лог
        RefreshStats();
        ScrollLogToBottom();
    }

    /// <summary>Добавить строку в главный лог.</summary>
    public void AppendLog(string message)
    {
        if (mainLog == null) return;

        bool wasAtBottom = IsNearBottom();

        mainLog.text += message + "\n";

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

    /// <summary>Обновить панель статистики (HP, сила, ловкость).</summary>
    /// <summary>Обновить панель статистики (HP, сила, ловкость, уровень, опыт).</summary>
    public void RefreshStats()
    {
        if (statsText == null) return;
        if (warrior == null)
        {
            statsText.text = "HP: -\nСИЛ: -\nЛОВ: -\nУР: -\nОПЫТ: -/-";
            return;
        }

        string xpInfo = "";
        if (warrior.experience != null)
        {
            xpInfo = $"\nУР: {warrior.experience.level}\nОПЫТ: {warrior.experience.currentXP}/{warrior.experience.xpToNextLevel}";
        }
        else
        {
            xpInfo = "\nУР: -\nОПЫТ: -/-";
        }

        statsText.text = $"HP: {warrior.health}\nСИЛ: {warrior.strength}\nЛОВ: {warrior.dexterity}{xpInfo}";
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
        // Делает прокрутку стабильной: высота Content/текста = preferredHeight текста
        if (mainLog == null || _logRect == null) return;

        mainLog.ForceMeshUpdate();
        float preferred = mainLog.preferredHeight;

        // Подгоняем высоту самого текста
        _logRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferred);

        // Подгоняем высоту Content (если он есть) — чуть больше, чтобы был отступ снизу
        if (_contentRect != null)
        {
            _contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferred + logContentPadding);
        }
    }

    /// <summary>Установить воина (из кода, если не задан в Inspector).</summary>
    public void SetWarrior(Warrior w)
    {
        warrior = w;
        RefreshStats();
    }

    public void AddEnemy(Enemy enemy)
    {
        enemies.Add(enemy);
        if (currentEnemy == null)
        {
            currentEnemy = enemy;
            AppendLog($"👺 Выбран автоматически: {enemy.enemyName}");
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