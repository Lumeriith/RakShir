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
            return "<color=yellow>" + element + "</color>";
        }
        catch
        {
            return "[" + element + "]";

        }

    }
}
