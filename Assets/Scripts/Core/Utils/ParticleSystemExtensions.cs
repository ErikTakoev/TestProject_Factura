using UnityEngine;

namespace TestProject_Factura
{
    public static class ParticleSystemExtensions
    {
        private static Gradient GetGradient(GradientColorKey[] colors, GradientAlphaKey[] alphas)
        {
            Gradient gradient = new Gradient();

            gradient.SetKeys(colors, alphas);
            return gradient;
        }
        public static void SetGradient(this ParticleSystem particle, GradientColorKey[] colors, GradientAlphaKey[] alphas)
        {
            var colorOverLifetime = particle.colorOverLifetime;
            colorOverLifetime.enabled = true;

            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(GetGradient(colors, alphas));
        }

        public static void SetGradient(this TrailRenderer trail, GradientColorKey[] colors, GradientAlphaKey[] alphas)
        {
            trail.colorGradient = GetGradient(colors, alphas);
        }
    }
}