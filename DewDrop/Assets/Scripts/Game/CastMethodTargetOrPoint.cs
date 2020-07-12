using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastMethodTargetOrPoint : SingletonBehaviour<CastMethodTarget>, ICastMethod
{
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
