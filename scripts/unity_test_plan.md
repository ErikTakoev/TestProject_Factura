# План виконання тестового завдання Unity Developer

## Загальна структура: 10 годин за 2 дні

---

## День 1 (5-6 годин)

### Блок 1: Підготовка та архітектура (1.5-2 год)
- **Налаштування проекту Unity 2022.3.x**
- **Інтеграція VContainer та UniTask** (пакети з Package Manager)
- **Створення базової архітектури:**
  - Інтерфейси для основних компонентів (ICarController, IEnemyController, ITurretController, ICameraController)
  - Налаштування DI контейнера
  - Створення ScriptableObjects для конфігурації (CarConfig, EnemyConfig, GameConfig)

### Блок 2: Базова механіка руху (1.5-2 год)
- **Імпорт та налаштування асетів** з наданого архіву
- **Реалізація руху автомобіля:**
  - CarController з Rigidbody
  - Константна швидкість вперед
  - HP система
- **Налаштування землі та зациклення** моделі на довжину рівня

### Блок 3: Система ворогів (1.5-2 год)
- **EnemyController з State Machine:**
  - Idle стан
  - Chase стан (біг до авто)
  - Attack стан (завдання шкоди)
- **Процедурна генерація ворогів** по рівню в рандомних позиціях
- **Детекція відстані** до авто для зміни станів
- **Базовий Object Pool** для ворогів

---

## День 2 (4-5 годин)

### Блок 4: Турель та стрільба (1.5-2 год)
- **TurretController з обертанням** за Input
- **Система стрільби:**
  - Object Pool для куль
  - Collision detection з ворогами
  - Завдання шкоди ворогам
- **Input система** для турелі (миша/тач)

### Блок 5: Камера та UI (1-1.5 год)
- **CameraController з двома станами:**
  - Static camera (початковий стан)
  - Follow camera (за авто)
  - Плавний перехід між станами
- **Базовий UI:**
  - HP бар автомобіля
  - "You Win" / "You Lose" екрани
  - Restart функціональність

### Блок 6: Інтеграція та поліровка (1-1.5 год)
- **GameManager з логікою гри:**
  - Початок/кінець рівня
  - Умови перемоги/поразки
  - Restart механіка
- **Тестування всіх сценаріїв**
- **Cleanup коду та коментарі**

---

## Технічні пріоритети

### Обов'язкові паттерни:
- **Dependency Injection** (VContainer)
- **Observer Pattern** для подій (CarDamaged, EnemyKilled, GameOver)
- **State Machine** для ворогів
- **Object Pool** для куль та ворогів
- **Strategy Pattern** для різних типів ворогів (якщо час дозволить)

### Структура проекту:
```
Scripts/
├── Core/
│   ├── Interfaces/
│   ├── Config/
│   └── DI/
├── Gameplay/
│   ├── Car/
│   ├── Enemy/
│   ├── Turret/
│   └── Camera/
├── UI/
└── Managers/
```

### UniTask використання:
- Async завантаження рівня
- Плавні переходи камери
- Delay між атаками ворогів

---

## Умови готовності

### Мінімальний MVP (обов'язково):
- ✅ Авто рухається вперед з HP
- ✅ Вороги генеруються, біжать та атакують
- ✅ Турель обертається та стріляє
- ✅ Камера перемикається між станами
- ✅ Win/Lose умови та restart

### Додатково (якщо час дозволить):
- 🎯 Візуальні ефекти (muzzle flash, hit effects)
- 🎯 Звукові ефекти
- 🎯 Анімації ворогів (з наданих асетів)
- 🎯 Різні типи ворогів

---

---

## Детальна архітектура класів

### Core/Interfaces/
```csharp
public interface ICarController
{
    float CurrentHP { get; }
    bool IsMoving { get; }
    UniTask StartMoving();
    void TakeDamage(float damage);
    void Stop();
}

public interface IEnemyController
{
    void Initialize(Vector3 spawnPosition);
    void SetTarget(Transform target);
    UniTask TakeDamage(float damage);
}

public interface ITurretController
{
    void UpdateRotation(Vector2 input);
    UniTask Shoot();
    bool CanShoot { get; }
}

public interface ICameraController
{
    UniTask SwitchToFollowMode(Transform target);
    void SwitchToStaticMode();
}

public interface IGameManager
{
    GameState CurrentState { get; }
    UniTask StartGame();
    void RestartGame();
    void GameOver(bool isWin);
}
```

