using UnityEngine;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public List<DialogueNode> dialogues = new List<DialogueNode>();
    public Dictionary<string, NPC> npcDict = new Dictionary<string, NPC>();
    private Dictionary<string, DialogueNode> dialogueDict = new Dictionary<string, DialogueNode>();
    private NPC currentNPC;
    
    void Awake()
    {
        BuildNPCs();
        BuildDialogues();
    }
    
    void BuildNPCs()
    {
        // Твердислав (Тиренор)
        npcDict["tverdislav"] = new NPC
        {
            id = "tverdislav",
            name = "Твердислав",
            locationId = "tirenor",
            defaultDialogueId = "tverdislav_start",
            reputationRequirements = new Dictionary<string, int>()
        };
        
        // Милана (Лунный Шёпот)
        npcDict["milana"] = new NPC
        {
            id = "milana",
            name = "Милана",
            locationId = "lunar_whisper",
            defaultDialogueId = "milana_start",
            reputationRequirements = new Dictionary<string, int>()
        };
        
        // Воротыслав (Соколиный Пик)
        npcDict["vorotyslav"] = new NPC
        {
            id = "vorotyslav",
            name = "Воротыслав",
            locationId = "falcon_peak",
            defaultDialogueId = "vorotyslav_start",
            reputationRequirements = new Dictionary<string, int>()
        };
        
        // Келл-ар-Торн (Гнилые Топи)
        npcDict["kell_artorn"] = new NPC
        {
            id = "kell_artorn",
            name = "Келл-ар-Торн",
            locationId = "rotten_swamps",
            defaultDialogueId = "kell_start",
            reputationRequirements = new Dictionary<string, int>()
        };
    }
    
    void BuildDialogues()
    {
        // ========== ТВЕРДИСЛАВ (Тиренор) ==========
        
        AddDialogue(new DialogueNode
        {
            id = "tverdislav_start",
            npcText = "Приветствую, путник. Я — Твердислав, держатель этих земель. Чем могу помочь?",
            options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    text = "Расскажи о землях Велиров.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = "tverdislav_about_land"
                },
                new DialogueOption
                {
                    text = "Как мне заслужить доверие народа?",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = "tverdislav_reputation"
                },
                new DialogueOption
                {
                    text = "Я хочу помочь. Есть ли работа?",
                    requiredReputation = 5,
                    effect = "none",
                    nextDialogueId = "tverdislav_quest"
                },
                new DialogueOption
                {
                    text = "Прощай.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = null
                }
            }
        });
        
        AddDialogue(new DialogueNode
        {
            id = "tverdislav_about_land",
            npcText = "Наши леса древни, а традиции — ещё древнее. Мы не строим каменных городов, но наши земли помнят больше, чем любая империя. На востоке — Лунный Шёпот, где лечат даже чужаков. На юге — Красный Бор, откуда идут к Гнилым Топям. Будь осторожен в тех местах.",
            options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    text = "Расскажи ещё.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = "tverdislav_start"
                },
                new DialogueOption
                {
                    text = "Пока.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = null
                }
            }
        });
        
        AddDialogue(new DialogueNode
        {
            id = "tverdislav_reputation",
            npcText = "Наш народ ценит тех, кто защищает лес. Убей гоблинов в Тренировочной поляне, помоги знахарям в Лунном Шёпоте, защити границу в Соколином Пике. Твои дела говорят громче слов.",
            options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    text = "Понял. Спасибо.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = null
                }
            }
        });
        
        AddDialogue(new DialogueNode
        {
            id = "tverdislav_quest",
            npcText = "В Гнилых Топях объявился изгой по имени Келл-ар-Торн. Говорят, он использует запретную магию. Если сможешь поговорить с ним или принести вести — я буду благодарен.",
            options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    text = "Я помогу.",
                    requiredReputation = 5,
                    effect = "add_reputation",
                    target = "Veliry",
                    amount = 5,
                    responseText = "Благодарю. Будь осторожен, путник.",
                    nextDialogueId = null
                },
                new DialogueOption
                {
                    text = "Пока рано. Уйду.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = null
                }
            }
        });
        
        // ========== МИЛАНА (Лунный Шёпот) ==========
        
        AddDialogue(new DialogueNode
        {
            id = "milana_start",
            npcText = "Добро пожаловать в Лунный Шёпот. Я — Милана, хранительница знаний. Ты пришёл лечиться или учиться?",
            options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    text = "Я хочу научиться лечить травами.",
                    requiredReputation = 5,
                    effect = "add_reputation",
                    target = "Veliry",
                    amount = 3,
                    responseText = "Это похвально. Возьми эти травы и запомни: лес даёт силу тем, кто его уважает.",
                    nextDialogueId = null
                },
                new DialogueOption
                {
                    text = "Мне нужно лечение.",
                    requiredReputation = 0,
                    effect = "heal",
                    target = "",
                    amount = 10,
                    responseText = "Конечно. Вот целебный настой. Береги себя.",
                    nextDialogueId = null
                },
                new DialogueOption
                {
                    text = "Я ищу знания о древних ритуалах.",
                    requiredReputation = 20,
                    effect = "none",
                    nextDialogueId = "milana_secrets"
                },
                new DialogueOption
                {
                    text = "Прощай.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = null
                }
            }
        });
        
        AddDialogue(new DialogueNode
        {
            id = "milana_secrets",
            npcText = "Ты слишком рано ищешь такие знания. Сначала заслужи доверие народа, а потом возвращайся.",
            options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    text = "Понял. Спасибо.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = null
                }
            }
        });
        
        // ========== ВОРОТЫСЛАВ (Соколиный Пик) ==========
        
        AddDialogue(new DialogueNode
        {
            id = "vorotyslav_start",
            npcText = "Ты на границе, путник. Я — Воротыслав, командующий лесной стражей. Что привело тебя сюда?",
            options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    text = "Я хочу помочь защищать границу.",
                    requiredReputation = 10,
                    effect = "add_reputation",
                    target = "Veliry",
                    amount = 4,
                    responseText = "Хорошо. Патрулируй окрестности, убивай волков и разведчиков Вальгрим. За каждую победу получишь награду.",
                    nextDialogueId = null
                },
                new DialogueOption
                {
                    text = "Что здесь происходит?",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = "vorotyslav_about"
                },
                new DialogueOption
                {
                    text = "Я ищу врагов.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = "vorotyslav_enemies"
                },
                new DialogueOption
                {
                    text = "Прощай.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = null
                }
            }
        });
        
        AddDialogue(new DialogueNode
        {
            id = "vorotyslav_about",
            npcText = "Северяне из Вальгрим всё чаще появляются на нашей границе. Они рубят лес, охотятся на наших зверей. Твердислав слишком мягок, но мы держим оборону.",
            options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    text = "Я помогу.",
                    requiredReputation = 10,
                    effect = "add_reputation",
                    target = "Veliry",
                    amount = 4,
                    responseText = "Благодарю. Каждая победа приближает мир.",
                    nextDialogueId = null
                },
                new DialogueOption
                {
                    text = "Мне нужно идти.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = null
                }
            }
        });
        
        AddDialogue(new DialogueNode
        {
            id = "vorotyslav_enemies",
            npcText = "На границе водятся волки и разведчики Вальгрим. Убей их, и я позабочусь о награде. Если встретишь отряд северян — сразу сообщи мне.",
            options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    text = "Буду начеку.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = null
                }
            }
        });
        
        // ========== КЕЛЛ-АР-ТОРН (Гнилые Топи) ==========
        
        AddDialogue(new DialogueNode
        {
            id = "kell_start",
            npcText = "Ты пришёл убить меня? Или хочешь услышать правду?",
            options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    text = "Расскажи свою правду.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = "kell_story"
                },
                new DialogueOption
                {
                    text = "Ты преступник, я здесь, чтобы вершить правосудие.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = "kell_fight"
                },
                new DialogueOption
                {
                    text = "Я хочу изучить твою магию.",
                    requiredReputation = 20,
                    effect = "add_reputation",
                    target = "Mortis",
                    amount = 10,
                    responseText = "Магия крови опасна. Но если ты готов платить цену — я научу тебя.",
                    nextDialogueId = null
                },
                new DialogueOption
                {
                    text = "Я пришёл от Твердислава.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = "kell_peace"
                }
            }
        });
        
        AddDialogue(new DialogueNode
        {
            id = "kell_story",
            npcText = "Мою семью убили чужаки, когда Велиры смотрели в другую сторону. Старейшины говорят о мире, а я потерял всё. Теперь они называют меня изгоем. Но разве я виноват, что защищаю свой народ?",
            options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    text = "Я помогу тебе отомстить.",
                    requiredReputation = 0,
                    effect = "add_reputation",
                    target = "Mortis",
                    amount = 15,
                    responseText = "Ты смел. Возможно, мы ещё встретимся.",
                    nextDialogueId = null
                },
                new DialogueOption
                {
                    text = "Я попробую убедить старейшин выслушать тебя.",
                    requiredReputation = 0,
                    effect = "add_reputation",
                    target = "Veliry",
                    amount = 10,
                    responseText = "Сомневаюсь, что они послушают. Но если сможешь — я буду должен.",
                    nextDialogueId = null
                },
                new DialogueOption
                {
                    text = "Ты зашёл слишком далеко. Я не могу тебе помочь.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = null
                }
            }
        });
        
        AddDialogue(new DialogueNode
        {
            id = "kell_peace",
            npcText = "Твердислав... он всегда был слишком мягок. Передай ему: пусть приходит сам, если хочет говорить. А пока — оставь меня.",
            options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    text = "Передам.",
                    requiredReputation = 0,
                    effect = "none",
                    nextDialogueId = null
                }
            }
        });
        
        AddDialogue(new DialogueNode
        {
            id = "kell_fight",
            npcText = "Тогда ты умрёшь. Как и все, кто вставал на моём пути.",
            options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    text = "...",
                    requiredReputation = 0,
                    effect = "start_battle",
                    target = "kell_artorn",
                    amount = 0,
                    responseText = "",
                    nextDialogueId = null
                }
            }
        });
    }
    
    void AddDialogue(DialogueNode node)
    {
        dialogues.Add(node);
        dialogueDict[node.id] = node;
    }
    
    public DialogueNode GetDialogue(string id)
    {
        return dialogueDict.ContainsKey(id) ? dialogueDict[id] : null;
    }
    
    public NPC GetNPC(string id)
    {
        return npcDict.ContainsKey(id) ? npcDict[id] : null;
    }
    
    public void StartDialogue(NPC npc)
    {
        currentNPC = npc;
        DialogueNode startNode = GetDialogue(npc.defaultDialogueId);
        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null)
        {
            ui.StartDialogue(npc, startNode);
        }
    }
}