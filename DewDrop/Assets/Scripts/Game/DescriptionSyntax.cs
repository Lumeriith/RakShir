using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DescriptionSyntax
{
    const string prefixMagic = "<b><color=magenta>";
    const string suffixMagic = "</color></b>";
    public static string Decode(string raw)
    {
        string remainingString = raw;
        string decodedString = "";
        string description = "";
        string totalDescription = "";
        int startIndex, endIndex;

        startIndex = raw.IndexOf("[");
        
        while(startIndex != -1)
        {

            decodedString += remainingString.Substring(0, startIndex);
            remainingString = remainingString.Substring(startIndex);

            endIndex = remainingString.IndexOf("]");
            if (endIndex == -1)
            {
                break;
            }
            description = "";
            decodedString += DecodeSyntaxElement(remainingString.Substring(1, endIndex - 1), ref description);
            totalDescription += description;
            remainingString = remainingString.Substring(endIndex + 1);



            startIndex = remainingString.IndexOf("[");
        }

        if(totalDescription == "")
        {
            return decodedString + remainingString;
        }
        else
        {
            return decodedString + remainingString + "<size=13>" + totalDescription + "</size>";
        }

        
            
    }

    private static string DecodeSyntaxElement(string element, ref string description)
    {
        try
        {
            if (element == "민첩") description = "\n\n<color=yellow>민첩</color> <color=grey>이동 속도, 공격 속도, 회피가 증가합니다.</color>";
            else if (element == "힘") description = "\n\n<color=yellow>힘</color> <color=grey>최대 체력, 초당 체력 재생, 공격 피해가 증가합니다.</color>";
            else if (element == "지능") description = "\n\n<color=yellow>지능</color> <color=grey>최대 마나, 초당 마나 재생, 주문력, 시간 왜곡이 증가합니다.</color>";
            else if (element == "주문력") description = "\n\n<color=yellow>주문력</color> <color=grey>자신이 주는 대부분의 마법 피해량, 치유량, 보호막량이 주문력의 영향을 받아 더 강력해집니다. 예를 들어 주문력이 150이면 대부분의 마법 피해가 50%의 추가 피해를 줍니다.</color>";
            else if (element == "시간 왜곡") description = "\n\n<color=yellow>시간 왜곡</color> <color=grey>자신의 마법들의 재사용 대기시간이 시간 왜곡의 영향을 받아 더 빠르게 감소합니다. 예를 들어 시간 왜곡이 100이면 내 마법들의 재사용 대기시간이 100% 더 빠르게 감소합니다.</color>";
            else if (element == "회피") description = "\n\n<color=yellow>회피</color> <color=grey>자신에게 가해지는 기본 공격을 피하여 무효화시킬 수 있습니다. 예를 들어 회피가 20이면 20%의 확률로 기본 공격을 회피합니다.</color>";
            else if (element == "저지 불가") description = "\n\n<color=yellow>저지 불가</color> <color=grey>저지 불가인 대상은 기절, 침묵, 밀치기 등의 방해 효과에 영향을 받지 않습니다.</color>";
            else if (element == "침묵") description = "\n\n<color=yellow>침묵</color> <color=grey>침묵된 대상은 대부분의 마법을 사용할 수 없습니다.</color>";
            else if (element == "실명") description = "\n\n<color=yellow>실명</color> <color=grey>실명된 대상이 행하는 모든 기본 공격은 빗나갑니다.</color>";
            else if (element == "보호") description = "\n\n<color=yellow>보호</color> <color=grey>보호된 대상은 그 어떤 피해도 입지 않습니다.</color>";
            else if (element == "무적") description = "\n\n<color=yellow>무적</color> <color=grey>무적인 대상은 그 어떤 피해와 해로운 효과에도 영향을 받지 않습니다.</color>";
            

            return "<color=yellow>" + element + "</color>";
        }
        catch
        {
            return "[" + element + "]";

        }

    }
}
