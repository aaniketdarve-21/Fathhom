using UnityEngine;

public class FlowParticlesController : MonoBehaviour
{
    public ParticleSystem ps;
    [Tooltip("Base particle size at model scale = 1.0")]
    public float baseSize = 0.2f;
    [Tooltip("Base particle speed at model scale = 1.0")]
    public float baseSpeed = 1.0f;

    // Call once after the model is spawned & scaled
    public void ApplyScale(float modelScale)
    {
        if (!ps) ps = GetComponent<ParticleSystem>();
        if (!ps) return;

        var main = ps.main;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;   // respect transform scale
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        main.startSizeMultiplier = Mathf.Max(0.005f, baseSize * modelScale * 0.2f);
        main.startSpeedMultiplier = Mathf.Max(0.01f, baseSpeed * modelScale * 0.2f);

        // Optional: also tighten the shape radius with scale
        var shape = ps.shape;
        shape.radius = Mathf.Max(0.001f, 0.02f * modelScale);
    }

    public void Play(bool on)
    {
        if (!ps) ps = GetComponent<ParticleSystem>();
        if (!ps) return;
        if (on) ps.Play(true); else ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
