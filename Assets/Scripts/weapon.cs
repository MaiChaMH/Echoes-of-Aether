using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weapon : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float fireCooldown = 0.5f;

    private float fireCooldownTimer = 0f;

    // Update is called once per frame
    void Update()
    {
        if (fireCooldownTimer > 0)
        {
            fireCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Fire1")  && fireCooldownTimer <= 0)
        {
            Shoot();
            fireCooldownTimer = fireCooldown;
        }
    }
    void Shoot()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}
