using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading.Tasks;
using UnityEngine.UI;

public class AIController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private LayerMask whatIsGround, whatisPlayer;

    //Patroling
    [SerializeField]
    private Vector3 walkPoint;
    bool walkPointSet;
    [SerializeField]
    private float walkPointRange;

    //Attacking
    [SerializeField]
    private float timeBetweenAttacks, deathTime = 1f;
    bool alreadyAttacked;

    //States
    [SerializeField]
    private float sightRange, attackRange;
    [SerializeField]
    private bool playerInSightRange, playerInAttackRange;

    [SerializeField]
    float health;
    [SerializeField]
    float maxHealth;
    [SerializeField] 
    private float damageRate;

    public GameObject healthBarUI;
    public Slider slider;

    private bool isAlive = true;

    // Start is called before the first frame update
    void Start()
    {
        //For Enemy
        //maxHealth = 10;
        health = maxHealth;
        slider.value = CalculateHealth();
    }

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void OnDisable()
    {
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (isAlive)
        {
            //Check for Sight and Attack range
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatisPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatisPlayer);

            if (!playerInSightRange && !playerInAttackRange)
                Patroling();
            if (playerInSightRange && !playerInAttackRange)
                Chasing();
            if (playerInSightRange && playerInAttackRange)
                AttackPlayer();
            if (health < 1)
                Die();
        }
    }
    float CalculateHealth()
    {
        return health / maxHealth;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        Invoke(nameof(DestryEnemy), .5f);
    }

    private void DestryEnemy()
    {
        Destroy(gameObject);
    }
    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        agent.speed = 0f;

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            //Attack code
            animator.SetBool("isAttacking", true);
            ///

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
        animator.SetBool("isAttacking", false);
    }
    private void Chasing()
    {
        agent.speed = 1;
        agent.SetDestination(player.position);
    }

    private void Patroling()
    {
        if (!walkPointSet)
            SearchWalkPoint();
        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walk point reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
        agent.speed = 1f;
    }

    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if(Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
    private async void enemyDeath()
    {
        await Task.Delay((int)deathTime*1000);
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().useGravity = true;
        Debug.Log("Destroyed");
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Enemy Collision!");
        health = health - 2;
        slider.value = CalculateHealth();
    }
    private void Die()
    {
        if (health < 0.1)
        {
            animator.Play("Die State");
            animator.SetBool("isDie", true);
            agent.speed = 0;
            enemyDeath();
            isAlive = false;
            
        }

    }
    /*    void Move(float speed)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = speed;
        }*/

    /*    void Stop()
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.speed = 0;
        }*/

    /*    public void NextPoint()
        {
            m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Length;
            navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
        }*/
    /*    void CaughtPlayer()
        {
            m_CaughtPlayer = true;
        }*/
    /*    void LookingPlayer(Vector3 player)
        {
            navMeshAgent.SetDestination(player);
            if (Vector3.Distance(transform.position, player) <= 0.3)
            {
                if (m_WaitTime <= 0)
                {
                    m_PlayerNear = false;
                    Move(speedWalk);
                    navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
                    m_WaitTime = startWaitTime;
                    m_TimetoRotate = timeToRotate;
                }
                else
                {
                    Stop();
                    m_WaitTime -= Time.deltaTime;
                }
            }
        }*/

    /*    void EnvironmentView()
        {
            Collider[] playerInRange = Physics.OverlapSphere(transform.position, viewRadius, playerMask);
            for (int i = 0; i < playerInRange.Length; i++)
            {
                Transform player = playerInRange[i].transform;
                Vector3 dirToPlayer = (player.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
                {
                    float dstToPlayer = Vector3.Distance(transform.position, player.position);
                    if (!Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleMask))
                    {
                        m_PlayerInRange = true;
                        m_IsPatrol = false;
                    }
                    else
                    {
                        m_PlayerInRange = false;
                    }
                }
                if (Vector3.Distance(transform.position, player.position) > viewRadius)
                {
                    m_PlayerInRange = false;
                }

                if (m_PlayerInRange)
                {
                    m_PlayerPosition = player.transform.position;
                }
            }
        }*/
}

