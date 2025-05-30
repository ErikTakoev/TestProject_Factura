using System.Collections;
using UnityEngine;
using VContainer;

namespace TestProject_Factura
{
    /// <summary>
    /// Test script for verifying health system functionality
    /// </summary>
    public class HealthSystemTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CarController carController;
        
        [Header("Test Configuration")]
        [SerializeField] private bool autoStartTest = false;
        [SerializeField] private float testDamageAmount = 10f;
        [SerializeField] private float testDamageInterval = 1f;
        [SerializeField] private int testDamageCount = 5;
        [SerializeField] private bool testKillCar = false;
        
        [Header("Test Status")]
        [SerializeField] private bool isTestRunning = false;
        [SerializeField] private string testResult = "";
        [SerializeField] private float initialHP = 0f;
        [SerializeField] private float currentHP = 0f;
        
        private bool carDestroyedEventReceived = false;
        private bool hpChangedEventReceived = false;
        
        // Dependencies
        private CarConfig carConfig;
        
        [Inject]
        private void Construct(CarConfig config)
        {
            carConfig = config;
        }
        
        private void OnEnable()
        {
            // Subscribe to events
            GameEvents.OnCarHPChanged += OnCarHPChanged;
            GameEvents.OnCarDestroyed += OnCarDestroyed;
        }
        
        private void OnDisable()
        {
            // Unsubscribe from events
            GameEvents.OnCarHPChanged -= OnCarHPChanged;
            GameEvents.OnCarDestroyed -= OnCarDestroyed;
        }
        
        private void Start()
        {
            if (autoStartTest)
            {
                StartTests();
            }
        }
        
        /// <summary>
        /// Start health system tests
        /// </summary>
        public void StartTests()
        {
            if (isTestRunning)
                return;
                
            isTestRunning = true;
            testResult = "Starting health system tests...";
            carDestroyedEventReceived = false;
            hpChangedEventReceived = false;
            
            // Get car controller reference if not assigned
            if (carController == null)
            {
                carController = FindObjectOfType<CarController>();
                
                if (carController == null)
                {
                    Debug.LogError("[HealthSystemTester] Car controller not found in the scene!");
                    testResult = "FAIL: Car controller not found in the scene!";
                    isTestRunning = false;
                    return;
                }
            }
            
            // Start test routine
            StartCoroutine(RunHealthTests());
        }
        
        private IEnumerator RunHealthTests()
        {
            // Test 1: Check initial HP
            initialHP = carController.CurrentHP;
            currentHP = initialHP;
            
            Debug.Log($"[HealthSystemTester] Initial HP: {initialHP}");
            testResult = $"Initial HP: {initialHP}";
            
            if (!Mathf.Approximately(initialHP, carConfig.maxHP))
            {
                Debug.LogError($"[HealthSystemTester] Initial HP mismatch! Expected: {carConfig.maxHP}, Got: {initialHP}");
                testResult = $"FAIL: Initial HP mismatch! Expected: {carConfig.maxHP}, Got: {initialHP}";
                isTestRunning = false;
                yield break;
            }
            
            yield return new WaitForSeconds(1f);
            
            // Test 2: Apply damage multiple times
            for (int i = 0; i < testDamageCount; i++)
            {
                // Skip if testing kill car and HP is already low
                if (testKillCar && currentHP <= testDamageAmount)
                {
                    break;
                }
                
                hpChangedEventReceived = false;
                float expectedHP = Mathf.Max(0, currentHP - testDamageAmount);
                
                Debug.Log($"[HealthSystemTester] Applying damage: {testDamageAmount}. Current HP: {currentHP}, Expected after: {expectedHP}");
                testResult = $"Applying damage: {testDamageAmount}. Current HP: {currentHP}, Expected after: {expectedHP}";
                
                // Apply damage
                carController.TakeDamage(testDamageAmount);
                
                // Wait a short time for event processing
                yield return new WaitForSeconds(0.1f);
                
                // Check if HP changed event was received
                if (!hpChangedEventReceived)
                {
                    Debug.LogError("[HealthSystemTester] HP changed event was not received!");
                    testResult = "FAIL: HP changed event was not received!";
                }
                
                // Check if HP was correctly updated
                currentHP = carController.CurrentHP;
                if (!Mathf.Approximately(currentHP, expectedHP))
                {
                    Debug.LogError($"[HealthSystemTester] HP not updated correctly! Expected: {expectedHP}, Got: {currentHP}");
                    testResult = $"FAIL: HP not updated correctly! Expected: {expectedHP}, Got: {currentHP}";
                    isTestRunning = false;
                    yield break;
                }
                
                yield return new WaitForSeconds(testDamageInterval);
            }
            
            // Test 3: Kill car if requested
            if (testKillCar)
            {
                carDestroyedEventReceived = false;
                
                Debug.Log("[HealthSystemTester] Testing car destruction...");
                testResult = "Testing car destruction...";
                
                // Calculate damage needed to kill
                float damageNeeded = currentHP;
                carController.TakeDamage(damageNeeded);
                
                // Wait a short time for event processing
                yield return new WaitForSeconds(0.1f);
                
                // Check if car destroyed event was received
                if (!carDestroyedEventReceived)
                {
                    Debug.LogError("[HealthSystemTester] Car destroyed event was not received!");
                    testResult = "FAIL: Car destroyed event was not received!";
                    isTestRunning = false;
                    yield break;
                }
                
                // Check if HP is zero
                currentHP = carController.CurrentHP;
                if (!Mathf.Approximately(currentHP, 0f))
                {
                    Debug.LogError($"[HealthSystemTester] HP should be zero after destruction! Got: {currentHP}");
                    testResult = $"FAIL: HP should be zero after destruction! Got: {currentHP}";
                    isTestRunning = false;
                    yield break;
                }
                
                Debug.Log("[HealthSystemTester] Car destruction test passed!");
                testResult = "Car destruction test passed!";
            }
            
            // Tests completed successfully
            Debug.Log("[HealthSystemTester] All health system tests passed!");
            testResult = "All health system tests passed!";
            isTestRunning = false;
        }
        
        private void OnCarHPChanged(float newHP)
        {
            hpChangedEventReceived = true;
            Debug.Log($"[HealthSystemTester] Received HP changed event: {newHP}");
        }
        
        private void OnCarDestroyed()
        {
            carDestroyedEventReceived = true;
            Debug.Log("[HealthSystemTester] Received car destroyed event!");
        }
        
        /// <summary>
        /// Stop ongoing tests
        /// </summary>
        public void StopTests()
        {
            if (!isTestRunning)
                return;
                
            isTestRunning = false;
            StopAllCoroutines();
            
            testResult = "Health system test stopped manually.";
            Debug.Log("[HealthSystemTester] Test stopped manually");
        }
        
        // Helper buttons for the inspector
        [ContextMenu("Start Health Tests")]
        private void StartTestsFromInspector()
        {
            StartTests();
        }
        
        [ContextMenu("Stop Health Tests")]
        private void StopTestsFromInspector()
        {
            StopTests();
        }
    }
} 