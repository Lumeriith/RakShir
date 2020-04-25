using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class Spider : MonoBehaviour
    {
        public float moveSpeed = 10f;

        private Animator animator;
        private Rigidbody rb;
        private Vector3 runDirection;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            float randomIdleSpeed = Random.Range(0.5f, 1.5f);

            animator.SetFloat("IdleSpeed", randomIdleSpeed);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Entity>() == null) return;
            Entity target = other.GetComponent<Entity>();
            if (target.type != LivingThingType.Player) return;

            animator.SetBool("Run", true);
            runDirection = (transform.position - target.transform.position).normalized;
            transform.forward = runDirection;
            rb.velocity = transform.forward * moveSpeed;
            Destroy(gameObject, 3f);
        }

    }
}
