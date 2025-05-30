using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TestProject_Factura
{
    public class DependencyTest : MonoBehaviour
    {
        [Inject] private ICarController carController;
        [Inject] private ITurretController turretController;
        [Inject] private ICameraController cameraController;
        [Inject] private IGameManager gameManager;
        [Inject] private CarConfig carConfig;
        [Inject] private EnemyConfig enemyConfig;
        [Inject] private GameConfig gameConfig;
        
        private void Start()
        {
            // Перевіряємо, чи правильно ін'єктовано залежності
            Debug.Log($"<color=green>DI Test: Car Controller injected: {carController != null}</color>");
            Debug.Log($"<color=green>DI Test: Turret Controller injected: {turretController != null}</color>");
            Debug.Log($"<color=green>DI Test: Camera Controller injected: {cameraController != null}</color>");
            Debug.Log($"<color=green>DI Test: Game Manager injected: {gameManager != null}</color>");
            Debug.Log($"<color=green>DI Test: Car Config injected: {carConfig != null}</color>");
            Debug.Log($"<color=green>DI Test: Enemy Config injected: {enemyConfig != null}</color>");
            Debug.Log($"<color=green>DI Test: Game Config injected: {gameConfig != null}</color>");
        }
    }
} 