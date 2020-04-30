using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;


public enum InfoTextIcon
{
    Weapon, Armor, Helmet, Boots, Ring, Consumable, Money, Book, Gem
}

public interface IInfoTextable
{
    bool shouldShowInfoText { get; }
    InfoTextIcon infoTextIcon { get; }
    Vector3 infoTextWorldPosition { get; }
    string infoTextName { get; }

    void OnInfoTextClick();
}


public class InfoText : MonoBehaviour
{
    private readonly Vector3 GlobalWorldOffset = Vector3.up * 2f;

    private static SortedList<float, Rect> _drawnInfoTextRects = new SortedList<float, Rect>(new DuplicateKeyComparer<float>());

    [SerializeField, FoldoutGroup("Required References")]
    private Image _iconImage;
    [SerializeField, FoldoutGroup("Required References")]
    private Text _text;
    [SerializeField, FoldoutGroup("Required References")]
    private Sprite[] _iconSprites;

    private Camera _main;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private IInfoTextable _target;

    private void Awake()
    {
        _main = Camera.main;
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
    }

    public void Setup(IInfoTextable target)
    {
        _target = target;
        _iconImage.sprite = _iconSprites[(int)_target.infoTextIcon];
        _text.text = _target.infoTextName;
    }

    private void Update()
    {
        if(_drawnInfoTextRects.Count != 0) _drawnInfoTextRects.Clear();
    }

    private void LateUpdate() {
        if (_target as Object == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 viewportPoint = _main.WorldToViewportPoint(_target.infoTextWorldPosition + GlobalWorldOffset).Quantitized();
        if (!_target.shouldShowInfoText || viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1 || viewportPoint.z < 0)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;   
            return;
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;

        Vector3 screenPoint = _main.WorldToScreenPoint(_target.infoTextWorldPosition + GlobalWorldOffset).Quantitized();

        transform.position = screenPoint;

        Rect myRect = new Rect((Vector2)transform.position - _rectTransform.sizeDelta / 2f, _rectTransform.sizeDelta);
        for (int i = 0; i < _drawnInfoTextRects.Count; i++)
        {
            if (myRect.Overlaps(_drawnInfoTextRects.Values[i]))
            {
                transform.position += Vector3.up * (_drawnInfoTextRects.Values[i].yMax - myRect.yMin);
                myRect = new Rect((Vector2)transform.position - _rectTransform.sizeDelta / 2f, _rectTransform.sizeDelta);

            }
        }
        _drawnInfoTextRects.Add(myRect.y, myRect);
    }

    public void Click()
    {
        _target.OnInfoTextClick();
    }
}

public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : System.IComparable
{
    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
            return 1;   // Handle equality as beeing greater
        else
            return result;
    }
}