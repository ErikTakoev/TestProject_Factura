using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestProject_Factura
{
    /// <summary>
    /// Test script for verifying input system functionality
    /// </summary>
    public class InputTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private Transform testIndicator; // Visual indicator for input position
        
        [Header("Test Configuration")]
        [SerializeField] private bool autoStartTest = false;
        [SerializeField] private bool visualizeInput = true;
        [SerializeField] private bool logInputData = true;
        [SerializeField] private float testDuration = 10f;
        
        [Header("Test Status")]
        [SerializeField] private bool isTestRunning = false;
        [SerializeField] private string testResult = "";
        [SerializeField] private Vector3 lastInputPosition;
        [SerializeField] private bool lastShootingState;
        
        private void Start()
        {
            if (autoStartTest)
            {
                StartTests();
            }
        }
        
        /// <summary>
        /// Start input system tests
        /// </summary>
        public void StartTests()
        {
            if (isTestRunning)
                return;
                
            isTestRunning = true;
            testResult = "Starting input tests...";
            
            // Get input manager reference if not assigned
            if (inputManager == null)
            {
                inputManager = FindObjectOfType<InputManager>();
                
                if (inputManager == null)
                {
                    Debug.LogError("[InputTester] Input manager not found in the scene!");
                    testResult = "FAIL: Input manager not found in the scene!";
                    isTestRunning = false;
                    return;
                }
            }
            
            // Create test indicator if needed and enabled
            if (visualizeInput && testIndicator == null)
            {
                GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                indicator.name = "InputTestIndicator";
                indicator.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                
                // Make it stand out
                Renderer renderer = indicator.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
                
                testIndicator = indicator.transform;
            }
            
            // Enable input and start monitoring
            inputManager.EnableInput();
            StartCoroutine(MonitorInputForDuration());
        }
        
        private IEnumerator MonitorInputForDuration()
        {
            float startTime = Time.time;
            
            while (Time.time - startTime < testDuration && isTestRunning)
            {
                // Get current input position
                lastInputPosition = inputManager.GetWorldPosition();
                
                // Get current shooting state
                lastShootingState = inputManager.IsShooting();
                
                // Update test indicator position
                if (visualizeInput && testIndicator != null)
                {
                    testIndicator.position = lastInputPosition;
                }
                
                // Log data if enabled
                if (logInputData)
                {
                    Debug.Log($"[InputTester] Position: {lastInputPosition}, Shooting: {lastShootingState}");
                }
                
                // Update status
                testResult = $"Input Position: {lastInputPosition}, Shooting: {lastShootingState}";
                
                yield return null;
            }
            
            // Test complete
            testResult = "Input test complete.";
            Debug.Log("[InputTester] Input monitoring complete");
            
            isTestRunning = false;
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
            
            // Clean up indicator
            if (testIndicator != null && testIndicator.name == "InputTestIndicator")
            {
                Destroy(testIndicator.gameObject);
                testIndicator = null;
            }
            
            testResult = "Input test stopped manually.";
            Debug.Log("[InputTester] Input test stopped manually");
        }
        
        private void OnDestroy()
        {
            // Clean up indicator
            if (testIndicator != null && testIndicator.name == "InputTestIndicator")
            {
                Destroy(testIndicator.gameObject);
            }
        }
        
        // Helper buttons for the inspector
        [ContextMenu("Start Input Test")]
        private void StartTestsFromInspector()
        {
            StartTests();
        }
        
        [ContextMenu("Stop Input Test")]
        private void StopTestsFromInspector()
        {
            StopTests();
        }
    }
} 