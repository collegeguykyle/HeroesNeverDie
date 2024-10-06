using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Example class for HitResult, can be extended with other tooltip types
public class KW_Hit : IToolTipKeyWord
{
    public string Keyword => "hit";
    public string TextColor => "#FF0000";  // Red color

    public int damage;
    public string statusEffect;
    public bool isCritical;

    public KW_Hit(int damage, string statusEffect, bool isCritical)
    {
        this.damage = damage;
        this.statusEffect = statusEffect;
    }

    // Text that will appear in the tool tip
    public string GetTooltipText()
    {
        return $"Damage: {damage}\n" +
            $"Status: {statusEffect}\n" +
            $"Critical Hit: {isCritical}";
    }

}
