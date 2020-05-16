using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ContextualMenu : MonoBehaviour
{
    private CanvasGroup _group;
    private GameObject _selection;

    private List<System.Action> callbacks = new List<System.Action>();

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        _selection = transform.Find("Selection").gameObject;
    }

    private void Update()
    {
        if((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) && !RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Input.mousePosition, GUIManager.instance.uiCamera))
        {
            Destroy(gameObject, 0.05f);
        }
    }

    private void OnDisable()
    {
        Destroy(gameObject);
    }

    public void SetSelections(params string[] list)
    {
        GameObject newSelection;
        for(int i = 0; i < list.Length; i++)
        {
            newSelection = Instantiate(_selection, transform);
            newSelection.transform.Find<Text>("Text").text = list[i];
            newSelection.GetComponent<ContextualMenuSelection>().index = i;
            newSelection.SetActive(true);
        }
    }

    public void SetCallbacks(params System.Action[] list)
    {
        callbacks.AddRange(list);
    }

    public void SelectionClicked(int index)
    {
        callbacks[index]();
        Destroy(gameObject);
    }
}
