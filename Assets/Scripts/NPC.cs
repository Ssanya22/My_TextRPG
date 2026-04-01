using System.Collections.Generic;

[System.Serializable]
public class NPC
{
    public string id;                      // уникальный ID
    public string name;                    // имя NPC
    public string locationId;              // где находится
    public string defaultDialogueId;       // стартовый диалог
    public Dictionary<string, int> reputationRequirements; // требования для доступа к диалогам
}