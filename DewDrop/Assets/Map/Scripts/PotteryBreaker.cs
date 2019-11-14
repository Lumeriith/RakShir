using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class PotteryBreaker : MonoBehaviour
    {
        public float explosionForce = 100f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<LivingThing>() == null) return;
            LivingThing target = other.GetComponent<LivingThing>();
            if (target.type != LivingThingType.Player) return;

            foreach (Transform child in transform)
            {
                MeshCollider childC = child.gameObject.AddComponent<MeshCollider>();
                childC.convex = true;
                childC.sharedMesh = child.GetComponent<MeshFilter>().sharedMesh;
                child.gameObject.AddComponent<Rigidbody>();
            }

            foreach (Transform child in transform)
            {
                Rigidbody childRb = child.GetComponent<Rigidbody>();
                childRb.AddExplosionForce(explosionForce, transform.position, 10f);
                child.parent = null;
            }
            Destroy(gameObject);
        }
    }
}
