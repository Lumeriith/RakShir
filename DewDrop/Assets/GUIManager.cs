using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GUIManager : SingletonBehaviour<GUIManager>
{

    [SerializeField, FoldoutGroup("Required References")]
    private Camera _uiCamera;

    public Camera uiCamera => _uiCamera;


}
