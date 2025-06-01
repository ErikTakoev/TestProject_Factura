using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestProject_Factura
{
    public interface ICameraController
    {
        UniTask SwitchToFollowMode(Transform target);
    }
}