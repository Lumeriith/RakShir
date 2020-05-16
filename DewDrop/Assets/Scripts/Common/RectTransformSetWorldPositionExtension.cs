using System;
using UnityEngine;

public static class RectTransformSetWorldPositionExtension
{
    /*
     * MIT License
     * 
     * Copyright (c) 2020 Eunseop Shim
     *
     * Permission is hereby granted, free of charge, to any person obtaining a copy
     * of this software and associated documentation files (the "Software"), to deal
     * in the Software without restriction, including without limitation the rights
     * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
     * copies of the Software, and to permit persons to whom the Software is
     * furnished to do so, subject to the following conditions:
     * 
     * The above copyright notice and this permission notice shall be included in all
     * copies or substantial portions of the Software.

     * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
     * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
     * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
     * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
     * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
     * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
     * SOFTWARE.
     */


    /* ================== Description =================== 
     * 
     * This extension provides a variety of functions for working with RectTransforms that need to look like they're at some world space points.
     * ex) Healthbars following enemies and players
     * 
     * It supports setting the world position for RectTransforms in canvas in Screen Space - Overlay, and Screen Space - Camera.
     * There are variants for each canvas render modes, including the fully automatic one (SetWorldPosition) that will do everything for you,
     * but it's not very performant so it's recommended to use other variants by using a cached referenced for their respective arguments.
     * 
     * ==================== Examples ====================
     * 
     * ((RectTransform)transform).SetWorldPosition(_monster.transform.position + Vector3.up * 2f);
     * 
     * _rectTransform.SetWorldPositionForScreenSpaceCamera(_target.transform.position + _worldOffset, _cachedParentCanvas);
     * 
     */

    /// <summary>
    /// Convert the given world position to this RectTransform's parent canvas space and sets this RectTransform's position to it. It's recommended to use the other SetWorldPosition variants for performance reasons.
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="worldPosition"></param>
    public static void SetWorldPosition(this RectTransform rt, Vector3 worldPosition)
    {
        Canvas canvas = rt.GetComponentInParent<Canvas>();
        if (canvas == null) throw new NullReferenceException("Cannot set the world position of a RectTransform without a parent canvas");
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            SetWorldPositionForScreenSpaceOverlay(rt, worldPosition, Camera.main);
        }
        else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            SetWorldPositionForScreenSpaceCamera(rt, worldPosition, canvas);
        }
        else
        {
            throw new InvalidOperationException("Cannot set the world position of a RectTransform in a World Space canvas");
        }
    }

    /// <summary>
    /// Convert the given world position to canvas space in Screen Space - Overlay and sets this RectTransform's position to it.
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="position"></param>
    /// <param name="camera"></param>
    public static void SetWorldPositionForScreenSpaceOverlay(this RectTransform rt, Vector3 position, Camera camera)
    {
        rt.position = camera.WorldToScreenPoint(position);
        rt.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Convert the given world position to canvas space in Screen Space - Camera and sets this RectTransform's position to it.
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="position"></param>
    /// <param name="canvas"></param>
    public static void SetWorldPositionForScreenSpaceCamera(this RectTransform rt, Vector3 position, Canvas canvas)
    {
        if (canvas.worldCamera == null) throw new NullReferenceException("Canvas in Screen Space - Camera doesn't have its world camera assigned");
        RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)canvas.transform, canvas.worldCamera.WorldToScreenPoint(position), canvas.worldCamera, out Vector3 worldPoint);
        rt.position = worldPoint;
        rt.rotation = canvas.transform.rotation;
    }
}
