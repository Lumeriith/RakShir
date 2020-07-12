using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CastMethodNone : SingletonBehaviour<CastMethodNone>, ICastMethod
{
    [SerializeField]
    private Transform _circleSpriteTransform;

    private CastMethodData _data;


    private void Update()
    {
        _circleSpriteTransform.gameObject.SetActive(_data != null);
        if (_data != null)
        {
            _circleSpriteTransform.position = UnitControlManager.instance.selectedUnit.transform.position;
            _circleSpriteTransform.localScale = Vector3.one * _data.radius * 2f;
        }
    }

    void ICastMethod.StartCast(CastMethodData data, CastInputMethod inputMethod, KeyCode button, Action<CastInfo> onYield, Action onCancel)
    {
        onYield(new CastInfo { owner = UnitControlManager.instance.selectedUnit });
    }

    void ICastMethod.ShowPreviewIndicator(CastMethodData data) => _data = data;
    void ICastMethod.HidePreviewIndicator() => _data = null;

    void ICastMethod.OnCastConfirmKeyDown(Vector2 position) { }
    void ICastMethod.OnCastConfirmKeyUp(Vector2 position) { }
    void ICastMethod.CancelCast() { }
}
