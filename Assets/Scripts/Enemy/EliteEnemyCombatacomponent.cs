using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class EliteEnemyCombatacomponent : EnemyCombatComponent
{
    Vector3 initialLocation;
    Quaternion initialRotation;
    bool isReturning = false;
    public GameObject Fireball {private get; set;} // 보스의 특별 공격용 오브젝트

    public override void Start()
    {
        base.Start();
        initialLocation = EnemyInfo.Transform.position;
        initialRotation = EnemyInfo.Transform.rotation;
    }

    public override void ChasePlayer()
    {   
        Vector3 target = new Vector3();
        if(isReturning)
        {
            target = initialLocation;
        }
        else
        {
            target = playerTransform.position;
        }

        EnemyInfo.Transform.LookAt(target);
        Vector3 direction = target - EnemyInfo.Transform.position;
        float distance = Vector3.Distance(target, EnemyInfo.Transform.position);
        direction.y = 0;

        if (distance > 1f)
        {
            EnemyInfo.Transform.rotation = Quaternion.Slerp(EnemyInfo.Transform.rotation, Quaternion.LookRotation(direction), 0.1f);
            if (direction.magnitude > 1f)
            {
                Vector3 movementVelocity = direction.normalized * EnemyInfo.ChaseSpeed;
                EnemyInfo.EnemyObject.GetComponent<CharacterController>().Move(movementVelocity * Time.deltaTime);
            }
        }
        else if(isReturning) // 초기 위치에 거의 다다랐다면
        {
            targetFound = false;
            animator.SetBool("See", false);
            currentHealth = EnemyInfo.MaxHealth;
            MonsterHPBar.instance.Recover_HP(EnemyInfo.MaxHealth);
            
            EnemyInfo.Transform.rotation = initialRotation;
            isReturning = false;
        }
        if (Vector3.Distance(EnemyInfo.Transform.position, initialLocation) > EnemyInfo.MaxDistance) isReturning = true;

        base.ChasePlayer(); // 부모 메서드 호출
    }

    public async override void AttackPlayer()
    {
        base.AttackPlayer();

        if (EnemyInfo.EnemyObject.name.Contains("Boss"))
        {
            // 보스 전용 공격 로직
            int n = UnityEngine.Random.Range(1, 12);
            if (n < 6)
            {
                animator.SetTrigger("Attack1");
                // 보스 공격 타입 1
            }
            else if (n < 10)
            {
                animator.SetTrigger("Attack2");
                // 보스 공격 타입 2
            }
            else
            {
                animator.SetTrigger("Attack3");
                ActivateFireball(); // 보스의 특별 공격
            }
        }
        else
        {
            int n = Random.Range(1, 10);
            if (n < 8)
            {
                animator.SetTrigger("Attack"); // 일반 공격
                PlayerInfoManager.instance.PlayerDamage(EnemyInfo.NormalAttackDamage);
            }
            else
            {
                animator.SetTrigger("StrongAttack");

                PlayerInfoManager.instance.PlayerDamage(EnemyInfo.StrongAttackDamage);
            }
        }
        
        await Task.Delay((int)EnemyInfo.TimeBetweenAttacks * 1000); // TimeBetweenAttacks초만큼 기다리고 다음 실행
    }

    public async void ActivateFireball()
    {
        Fireball.SetActive(true);
        await Task.Delay(1000); // 1초 기다리고 다음 실행

        DeactivateFireball();
    }

    void DeactivateFireball()
    {
        Fireball.SetActive(false);
    }

    public override void Die()
    {
        if(EnemyInfo.EnemyObject.name.Contains("Boss"))
        {
            SoundManager.instance.PlayDragonDieSound();
            GameDirector.instance.Invoke(nameof(GameDirector.instance.AfterDragonDead),0.5f); //대화 준비
        }
        else
        {
            PlayerInfoManager.instance.Invoke(nameof(PlayerInfoManager.instance.GetEXP), 1.5f);
        }

        base.Die();
    }
}