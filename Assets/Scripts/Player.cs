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
        healthBar = null
    };

    // Timers and States
    private float fireCooldownTimer = 0f;
    private bool isJumping;
    private float jumpTimeCounter;
    private bool isGravityFlipped = false;

    // References and Identifiers
    private Rigidbody2D rb;
    private PolygonCollider2D coll;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
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
        spriteRenderer = GetComponent<SpriteRenderer>();
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
            Vector2 jumpDirection = isGravityFlipped ? Vector2.down : Vector2.up;
            rb.velocity = jumpDirection * movementSettings.jumpForce;
        }

        if (Input.GetButton("Jump") && isJumping)
        {
            if (jumpTimeCounter <= 0)
            {
                isJumping = false;
            }
            else
            {
                Vector2 jumpDirection = isGravityFlipped ? Vector2.down : Vector2.up;
                rb.velocity = jumpDirection * movementSettings.jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            }
        }

        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
        }

        if (Input.GetButtonDown("Flip") && isGrounded())
        {
            FlipGravity();
        }

        float nVertical = NormalizeValue(verticalInput, -10f, 10f);

        if (fireCooldownTimer > 0)
        {
            fireCooldownTimer -= Time.deltaTime;
            CharacterDS(horizontalInput);
        }

        if (Input.GetButtonDown("Fire1") && fireCooldownTimer <= 0)
        {
            animator.SetTrigger("Fire");
            fireCooldownTimer = combatSettings.fireCooldown;
        }
       

        if (fireCooldownTimer <= 0)
        {
            CharacterDS(horizontalInput);
        }

        // Animator Communication
        animator.SetFloat("xVelocity", Mathf.Abs(horizontalInput));
        animator.SetFloat("yVelocity", nVertical);
        animator.SetBool("isGround", isGrounded());
    }
    
    //Flip Gravity
    void FlipGravity()
    {
        spriteRenderer.flipY = !spriteRenderer.flipY;
        isGravityFlipped = !isGravityFlipped;
        rb.gravityScale *= -1;
        transform.rotation = Quaternion.identity;
        //transform.Rotate(180f, 0f, 0f);
    }
    
    //Character Direction/Speed Logic
    void CharacterDS(float horizontalInput)
    {
        if (horizontalInput < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
            
            if (!Input.GetButton("Horizontal")) return;
            transform.Translate(Vector2.left * LEFT * movementSettings.speed * Time.deltaTime);
        }
        else if (horizontalInput > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);

            if (!Input.GetButton("Horizontal")) return;
            transform.Translate(Vector2.right * RIGHT * movementSettings.speed * Time.deltaTime);
        }
    }

    //Taking Damage
    void TakeDamage(int damage)
    {
        healthSettings.currentHealth -= damage;
        healthSettings.healthBar.SetHealth(healthSettings.currentHealth);

        if (healthSettings.currentHealth > 0) return;
        Respawn();
    }
    
    //Respawn
    void Respawn()
    {
        transform.position = lastSavepointPosition;
        healthSettings.currentHealth = healthSettings.maxHealth;
        healthSettings.healthBar.SetHealth(healthSettings.currentHealth);
        if (isGravityFlipped)
        {
          FlipGravity();  
        }
        //isGravityFlipped = false;
        //rb.gravityScale = Mathf.Abs(rb.gravityScale);
        //transform.rotation = Quaternion.identity;
        Debug.Log("Respawn");
    }

    //NormalizeValue
    float NormalizeValue(float value, float min, float max)
    {
        if (min == max) { return 0f; }
        return (value - min) / (max - min) * 2f - 1f;
    }

    // Ground Check
    private bool isGrounded()
    {
        Vector2 groundDirection = isGravityFlipped ? Vector2.up : Vector2.down;
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, groundDirection, .1f, jumpground);
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
            return;
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
