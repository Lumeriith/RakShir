using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class Crow : MonoBehaviour
    {
        public float moveSpeed = 10f;
        public float upAngle = 10f;

        private Animator animator;
        private Rigidbody rb;
        private Vector3 flyDirection;

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

            animator.SetBool("Fly", true);
            flyDirection = (transform.position - target.transform.position + new Vector3(0, upAngle, 0)).normalized;
            transform.forward = flyDirection;
            rb.velocity = transform.forward * moveSpeed;
            Destroy(gameObject, 5f);
        }
           
    }
}
