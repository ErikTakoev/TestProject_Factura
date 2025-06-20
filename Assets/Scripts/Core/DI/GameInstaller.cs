using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TestProject_Factura
{
    public class GameInstaller : LifetimeScope
    {
        [Header("Configs")]
        [SerializeField] private CarConfig carConfig;
        [SerializeField] private EnemyConfig enemyConfig;
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private BulletConfig bulletConfig;

        [Header("Components")]
        [SerializeField] private InputManager inputManager;

        protected override void Configure(IContainerBuilder builder)
        {
            // Реєстрація конфігурацій
            builder.RegisterInstance(carConfig).AsSelf();
            builder.RegisterInstance(enemyConfig).AsSelf();
            builder.RegisterInstance(gameConfig).AsSelf();
            builder.RegisterInstance(bulletConfig).AsSelf();

            // Реєстрація компонентів сцени
            builder.RegisterComponent(inputManager).AsSelf();

            // Для EnemyController використовуємо системний метод VContainer для ін'єкції у створені об'єкти
            builder.RegisterEntryPointExceptionHandler(ex => Debug.LogError($"VContainer Error: {ex.Message}"));

            // Реєстрація систем і контролерів
            RegisterGameSystems(builder);
            RegisterControllers(builder);

            // Реєстрація UI
            RegisterUI(builder);

            RegisterTest(builder);
        }

        private void RegisterGameSystems(IContainerBuilder builder)
        {
            // Реєструємо ігрові системи
            builder.RegisterComponentInHierarchy<GameManager>().AsImplementedInterfaces().AsSelf();
            builder.RegisterComponentInHierarchy<EnemySpawner>().AsSelf();
            builder.RegisterComponentInHierarchy<TurretManager>().AsSelf();
            builder.RegisterComponentInHierarchy<LevelManager>().AsSelf();
        }

        private void RegisterControllers(IContainerBuilder builder)
        {
            // Реєструємо контролери
            builder.RegisterComponentInHierarchy<CarController>().AsImplementedInterfaces().AsSelf();
            builder.RegisterComponentInHierarchy<TurretController>().AsImplementedInterfaces().AsSelf();
            builder.RegisterComponentInHierarchy<CameraController>().AsImplementedInterfaces().AsSelf();
        }

        private void RegisterUI(IContainerBuilder builder)
        {
            // Реєструємо UI-компоненти
            builder.RegisterComponentInHierarchy<UIManager>().AsSelf();
        }

        private void RegisterTest(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<DependencyTest>().AsSelf();
        }
    }
}