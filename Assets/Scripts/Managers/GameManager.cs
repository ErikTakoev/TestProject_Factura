using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace TestProject_Factura
{
    [Expecto.ContextCodeAnalyzer("GameManager - test context, test context 2, test context 3")]
    public class GameManager : MonoBehaviour, IGameManager
    {
        [Expecto.ContextCodeAnalyzer("currentState - test context, test context 2, test context 3")]
        private GameState currentState = GameState.Menu;
        [Expecto.ContextCodeAnalyzer("gameConfig - test context, test context 2, test context 3")]
        private GameConfig gameConfig;
        [Expecto.ContextCodeAnalyzer("carController - test context, test context 2, test context 3")]
        private ICarController carController;
        [Expecto.ContextCodeAnalyzer("cameraController - test context, test context 2, test context 3")]
        private ICameraController cameraController;
        [Expecto.ContextCodeAnalyzer("enemySpawner - test context, test context 2, test context 3")]
        private EnemySpawner enemySpawner;
        [Expecto.ContextCodeAnalyzer("inputHandler - test context, test context 2, test context 3")]
        private TurretManager inputHandler;

        private float levelStartZ;

        public GameState CurrentState => currentState;

        [Inject]
        private void Construct(GameConfig config, ICarController car, ICameraController camera,
                              EnemySpawner spawner, TurretManager input)
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

            // Початковий стан гри
            ChangeState(GameState.Menu);
        }

        private void OnDestroy()
        {
            // Відписуємося від подій
            GameEvents.OnCarDestroyed -= OnCarDestroyed;
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
                GameOver(true); // Перемога
            }
        }

        public async UniTask StartGame()
        {
            GameStatistics.IncrementUniTaskCreated();
            if (currentState == GameState.Playing)
                return;

            GameStatistics.ResetStatistics();

            // Змінюємо стан на "гра"
            ChangeState(GameState.Playing);

            // Запускаємо камеру слідкування
            if (cameraController != null && carController != null)
            {
                await cameraController.SwitchToFollowMode(carController.Transform);
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

        public void RestartGame()
        {
            // Перезавантажуємо поточну сцену
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void GameOver(bool isWin)
        {
            GameEvents.OnCarDestroyed -= OnCarDestroyed;

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
    }
}