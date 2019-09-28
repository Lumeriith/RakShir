using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [Header("Item Type")]
    public string itemType;

    /// <summary>
    /// 아이템이 장착됐을 때 효과
    /// 스탯의 증가, 플레이어 외형의 변경
    /// </summary>
    public abstract void SetItem();

    /// <summary>
    /// 아이템이 해제됐을 때 효과
    /// 스탯의 복구, 플레이어 외형 복구
    /// </summary>
    public abstract void PopItem();
}
