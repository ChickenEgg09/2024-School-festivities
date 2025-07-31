using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State
    {
        Idle,
        Chasing,
        Attacking
    };

    State currentState;

    public ParticleSystem deathEffect;
    public static event System.Action OnDeathStatic;

    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;
    Material skinMaterial;

    Color originalColour;

    float attackDistanceThreshold = .5f;
    float timeBetweenAttacks = 1;
    float damage = 1;

    float nextAttackTime;
    float myCollisionRadius;
    float targetCollisionRadius;

    bool hasTarget;

    void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = GetComponent<CapsuleCollider>().radius;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (hasTarget)
        {
            currentState = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;

            StartCoroutine(Updatepath());
        }
    }

    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColour)
    {
        pathfinder.speed = moveSpeed;

        if (hasTarget)
        {
            damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
        }

        startingHealth = enemyHealth;

        deathEffect.startColor = new Color(skinColour.r, skinColour.g, skinColour.b, 1);
        skinMaterial = GetComponent<Renderer>().material;
        skinMaterial.color = skinColour;
        originalColour = skinMaterial.color;
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);
        if (damage >= health)
        {
            if(OnDeathStatic != null)
            {
                OnDeathStatic();
            }
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)), deathEffect.startLifetime);
        }

        base.TakeHit(damage, hitPoint, hitDirection);
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    // Update is called once per framef
    void Update()
    {
        if (hasTarget)
        {
            if (Time.time > nextAttackTime)
            {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;

                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

        float attackSpeed = 3;
        float percent = 0;

        // 공격을 받은 적의 색만 빨간색으로 변경
        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;

        while (percent <= 1)
        {
            if (percent >= .5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) * percent + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        // 공격이 끝난 후 원래 색으로 복원
        skinMaterial.color = originalColour;
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator Updatepath()
    {
        float refreshRate = .25f;

        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);

                if (!dead)
                {
                    pathfinder.SetDestination(targetPosition);
                }
            }

            yield return new WaitForSeconds(refreshRate);
        }
    }
}
