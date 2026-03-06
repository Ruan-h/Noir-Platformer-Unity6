using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour
{
    // --- MÁQUINA DE ESTADOS ---
    private enum State
    {
        Normal,
        Dashing
    }
    private State currentState;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private float moveInput;
    private bool isFacingRight = true;

    [Header("Jump")]
    public float jumpForce = 7f;
    [Range(0, 1)] public float jumpCutMultiplier = 0.5f; 
    public float fallGravityMultiplier = 2.5f; 
    
    [Space(5)]
    public float coyoteTime = 0.1f; 
    private float coyoteTimeCounter; 

    [Space(5)]
    public float jumpBufferTime = 0.2f; 
    private float jumpBufferCounter; 

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;
    
    [Header("Attack Settings")]
    public Transform attackPoint;
    public Vector2 attackSize = new Vector2(1.5f, 0.8f); 
    public LayerMask enemyLayer;
    private bool isBackstabAvailable = false;
    
    [Header("Audio System")]
    [SerializeField] private PlayerAudio playerAudio; 

    [Tooltip("Tempo em segundos que o comando fica na memória. 0.2s é o padrão para deixar fluido.")]
    public float attackBufferTime = 0.2f; 
    private float bufferTimer;

    [Header("Charge Settings (Dash Ammo)")]
    public int maxCharges = 3;
    [SerializeField] private int currentCharges = 0;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private float lastDashTime = -10f;
    private float originalGravity; 

    [Header("VFX Settings")]
    public ParticleSystem dashBurstEffect;
    public TrailRenderer dashTrail;
    public ParticleSystem dashParticles;
    public ParticleSystem backstabVFX;
    
    private Animator anim;
    private string currentAnimationState;
    private Collider2D playerCollider;

    const string PLAYER_IDLE = "Idle";
    const string PLAYER_WALKING = "Walking";
    
    [Header("Self Destruct")]
    [SerializeField] private float timeToSuicide = 2.0f;
    private float suicideTimer = 0f;
    private SpriteRenderer spriteRenderer;
    
    public bool isDead = false;

    void Awake()
    {
         spriteRenderer = GetComponentInChildren<SpriteRenderer>();
         
         if (playerAudio == null) playerAudio = GetComponent<PlayerAudio>();
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        
        originalGravity = rb.gravityScale;
        currentState = State.Normal;
    }

    void Update()
    {
        if (isDead) return;

        HandleBackstabVFX();

        switch (currentState)
        {
            case State.Normal:
                HandleInputNormal();
                break;
            
            case State.Dashing:
                break;
        }
        HandleSelfDestructInput();
        UpdateAnimationState();
    }

    void FixedUpdate()
    {   
        if (isDead) return;
        switch (currentState)
        {
            case State.Normal:
                HandleMovementNormal();
                break;
            
            case State.Dashing:
                break;
        }
    }

    private void HandleBackstabVFX()
    {
        if (currentState != State.Normal)
        {
            if (backstabVFX != null && backstabVFX.isPlaying) backstabVFX.Stop();
            return;
        }

        isBackstabAvailable = CheckBackstabAvailable();

        if (backstabVFX != null)
        {
            if (isBackstabAvailable && !backstabVFX.isPlaying)
            {
                backstabVFX.Play(); 
            }
            else if (!isBackstabAvailable && backstabVFX.isPlaying)
            {
                backstabVFX.Stop(); 
            }
        }
    }

    // --- FUNÇÃO DE CHECAGEM COM CORREÇÃO DO OBSER ---
    private bool CheckBackstabAvailable()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackSize, 0f, enemyLayer);
        
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Enemy target = enemyCollider.GetComponent<Enemy>();
            if (target != null)
            {
                // Regra 1: Tem que estar olhando para a mesma direção
                if (this.isFacingRight == target.isFacingRight)
                {
                    // Regra 2: Não pode estar alerta, EXCETO se for o Obser
                    if (!target.IsAlert || target is Obser)
                    {
                        return true; 
                    }
                }
            }
        }
        return false; 
    }
    // ---------------------------------------------------


    private void HandleInputNormal()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space)) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (playerAudio != null) playerAudio.PlayJump();
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f; 
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            coyoteTimeCounter = 0f; 
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            bufferTimer = attackBufferTime;

            if (Attack()) 
            {
                if (playerAudio != null) playerAudio.PlayKillConfirm();
                bufferTimer = 0f; 
            }
        }

        if (bufferTimer > 0)
        {
            if (Attack()) 
            {
                if (playerAudio != null) playerAudio.PlayKillConfirm();
                bufferTimer = 0f; 
            }
            else 
            {
                bufferTimer -= Mathf.Min(Time.deltaTime, 0.06f);
            }
        }

        if (Input.GetKeyDown(KeyCode.Z)) TryDash();
    }
    
    private void HandleMovementNormal()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput < 0 && isFacingRight) Flip();
        else if (moveInput > 0 && !isFacingRight) Flip();

        if (rb.linearVelocity.y < 0) rb.gravityScale = originalGravity * fallGravityMultiplier;
        else rb.gravityScale = originalGravity;
    }
    
    // --- FUNÇÃO ATTACK: PRIORIDADE + FIX OBSER ---
    private bool Attack()
    {
         Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackSize, 0f, enemyLayer);
         
         Enemy targetInFront = null;
         Enemy targetBehind = null;

         // 1. VARREDURA E FILTRO
         foreach (Collider2D enemyCollider in hitEnemies)
         {
             Enemy target = enemyCollider.GetComponent<Enemy>();

             if (target != null)
             {
                 // Regra 1: Mesmo lado (Costas)
                 if (this.isFacingRight == target.isFacingRight)
                 {
                     // Regra 2: Alerta? (Com exceção para Obser)
                     if (!target.IsAlert || target is Obser)
                     {
                         // Classifica: Frente ou Trás?
                         bool isFront = false;

                         if (isFacingRight)
                         {
                             // Olhando p/ direita: X do inimigo > Meu X
                             if (target.transform.position.x > transform.position.x) isFront = true;
                         }
                         else
                         {
                             // Olhando p/ esquerda: X do inimigo < Meu X
                             if (target.transform.position.x < transform.position.x) isFront = true;
                         }

                         if (isFront) targetInFront = target;
                         else targetBehind = target;
                     }
                 }
             }
         }

         // 2. SELEÇÃO (Prioridade para Frente)
         Enemy finalTarget = null;
         if (targetInFront != null) finalTarget = targetInFront;
         else if (targetBehind != null) finalTarget = targetBehind;

         // 3. EXECUÇÃO
         if (finalTarget != null)
         {
             Vector3 enemyPosition = finalTarget.transform.position;

             finalTarget.Die();
             
 
             transform.position = new Vector3(enemyPosition.x, enemyPosition.y + 1f, transform.position.z);
             
             if (rb != null) rb.linearVelocity = Vector2.zero;

             GainCharge();
             return true; 
         }
         
         return false;
    }

    private void GainCharge()
    {
        if (currentCharges < maxCharges) currentCharges++;
    }
    
    public int GetCharges()
    {
        return currentCharges;
    }

    private void TryDash()
    {
        if (currentCharges > 0 && Time.time >= lastDashTime + dashCooldown && currentState == State.Normal)
        {
            currentCharges--;
            StartCoroutine(DashSequence());
        }
        else if(currentCharges <= 0) Debug.Log("Sem cargas de Dash!");
    }
    
    public void ResetCharges()
    {
         currentCharges = 0;
    }

    private IEnumerator DashSequence()
    {
         currentState = State.Dashing;
         lastDashTime = Time.time;
         rb.gravityScale = 0f; 
         
         if (playerAudio != null) playerAudio.PlayDash();

         float dashDirection = isFacingRight ? 1f : -1f;

         int originalLayer = gameObject.layer; 
         gameObject.layer = LayerMask.NameToLayer("Invulnerable");

         float dashDistance = dashSpeed * dashDuration;
         
         RaycastHit2D[] hits = Physics2D.BoxCastAll(playerCollider.bounds.center, playerCollider.bounds.size, 0f, new Vector2(dashDirection, 0), dashDistance, enemyLayer);

         foreach (var hit in hits) {
             Enemy enemy = hit.collider.GetComponent<Enemy>();
             if (enemy != null) enemy.Die();
         }

         rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

         if (dashBurstEffect != null) {
             float burstRotationY = isFacingRight ? -90f : 90f;
             dashBurstEffect.transform.rotation = Quaternion.Euler(0, burstRotationY, 0);
             dashBurstEffect.Play();
         }
         if (dashTrail != null) dashTrail.emitting = true;
         if (dashParticles != null) dashParticles.Play();

         yield return new WaitForSeconds(dashDuration);

         rb.linearVelocity = Vector2.zero;
         if (dashTrail != null) dashTrail.emitting = false;
         rb.gravityScale = originalGravity; 
         
         gameObject.layer = originalLayer;

         currentState = State.Normal;
    }

    private void UpdateAnimationState()
    {
        string newState;
        if (currentState == State.Dashing) return; 
        if (moveInput != 0) newState = PLAYER_WALKING;
        else newState = PLAYER_IDLE;

        if (currentAnimationState != newState) {
            anim.Play(newState);
            currentAnimationState = newState;
        }
    }

    private void HandleSelfDestructInput()
    {
        if (Input.GetKey(KeyCode.R))
        {
            suicideTimer += Time.deltaTime;
            if (spriteRenderer != null)
            {
                float progress = suicideTimer / timeToSuicide;
                spriteRenderer.color = Color.Lerp(Color.white, Color.red, progress);
            }

            if (suicideTimer >= timeToSuicide)
            {
                suicideTimer = 0f;
                if (spriteRenderer != null) spriteRenderer.color = Color.white; 
                Die(); 
            }
        }
        else
        {
            if (suicideTimer > 0)
            {
                suicideTimer = 0f;
                if (spriteRenderer != null) spriteRenderer.color = Color.white;
            }
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (playerAudio != null) playerAudio.PlayPlayerDeath();
        
        if (GameManager.instance != null)
        {
            GameManager.instance.HandlePlayerDeath();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    public void Revive()
    {
        isDead = false;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, attackSize);
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
}