### Core/Config/
```csharp
[CreateAssetMenu(fileName = "CarConfig", menuName = "Game/CarConfig")]
public class CarConfig : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    
    [Header("Health")]
    public float maxHP = 100f;
    
    [Header("Physics")]
    public float acceleration = 5f;
}

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Game/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    [Header("Health")]
    public float maxHP = 30f;
    
    [Header("Movement")]
    public float chaseSpeed = 8f;
    public float detectionRange = 15f;
    public float attackRange = 2f;
    
    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
}

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Level")]
    public float levelLength = 200f;
    public int enemyCount = 20;
    public Vector2 enemySpawnRangeX = new Vector2(-10f, 10f);
    
    [Header("Shooting")]
    public float bulletSpeed = 20f;
    public float bulletDamage = 25f;
    public float shootCooldown = 0.3f;
}
```

### Gameplay/Car/
```csharp
public class CarController : MonoBehaviour, ICarController
{
    // Поля
    private Rigidbody rb;
    private CarConfig config;
    private float currentHP;
    
    // Властивості
    public float CurrentHP => currentHP;
    public bool IsMoving { get; private set; }
    
    // Події
    public static event System.Action<float> OnHPChanged;
    public static event System.Action OnCarDestroyed;
    
    // Методи
    private void Awake() // Ініціалізація компонентів
    private void Start() // Отримання config через DI
    
    public async UniTask StartMoving() // Запуск руху з плавним прискоренням
    public void TakeDamage(float damage) // Отримання урону + події
    public void Stop() // Зупинка авто
    
    private void FixedUpdate() // Фізичний рух вперед
    private void OnTriggerEnter(Collider other) // Колізія з ворогами
}

public class CarHealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    
    private void OnEnable() // Підписка на події CarController
    private void OnDisable() // Відписка від подій
    private void UpdateHealthBar(float currentHP) // Оновлення UI
}
```

### Gameplay/Enemy/
```csharp
public class EnemyController : MonoBehaviour, IEnemyController
{
    // Компоненти
    private Rigidbody rb;
    private Animator animator;
    
    // Конфігурація
    private EnemyConfig config;
    private float currentHP;
    private Transform target;
    private bool isActive = false;
    
    // Властивості
    public Transform Target => target;
    public float DistanceToTarget => Vector3.Distance(transform.position, target.position);
    
    // Методи
    public void Initialize(Vector3 spawnPosition) // Початкове налаштування
    public void SetTarget(Transform target) // Встановлення цілі
    public async UniTask TakeDamage(float damage) // Отримання урону + death effect
    
    private void Update() // Простий AI: наближається авто -> біжить і атакує
    private void OnTriggerEnter(Collider other) // Детекція авто для активації
    private void OnCollisionEnter(Collision collision) // Атака при зіткненні з авто
    
    private void MoveTowardsTarget() // Рух до цілі
    private void AttackCar() // Завдання шкоди авто
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform levelParent;
    
    private GameConfig gameConfig;
    private ObjectPool<EnemyController> enemyPool;
    
    public void SpawnEnemies(Transform carTransform) // Генерація ворогів по рівню
    private Vector3 GetRandomSpawnPosition(float zPosition) // Рандомна позиція spawn
    private void InitializeObjectPool() // Налаштування пулу ворогів
}
```

### Gameplay/Turret/
```csharp
public class TurretController : MonoBehaviour, ITurretController
{
    [Header("Rotation")]
    [SerializeField] private Transform turretPivot;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private Vector2 rotationLimits = new Vector2(-90f, 90f);
    
    [Header("Shooting")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject bulletPrefab;
    
    private Camera mainCamera;
    private GameConfig gameConfig;
    private ObjectPool<Bullet> bulletPool;
    private float lastShootTime;
    
    // Властивості
    public bool CanShoot => Time.time - lastShootTime >= gameConfig.shootCooldown;
    
    // Методи
    private void Start() // Ініціалізація камери та пулу
    private void Update() // Обробка input для обертання
    
    public void UpdateRotation(Vector2 input) // Обертання турелі за input
    public async UniTask Shoot() // Стрільба з кулдауном
    
    private Vector3 GetWorldPointFromInput(Vector2 screenPoint) // Конвертація input в world coordinates
    private float ClampRotationAngle(float angle) // Обмеження кута повороту
}

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    private float damage;
    private float lifetime = 5f;
    
    public void Initialize(Vector3 direction, float speed, float bulletDamage) // Налаштування кулі
    
    private void Start() // Автоматичне знищення через lifetime
    private void OnTriggerEnter(Collider other) // Колізія з ворогами
    private void ReturnToPool() // Повернення в Object Pool
}

public class InputHandler : MonoBehaviour
{
    private ITurretController turretController;
    private ICameraController cameraController;
    private IGameManager gameManager;
    
    [Inject] // VContainer DI
    private void Construct(ITurretController turret, ICameraController camera, IGameManager game)
    
    private void Update() // Обробка input (миша/тач)
    private void HandleTouchInput() // Логіка для мобільних пристроїв
    private void HandleMouseInput() // Логіка для ПК
}
```

