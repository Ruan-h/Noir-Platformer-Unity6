using UnityEngine;

[RequireComponent(typeof(EnemyPatrol))] 
[RequireComponent(typeof(AudioSource))] // --- NOVO: Exige AudioSource ---
public class Anda : Enemy, IEnemyVision
{
    public enum State { Patrolling, Chasing }
    private State currentState;
    public State CurrentState { get { return currentState; } }

    private EnemyPatrol patrolComponent;

    [Header("Chasing Settings")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float chaseStopDistance = 3f;
    [SerializeField] private float timeToLoseTarget = 1f;
    
    [Header("Vision Settings")]
    [SerializeField] public float visionRange = 8f;
    [SerializeField] [Range(0, 360)] public float visionAngle = 90f;
    [SerializeField] public Transform eyePosition;
    [SerializeField] public LayerMask obstacleLayer;

    [Header("Initial Settings")]
    [SerializeField] private bool startFacingLeft = false; 

    // --- NOVO: Configurações de Áudio ---
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip alertSound; // Som de "Te vi!"
    // ------------------------------------

    public float VisionRange => visionRange;    
    public float VisionAngle => visionAngle;        
    public Transform EyePosition => eyePosition;  
    public LayerMask ObstacleLayer => obstacleLayer; 
    public override bool IsAlert => currentState == State.Chasing;

    private Rigidbody2D rb;
    private Transform playerTransform;
    private Transform playerTargetPoint;
    private float loseTargetTimer;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        patrolComponent = GetComponent<EnemyPatrol>();
        
        // --- NOVO: Pega o componente de áudio ---
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            playerTargetPoint = playerTransform.Find("Target");  
            if (playerTargetPoint == null) playerTargetPoint = playerTransform;
        }
        else
        {
            enabled = false;
            return;
        }

        if (startFacingLeft && isFacingRight)
        {
            Flip(); 
            startFacingRight = isFacingRight; 
        }

        ResetState();
    }

    protected override void ResetState()
    {
        currentState = State.Patrolling;
        loseTargetTimer = 0f;
        
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        HandleStateTransitions();

        switch (currentState)
        {
            case State.Patrolling:
                patrolComponent.PatrolUpdate(rb, isFacingRight, Flip); 
                break;
            case State.Chasing:
                ExecuteChasingState();
                break;
        }
    }

    private void HandleStateTransitions()
    {
        if (playerTargetPoint == null) return;

        bool canSeePlayer = CheckForPlayerInVisionCone();

        if (canSeePlayer)
        {
            // Se ele NÃO estava perseguindo antes, e agora vai começar...
            if (currentState != State.Chasing)
            {
                currentState = State.Chasing;
                
                // --- NOVO: TOCA O SOM DE ALERTA ---
                PlayAlertSound();
                // ----------------------------------
            }
            
            loseTargetTimer = timeToLoseTarget;
        }
        else
        {
            if (currentState == State.Chasing)
            {
                loseTargetTimer -= Time.deltaTime;
                if (loseTargetTimer <= 0) currentState = State.Patrolling;
            }
        }
    }

    // --- NOVO: Função auxiliar para tocar o som ---
    private void PlayAlertSound()
    {
        if (audioSource != null && alertSound != null)
        {
            audioSource.pitch = 1.0f; // Reset do pitch para soar claro
            audioSource.PlayOneShot(alertSound);
        }
    }
    // ----------------------------------------------

    private void ExecuteChasingState()
    {
        if (playerTargetPoint == null) { currentState = State.Patrolling; return; }
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTargetPoint.position);

        if (distanceToPlayer > chaseStopDistance)
        {
            float moveDirection = Mathf.Sign(playerTargetPoint.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(moveDirection * chaseSpeed, rb.linearVelocity.y);
            FaceTargetDirection(playerTargetPoint.position); 
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private bool CheckForPlayerInVisionCone()
    {
        if (playerTargetPoint == null) return false;
        
        float distanceToPlayer = Vector2.Distance(eyePosition.position, playerTargetPoint.position);
        if (distanceToPlayer > visionRange) return false;

        Vector2 directionToPlayer = (playerTargetPoint.position - eyePosition.position).normalized;
        Vector2 forwardDirection = isFacingRight ? Vector2.right : Vector2.left;

        if (Vector2.Angle(forwardDirection, directionToPlayer) > visionAngle / 2) return false;

        RaycastHit2D hit = Physics2D.Raycast(eyePosition.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        if (hit.collider != null) return false;

        return true;
    }

    private void FaceTargetDirection(Vector2 targetPosition)
    {
        if (targetPosition.x > transform.position.x && !isFacingRight) Flip();
        else if (targetPosition.x < transform.position.x && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }
    
    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
          if (otherCollider.gameObject.CompareTag("Player") && currentState == State.Chasing)
          {
              Controller playerController = otherCollider.gameObject.GetComponent<Controller>();
              if (playerController != null) playerController.Die();
          }
    }

    private void OnDrawGizmosSelected()
    {
        if (eyePosition == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseStopDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePosition.position, visionRange);
        
        Vector3 forward = isFacingRight ? Vector3.right : Vector3.left;
        Vector3 coneDirection1 = Quaternion.Euler(0, 0, visionAngle / 2) * forward;
        Vector3 coneDirection2 = Quaternion.Euler(0, 0, -visionAngle / 2) * forward;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(eyePosition.position, eyePosition.position + coneDirection1 * visionRange);
        Gizmos.DrawLine(eyePosition.position, eyePosition.position + coneDirection2 * visionRange);
    }
}
