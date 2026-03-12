using UnityEngine;

public class Orc : Enemy
{
    // Awake вызывается сразу при создании объекта (даже в редакторе)
    void Awake()
    {
        enemyName = "Орк-берсерк";
        health = 20;
        strength = 16;
        xpReward = 100;  // больше опыта
    }

    // Переопределяем атаку для орка
    public override string Attack()
    {
        int d20 = Random.Range(1, 21);
        int total = d20 + StrengthModifier;
        return $"👹 {enemyName} замахивается топором: {d20} + {StrengthModifier} = {total}";
    }
}