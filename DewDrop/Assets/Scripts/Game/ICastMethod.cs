using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ICastMethod
{
    void StartCast(CastMethodData data, CastInputMethod inputMethod, KeyCode button, Action<CastInfo> onYield, Action onCancel);
    void CancelCast();
    void ShowPreviewIndicator(CastMethodData data);
    void HidePreviewIndicator();
    void OnCastConfirmKeyDown(Vector2 position);
    void OnCastConfirmKeyUp(Vector2 position);
}
