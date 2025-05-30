using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestProject_Factura
{
    public enum GameState { Menu, Playing, GameOver, Win }

    public interface IGameManager
    {
        GameState CurrentState { get; }
        UniTask StartGame();
        void RestartGame();
        void GameOver(bool isWin);
    }
} 