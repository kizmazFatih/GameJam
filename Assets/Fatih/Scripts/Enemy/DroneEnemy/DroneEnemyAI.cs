using UnityEngine;

// ARTIK LINE RENDERER ZORUNLU DEĞİL
[RequireComponent(typeof(AudioSource))]
public class DroneEnemyAI : MonoBehaviour
{
    [Header("Hedef")]
    private Transform player;
    private Transform playerHead; 
    private Vector3 startPosition;

    private enum State { Patrol, Approach, Scanning, Retreat, Attack }
    [SerializeField] private State currentState;

    [Header("Hafıza")]
    [SerializeField] private bool hasScannedPlayer = false;

    [Header("Genel Ayarlar")]
    public float moveSpeed = 2f;         
    public float chaseSpeed = 5f;        
    public float retreatSpeed = 5f;      
    public float combatSpeed = 3f;       
    public float rotationSpeed = 10f;    
    public float detectionRange = 20f;   
    public float heightLimit = 3f; 

    [Header("Mesafe Ayarları")]
    public float scanDistance = 10f;    
    public float combatDistance = 15f;  
    public float stopPursuitDistance = 25f; 

    [Header("Tarama Ayarları")]
    public float timeToScan = 3.0f;      
    private float currentScanTimer = 0f;
    public float scanSpeed = 3f;        
    public float scanAmplitude = 0.3f; 
    
    [Header("Efektler")]
    public Light scannerLight;           // Sadece Işık var
    public ParticleSystem scanHitEffect; // Sadece Efekt var
    private AudioSource audioSource;     // Sadece Ses var
    // private LineRenderer laserLine;   // <-- BU SİLİNDİ!

    // Renk Ayarları
    public Color scanColor = Color.red;
    public Color attackColor = Color.yellow;

    [Header("Devriye")]
    public float patrolRadius = 8f;
    public float changeDestInterval = 3f;
    private float nextPatrolTime;
    private Vector3 currentTargetPos;

    [Header("Savaş")]
    public float fireRate = 1.5f;
    private float nextFireTime = 0f;
    public float strafeInterval = 2f;    
    public float strafeDistance = 4f;    
    private float nextStrafeTime;
    private Vector3 strafeTargetPos;

