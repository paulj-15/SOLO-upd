using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private Rigidbody projectileRB;

    private void Awake()
    {
        projectileRB = GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        float speed = 50f;
        projectileRB.velocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("projectile entered trigger Zone");
        Destroy(gameObject);
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Projectile collided {collision.gameObject.name}");
        Destroy(this.gameObject);
    }
}
