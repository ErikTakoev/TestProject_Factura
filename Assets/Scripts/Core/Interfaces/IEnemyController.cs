using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestProject_Factura
{
    public interface IEnemyController
    {
        void Initialize(Vector3 spawnPosition, ObjectPool<EnemyController> objectPool = null);
        void SetTarget(Transform target);
        UniTask TakeDamage(float damage);
    }
} 