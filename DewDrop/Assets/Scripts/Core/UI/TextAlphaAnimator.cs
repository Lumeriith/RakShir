using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextAlphaAnimator : MonoBehaviour
{
    private Color textColor;
    private Text text;
    public Vector2 alphaRange;
    public float sineSpeed = 5f;
    private void Awake()
    {
        text = GetComponent<Text>();
        textColor = text.color;
    }

    void Update()
    {
        textColor.a = alphaRange.x  + (alphaRange.y - alphaRange.x) * (Mathf.Sin(Time.unscaledTime * sineSpeed) / 2 + 0.5f);
        text.color = textColor;
    }
}
