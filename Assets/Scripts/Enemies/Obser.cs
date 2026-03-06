using UnityEngine;

[RequireComponent(typeof(TargetedPatrol))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(AudioSource))]
public class Obser : Enemy, IEnemyVision
{
    public enum State { Patrolling, Aiming, Shooting }
    private State currentState;

    [Header("Vision Settings")]
    [SerializeField] public float visionRange = 12f;
    [SerializeField] [Range(0, 360)] public float visionAngle = 45f;
    [SerializeField] public Transform eyePosition;
    [SerializeField] public LayerMask obstacleLayer;
    
    public override bool IsAlert => currentState == State.Aiming || currentState == State.Shooting;
    public float VisionRange => visionRange;
    public float VisionAngle => visionAngle;
    public Transform EyePosition => eyePosition;
    public LayerMask ObstacleLayer => obstacleLayer;

    [Header("Combat Settings")]
    [SerializeField] private float aimDuration = 0.5f;
    [SerializeField] private float shootDuration = 1.0f;
    
    [Header("Laser Core")]
    [SerializeField] private LineRenderer coreLaser;
    [SerializeField] private float coreWidthAim = 0.05f;
    [SerializeField] private float coreWidthShoot = 0.2f;
    [SerializeField] [ColorUsage(true, true)] private Color coreAimColor = Color.red;
    [SerializeField] [ColorUsage(true, true)] private Color coreShootColor = Color.white;

    [Header("Laser Aura")]
    [SerializeField] private LineRenderer auraLaser;
    [SerializeField] private float auraMultiplier = 3f;
    [SerializeField] private Color auraAimColor = new Color(1, 0, 0, 0.3f);
    [SerializeField] private Color auraShootColor = new Color(1, 0, 0, 0.5f);

    [Header("VFX")]
    [SerializeField] private ParticleSystem impactSparksPrefab;
    [SerializeField] private ParticleSystem impactGlowPrefab;
    [SerializeField] private ParticleSystem muzzleGlowPrefab;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip alertSound; 
    [SerializeField] private AudioClip laserSound; 
    
    [Range(0f, 1f)] 
    [SerializeField] private float laserVolume = 1.0f;

    private ParticleSystem currentSparks; 
    private ParticleSystem currentImpactGlow;
    private ParticleSystem currentMuzzleGlow;

    private TargetedPatrol patrolScript;
    private Rigidbody2D rb;
    private Transform playerTarget;
    private Vector3 lockedTargetPosition;
    private float stateTimer;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        patrolScript = GetComponent<TargetedPatrol>();
        
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (coreLaser == null) coreLaser = GetComponent<LineRenderer>();
        
        if (coreLaser != null) { coreLaser.useWorldSpace = true; coreLaser.enabled = false; }
        if (auraLaser != null) { auraLaser.useWorldSpace = true; auraLaser.enabled = false; }

        if (transform.localScale.x > 0) isFacingRight = true;
        else isFacingRight = false;

        if (impactSparksPrefab != null)
        {
            currentSparks = Instantiate(impactSparksPrefab, transform.position, Quaternion.identity);
            currentSparks.Stop();
            currentSparks.gameObject.SetActive(false);
        }
        if (impactGlowPrefab != null)
        {
            currentImpactGlow = Instantiate(impactGlowPrefab, transform.position, Quaternion.identity);
            currentImpactGlow.Stop();
            currentImpactGlow.gameObject.SetActive(false);
        }
        if (muzzleGlowPrefab != null)
        {
            currentMuzzleGlow = Instantiate(muzzleGlowPrefab, transform); 
            currentMuzzleGlow.Stop();
            currentMuzzleGlow.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        ResetState();
    }

    protected override void ResetState()
    {
        currentState = State.Patrolling;
        stateTimer = 0f;

        if (coreLaser != null) coreLaser.enabled = false;
        if (auraLaser != null) auraLaser.enabled = false;

        StopEffect(currentSparks);
        StopEffect(currentImpactGlow);
        StopEffect(currentMuzzleGlow);

        if (rb != null) rb.linearVelocity = Vector2.zero;

        float defaultAngle = isFacingRight ? 0f : 180f;
        if (eyePosition != null) eyePosition.rotation = Quaternion.Euler(0, 0, defaultAngle);

        if (patrolScript != null)
        {
            patrolScript.ResetPatrol();
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrolling: HandlePatrolling(); break;
            case State.Aiming: HandleAiming(); break;
            case State.Shooting: HandleShooting(); break;
        }
    }

    private void HandlePatrolling()
    {
        patrolScript.PatrolUpdate(rb, isFacingRight, Flip);
        Transform patrolTarget = patrolScript.GetCurrentLookTarget();
        if (patrolTarget != null)
        {
            Vector3 direction = patrolTarget.position - eyePosition.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            eyePosition.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            float defaultAngle = isFacingRight ? 0f : 180f;
            eyePosition.rotation = Quaternion.Euler(0, 0, defaultAngle);
        }

        if (CheckForPlayer()) StartAiming();
    }

