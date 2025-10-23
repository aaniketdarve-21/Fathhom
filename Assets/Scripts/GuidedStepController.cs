using System;
using UnityEngine;

public class GuidedStepController : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public SessionManager session;
    public PlaceOnPlane placer;
    public StepHintUI ui;
    public HidePlanesAfterPlacement planeHider;

    CouplingAnimator coupling;

    enum Step { Place, Connect, Flow, StopFlow, Detach, Done }
    Step step = Step.Place;

    Action<GameObject> placedHandler;
    Action connectedHandler;
    Action<bool> flowHandler;
    Action detachedHandler;

    TrainingMode currentMode = TrainingMode.Guided;

    void Awake()
    {
        placedHandler = OnPlacedObject;
        connectedHandler = () => AutoAdvance(Step.Connect);
        flowHandler = started => AutoAdvance(started ? Step.Flow : Step.StopFlow);
        detachedHandler = () => AutoAdvance(Step.Detach);
    }

    void Start()
    {
        SetARActive(false);    // disable AR while on Login
        if (ui) ui.SetVisible(false);
    }

    void OnEnable()
    {
        if (session) session.OnModeChosen += OnModeChosen;
        if (placer) placer.OnPlacedObject += placedHandler;

        if (ui)
        {
            ui.OnPrev = PrevStep;
            ui.OnNext = NextStep;
            ui.OnDoStep = DoCurrentStep;
            ui.OnBack = OnGoBack;
        }
    }

    void OnDisable()
    {
        if (session) session.OnModeChosen -= OnModeChosen;
        if (placer) placer.OnPlacedObject -= placedHandler;
        UnhookCoupling();

        if (ui) { ui.OnPrev = ui.OnNext = ui.OnDoStep = ui.OnBack = null; }
    }

    // ===== Mode chosen from Login =====
    void OnModeChosen(TrainingMode mode)
    {
        currentMode = mode;
        SetARActive(true);   // only enable AR after a mode is picked

        if (mode == TrainingMode.Guided)
        {
            if (ui) ui.SetVisible(true);
            ShowStep(Step.Place);
        }
        else // Assessment
        {
            if (ui) ui.SetVisible(false);   // no hint panel in Assessment
            // planes left visible so user can place
        }
    }

    // ===== AR Placement =====
    void OnPlacedObject(GameObject go)
    {
        coupling = go.GetComponentInChildren<CouplingAnimator>(true);
        if (!coupling) { Debug.LogError("[Guided] CouplingAnimator not found!"); return; }

        coupling.OnConnected += connectedHandler;
        coupling.OnFlow += flowHandler;
        coupling.OnDetached += detachedHandler;

        AutoAdvance(Step.Place);
    }

    void UnhookCoupling()
    {
        if (!coupling) return;
        coupling.OnConnected -= connectedHandler;
        coupling.OnFlow -= flowHandler;
        coupling.OnDetached -= detachedHandler;
        coupling = null;
    }

    // ===== Step UI Flow (Guided only) =====
    void ShowStep(Step s)
    {
        step = s;
        if (currentMode != TrainingMode.Guided || !ui) return;

        ui.SetVisible(true);

        switch (step)
        {
            case Step.Place: ui.SetHint("Step 1/5: Tap a plane to PLACE the coupling."); break;
            case Step.Connect: ui.SetHint("Step 2/5: Press Do Step to CONNECT & LOCK."); break;
            case Step.Flow: ui.SetHint("Step 3/5: Press Do Step to START FLOW."); break;
            case Step.StopFlow: ui.SetHint("Step 4/5: Press Do Step to STOP FLOW."); break;
            case Step.Detach: ui.SetHint("Step 5/5: Press Do Step to DETACH."); break;
            case Step.Done: ui.SetHint("All done! You can repeat or try Assessment."); break;
        }
    }

    void DoCurrentStep()
    {
        if (currentMode != TrainingMode.Guided || step == Step.Place) return;

        ui.SetVisible(false); // hide while performing step

        if (!coupling)
        {
            Debug.Log("[Guided] Place model first!");
            ui.SetVisible(true);
            return;
        }

        switch (step)
        {
            case Step.Connect: coupling.PlayConnect(); break;
            case Step.Flow: coupling.PlayFlow(); break;
            case Step.StopFlow: coupling.PlayStop(); break;
            case Step.Detach: coupling.PlayDetach(); break;
        }
    }

    void NextStep() => ShowStep(step switch
    {
        Step.Place => Step.Connect,
        Step.Connect => Step.Flow,
        Step.Flow => Step.StopFlow,
        Step.StopFlow => Step.Detach,
        Step.Detach => Step.Done,
        _ => Step.Done
    });

    void PrevStep() => ShowStep(step switch
    {
        Step.Connect => Step.Place,
        Step.Flow => Step.Connect,
        Step.StopFlow => Step.Flow,
        Step.Detach => Step.StopFlow,
        Step.Done => Step.Detach,
        _ => Step.Place
    });

    void AutoAdvance(Step completed)
    {
        if (step == completed) NextStep();

        // 👉 Auto-show summary at end of ASSESSMENT (after Detach)
        if (currentMode == TrainingMode.Assessment && completed == Step.Detach)
        {
            EndAssessmentAndShowSummary();
        }
    }

    // ===== Back -> Login =====
    void OnGoBack()
    {
        Debug.Log("[UI] Go Back → Login reset");

#if UNITY_2023_1_OR_NEWER
        var logger = UnityEngine.Object.FindAnyObjectByType<SessionLogger>();
        var summary = UnityEngine.Object.FindAnyObjectByType<SessionSummaryUI>();
#else
        var logger  = UnityEngine.Object.FindObjectOfType<SessionLogger>();
        var summary = UnityEngine.Object.FindObjectOfType<SessionSummaryUI>();
#endif
        SessionLogger.SessionPayload payload = null;
        if (logger)
        {
            logger.StopSaveAndReset();
            payload = logger.GetLastPayload();
        }

        ClearPlacedObjectAndPlanes();

        if (summary && payload != null) summary.ShowSummary(payload);

        if (session) session.ShowLogin(true);
        if (ui) ui.SetVisible(false);
        currentMode = TrainingMode.Guided;
        SetARActive(false);   // lock AR while on login
    }

    // ===== Helpers =====
    void EndAssessmentAndShowSummary()
    {
#if UNITY_2023_1_OR_NEWER
        var logger = UnityEngine.Object.FindAnyObjectByType<SessionLogger>();
        var summary = UnityEngine.Object.FindAnyObjectByType<SessionSummaryUI>();
#else
        var logger  = UnityEngine.Object.FindObjectOfType<SessionLogger>();
        var summary = UnityEngine.Object.FindObjectOfType<SessionSummaryUI>();
#endif
        SessionLogger.SessionPayload payload = null;
        if (logger)
        {
            logger.StopSaveAndReset();
            payload = logger.GetLastPayload();
        }

        if (summary && payload != null) summary.ShowSummary(payload);

        ClearPlacedObjectAndPlanes();
        if (session) session.ShowLogin(true);
        SetARActive(false);
        currentMode = TrainingMode.Guided;
    }

    void ClearPlacedObjectAndPlanes()
    {
        if (placer) placer.ResetPlacement();
        UnhookCoupling();
        // If your plane hider has a HidePlanes(), call it here; otherwise do nothing.
    }

    void SetARActive(bool on)
    {
        if (placer) placer.enabled = on;

        if (planeHider)
        {
            if (on) planeHider.ShowPlanes();
            // else: optionally planeHider.HidePlanes();
        }
    }
}
