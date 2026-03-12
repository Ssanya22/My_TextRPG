using UnityEngine;

// Это простой враг
public class Enemy : MonoBehaviour
{
    // Характеристики врага (можно менять в Unity)
    public string enemyName = "Гоблин";
    public int health = 10;
    public int xpReward = 50;
    public int strength = 12;
    public int StrengthModifier => (strength - 10) / 2;

    // Эта функция считает, насколько сильно враг бьёт
    // (по правилам D&D: (сила - 10) / 2)
    int GetStrengthModifier()
    {
        return (strength - 10) / 2;
    }

    // Враг атакует (кидает 20-гранный кубик + сила)
    public virtual  string Attack()
    {
        int d20 = Random.Range(1, 21);  // бросок кубика от 1 до 20
        int total = d20 + GetStrengthModifier();

        // Возвращаем текст, который потом покажем в окошке
        return $"👺 {enemyName} атакует: {d20} + {GetStrengthModifier()} = {total}";
    }

    // Враг получает урон
    public virtual string TakeDamage(int damage)
{
    int oldHealth = health;
    health -= damage;
    
    // Здоровье не может быть меньше 0
    if (health < 0) health = 0;
    
    string result = $"👺 {enemyName} получил {damage} урона! Осталось жизней: {health}";
    
    if (health <= 0 && oldHealth > 0)
    {
        result += $"\n💀 {enemyName} повержен!";
    }
    
    return result;
    }

    // Проверка: жив ли враг?
    public bool IsAlive()
    {
        return health > 0;  // если здоровье больше 0 — значит жив
    }
    public string GetStats()
    {
        return $"👺 {enemyName}: HP {health}, СИЛА {strength}";
    }
}