using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyController
{
    Animator Animator {get;}

    void Start();

    int GetLevel();

    float GetMaxHealth();

    void TakeDamage(float damage);

    bool IsDead();

    void Disable();

    void DestroyMyself();

    void OnTriggerEnter(Collider other);

}