    private void StartAiming()
    {
        currentState = State.Aiming;
        stateTimer = aimDuration;
        
        if (audioSource != null && alertSound != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(alertSound);
        }

        Collider2D playerCollider = playerTarget.GetComponent<Collider2D>();
        if (playerCollider != null) lockedTargetPosition = playerCollider.bounds.center;
        else lockedTargetPosition = playerTarget.position;

        if (coreLaser != null)
        {
            coreLaser.enabled = true;
            coreLaser.startWidth = coreWidthAim; coreLaser.endWidth = coreWidthAim;
            coreLaser.startColor = coreAimColor; coreLaser.endColor = coreAimColor;
        }
        if (auraLaser != null)
        {
            auraLaser.enabled = true;
            auraLaser.startWidth = coreWidthAim * auraMultiplier; auraLaser.endWidth = coreWidthAim * auraMultiplier;
            auraLaser.startColor = auraAimColor; auraLaser.endColor = auraAimColor;
        }
        
        rb.linearVelocity = Vector2.zero;
    }

    private void HandleAiming()
     {
         stateTimer -= Time.deltaTime;
         Vector3 dir = lockedTargetPosition - eyePosition.position;
         float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
         eyePosition.rotation = Quaternion.Euler(0, 0, angle);

         DrawLaser(lockedTargetPosition, false); 
         if (stateTimer <= 0) StartShooting();
     }

    private void StartShooting()
    {
        currentState = State.Shooting;
        stateTimer = shootDuration;

        if (audioSource != null && laserSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            
            audioSource.PlayOneShot(laserSound, laserVolume);
        }

        if (coreLaser != null)
        {
            coreLaser.startWidth = coreWidthShoot; coreLaser.endWidth = coreWidthShoot;
            coreLaser.startColor = coreShootColor; coreLaser.endColor = coreShootColor;
        }
        if (auraLaser != null)
        {
            auraLaser.startWidth = coreWidthShoot * auraMultiplier; auraLaser.endWidth = coreWidthShoot * auraMultiplier;
            auraLaser.startColor = auraShootColor; auraLaser.endColor = auraShootColor;
        }
    }

    private void HandleShooting()
    {
        stateTimer -= Time.deltaTime;
        DrawLaser(lockedTargetPosition, true); 

        Vector2 direction = (lockedTargetPosition - eyePosition.position).normalized;
        float distance = Vector2.Distance(eyePosition.position, lockedTargetPosition);
        RaycastHit2D hit = Physics2D.Raycast(eyePosition.position, direction, distance, obstacleLayer | (1 << LayerMask.NameToLayer("Player")));

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            Controller playerController = hit.collider.GetComponent<Controller>();
            if (playerController != null) playerController.Die();
        }

        if (stateTimer <= 0)
        {
            ResetState(); 
        }
    }

    private void StopEffect(ParticleSystem effect)
    {
        if (effect != null)
        {
            effect.Stop();
            effect.gameObject.SetActive(false);
        }
    }

    private void DrawLaser(Vector3 targetPos, bool showParticles)
    {
        Vector3 startPoint = eyePosition.position;
        Vector3 endPoint;
        Vector2 direction = (targetPos - startPoint).normalized;
        float maxLaserDistance = 100f;

        if (showParticles && currentMuzzleGlow != null)
        {
            if (!currentMuzzleGlow.gameObject.activeSelf) currentMuzzleGlow.gameObject.SetActive(true);
            currentMuzzleGlow.transform.position = startPoint; 
            if (!currentMuzzleGlow.isPlaying) currentMuzzleGlow.Play();
        }
        else if (!showParticles)
        {
            StopEffect(currentMuzzleGlow);
        }

        RaycastHit2D hitWall = Physics2D.Raycast(startPoint, direction, maxLaserDistance, obstacleLayer);
        
        if (hitWall.collider != null)
        {
            endPoint = hitWall.point;
            if (showParticles)
            {
                Vector3 spawnPos = new Vector3(hitWall.point.x, hitWall.point.y, -2f);
                if (currentSparks != null)
                {
                    if (!currentSparks.gameObject.activeSelf) currentSparks.gameObject.SetActive(true);
                    currentSparks.transform.position = spawnPos - (Vector3)(direction * 0.1f);
                    currentSparks.transform.rotation = Quaternion.LookRotation(-direction);
                    if (!currentSparks.isPlaying) currentSparks.Play();
                }
                if (currentImpactGlow != null)
                {
                    if (!currentImpactGlow.gameObject.activeSelf) currentImpactGlow.gameObject.SetActive(true);
                    currentImpactGlow.transform.position = spawnPos;
                    if (!currentImpactGlow.isPlaying) currentImpactGlow.Play();
                }
            }
            else
            {
                StopEffect(currentSparks);
                StopEffect(currentImpactGlow);
            }
        }
        else 
        {
            endPoint = startPoint + (Vector3)(direction * maxLaserDistance);
            StopEffect(currentSparks);
            StopEffect(currentImpactGlow);
        }

        if (coreLaser != null) { coreLaser.SetPosition(0, startPoint); coreLaser.SetPosition(1, endPoint); }
        if (auraLaser != null) { auraLaser.SetPosition(0, startPoint); auraLaser.SetPosition(1, endPoint); }
    }

    private bool CheckForPlayer()
    {
        int detectionRayCount = 10;
        Vector2 eyeDirection = eyePosition.right; 
        float angleStep = visionAngle / (detectionRayCount - 1);
        float startAngle = -visionAngle / 2;

        for (int i = 0; i < detectionRayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            Vector2 rayDirection = rotation * eyeDirection;
            RaycastHit2D hit = Physics2D.Raycast(eyePosition.position, rayDirection, visionRange, obstacleLayer | (1 << LayerMask.NameToLayer("Player")));
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                playerTarget = hit.transform;
                return true;
            }
        }
        return false;
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }
}
