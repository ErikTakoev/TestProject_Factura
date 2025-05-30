using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestProject_Factura
{
    public interface ICarController
    {
        Transform Transform { get; }
        float CurrentHP { get; }
        bool IsMoving { get; }
        UniTask StartMoving();
        void TakeDamage(float damage);
        void Stop();
    }
} 