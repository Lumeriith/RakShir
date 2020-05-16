using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MaxWidth : MonoBehaviour
{

    public RectTransform textTransform;
    public LayoutElement layoutElement;

    private void OnEnable()
    {
        checkWidth();
    }

    private void Update()
    {
        checkWidth();
    }

    public void checkWidth()
    {
        layoutElement.enabled = (textTransform.rect.width + 60 > layoutElement.preferredWidth);
    }

}
