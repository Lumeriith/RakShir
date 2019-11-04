using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class Crow : MonoBehaviour
    {
        public float moveSpeed = 10f;

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
            float randomIdleSpeed = Random.Range(1f, 2f);

            animator.SetFloat("IdleSpeed", randomIdleSpeed);
        }

        private void OnTriggerEnter(Collider other)
        {
            LivingThing target = other.GetComponent<LivingThing>();
            if (target.type == LivingThingType.Player)
            {
                animator.SetBool("Fly", true);
                flyDirection = (transform.position - (target.transform.position + target.GetCenterOffset())).normalized;
                transform.forward = flyDirection;
                rb.velocity = transform.forward * moveSpeed;
                Destroy(gameObject, 5f);
            }
        }
           
    }
}
