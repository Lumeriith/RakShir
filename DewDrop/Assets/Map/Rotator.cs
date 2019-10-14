using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Juhwan
{
    public class Rotator : MonoBehaviour
    {
        public float rotateSpeed = 30f;
        public enum RotateDirection { X, Y, Z };
        public RotateDirection rotateDirection;

        private void Start()
        {
            StartCoroutine("Rotate");
        }

        private IEnumerator Rotate()
        {
            while (true)
            {
                switch (rotateDirection)
                {
                    case RotateDirection.X:
                        transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime);
                        break;
                    case RotateDirection.Y:
                        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
                        break;
                    case RotateDirection.Z:
                        transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
                        break;
                }

                yield return null;
            }
        }
    }

}
