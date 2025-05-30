using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace TestProject_Factura
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        [SerializeField] private bool endlessMode = true;
        
        private GameState currentState = GameState.Menu;
        private GameConfig gameConfig;
        private ICarController carController;
        private ICameraController cameraController;
        private EnemySpawner enemySpawner;
        private InputHandler inputHandler;
        
        private float levelStartZ;
        private int enemiesKilled = 0;
        private int currentLevel = 1;
        
        public GameState CurrentState => currentState;
        
        [Inject]
        private void Construct(GameConfig config, ICarController car, ICameraController camera, 
                              EnemySpawner spawner, InputHandler input)
        {
            gameConfig = config;
            carController = car;
            cameraController = camera;
            enemySpawner = spawner;
            inputHandler = input;
        }
        
        private void Start()
        {
            // Зберігаємо початкову позицію по Z для визначення завершення рівня
            if (carController != null)
            {
                levelStartZ = carController.Transform.position.z;
            }
            
            // Підписуємося на події
            GameEvents.OnCarDestroyed += OnCarDestroyed;
            GameEvents.OnEnemyKilled += OnEnemyKilled;
            
            // Початковий стан гри
            ChangeState(GameState.Menu);
        }
        
        private void OnDestroy()
        {
            // Відписуємося від подій
            GameEvents.OnCarDestroyed -= OnCarDestroyed;
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
        }
        
        private void Update()
        {
            if (currentState != GameState.Playing || carController == null)
                return;
                
            // Перевіряємо, чи досягнуто кінця рівня
            float currentZ = carController.Transform.position.z;
            float traveledDistance = currentZ - levelStartZ;
            
            if (traveledDistance >= gameConfig.levelLength)
            {
                if (endlessMode)
                {
                    StartNextLevel().Forget();
                }
                else
                {
                    GameOver(true); // Перемога
                }
            }
        }
        
        public async UniTask StartGame()
        {
            if (currentState == GameState.Playing)
                return;
                
            // Скидаємо лічильник вбитих ворогів
            enemiesKilled = 0;
            currentLevel = 1;
            
            // Змінюємо стан на "гра"
            ChangeState(GameState.Playing);
            
            // Запускаємо камеру слідкування
            if (cameraController != null && carController != null)
            {
                await cameraController.SwitchToFollowMode(carController.Transform);
            }
            
            // Активуємо обробку вводу
            if (inputHandler != null)
            {
                inputHandler.TestStartGame();
            }
            
            // Запускаємо рух автомобіля
            if (carController != null)
            {
                await carController.StartMoving();
            }
            
            // Запускаємо спавн ворогів
            if (enemySpawner != null && carController != null)
            {
                enemySpawner.SpawnEnemies(carController.Transform);
            }
        }
        
        private async UniTask StartNextLevel()
        {
            // Оновлюємо початкову позицію для наступного рівня
            if (carController != null)
            {
                levelStartZ = carController.Transform.position.z;
            }
            
            // Збільшуємо номер рівня
            currentLevel++;
            
            // Оновлюємо HUD з інформацією про рівень
            Debug.Log($"Starting Level {currentLevel}");
            
            // Перезапускаємо спавн ворогів
            if (enemySpawner != null && carController != null)
            {
                enemySpawner.StopSpawning();
                await UniTask.Delay(TimeSpan.FromSeconds(1)); // Невелика пауза
                enemySpawner.SpawnEnemies(carController.Transform);
            }
        }
        
        public void RestartGame()
        {
            // Перезавантажуємо поточну сцену
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        public void GameOver(bool isWin)
        {
            // Зупиняємо автомобіль
            if (carController != null && carController.IsMoving)
            {
                carController.Stop();
            }
            
            // Зупиняємо спавн ворогів
            if (enemySpawner != null)
            {
                enemySpawner.StopSpawning();
            }
            
            // Змінюємо стан гри на перемогу або поразку
            ChangeState(isWin ? GameState.Win : GameState.GameOver);
        }
        
        private void ChangeState(GameState newState)
        {
            currentState = newState;
            
            // Сповіщаємо про зміну стану гри
            GameEvents.GameStateChanged(newState);
        }
        
        private void OnCarDestroyed()
        {
            GameOver(false); // Поразка
        }
        
        private void OnEnemyKilled(int count)
        {
            enemiesKilled += count;
            
            // Якщо всі вороги знищені, вважаємо це перемогою
            if (enemiesKilled >= gameConfig.enemyCount && !endlessMode)
            {
                GameOver(true); // Перемога
            }
        }
    }
} 