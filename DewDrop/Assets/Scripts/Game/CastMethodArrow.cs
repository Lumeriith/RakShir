using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastMethodArrow : SingletonBehaviour<CastMethodArrow>, ICastMethod
{
    public void CancelCast()
    {
        throw new NotImplementedException();
    }

    public void ShowPreviewIndicator(bool show)
    {
        throw new NotImplementedException();
    }

    public void StartCast(CastMethodData data, CastInputMethod inputMethod, KeyCode button, Action<CastInfo> onYield, Action onCancel)
    {
        throw new NotImplementedException();
    }

    void ICastMethod.CancelCast()
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    void ICastMethod.StartCast(CastMethodData data, CastInputMethod inputMethod, KeyCode button, Action<CastInfo> onYield, Action onCancel)
    {
        throw new NotImplementedException();
    }
}
