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
    public float randomPosMagnitude = 0.3f;
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
        worldPosition += Random.onUnitSphere * randomPosMagnitude;
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

        Vector3 viewportPoint = main.WorldToViewportPoint(worldPosition);
        if(viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1 || viewportPoint.z < 0)
        {
            uiText.enabled = false;
            return;
        }
        else
        {
            uiText.enabled = true;
        }


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
