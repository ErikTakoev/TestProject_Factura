using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestProject_Factura
{
    public interface ITurretController
    {
        void UpdateRotation(Vector2 input);
        UniTask Shoot();
        bool CanShoot { get; }
    }
} 