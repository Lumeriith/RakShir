using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FloatingText : MonoBehaviour
{
    public float duration = 2;
    private Vector3 velocity;
    public Vector3 initialVelocity;
    public Vector3 gravity;
    public float randomnessMagnitude;
    private Transform uiElement;
    private CanvasGroup group;
    private Text uiText;
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
    void Awake()
    {
        group = transform.Find("UI Element").GetComponent<CanvasGroup>();
        uiText = transform.Find("UI Element/Text").GetComponent<Text>();
        uiElement = transform.Find("UI Element").transform;
        
    }

  


    void Start()
    {
        uiElement.SetParent(transform.Find("/Floating Text Canvas")); // Fix
        StartCoroutine("CoroutineDisappear");
    }

    IEnumerator CoroutineDisappear()
    {
        velocity = initialVelocity + Vector3.one * ((Random.value-1) * randomnessMagnitude);
        for (float t=0;t<duration;t += Time.deltaTime)
        {
            velocity += gravity * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            group.alpha = (1f - t / duration) * 3;
            uiElement.position = Camera.main.WorldToScreenPoint(transform.position);
            yield return null;
        }

        Destroy(gameObject);
        Destroy(uiElement.gameObject);
    }
}
