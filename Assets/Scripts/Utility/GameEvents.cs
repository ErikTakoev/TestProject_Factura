using System;
using UnityEngine;

namespace TestProject_Factura
{
    public static class GameEvents
    {
        // Події для автомобіля
        public static event Action<float> OnCarHPChanged;
        public static event Action OnCarDestroyed;
        
        // Події для ворогів
        public static event Action<int> OnEnemyKilled;
        
        // Події для гри
        public static event Action<GameState> OnGameStateChanged;
        public static event Action OnLevelCompleted;
        
        // Методи для виклику подій
        public static void CarHPChanged(float newHP) => OnCarHPChanged?.Invoke(newHP);
        public static void CarDestroyed() => OnCarDestroyed?.Invoke();
        public static void EnemyKilled(int count) => OnEnemyKilled?.Invoke(count);
        public static void GameStateChanged(GameState newState) => OnGameStateChanged?.Invoke(newState);
        public static void LevelCompleted() => OnLevelCompleted?.Invoke();
    }
} 