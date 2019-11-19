using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class DoorTrg : MonoBehaviour
    {
        public float dissolveTime = 3f;
        public Material mat;

        private Collider coll;
        private float deltaDissolveTime = 0;

        private void Awake()
        {
            coll = GetComponent<Collider>();
            ResetDoor();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine("Active");
            }
        }

        public void ResetDoor()
        {
            mat.SetFloat("_DissolveCutoff", 1);
            coll.enabled = false;
        }

        public IEnumerator Active()
        {
            coll.enabled = true;
            while (true)
            {
                if (deltaDissolveTime >= dissolveTime)
                {
                    mat.SetFloat("_DissolveCutoff", 0);
                    yield break;
                }

                mat.SetFloat("_DissolveCutoff", Mathf.Clamp01(1 - (deltaDissolveTime / dissolveTime)));
                deltaDissolveTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}
