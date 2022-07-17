using UnityEngine;
using UnityEngine.AI;

public class GoombaMovment : MonoBehaviour
{
    // Very Basic Ai built from https://www.youtube.com/watch?v=UjkSFoLxesw

    public NavMeshAgent agent;
    private Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    bool startedAttack;
    bool startedChase;
    bool startedPatrol;
    bool squished;


    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    // Refrences

    private Animator anim;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && squished == false)
        {
            if(other.transform.position.y > transform.position.y && other.GetComponent<PlayerStateMachineCore>().GetVelocity().y < 0)
            {
                anim.SetBool("Squish", true);
                squished = true;
                Destroy(agent);
                Invoke(nameof(Leave), 1f);
            }
        }
    }

    private void Leave()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        if (squished) { return; }

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange)
        {
            Patrolling();
            if (startedPatrol == false)
            {
                startPatrol();
                startedPatrol = true;
            }
        }
        else
        {
            startedPatrol = false;
        }

        if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
            if (startedChase == false)
            {
                startChase();
                startedChase = true;
            }
        }
        else
        {
            startedChase = false;
        }

        if (playerInSightRange && playerInAttackRange) AttackPlayer();
    }

    private void startPatrol()
    {
        anim.SetBool("Run", false);
        anim.SetBool("Walk", true);
        agent.speed = 2.5f;
    }

    private void startChase()
    {
        anim.SetBool("Walk", false);
        anim.SetBool("Run", true);
        agent.speed = 12;
    }

    private void Patrolling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(player.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            /// Attack Code
            Debug.Log("Attacked!");

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}
