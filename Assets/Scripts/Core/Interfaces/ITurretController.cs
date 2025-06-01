using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestProject_Factura
{
    public interface ITurretController
    {
        void UpdateRotation(Vector3 worldPos);
        UniTask Shoot();
        bool CanShoot { get; }
    }
}