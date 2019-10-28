using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FloatingText : MonoBehaviour
{
    public AnimationCurve sizeCurve;
    public Gradient colourCurve;
    public float duration = 1f;
    [HideInInspector]
    public Vector3 worldPosition;
    public Vector3 gravity;
    public Vector3 initialVelocity;
    private Vector3 velocity;
    public string text
    {
        get
        {
            return uiText.text;
        }
        set
        {
            uiText.text = value;
        }
    }

    private float creationTime;
    private Text uiText;
    private Outline outline;
    private Shadow shadow;
    private Camera main;
    private float elapsedTime
    {
        get
        {
            return Time.time - creationTime;
        }
    }

    private void Awake()
    {
        creationTime = Time.time;
        uiText = GetComponentInChildren<Text>();
        outline = GetComponentInChildren<Outline>();
        shadow = GetComponentInChildren<Shadow>();
        velocity = initialVelocity;
        main = Camera.main;
    }
    private void Start()
    {
        Update();
    }

    private void Update()
    {
        if(elapsedTime > duration)
        {
            Destroy(gameObject);
            return;
        }

        worldPosition += velocity * Time.deltaTime;
        velocity += gravity * Time.deltaTime;

        transform.position = main.WorldToScreenPoint(worldPosition);

        transform.localScale = Vector3.one * sizeCurve.Evaluate(elapsedTime / duration);
        
        Color color = colourCurve.Evaluate(elapsedTime / duration);
        uiText.color = color;
        color.a = 1f;
        color.r /= 4f;
        color.g /= 4f;
        color.b /= 4f;
        outline.effectColor = color;
        shadow.effectColor = color;
    }



}
