using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
public class GameManager : MonoBehaviour
{
    public Texture2D normalCursor;
    public Vector2 normalCursorHotspot;
    public Texture2D attackCursor;
    public Vector2 attackCursorHotspot;
    
    public enum CursorShapeType { None, Normal, Attack, AbilityInstance }
    public CursorShapeType cursorShape = CursorShapeType.Normal;
    private CursorShapeType lastCursorShape;

    public LivingThing localPlayer
    {
        get
        {
            return _localPlayer;
        }
        set
        {
            _localPlayer = value;
            _localPlayer.OnDoBasicAttackHit = (InfoBasicAttackHit info) =>
            {
                Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();
                GameObject floatingText = Instantiate(physicalDamageFloatingText.gameObject, worldPos, Quaternion.identity);
                floatingText.SetActive(true);
                floatingText.GetComponent<FloatingText>().text = Mathf.Ceil(info.damage).ToString();
                
            };

            _localPlayer.OnDealMagicDamage = (InfoMagicDamage info) =>
            {
                Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();
                GameObject floatingText = Instantiate(magicDamageFloatingText.gameObject, worldPos, Quaternion.identity);
                floatingText.SetActive(true);
                floatingText.GetComponent<FloatingText>().text = Mathf.Ceil(info.finalDamage).ToString();
                
            };
        }
    }

    private LivingThing _localPlayer;


    public FloatingText magicDamageFloatingText;
    public FloatingText physicalDamageFloatingText;

    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        magicDamageFloatingText.gameObject.SetActive(false);
        physicalDamageFloatingText.gameObject.SetActive(false);
     }

    private void Update()
    {
        if(lastCursorShape != cursorShape)
        {
            SetAppropriateCursor();
            lastCursorShape = cursorShape;
        }

    }

    private void SetAppropriateCursor()
    {
        switch (cursorShape)
        {
            case CursorShapeType.None:
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                break;
            case CursorShapeType.Normal:
                Cursor.SetCursor(normalCursor, normalCursorHotspot, CursorMode.Auto);
                break;
            case CursorShapeType.Attack:
                Cursor.SetCursor(attackCursor, attackCursorHotspot, CursorMode.Auto);
                break;
            case CursorShapeType.AbilityInstance:
                Cursor.SetCursor(attackCursor, attackCursorHotspot, CursorMode.Auto);
                break;
        }
    }

    

}
