using TMPro;
using UnityEngine;

namespace TestProject_Factura
{
    public class GameStatistics : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI enemiesKilledText;
        [SerializeField] TextMeshProUGUI bulletsFiredText;
        [SerializeField] TextMeshProUGUI particlesCreatedText;
        [SerializeField] TextMeshProUGUI uniTaskCreatedText;

        static int totalEnemiesKilled;
        static int totalBulletsFired;
        static int totalParticlesCreated;
        static int totalUniTaskCreated;


        void OnEnable()
        {
            enemiesKilledText.text = totalEnemiesKilled.ToString();
            bulletsFiredText.text = totalBulletsFired.ToString();
            particlesCreatedText.text = totalParticlesCreated.ToString();
            uniTaskCreatedText.text = totalUniTaskCreated.ToString();
        }

        public static void IncrementEnemiesKilled()
        {
            totalEnemiesKilled++;
        }

        public static void IncrementBulletsFired()
        {
            totalBulletsFired++;
        }

        public static void IncrementParticlesCreated()
        {
            totalParticlesCreated++;
        }

        public static void IncrementUniTaskCreated()
        {
            totalUniTaskCreated++;
        }

        public static void ResetStatistics()
        {
            totalEnemiesKilled = 0;
            totalBulletsFired = 0;
            totalParticlesCreated = 0;
            totalUniTaskCreated = 0;
        }

    }
}