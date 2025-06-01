using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestProject_Factura
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private TrailRenderer trail;

        private float damage;
        private float lifetime = 0.5f;
        private float lifeTimer;
        private ObjectPool<Bullet> pool;

        public void Initialize(Vector3 direction, float speed, float bulletDamage, ObjectPool<Bullet> bulletPool)
        {
            // Зберігаємо посилання на пул
            pool = bulletPool;

            // Встановлюємо параметри кулі
            damage = bulletDamage;
            lifeTimer = 0f;

            // Скидаємо фізику кулі
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
                rb.AddForce(direction.normalized * speed, ForceMode.VelocityChange);
            }

            // Скидаємо трейл, якщо він є
            if (trail != null)
            {
                // Generate a random color
                Color randomColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                Color random2Color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));

                trail.SetGradient(new GradientColorKey[] {
                        new GradientColorKey(randomColor, 0.0f),
                        new GradientColorKey(random2Color, 1.0f)
                    }, new GradientAlphaKey[] {
                        new GradientAlphaKey(1.0f, 0.0f),
                        new GradientAlphaKey(0.0f, 1.0f)
                    });

                trail.Clear();
                trail.enabled = true;
            }

            // Активуємо об'єкт
            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            lifeTimer = 0f;
        }

        private void Update()
        {
            // Відстежуємо час життя кулі
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifetime)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Перевіряємо, чи зіткнулися з ворогом
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                // Наносимо шкоду ворогу
                enemy.TakeDamage(damage).Forget();

                // Повертаємо кулю в пул
                ReturnToPool();
                return;
            }
        }

        private void ReturnToPool()
        {
            // Вимикаємо трейл, щоб він не відображався при повторному використанні кулі
            if (trail != null)
            {
                trail.enabled = false;
            }

            // Зупиняємо рух кулі
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // Повертаємо кулю в пул
            if (pool != null)
            {
                pool.Return(this);
            }
        }
    }
}