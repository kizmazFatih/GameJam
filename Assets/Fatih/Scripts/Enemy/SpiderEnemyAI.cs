using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SpiderEnemyAI : MonoBehaviour
{
    private bool hasExploded = false;
    private NavMeshAgent agent;
    private Transform player;

    // --- YENİ EKLENEN: Devriye Tipi Seçimi ---
    public enum PatrolType { Random, Circular }
    
    [Header("Devriye Ayarları")]
    [Tooltip("Örümceğin nasıl devriye gezeceğini seç.")]
    public PatrolType patrolType = PatrolType.Random;

    [Header("Dairesel Devriye Ayarları (Sadece Circular seçiliyse)")]
    [Tooltip("Etrafında dönülecek obje (Kolon vb.)")]
    public Transform circleCenter; 
    [Tooltip("Dönüş yarıçapı")]
    public float circleRadius = 5f;
    [Tooltip("Dönüş hızı (Açısal hız)")]
    public float rotationSpeed = 1f;
    [Tooltip("Saat yönünde mi dönsün?")]
    public bool clockwise = true;

    // Dairesel hareket için anlık açı hesabı
    private float currentAngle;

    [Header("Hareket Ayarları")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 4.5f;

    [Header("Mesafe Ayarları")]
    public float detectionRadius = 15f;
    public float explosionRadius = 2.5f;
    public float patrolRange = 10f;

    private float waitTime = 2f;
    private float m_WaitTime;

    private enum State { Patrol, Chase, Attack }
    private State currentState;
    [SerializeField] private GameObject explosionPrefab;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("DİKKAT: Sahnede 'Player' tagine sahip bir obje yok!");

        currentState = State.Patrol;
        m_WaitTime = waitTime;

        // Başlangıç açısını o anki konuma göre hesapla ki zıplama yapmasın
        if (circleCenter != null)
        {
            Vector3 offset = transform.position - circleCenter.position;
            currentAngle = Mathf.Atan2(offset.z, offset.x);
        }
    }

    private void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                break;
            case State.Chase:
                ChaseLogic();
                break;
            case State.Attack:
                ExplodeAndSteal();
                break;
        }
    }

    void PatrolLogic()
    {
        agent.speed = patrolSpeed;

        // --- 1. Oyuncuyu Gördü mü Kontrolü ---
        // Bu kontrol her iki devriye modunda da geçerli olmalı
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < detectionRadius)
        {
            currentState = State.Chase;
            return; // Chase'e geçtiysek aşağıdakileri yapma
        }

        // --- 2. Seçilen Devriye Moduna Göre Hareket ---
        if (patrolType == PatrolType.Random)
        {
            PerformRandomPatrol();
        }
        else if (patrolType == PatrolType.Circular)
        {
            PerformCircularPatrol();
        }
    }

    // --- ESKİ RASTGELE DEVRİYE MANTIĞI ---
    void PerformRandomPatrol()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            m_WaitTime -= Time.deltaTime;
            if (m_WaitTime <= 0)
            {
                MoveToRandomPoint();
                m_WaitTime = waitTime;
            }
        }
    }

    // --- YENİ DAİRESEL DEVRİYE MANTIĞI ---
    void PerformCircularPatrol()
    {
        if (circleCenter == null)
        {
            Debug.LogWarning("Dairesel devriye seçili ama 'circleCenter' (Kolon) atanmamış!");
            return;
        }

        // Açıyı zamana göre artır veya azalt
        float direction = clockwise ? 1f : -1f;
        currentAngle += direction * rotationSpeed * Time.deltaTime;

        // Matematiksel olarak daire üzerindeki X ve Z noktasını bul
        float x = circleCenter.position.x + Mathf.Cos(currentAngle) * circleRadius;
        float z = circleCenter.position.z + Mathf.Sin(currentAngle) * circleRadius;

        Vector3 targetPos = new Vector3(x, transform.position.y, z);

        // NavMeshAgent'a sürekli bu yeni noktaya gitmesini söyle
        agent.SetDestination(targetPos);
    }

    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRange;
        randomDirection += transform.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRange, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    void ChaseLogic()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Oyuncudan çok uzaklaşırsa devriyeye dön
        if (distanceToPlayer > detectionRadius * 1.5f)
        {
            currentState = State.Patrol;
            
            // Eğer dairesel moddaysa ve kovalamaca bittiyse, 
            // tekrar daireye girmesi için açıyı güncellememiz iyi olur
            if (patrolType == PatrolType.Circular && circleCenter != null)
            {
                Vector3 offset = transform.position - circleCenter.position;
                currentAngle = Mathf.Atan2(offset.z, offset.x);
            }
            else
            {
                MoveToRandomPoint();
            }
        }

        // Patlama mesafesine girerse saldırı moduna geç
        if (distanceToPlayer < explosionRadius)
        {
            currentState = State.Attack;
        }
    }

    public void ExplodeAndSteal()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, transform.rotation);
            Destroy(fx, 5f);
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (InventoryController.instance != null)
        {
            InventoryController.instance.RemoveRandomSkill();
        }

        Debug.Log("BOOM! Örümcek patladı!");
        Destroy(gameObject, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        // Dairesel yolun Gizmo çizimi
        if (patrolType == PatrolType.Circular && circleCenter != null)
        {
            Gizmos.color = Color.blue;
            // Daireyi çizmek yerine basitçe merkezi ve yarıçapı gösterelim
            Gizmos.DrawWireSphere(circleCenter.position, circleRadius);
            Gizmos.DrawLine(transform.position, circleCenter.position);
        }
    }
}