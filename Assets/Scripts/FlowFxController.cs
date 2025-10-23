using UnityEngine;

public class FlowFxController : MonoBehaviour
{
    [Header("Assign")]
    public CouplingAnimator coupling;   // drag from same prefab root
    public ParticleSystem flowFx;       // drag the FlowFX particle system

    void Awake()
    {
        if (!coupling) coupling = GetComponentInParent<CouplingAnimator>(true);
        if (!flowFx) flowFx = GetComponentInChildren<ParticleSystem>(true);
    }

    void OnEnable()
    {
        if (coupling != null) coupling.OnFlow += HandleFlow;
    }

    void OnDisable()
    {
        if (coupling != null) coupling.OnFlow -= HandleFlow;
    }

    void HandleFlow(bool start)
    {
        if (!flowFx) return;
        if (start) flowFx.Play();
        else flowFx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
