using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class player : MonoBehaviour
{
    public float speed = 5.0f;
    public float jumpForce = 10.0f, turnAccelerationForce = 10.0f;
    public float jumpTime = 0.35f, turnAccTime = 0.35f;
    public float fireCooldown = 0.5f;
    public int maxHealth = 100, currentHealth, damage = 20;
    public HealthBar healthBar;

    private float fireCooldownTimer = 0f;
    private bool shootable = false;
    private Rigidbody2D rb;
    private PolygonCollider2D coll;
    private bool isJumping;
    private float jumpTimeCounter;
    private float left = -1, right = 1;
    private Animator animator;
    private string Level1 = "Level1", EndLevel = "End";

    [SerializeField] private LayerMask jumpground;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<PolygonCollider2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = rb.velocity.y;

        if (Input.GetButtonDown("Jump") && isGrounded())
        {
            GetComponent<AudioSource>().Play();
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.velocity = Vector2.up * jumpForce;
        }

        if (Input.GetButton("Jump") && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;

            }
        }

        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
        }

        float nVertical = NormalizeValue(verticalInput, -10f, 10f);

        if (fireCooldownTimer > 0)
        {
            fireCooldownTimer -= Time.deltaTime;

            if (horizontalInput < 0)
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
                transform.Translate(Vector2.left * 0);
            }
            else if (horizontalInput > 0)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                transform.Translate(Vector2.left * 0);
            }
        }

        if (Input.GetButtonDown("Fire1") && fireCooldownTimer <= 0)
        {
            shootable = true;
            fireCooldownTimer = fireCooldown;
        }

        if (fireCooldownTimer <= 0)
        {
            shootable = false;

            if (horizontalInput < 0)
            {

                transform.eulerAngles = new Vector3(0, 180, 0);

                if (Input.GetButton("Horizontal"))
                {
                    transform.Translate(Vector2.left * left * speed * Time.deltaTime);
                }
                else
                {
                    transform.Translate(Vector2.left * 0);
                }


            }
            else if (horizontalInput > 0)
            {

                transform.eulerAngles = new Vector3(0, 0, 0);

                if (Input.GetButton("Horizontal"))
                {
                    transform.Translate(Vector2.right * right * speed * Time.deltaTime);
                }
                else
                {
                    transform.Translate(Vector2.right * 0);
                }

            }
        }


        //Animator Communication
        animator.SetFloat("xVelocity", Mathf.Abs(horizontalInput));
        animator.SetFloat("yVelocity", nVertical);
        animator.SetBool("isGround", isGrounded());
        animator.SetBool("shootable", shootable);
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
    }

    float NormalizeValue(float value, float min, float max)
    {
        if (min == max) { return 0f; }
        return (value - min) / (max - min) * 2f - 1f;
    }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpground);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("portal"))
        {
            SceneManager.LoadScene(Level1);
        }
        if (collision.gameObject.CompareTag("endportal"))
        {
            SceneManager.LoadScene(EndLevel);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(damage);
            if (currentHealth <= 0f)
            {
                SceneManager.LoadScene(Level1);
            }
        }
        if (collision.gameObject.CompareTag("Traps"))
        {
            SceneManager.LoadScene(Level1);
        }
    }
}