    [Header("Mermi")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (Camera.main != null) playerHead = Camera.main.transform;
        else playerHead = player;

        startPosition = transform.position;
        currentTargetPos = startPosition;
        strafeTargetPos = transform.position;

        // Line Renderer alma kodu SİLİNDİ.

        audioSource = GetComponent<AudioSource>();

        if (scannerLight != null) scannerLight.enabled = false;
        if (scanHitEffect != null) scanHitEffect.Stop();

        currentState = State.Patrol;
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                ResetEffects();
                if (distanceToPlayer <= detectionRange)
                {
                    if (hasScannedPlayer) currentState = State.Retreat; 
                    else currentState = State.Approach;
                }
                PatrolBehavior();
                break;

            case State.Approach:
                ResetEffects();
                
                // Yaklaşırken hiçbir çizgi veya efekt yok, sadece geliyor.
                
                if (distanceToPlayer <= scanDistance)
                {
                    currentState = State.Scanning;
                    currentScanTimer = 0f;
                    if(audioSource.clip != null) audioSource.Play();
                }
                else if (distanceToPlayer > detectionRange * 1.2f)
                {
                    currentState = State.Patrol; 
                }
                else
                {
                    ApproachBehavior();
                }
                break;

            case State.Scanning:
                ScanningBehavior();
                break;

            case State.Retreat:
                ResetEffects();
                if (distanceToPlayer >= combatDistance) currentState = State.Attack;
                else if (distanceToPlayer > stopPursuitDistance) currentState = State.Patrol;
                else RetreatBehavior();
                break;

            case State.Attack:
                ResetEffects();
                if (distanceToPlayer > stopPursuitDistance) currentState = State.Patrol;
                else AttackBehavior();
                break;
        }
    }

    void ResetEffects()
    {
        // laserLine satırı SİLİNDİ
        if (scannerLight != null) scannerLight.enabled = false; 
        if (scanHitEffect != null) scanHitEffect.Stop(); 
        if (audioSource.isPlaying) audioSource.Stop(); 
        audioSource.pitch = 1f;
    }

    void PatrolBehavior()
    {
        if (Time.time >= nextPatrolTime)
        {
            Vector3 randomPoint = startPosition + Random.insideUnitSphere * patrolRadius;
            randomPoint.y = Mathf.Clamp(randomPoint.y, startPosition.y - heightLimit, startPosition.y + heightLimit);
            currentTargetPos = randomPoint;
            nextPatrolTime = Time.time + changeDestInterval;
        }
        transform.position = Vector3.MoveTowards(transform.position, currentTargetPos, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, currentTargetPos) > 0.1f)
        {
            Quaternion lookRot = Quaternion.LookRotation((currentTargetPos - transform.position).normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 2f);
        }
    }

    void ApproachBehavior()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
        transform.position = Vector3.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
    }

    void ScanningBehavior()
    {
        // 1. IŞIĞI AÇ (Çizgi yok)
        if (scannerLight != null) scannerLight.enabled = true;

        // 2. HAREKET (Kafa Sallama)
        Vector3 baseTarget = (playerHead != null) ? playerHead.position : (player.position + Vector3.up * 1.6f);
        float verticalOffset = Mathf.Sin(Time.time * scanSpeed) * scanAmplitude;
        Vector3 visualTarget = baseTarget + (Vector3.up * verticalOffset);
        
        Vector3 dirToVisual = (visualTarget - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dirToVisual);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);

        // 3. PARTICLE EFFECT
        RaycastHit visualHit;
        if (Physics.Raycast(firePoint.position, dirToVisual, out visualHit, detectionRange))
        {
            if (scanHitEffect != null)
            {
                if (!scanHitEffect.isPlaying) scanHitEffect.Play();
                scanHitEffect.transform.position = visualHit.point; 
                scanHitEffect.transform.LookAt(transform.position); 
            }
        }
        else
        {
            if (scanHitEffect != null) scanHitEffect.Stop();
        }

        // 4. MANTIK VE SES/RENK
        Vector3 dirToEyes = (baseTarget - firePoint.position).normalized;
        RaycastHit logicHit;
        bool isPlayerVisible = false;

        if (Physics.Raycast(firePoint.position, dirToEyes, out logicHit, detectionRange))
        {
            if (logicHit.collider.CompareTag("Player"))
            {
                isPlayerVisible = true;
                currentScanTimer += Time.deltaTime;
                
                float progress = currentScanTimer / timeToScan;
                Color currentColor = Color.Lerp(scanColor, attackColor, progress);
                
                if (scannerLight != null) scannerLight.color = currentColor;

                if(audioSource.isPlaying) audioSource.pitch = Mathf.Lerp(1f, 3f, progress);
            }
        }

        if (!isPlayerVisible)
        {
            currentScanTimer = 0f;
            if (scannerLight != null) scannerLight.color = scanColor;
            audioSource.pitch = 1f;
            if (scanHitEffect != null) scanHitEffect.Stop();
        }

        if (Vector3.Distance(transform.position, player.position) > scanDistance * 1.5f)
        {
            currentState = State.Approach;
            currentScanTimer = 0f;
            ResetEffects();
        }

        if (currentScanTimer >= timeToScan)
        {
            Debug.Log("TARAMA BAŞARILI! SALDIRI!");
            hasScannedPlayer = true; 
            ResetEffects(); 
            currentState = State.Retreat;
        }
    }

    void RetreatBehavior()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dirToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
        Vector3 retreatDir = (transform.position - player.position).normalized;
        transform.position += retreatDir * retreatSpeed * Time.deltaTime;
    }

    void AttackBehavior()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
        if (Time.time >= nextStrafeTime)
        {
            PickNewStrafePosition();
            nextStrafeTime = Time.time + strafeInterval;
        }
        transform.position = Vector3.Lerp(transform.position, strafeTargetPos, Time.deltaTime * combatSpeed);
    }

    void PickNewStrafePosition()
    {
        float randomRight = Random.Range(-1f, 1f); 
        float randomUp = Random.Range(-0.5f, 0.5f);
        float randomForward = Random.Range(-0.2f, 0.2f);
        Vector3 moveDir = (transform.right * randomRight) + (transform.up * randomUp) + (transform.forward * randomForward);
        Vector3 potentialPos = transform.position + (moveDir * strafeDistance);
        potentialPos.y = Mathf.Clamp(potentialPos.y, startPosition.y - heightLimit, startPosition.y + heightLimit);
        strafeTargetPos = potentialPos;
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null) Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
    
     private void OnDrawGizmos()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.gray; Gizmos.DrawWireSphere(transform.position, stopPursuitDistance);
        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.3f); Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue; Gizmos.DrawWireSphere(transform.position, combatDistance);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, scanDistance);
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(center, patrolRadius);

        Vector3 topPoint = center + Vector3.up * heightLimit;
        Vector3 bottomPoint = center - Vector3.up * heightLimit;
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawWireSphere(topPoint, patrolRadius);
        Gizmos.DrawWireSphere(bottomPoint, patrolRadius);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(topPoint, bottomPoint);
    }
}