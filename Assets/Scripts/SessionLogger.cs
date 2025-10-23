using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SessionLogger : MonoBehaviour
{
    [Header("Hook these in Inspector")]
    public SessionManager session;
    public PlaceOnPlane placer;

    [Header("Optional HUD (assign if you like)")]
    public Text hudText;

    DateTime startUtc;
    DateTime endUtc;
    bool running;

    int placements, connects, detaches, flowStarts, flowStops;

    CouplingAnimator coupling;

    // strong refs
    Action<GameObject> placedHandler;
    Action connectedHandler;
    Action<bool> flowHandler;
    Action detachedHandler;

    // keep the last payload so UI can display a summary
    SessionPayload lastPayload;

    void Awake()
    {
        placedHandler = OnPlaced;
        connectedHandler = () => { connects++; UpdateHud(); };
        flowHandler = started => { if (started) flowStarts++; else flowStops++; UpdateHud(); };
        detachedHandler = () => { detaches++; UpdateHud(); };
    }

    void OnEnable()
    {
        if (session) session.OnModeChosen += OnModeChosen;
        if (placer) placer.OnPlacedObject += placedHandler;
    }

    void OnDisable()
    {
        if (session) session.OnModeChosen -= OnModeChosen;
        if (placer) placer.OnPlacedObject -= placedHandler;
        UnhookCoupling();
        if (running) StopAndSave();
    }

    void UpdateHud()
    {
        if (!hudText) return;
        var dur = running ? (DateTime.UtcNow - startUtc) : (endUtc - startUtc);
        hudText.text = $"⏱ {(int)dur.TotalSeconds}s  |  📍 {placements}  |  🔒 {connects}  |  🔄 {detaches}  |  💧 {flowStarts}/{flowStops}";
    }

    void OnModeChosen(TrainingMode mode) => StartSession();

    public void StartSession()
    {
        placements = connects = detaches = flowStarts = flowStops = 0;
        startUtc = DateTime.UtcNow;
        running = true;
        UpdateHud();
        Debug.Log("[Logger] Session started");
    }

    public void StopAndSave()
    {
        if (!running) return;
        running = false;
        endUtc = DateTime.UtcNow;
        UpdateHud();

        lastPayload = new SessionPayload
        {
            playerName = SessionManager.I ? SessionManager.I.PlayerName : "Unknown",
            mode = SessionManager.I ? SessionManager.I.Mode.ToString() : "Unknown",
            startUtc = startUtc.ToString("o"),
            endUtc = endUtc.ToString("o"),
            durationSec = (endUtc - startUtc).TotalSeconds,
            placements = placements,
            connects = connects,
            detaches = detaches,
            flowStarts = flowStarts,
            flowStops = flowStops,
        };

        string json = JsonUtility.ToJson(lastPayload, true);
        string file = Path.Combine(Application.persistentDataPath,
                                   $"session_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
        try
        {
            File.WriteAllText(file, json);
            Debug.Log($"[Logger] Saved {file}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Logger] Failed to save log: {ex.Message}");
        }
    }

    public SessionPayload GetLastPayload() => lastPayload;

    void OnPlaced(GameObject go)
    {
        placements++;

        UnhookCoupling();
        coupling = go.GetComponentInChildren<CouplingAnimator>(true);
        if (coupling)
        {
            coupling.OnConnected += connectedHandler;
            coupling.OnFlow += flowHandler;
            coupling.OnDetached += detachedHandler;
        }
        UpdateHud();
    }

    void UnhookCoupling()
    {
        if (!coupling) return;
        coupling.OnConnected -= connectedHandler;
        coupling.OnFlow -= flowHandler;
        coupling.OnDetached -= detachedHandler;
        coupling = null;
    }

    public void StopSaveAndReset() => StopAndSave();

    // NOW PUBLIC so other scripts can reference it
    [Serializable]
    public class SessionPayload
    {
        public string playerName;
        public string mode;
        public string startUtc;
        public string endUtc;
        public double durationSec;
        public int placements;
        public int connects;
        public int detaches;
        public int flowStarts;
        public int flowStops;
    }
}
