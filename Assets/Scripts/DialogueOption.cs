[System.Serializable]
public class DialogueOption
{
    public string text;                    // текст варианта ответа
    public int requiredReputation;         // нужная репутация для выбора
    public string effect;                  // "add_reputation", "open_location", "give_item", "start_quest"
    public string target;                  // цель эффекта (фракция, локация, предмет)
    public int amount;                     // количество (репутации, предметов)
    public string nextDialogueId;          // ID следующего диалога
    public string responseText;            // что скажет NPC после выбора
}