### Gameplay/Camera/
```csharp
public class CameraController : MonoBehaviour, ICameraController
{
    [Header("Static Camera")]
    [SerializeField] private Vector3 staticPosition;
    [SerializeField] private Vector3 staticRotation;
    
    [Header("Follow Camera")]
    [SerializeField] private Vector3 followOffset = new Vector3(0, 5, -10);
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;
    
    private Transform target;
    private bool isFollowing;
    
    // Методи
    public async UniTask SwitchToFollowMode(Transform carTarget) // Плавний перехід до follow режиму
    public void SwitchToStaticMode() // Повернення до статичної позиції
    
    private void LateUpdate() // Оновлення позиції камери (follow mode)
    private async UniTask AnimateToPosition(Vector3 targetPos, Vector3 targetRot) // Плавна анімація переходу
}
```

### Managers/
```csharp
public enum GameState { Menu, Playing, GameOver, Win }

public class GameManager : MonoBehaviour, IGameManager
{
    private GameState currentState;
    private ICarController carController;
    private ICameraController cameraController;
    private EnemySpawner enemySpawner;
    
    // Події
    public static event System.Action<GameState> OnGameStateChanged;
    
    // Властивості
    public GameState CurrentState => currentState;
    
    // Методи (вводжуються через DI)
    [Inject]
    private void Construct(ICarController car, ICameraController camera, EnemySpawner spawner)
    
    private void Start() // Підписка на події, початковий стан
    private void OnEnable() // Підписка на події компонентів
    private void OnDisable() // Відписка від подій
    
    public async UniTask StartGame() // Запуск гри (камера + рух авто)
    public void RestartGame() // Рестарт рівня
    public void GameOver(bool isWin) // Завершення гри
    
    private void ChangeState(GameState newState) // Зміна стану гри
    private void OnCarDestroyed() // Обробка знищення авто
    private void OnLevelCompleted() // Обробка завершення рівня
}

public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject gameplayUI;
    
    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button startButton;
    
    private IGameManager gameManager;
    
    [Inject]
    private void Construct(IGameManager manager) // DI ін'єкція
    
    private void Start() // Налаштування кнопок та підписка на події
    private void OnGameStateChanged(GameState newState) // Перемикання UI екранів
    private void ShowGameOverScreen() // Показ екрану поразки
    private void ShowWinScreen() // Показ екрану перемоги
    private void ShowGameplayUI() // Показ ігрового UI
}
```

### Core/DI/
```csharp
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
    
    protected override void Configure(IContainerBuilder builder)
    {
        // Конфігурації
        builder.RegisterInstance(carConfig);
        builder.RegisterInstance(enemyConfig);
        builder.RegisterInstance(gameConfig);
        
        // Контролери
        builder.Register<CarController>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<TurretController>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<CameraController>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<GameManager>(Lifetime.Singleton).AsImplementedInterfaces();
        
        // Системи
        builder.Register<EnemySpawner>(Lifetime.Singleton);
        builder.Register<InputHandler>(Lifetime.Singleton);
        builder.Register<UIManager>(Lifetime.Singleton);
        
        // Object Pools
        builder.Register<ObjectPool<EnemyController>>(Lifetime.Singleton);
        builder.Register<ObjectPool<Bullet>>(Lifetime.Singleton);
    }
}
```

### Utility/
```csharp
public class ObjectPool<T> where T : MonoBehaviour
{
    private Queue<T> pool = new Queue<T>();
    private T prefab;
    private Transform parent;
    
    public ObjectPool(T prefab, Transform parent, int initialSize = 10) // Конструктор пулу
    public T Get() // Отримання об'єкта з пулу
    public void Return(T item) // Повернення об'єкта в пул
    private T CreateNewItem() // Створення нового об'єкта
}

public static class GameEvents
{
    public static event System.Action<float> OnCarHPChanged;
    public static event System.Action OnCarDestroyed;
    public static event System.Action<int> OnEnemyKilled;
    public static event System.Action<GameState> OnGameStateChanged;
    
    // Методи для виклику подій
    public static void CarHPChanged(float newHP) => OnCarHPChanged?.Invoke(newHP);
    public static void CarDestroyed() => OnCarDestroyed?.Invoke();
    public static void EnemyKilled(int enemiesLeft) => OnEnemyKilled?.Invoke(enemiesLeft);
    public static void GameStateChanged(GameState newState) => OnGameStateChanged?.Invoke(newState);
}
```

---

## Фінальні кроки

### Перед здачею:
1. **Створити відео геймплею** (до 1 хвилини)
2. **Cleanup коду** - видалити Debug.Log, закоментувати складні місця
3. **README.md** з описом архітектури та часом виконання
4. **Push на GitHub** з правильною структурою commit'ів

### Орієнтовний час виконання: **9-10 годин**