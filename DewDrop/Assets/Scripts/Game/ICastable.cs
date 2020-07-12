using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICastable
{
    CastMethodData castMethod { get; }

    void Cast(CastInfo info);

    /// <summary>
    /// Is the castable ready? Should the cast button be enabled? (Is it cooled down? Does the owner have enough mana? Does SelfValidator evaluates true? Does CanBeCast() return true?)
    /// </summary>
    /// <returns></returns>
    bool IsReady();

    /// <summary>
    /// Is the cast valid with the given info? (Does TargetValidator evaluates true?)
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    bool IsCastValid(CastInfo info);
}
