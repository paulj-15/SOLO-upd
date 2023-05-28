using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading.Tasks;
using UnityEngine.UI;

public class BossEnemy : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private LayerMask whatIsGround, whatisPlayer;

    bool isAwake;

    //Patroling
    [SerializeField]
    private Vector3 walkPoint;
    bool walkPointSet;
    [SerializeField]
    private float walkPointRange;

    //Attacking
    [SerializeField]
    private float timeBetweenAttacks;
    bool alreadyAttacked;

    //States
    [SerializeField]
    private float chaseRange, lightAttackRange, heavyAttackRange, longRange, awakeRange;
    [SerializeField]
    private bool playerInChaseRange, playerInLightAttackRange, playerInLongRange, playerInHeavyAttackRange, playerInAwakeRange;

    [SerializeField]
    float health;
    [SerializeField]
    float maxHealth;
    [SerializeField]
    private float damageRate;

    public GameObject healthBarUI;
    public Slider slider;

    private bool isAlive = true;

    private bool isChasing = false;

    [SerializeField]
    private ParticleSystem shockWave;

    [SerializeField]
    private float timer = 5;
    private float bulletTime;

    public GameObject enemyBullet;
    public Transform spawnPoint;
    public float enemySpeed;

    [SerializeField]
    private ParticleSystem bossDeath;

    [SerializeField]
    private ParticleSystem bossDamage;


    float randNum;

    // Start is called before the first frame update
    void Start()
    {
        //For Enemy
        //maxHealth = 10;
        health = maxHealth;
        slider.value = CalculateHealth();
        randNum = Random.Range(0f, 1f);
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
            playerInChaseRange = Physics.CheckSphere(transform.position, chaseRange, whatisPlayer);
            playerInLightAttackRange = Physics.CheckSphere(transform.position, lightAttackRange, whatisPlayer);
            playerInHeavyAttackRange = Physics.CheckSphere(transform.position, heavyAttackRange, whatisPlayer);
            playerInLongRange = Physics.CheckSphere(transform.position, longRange, whatisPlayer);
            playerInAwakeRange = Physics.CheckSphere(transform.position, awakeRange, whatisPlayer);

            //if(playerInAwakeRange && !playerInChaseRange)
            //{

            //}
            if (!playerInChaseRange && !playerInLightAttackRange)
                AwakeBoss();
            if (playerInChaseRange && !playerInLongRange) //Used for Chase and Long-Ranged Attacks
                Chasing();
            //if (playerInChaseRange && !playerInHeavyAttackRange)
            //    RangedAttack();
            if (playerInLongRange && !playerInHeavyAttackRange)
            {
                //if (randNum < 0.5f)
                RangedAttack();
                ShootAtPlayer();
                
                //else
                //    Chasing();
            }
            if (playerInHeavyAttackRange && !playerInLightAttackRange)
            {
                HeavyAttack();
                if (shockWave.isPlaying == true)
                    shockWave.Stop();
                else
                    shockWave.Play();
            }
            if (playerInChaseRange && playerInLightAttackRange)  //Used for Light Attacks
                LightAttack();
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

    //For Light Attack
    private void LightAttack()
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

    //For Ranged Attack
    private void RangedAttack()
    {
        agent.SetDestination(transform.position);
        agent.speed = 0f;

        transform.LookAt(player);
        if(!alreadyAttacked)
        {
            animator.SetBool("isRanged", true);

            alreadyAttacked = true;
            Invoke(nameof(ResetRangedAttack), timeBetweenAttacks);
        }
    }

    //For Heavy Attack
    private void HeavyAttack()
    {
        agent.speed = 0f;
        Instantiate(shockWave, transform.position, Quaternion.identity);
        transform.LookAt(player);
        if(!alreadyAttacked)
        {
            animator.SetBool("isHeavy", true);
            alreadyAttacked = true;
            Invoke(nameof(ResetHeavyAttack), timeBetweenAttacks);
        }
    }

    void ShootAtPlayer()
    {
        bulletTime -= Time.deltaTime;

        if (bulletTime > 0) return;

        bulletTime = timer;

        GameObject bulletObj = Instantiate(enemyBullet, spawnPoint.transform.position, spawnPoint.transform.rotation) as GameObject;
        Rigidbody bulletRig = bulletObj.GetComponent<Rigidbody>();
        bulletRig.AddForce(bulletRig.transform.forward * enemySpeed);      
        Destroy(bulletObj, 2f);
       
    }

    private void AwakeBoss()
    {
        agent.speed = 0;
        
        if(!isAwake)
        {
            animator.SetBool("isStopped", true);
            isAwake = true;
            ResetAwakeBoss();
        }
    }

    private void Chasing()
    {
        
        if (!isChasing)
        {
            agent.speed = 1;
            agent.SetDestination(player.position);
            isChasing = true;
            animator.SetBool("isStopped", false);
            ResetChaseplayer();
        }
        
    }

    private void ResetChaseplayer()
    {
        isChasing = false;
        animator.SetBool("isStopped", true);
    }

    private void ResetAwakeBoss()
    {
        isAwake = false;
        animator.SetBool("isStopped", false);
    }

    //For Heavy Attack
    private void ResetHeavyAttack()
    {
        alreadyAttacked = false;
        animator.SetBool("isHeavy", false);
    }

    //For Ranged Attack
    private void ResetRangedAttack()
    {
        alreadyAttacked = false;
        animator.SetBool("isRanged", false);
    }

    //For Light Attack
    private void ResetAttack()
    {
        alreadyAttacked = false;
        animator.SetBool("isAttacking", false);
    }
    

    
    //private void Patroling()
    //{
    //    if (!walkPointSet)
    //        SearchWalkPoint();
    //    if (walkPointSet)
    //        agent.SetDestination(walkPoint);

    //    Vector3 distanceToWalkPoint = transform.position - walkPoint;

    //    //Walk point reached
    //    if (distanceToWalkPoint.magnitude < 1f)
    //        walkPointSet = false;
    //    agent.speed = 1f;
    //}

    //private void SearchWalkPoint()
    //{
    //    //Calculate random point in range
    //    float randomZ = Random.Range(-walkPointRange, walkPointRange);
    //    float randomX = Random.Range(-walkPointRange, walkPointRange);

    //    walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

    //    if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
    //    {
    //        walkPointSet = true;
    //    }
    //}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lightAttackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, heavyAttackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, longRange);
    }
    private async void enemyDeath()
    {
        await Task.Delay(3500);
        Debug.Log("Destroyed");
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        bossDamage.Play();
        Debug.Log("Enemy Collision!");
        health = health - 2;
        slider.value = CalculateHealth();
    }
    private void Die()
    {
        if (health < 0.1)
        {
            bossDeath.Play();
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

