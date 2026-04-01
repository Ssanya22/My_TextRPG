using System.Collections.Generic;

[System.Serializable]
public class DialogueNode
{
    public string id;                      // уникальный ID диалога
    public string npcText;                 // что говорит NPC
    public List<DialogueOption> options;   // варианты ответа игрока
}