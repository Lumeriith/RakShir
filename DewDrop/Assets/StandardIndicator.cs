using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public enum StandardIndicatorType { Cone, Arrow }
public enum StandardIndicatorPlacementType { Player, Point, ClampedPoint, Target }

public class StandardIndicator : MonoBehaviour, IIndicator
{
    public StandardIndicatorType type = StandardIndicatorType.Cone;
    public StandardIndicatorPlacementType from = StandardIndicatorPlacementType.Player;
    [ShowIf("isFromClampedPoint")]
    public float fromClampRange = 0f;
    public StandardIndicatorPlacementType to = StandardIndicatorPlacementType.Point;
    [ShowIf("isToClampedPoint")]
    public float toClampRange = 0f;
    public float directionOffsetAngle = 0f;

    [ShowIf("isTypeCone")]
    [Header("Cone Settings")]
    public float coneRadius = 5f;
    [ShowIf("isTypeCone")]
    public float coneAngle = 360f;

    [ShowIf("isTypeArrow")]
    [Header("Arrow Settings")]
    public float arrowLength = 5f;
    [ShowIf("isTypeArrow")]
    public float arrowWidth = 1f;




    private bool isTypeCone { get { return type == StandardIndicatorType.Cone; } }
    private bool isTypeArrow { get { return type == StandardIndicatorType.Arrow; } }
    private bool isFromClampedPoint { get { return from == StandardIndicatorPlacementType.ClampedPoint; } }
    private bool isToClampedPoint { get { return to == StandardIndicatorPlacementType.ClampedPoint; } }

    public void Indicate()
    {

    }
}
