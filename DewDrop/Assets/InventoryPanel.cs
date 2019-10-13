    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InventoryPanel : MonoBehaviour
{
    private Transform layout;
    public GameObject itemInInventoryPrefab;
    public int count;
    private void Awake()
    {
        layout = transform.Find("Layout");
    }


    void Update()
    {
        
    }
}
