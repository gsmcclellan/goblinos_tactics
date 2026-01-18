using System;
using System.Collections.Generic;

namespace Goblinos.Scripts.Battle.Types;

public static class PrimaryActionInfo
{
    public static readonly IReadOnlyList<PrimaryActionType> PrimaryActionOrder = new[]
    {
        PrimaryActionType.Attack,
        PrimaryActionType.Ability,
        PrimaryActionType.Item,
        PrimaryActionType.Trade,
        PrimaryActionType.Wait
    };

    public static bool RequiresTarget(PrimaryActionType actionType)
    {
        return actionType switch
        {
            PrimaryActionType.Attack or PrimaryActionType.Trade => true,
            PrimaryActionType.Item or PrimaryActionType.Wait or PrimaryActionType.None => false,
            PrimaryActionType.Ability => true, // TODO - this depends on ability

            _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, "Unhandled PrimaryActionType.")
        };
    }
}