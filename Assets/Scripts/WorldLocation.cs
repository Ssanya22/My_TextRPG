using System.Collections.Generic;

[System.Serializable]
public class WorldLocation
{
    public string id;                    // уникальный идентификатор
    public string name;                  // название
    public string description;           // описание
    public Faction faction;              // к какому народу относится
    public string type;                  // "village", "city", "forest", "mountain", "dungeon", "shrine"
    public List<string> connections;     // куда можно пойти (id локаций)
    public List<string> enemies;         // какие враги тут водятся
    public int reputationRequired;       // какая репутация нужна для входа
    public int maxReputation;            // максимальная репутация, которую можно получить в этой локации
    public bool isUnlocked;              // открыта ли локация
    public bool isDangerous;             // опасная зона? (враги спавнятся)
}