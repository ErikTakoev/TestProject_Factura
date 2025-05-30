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
        
        [Header("Prefabs")]
        [SerializeField] private GameObject carPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject bulletPrefab;
        
        [Header("Components")]
        [SerializeField] private InputManager inputManager;
        
        protected override void Configure(IContainerBuilder builder)
        {
            // Реєстрація конфігурацій
            builder.RegisterInstance(carConfig).AsSelf();
            builder.RegisterInstance(enemyConfig).AsSelf();
            builder.RegisterInstance(gameConfig).AsSelf();
            
            // Реєстрація компонентів сцени
            builder.RegisterComponent(inputManager).AsSelf();
            
            // Реєстрація контролерів
            builder.RegisterComponentInHierarchy<CarController>().As<ICarController>();
            builder.RegisterComponentInHierarchy<TurretController>().As<ITurretController>();
            builder.RegisterComponentInHierarchy<CameraController>().As<ICameraController>();
            builder.RegisterComponentInHierarchy<GameManager>().As<IGameManager>();
            builder.RegisterComponentInHierarchy<InputHandler>().AsSelf();
            builder.RegisterComponentInHierarchy<EnemySpawner>().AsSelf();
            builder.RegisterComponentInHierarchy<UIManager>().AsSelf();
            builder.RegisterComponentInHierarchy<DependencyTest>().AsSelf();
            
            // Для EnemyController використовуємо системний метод VContainer для ін'єкції у створені об'єкти
            builder.RegisterEntryPointExceptionHandler(ex => Debug.LogError($"VContainer Error: {ex.Message}"));
            
            // Реєстрація фабрик і сервісів
            RegisterFactories(builder);
            
            // Реєстрація префабів
            RegisterPrefabs(builder);
            
            // Реєстрація систем і контролерів
            RegisterGameSystems(builder);
            RegisterControllers(builder);
            
            // Реєстрація UI
            RegisterUI(builder);
        }
        
        private void RegisterFactories(IContainerBuilder builder)
        {
            // Реєстрація фабрик
            builder.Register<BulletFactory>(Lifetime.Singleton);
            builder.Register<EnemyFactory>(Lifetime.Singleton);
        }
        
        private void RegisterPrefabs(IContainerBuilder builder)
        {
            // Реєстрація префабів з ключами
            builder.RegisterInstance(carPrefab).AsSelf();
            builder.RegisterInstance(enemyPrefab).AsSelf();
            builder.RegisterInstance(bulletPrefab).AsSelf();
            
            // Зберігаємо посилання на префаби як синглтони з конкретними типами
            builder.RegisterInstance(new PrefabReference(carPrefab, "CarPrefab")).AsSelf();
            builder.RegisterInstance(new PrefabReference(enemyPrefab, "EnemyPrefab")).AsSelf();
            builder.RegisterInstance(new PrefabReference(bulletPrefab, "BulletPrefab")).AsSelf();
        }
        
        private void RegisterGameSystems(IContainerBuilder builder)
        {
            // Реєструємо ігрові системи
            builder.RegisterComponentInHierarchy<GameManager>().AsImplementedInterfaces().AsSelf();
            builder.RegisterComponentInHierarchy<EnemySpawner>().AsSelf();
            builder.RegisterComponentInHierarchy<InputHandler>().AsSelf();
            builder.RegisterComponentInHierarchy<LevelManager>().AsSelf();
        }
        
        private void RegisterControllers(IContainerBuilder builder)
        {
            // Реєструємо контролери
            builder.RegisterComponentInHierarchy<CarController>().AsImplementedInterfaces().AsSelf();
            builder.RegisterComponentInHierarchy<TurretController>().AsImplementedInterfaces().AsSelf();
            builder.RegisterComponentInHierarchy<CameraController>().AsImplementedInterfaces().AsSelf();
            
            // Object Pool - буде реалізовано пізніше
            RegisterObjectPools(builder);
        }
        
        private void RegisterUI(IContainerBuilder builder)
        {
            // Реєструємо UI-компоненти
            builder.RegisterComponentInHierarchy<UIManager>().AsSelf();
        }
        
        private void RegisterObjectPools(IContainerBuilder builder)
        {
            // Пули об'єктів буде реалізовано пізніше
            // Приклад реєстрації:
            // builder.Register<ObjectPool<Bullet>>(Lifetime.Singleton);
            // builder.Register<ObjectPool<EnemyController>>(Lifetime.Singleton);
        }
    }
    
    // Клас-обгортка для префабів
    public class PrefabReference
    {
        public GameObject Prefab { get; }
        public string Key { get; }
        
        public PrefabReference(GameObject prefab, string key)
        {
            Prefab = prefab;
            Key = key;
        }
    }
} 