using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    //Movement
    [System.Serializable]
    public struct MovementSettings
    {
        public float speed;
        public float jumpForce;
        public float jumpTime;
        public float turnAccelerationForce;
        public float turnAccTime;
    }
    public MovementSettings movementSettings = new MovementSettings 
    {
        speed = 5.0f,
        jumpForce = 10.0f,
        jumpTime = 0.35f,
        turnAccelerationForce = 10.0f,
        turnAccTime = 0.35f
    };
    //Combat
    [System.Serializable]
    public struct CombatSettings
    {
        public float fireCooldown;
        public int damage;
    }
    public CombatSettings combatSettings = new CombatSettings 
    {
        fireCooldown = 0.5f,
        damage = 20
    };
    //Health
    [System.Serializable]
    public struct HealthSettings
    {
        public int maxHealth;
        public int currentHealth;
        public HealthBar healthBar;
    }
    public HealthSettings healthSettings = new HealthSettings 
    {
        maxHealth = 100,
        currentHealth = 100,
        healthBar = null // Assign in Inspector
    };

    // Timers and States
    private float fireCooldownTimer = 0f;
    private bool shootable = false;
    private bool isJumping;
    private float jumpTimeCounter;

    // References and Identifiers
    private Rigidbody2D rb;
    private PolygonCollider2D coll;
    private Animator animator;
    private string Level1 = "Level1";
    private string EndLevel = "End";
    [SerializeField] private LayerMask jumpground;

    // Constants
    private const float LEFT = -1f;
    private const float RIGHT = 1f;

    private Vector2 lastSavepointPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<PolygonCollider2D>();
        animator = GetComponent<Animator>();
        healthSettings.currentHealth = healthSettings.maxHealth;
        healthSettings.healthBar.SetMaxHealth(healthSettings.maxHealth);
        lastSavepointPosition = transform.position;
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = rb.velocity.y;

        if (Input.GetButtonDown("Jump") && isGrounded())
        {
            GetComponent<AudioSource>().Play();
            isJumping = true;
            jumpTimeCounter = movementSettings.jumpTime;
            rb.velocity = Vector2.up * movementSettings.jumpForce;
        }

        if (Input.GetButton("Jump") && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                rb.velocity = Vector2.up * movementSettings.jumpForce;
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
            fireCooldownTimer = combatSettings.fireCooldown;
        }

        if (fireCooldownTimer <= 0)
        {
            shootable = false;

            if (horizontalInput < 0)
            {
                transform.eulerAngles = new Vector3(0, 180, 0);

                if (Input.GetButton("Horizontal"))
                {
                    transform.Translate(Vector2.left * LEFT * movementSettings.speed * Time.deltaTime);
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
                    transform.Translate(Vector2.right * RIGHT * movementSettings.speed * Time.deltaTime);
                }
                else
                {
                    transform.Translate(Vector2.right * 0);
                }
            }
        }

        // Animator Communication
        animator.SetFloat("xVelocity", Mathf.Abs(horizontalInput));
        animator.SetFloat("yVelocity", nVertical);
        animator.SetBool("isGround", isGrounded());
        animator.SetBool("shootable", shootable);
    }

    void TakeDamage(int damage)
    {
        healthSettings.currentHealth -= damage;
        healthSettings.healthBar.SetHealth(healthSettings.currentHealth);

        if (healthSettings.currentHealth <= 0)
        {
            Respawn();
        }
    }
    void Respawn()
    {
        transform.position = lastSavepointPosition;
        healthSettings.currentHealth = healthSettings.maxHealth;
        healthSettings.healthBar.SetHealth(healthSettings.currentHealth);
        Debug.Log("Respawn");
    }

    float NormalizeValue(float value, float min, float max)
    {
        if (min == max) { return 0f; }
        return (value - min) / (max - min) * 2f - 1f;
    }

    // Ground Check
    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpground);
    }

    // Portal
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

    // Damage
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(combatSettings.damage);
            if (healthSettings.currentHealth <= 0f)
            {
                Respawn();
            }
        }
        if (collision.gameObject.CompareTag("Traps"))
        {
            Respawn();
        }
    }

    // Set Save Point
    public void SetSavepoint(Vector2 position)
    {
        lastSavepointPosition = position;
    }
}
