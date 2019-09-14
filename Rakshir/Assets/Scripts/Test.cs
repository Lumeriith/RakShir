using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Rigidbody rb;
    public float moveSpeed;

    private void Update()
    {
        rb.AddForce(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * moveSpeed);
    }
}
