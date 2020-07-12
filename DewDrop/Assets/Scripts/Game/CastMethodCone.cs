using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CastMethodCone : SingletonBehaviour<CastMethodCone>, ICastMethod
{
    private bool _isCasting;
    private CastMethodData _data;
    private CastInputMethod _inputMethod;
    private KeyCode _keyCode;
    private Action<CastInfo> _onYield;
    private Action _onCancel;

    void ICastMethod.CancelCast()
    {
        _isCasting = false;
        _data = null;
        _onYield = null;
        _onCancel = null;
    }

    void ICastMethod.HidePreviewIndicator()
    {
        throw new NotImplementedException();
    }

    void ICastMethod.OnCastConfirmKeyDown(Vector2 position)
    {
        throw new NotImplementedException();
    }

    void ICastMethod.OnCastConfirmKeyUp(Vector2 position)
    {
        throw new NotImplementedException();
    }

    void ICastMethod.ShowPreviewIndicator(CastMethodData data)
    {
        _data = data;
    }

    void ICastMethod.StartCast(CastMethodData data, CastInputMethod inputMethod, KeyCode button, Action<CastInfo> onYield, Action onCancel)
    {
        _isCasting = true;
        _data = data;
        _inputMethod = inputMethod;
        _keyCode = button;
        _onYield = onYield;
        _onCancel = onCancel;
    }
}
