using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class DoorTrg : MonoBehaviour
    {
        public Vector3 startPosition;
        public Vector3 endPosition;
        public float speedFactor;

        private Collider coll;

        private void Awake()
        {
            transform.position = startPosition;
            coll = GetComponent<Collider>();
            coll.enabled = false;
        }

        /*
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine("Active");
            }
        }
        */

        public IEnumerator Active()
        {
            while (true)
            {
                transform.position = Vector3.Lerp(transform.position, endPosition, Time.deltaTime * speedFactor);

                if (Vector3.Distance(startPosition, endPosition) <= float.Epsilon)
                {
                    coll.enabled = true;
                    yield break;
                }
                yield return null;
            }
        }
    }
}
