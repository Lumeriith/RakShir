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
            decodedString += DecodeSyntaxElement(remainingString.Substring(1, endIndex - 1));
            remainingString = remainingString.Substring(endIndex + 1);



            startIndex = remainingString.IndexOf("[");
        }



        return decodedString + remainingString;
            
    }

    private static string DecodeSyntaxElement(string element)
    {
        try
        {
            string[] parameters = element.Split(' ');
            AbilityInstance ai;
            float amount;
            switch (parameters[0].ToUpper())
            {
                case "MAGIC":
                    ai = ((GameObject)Resources.Load(parameters[1])).GetComponent<AbilityInstance>();
                    amount = (float)ai.GetType().GetField(parameters[2]).GetValue(ai);
                    amount = amount * UnitControlManager.instance.selectedUnit.stat.finalSpellPower / 100f;
                    return prefixMagic + ((int)amount).ToString() + suffixMagic;
                case "MAGIC_IGNORE":
                    ai = ((GameObject)Resources.Load(parameters[1])).GetComponent<AbilityInstance>();
                    return prefixMagic + ((int)ai.GetType().GetField(parameters[2]).GetValue(ai)).ToString() + suffixMagic;
                case "VALUE":
                    ai = ((GameObject)Resources.Load(parameters[1])).GetComponent<AbilityInstance>();
                    return (string)ai.GetType().GetField(parameters[2]).GetValue(ai);
            }

            return "[" + element + "]";
        }
        catch
        {
            return "[" + element + "]";

        }

    }
}
