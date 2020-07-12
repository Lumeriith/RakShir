using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class CastMethod : MonoBehaviour, ICastMethod
{
    protected enum CastMethodState { None, Casting, Previewing }

    protected CastMethodState _state;

    protected CastMethodData _data;
    protected CastInputMethod _inputMethod;
    protected KeyCode _button;
    
    private Action<CastInfo> _onYield;
    private Action _onCancel;

    protected abstract void OnCastConfirmKeyDown(Vector2 position);
    protected abstract void OnCastConfirmKeyUp(Vector2 position);
    
    void ICastMethod.OnCastConfirmKeyDown(Vector2 position) => OnCastConfirmKeyDown(position);
    void ICastMethod.OnCastConfirmKeyUp(Vector2 position) => OnCastConfirmKeyUp(position);

    void ICastMethod.StartCast(CastMethodData data, CastInputMethod inputMethod, KeyCode button, Action<CastInfo> onYield, Action onCancel)
    {
        _state = CastMethodState.Casting;
        _data = data;
        _inputMethod = inputMethod;
        _button = button;
        _onYield = onYield;
        _onCancel = onCancel;
    }

    void ICastMethod.CancelCast()
    {
        _state = CastMethodState.None;
        _data = null;
        _onYield = null;
        _onCancel = null;
    }

    void ICastMethod.ShowPreviewIndicator(CastMethodData data)
    {
        _state = CastMethodState.Previewing;
        _data = data;
    }

    void ICastMethod.HidePreviewIndicator()
    {
        _state = CastMethodState.None;
        _data = null;
    }
}
