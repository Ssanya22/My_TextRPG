using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WorldMap : MonoBehaviour
{
    [Header("Текущее местоположение")]
    public string currentLocationId = "tirenor";

    [Header("Все локации")]
    public List<WorldLocation> locationsList = new List<WorldLocation>();

    private Dictionary<string, WorldLocation> locationsDict = new Dictionary<string, WorldLocation>();

    void Awake()
    {
        BuildWorld();
    }

    void BuildWorld()
    {
        // ========== ЗЕМЛИ ВЕЛИРОВ ==========

        // Тиренор — главное поселение (стартовая локация)
        AddLocation(new WorldLocation
        {
            id = "tirenor",
            name = "Тиренор",
            description = "Главное поселение Велиров. Деревянные дома из вековых сосен, частокол, длинный дом держателя и священная роща. Здесь решают судьбы не мечом, а голосом.",
            faction = Faction.Veliry,
            type = "village",
            connections = new List<string> { "lunar_whisper", "eagle_cliff", "red_bor", "great_kurgan", "falcon_peak", "forgotten_spring", "rotten_swamps", "training_ground" },
            enemies = new List<string>(),
            reputationRequired = 0,
            isUnlocked = true,
            isDangerous = false,
            npcs = new List<string> { "tverdislav" }  // ← Твердислав
        });

        // Лунный Шёпот — деревня знахарей
        AddLocation(new WorldLocation
        {
            id = "lunar_whisper",
            name = "Лунный Шёпот",
            description = "Деревня знахарей. Сюда приходят лечиться даже из других земель. В воздухе пахнет травами, настойками и древними знаниями.",
            faction = Faction.Veliry,
            type = "village",
            connections = new List<string> { "tirenor" },
            enemies = new List<string>(),
            reputationRequired = 5,
            isUnlocked = false,
            isDangerous = false,
            npcs = new List<string> { "milana" }  // ← Милана (пока заглушка)
        });

        // Орлиный Утёс — торговый форпост
        AddLocation(new WorldLocation
        {
            id = "eagle_cliff",
            name = "Орлиный Утёс",
            description = "Бывший лагерь Соларны, теперь торговый форпост. Единственное место, где чужаки могут торговать без сопровождения. Здесь смешались культуры и языки.",
            faction = Faction.Veliry,
            type = "town",
            connections = new List<string> { "tirenor" },
            enemies = new List<string> { "бандиты", "контрабандисты" },
            reputationRequired = 10,
            maxReputation = 25,
            isUnlocked = false,
            isDangerous = true
        });

        // Красный Бор — суровое поселение на юге
        AddLocation(new WorldLocation
        {
            id = "red_bor",
            name = "Красный Бор",
            description = "Суровое поселение на южной границе. Здесь живут отчаянные охотники. Отсюда идут к Гнилым Топям.",
            faction = Faction.Veliry,
            type = "village",
            connections = new List<string> { "tirenor", "rotten_swamps" },
            enemies = new List<string>(),
            reputationRequired = 15,
            isUnlocked = false,
            isDangerous = false
        });

        // Курган Велигора — святое место
        AddLocation(new WorldLocation
        {
            id = "great_kurgan",
            name = "Курган Велигора",
            description = "Могила первого вождя. Святое место. Говорят, в трудный час его дух встаёт на защиту. Тишина здесь особенная.",
            faction = Faction.Veliry,
            type = "shrine",
            connections = new List<string> { "tirenor" },
            enemies = new List<string>(),
            reputationRequired = 20,
            isUnlocked = false,
            isDangerous = false
        });

        // Соколиный Пик — дозорная вышка на границе с Вальгрим
        AddLocation(new WorldLocation
        {
            id = "falcon_peak",
            name = "Соколиный Пик",
            description = "Высокий холм на границе с землями Вальгрим. Здесь дозорная вышка и воинский лагерь. Командует Воротыслав.",
            faction = Faction.Veliry,
            type = "military",
            connections = new List<string> { "tirenor" },
            enemies = new List<string> { "волки", "разведчики Вальгрим" },
            reputationRequired = 25,
            maxReputation = 40,
            isUnlocked = false,
            isDangerous = true,
            npcs = new List<string> { "vorotyslav" }  // ← Воротыслав (пока заглушка)
        });

        // Источник Забвения — таинственное озеро
        AddLocation(new WorldLocation
        {
            id = "forgotten_spring",
            name = "Источник Забвения",
            description = "Озеро с прозрачной водой. Говорят, вода может стереть память. Старейшины знают, что это правда.",
            faction = Faction.Veliry,
            type = "shrine",
            connections = new List<string> { "tirenor" },
            enemies = new List<string>(),
            reputationRequired = 30,
            isUnlocked = false,
            isDangerous = false
        });

        // Гнилые Топи — проклятое место
        AddLocation(new WorldLocation
        {
            id = "rotten_swamps",
            name = "Гнилые Топи",
            description = "Лес больной, вода ржавая, животные мутировавшие. Место проклятое. Здесь скрывается изгой Келл-ар-Торн.",
            faction = Faction.Veliry,
            type = "dungeon",
            connections = new List<string> { "red_bor" },
            enemies = new List<string> { "мутанты", "призраки", "Келл-ар-Торн" },
            reputationRequired = 40,
            maxReputation = 100,
            isUnlocked = false,
            isDangerous = true,
            npcs = new List<string> { "kell_artorn" }  // ← Келл-ар-Торн (пока заглушка)
        });

        // Тренировочная поляна — временная боевая зона
        AddLocation(new WorldLocation
        {
            id = "training_ground",
            name = "Тренировочная поляна",
            description = "Поляна на опушке леса, где можно встретить диких гоблинов. Хорошее место для тренировок.",
            faction = Faction.Veliry,
            type = "forest",
            connections = new List<string> { "tirenor" },
            enemies = new List<string> { "гоблины" },
            reputationRequired = 0,
            maxReputation = 10,
            isUnlocked = true,
            isDangerous = true
        });

        // ========== В ДАЛЬНЕЙШЕМ ДОБАВИМ ДРУГИЕ НАРОДЫ ==========
    }

    void AddLocation(WorldLocation loc)
    {
        locationsList.Add(loc);
        locationsDict[loc.id] = loc;
    }

    public WorldLocation GetLocation(string id)
    {
        return locationsDict.ContainsKey(id) ? locationsDict[id] : null;
    }

    public WorldLocation GetCurrentLocation()
    {
        return GetLocation(currentLocationId);
    }

    public bool TravelTo(string locationId)
    {
        var current = GetCurrentLocation();
        if (current == null) return false;

        Debug.Log($"=== TravelTo: текущая локация {current.name}, ищем {locationId} ===");

        if (!current.connections.Contains(locationId))
        {
            Debug.Log($"Нельзя пойти в {locationId} из {current.name}");
            return false;
        }

        var target = GetLocation(locationId);
        if (target == null) return false;

        Debug.Log($"Цель: {target.name}, isUnlocked={target.isUnlocked}, repRequired={target.reputationRequired}");

        // ====== ПРОВЕРКА РЕПУТАЦИИ ======
        if (!target.isUnlocked)
        {
            Character character = FindFirstObjectByType<Character>();
            if (character != null)
            {
                int playerRep = character.GetReputation(target.faction);
                Debug.Log($"Репутация игрока с {target.faction}: {playerRep}, требуется: {target.reputationRequired}");

                if (playerRep >= target.reputationRequired)
                {
                    target.isUnlocked = true;
                    Debug.Log($"📍 Открыта новая локация: {target.name}");

                    UIManager ui = FindFirstObjectByType<UIManager>();
                    if (ui != null)
                    {
                        ui.AppendLog($"✨ Твоя репутация с {target.faction} выросла! Теперь тебе доступна {target.name}.");
                    }
                }
                else
                {
                    Debug.Log($"❌ Нужна репутация {target.reputationRequired} с {target.faction}, у тебя {playerRep}");
                    return false;
                }
            }
        }

        currentLocationId = locationId;
        return true;
    }

    public void UnlockLocation(string locationId)
    {
        var loc = GetLocation(locationId);
        if (loc != null)
        {
            loc.isUnlocked = true;
            Debug.Log($"📍 Открыта новая локация: {loc.name}");
        }
    }

    public List<string> GetAvailableDestinations()
    {
        var current = GetCurrentLocation();
        if (current == null) return new List<string>();

        var result = new List<string>();
        foreach (var connId in current.connections)
        {
            var loc = GetLocation(connId);
            if (loc != null && loc.isUnlocked)
            {
                result.Add(loc.name);
            }
        }
        return result;
    }

    public string GetMapString()
    {
        string result = "═══════════════════════════════════\n";
        result += "🗺️  КАРТА ВЕЛИРОВ  🗺️\n";
        result += "═══════════════════════════════════\n\n";

        var veliryLocations = locationsList.Where(l => l.faction == Faction.Veliry).OrderBy(l => l.isUnlocked ? 0 : 1);

        foreach (var loc in veliryLocations)
        {
            string icon = GetLocationIcon(loc.type);
            string status = loc.isUnlocked ? "" : " 🔒 (закрыто)";
            string danger = loc.isDangerous ? " ⚠️" : "";

            result += $"{icon} {loc.name}{status}{danger}\n";
            if (loc.isUnlocked)
            {
                result += $"   {loc.description}\n\n";
            }
        }

        result += "═══════════════════════════════════\n";
        return result;
    }

    string GetLocationIcon(string type)
    {
        switch (type)
        {
            case "village": return "🏡";
            case "town": return "🏰";
            case "shrine": return "🙏";
            case "military": return "⚔️";
            case "dungeon": return "💀";
            default: return "📍";
        }
    }
}