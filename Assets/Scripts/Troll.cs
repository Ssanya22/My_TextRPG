using UnityEngine;

public class Troll : Enemy
{
    void Awake()
    {
        enemyName = "Тролль";
        health = 30;
        strength = 18;
         xpReward = 150;  
    }

    public override string Attack()
    {
        int d20 = Random.Range(1, 21);
        int total = d20 + StrengthModifier;
        return $"🧌 {enemyName} бьёт дубиной: {d20} + {StrengthModifier} = {total}";
    }
}