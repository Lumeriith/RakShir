using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FloatingText : MonoBehaviour
{
    public AnimationCurve sizeCurve;
    public Gradient colourCurve;
    public float duration = 1f;
    public Vector3 gravity;
    public Vector3 initialVelocity;

    private Vector3 _worldPosition;
    private Vector3 _velocity;

    private float _creationTime;
    private Text _uiText;
    private Outline _outline;
    private Shadow _shadow;
    private Camera _main;
    private float _elapsedTime
    {
        get
        {
            return Time.time - _creationTime;
        }
    }

    private void Awake()
    {
        _uiText = GetComponentInChildren<Text>();
        _outline = GetComponentInChildren<Outline>();
        _shadow = GetComponentInChildren<Shadow>();
        _main = Camera.main;
    }

    private void OnEnable()
    {
        _creationTime = Time.time;
        _velocity = initialVelocity;
    }

    public void Setup(Vector3 worldPosition)
    {
        _worldPosition = worldPosition;
        UpdateFloatingText();
    }

    public void Setup(Vector3 worldPosition, string text)
    {
        _worldPosition = worldPosition;
        _uiText.text = text;
        UpdateFloatingText();
    }

    private void Update()
    {
        if(_elapsedTime > duration)
        {
            PoolManager.Despawn(gameObject);
            return;
        }

        UpdateFloatingText();
    }


    private void UpdateFloatingText()
    {
        _worldPosition += _velocity * Time.deltaTime;
        _velocity += gravity * Time.deltaTime;

        Vector3 viewportPoint = _main.WorldToViewportPoint(_worldPosition);
        if (viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1 || viewportPoint.z < 0)
        {
            _uiText.enabled = false;
            return;
        }
        else
        {
            _uiText.enabled = true;
        }


        transform.position = _main.WorldToScreenPoint(_worldPosition);

        transform.localScale = Vector3.one * sizeCurve.Evaluate(_elapsedTime / duration);

        Color color = colourCurve.Evaluate(_elapsedTime / duration);
        _uiText.color = color;
        color.a = 1f;
        color.r /= 4f;
        color.g /= 4f;
        color.b /= 4f;
        _outline.effectColor = color;
        _shadow.effectColor = color;
    }



}
