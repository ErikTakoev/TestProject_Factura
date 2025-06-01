using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace TestProject_Factura
{
    public enum CameraMode
    {
        Initial,
        MoveToFollow,
        Follow
    }

    public class CameraController : MonoBehaviour, ICameraController
    {
        [Header("Camera Settings")]
        [SerializeField] private Camera mainCamera;

        [Header("Follow Mode")]
        [SerializeField] private Vector3 followOffset = new Vector3(0, 7, -10);
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float rotationSpeed = 3f;
        [SerializeField] private float lookAheadDistance = 5f;

        [Header("Transition")]
        [SerializeField] private float transitionDuration = 1.5f;

        private Transform targetTransform;
        private Vector3 velocity = Vector3.zero;
        private CameraMode currentMode = CameraMode.Initial;

        [Inject]
        private void Construct()
        {
            // Ін'єкція не потрібна, але метод вимагається інтерфейсом
        }

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = GetComponent<Camera>();

                if (mainCamera == null)
                {
                    mainCamera = Camera.main;
                }
            }
        }

        private void LateUpdate()
        {
            // Оновлюємо положення камери в режимі стеження
            if (currentMode == CameraMode.Follow && targetTransform != null)
            {
                UpdateFollowPosition();
            }
        }

        public async UniTask MoveToFollowPosition()
        {
            // Зберігаємо поточне положення та орієнтацію камери
            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;

            // Визначаємо цільове положення та орієнтацію
            Vector3 targetPosition;
            Quaternion targetRotation;

            if (targetTransform == null)
            {
                Debug.LogWarning("No target transform set for follow mode!");
                return;
            }
            currentMode = CameraMode.MoveToFollow;

            // Обчислюємо цільову позицію для режиму стеження
            targetPosition = targetTransform.position + followOffset;

            // Розраховуємо цільову орієнтацію для спостереження за точкою попереду автомобіля
            Vector3 lookAtPosition = targetTransform.position + targetTransform.forward * lookAheadDistance;
            Vector3 direction = lookAtPosition - targetPosition;
            targetRotation = Quaternion.LookRotation(direction);

            float elapsedTime = 0;

            while (elapsedTime < transitionDuration)
            {
                float t = elapsedTime / transitionDuration;
                t = Mathf.SmoothStep(0, 1, t); // Згладжуємо перехід

                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

                elapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }

            // Встановлюємо точне кінцеве положення та орієнтацію
            transform.position = targetPosition;
            transform.rotation = targetRotation;

            currentMode = CameraMode.Follow;
        }

        public async UniTask SwitchToFollowMode(Transform target)
        {
            targetTransform = target;
            await MoveToFollowPosition();
        }

        private void UpdateFollowPosition()
        {
            // Обчислюємо цільову позицію для режиму стеження
            Vector3 targetPosition = targetTransform.position + followOffset;

            // Плавно переміщуємо камеру до цільової позиції
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / followSpeed);

            // Розраховуємо цільову орієнтацію для спостереження за точкою попереду автомобіля
            Vector3 lookAtPosition = targetTransform.position + targetTransform.forward * lookAheadDistance;
            Quaternion targetRotation = Quaternion.LookRotation(lookAtPosition - transform.position);

            // Плавно повертаємо камеру до цільової орієнтації
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}