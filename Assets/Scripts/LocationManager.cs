using UnityEngine;
using System.Collections.Generic;

public class LocationManager : MonoBehaviour
{
    public string currentLocation = "Таверна";
    
    // Словарь локаций (название -> описание)
    private Dictionary<string, string> locations = new Dictionary<string, string>();

    void Start()
    {
        // Добавляем локации
        locations.Add("Таверна", "Уютное место с камином. Здесь можно отдохнуть.");
        locations.Add("Лес", "Тёмный и опасный лес. Здесь водятся гоблины.");
        
        Debug.Log($"Ты в локации: {currentLocation}");
    }
    
    public void GoToLocation(string locationName)
    {
        if (locations.ContainsKey(locationName))
        {
            currentLocation = locationName;
            Debug.Log($"Ты пришёл в: {currentLocation} — {locations[currentLocation]}");
            
            // Находим спавнер и говорим ему, какая теперь локация
          EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.UpdateLocation(currentLocation);
            }
        }
        else
        {
            Debug.Log($"Локация '{locationName}' не найдена.");
        }
    }
    
    public string GetCurrentLocation()
    {
        return currentLocation;
    }
    
    public string GetLocationDescription()
    {
        if (locations.ContainsKey(currentLocation))
            return locations[currentLocation];
        else
            return "Неизвестное место";
    }
}