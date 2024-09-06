using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyInfos", menuName = "ScriptableObjects/EnemyInfos", order = 1)]
public class EnemyInfo : ScriptableObject
{
    [SerializeField] GameObject enemyObject;
    [SerializeField] Transform transform;
    [SerializeField] float sightRange;
    [SerializeField] float attackRange;
    [SerializeField] float maxDistance; // 초기위치로부터 이동할 수 있는 최대 거리

    [SerializeField] float chaseSpeed;

    [SerializeField] float maxHealth;

    [SerializeField] int normalAttackDamage;
    [SerializeField] int strongAttackDamage;
    [SerializeField] float timeBetweenAttacks;

    // getter
    public GameObject EnemyObject => enemyObject;
    public Transform Transform => transform;
    public float SightRange => sightRange;
    public float AttackRange => attackRange;
    public float MaxDistance => maxDistance;
    public float ChaseSpeed => chaseSpeed;
    public float MaxHealth => maxHealth;
    public int NormalAttackDamage => normalAttackDamage;
    public int StrongAttackDamage => strongAttackDamage;    
    public float TimeBetweenAttacks => timeBetweenAttacks;

}