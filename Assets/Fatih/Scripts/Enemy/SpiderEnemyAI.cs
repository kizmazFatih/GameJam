using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SpiderEnemyAI : MonoBehaviour
{
    private bool hasExploded = false; 
    private NavMeshAgent agent;
    private Transform player;

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

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            m_WaitTime -= Time.deltaTime;
            if (m_WaitTime <= 0)
            {
                MoveToRandomPoint();
                m_WaitTime = waitTime;
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < detectionRadius)
        {
            currentState = State.Chase;
        }
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
            MoveToRandomPoint();
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
    }
}