using UnityEngine;

public class RuntimeDebugPanel : MonoBehaviour
{
    CouplingAnimator anim;

    void Update()
    {
#if UNITY_2023_1_OR_NEWER
        if (!anim) anim = FindFirstObjectByType<CouplingAnimator>();
#else
        if (!anim) anim = FindObjectOfType<CouplingAnimator>();
#endif
    }

    void OnGUI()
    {
        if (!anim) return;

        const int w = 160, h = 42;
        int x = 20, y = 20, pad = 6;

        if (GUI.Button(new Rect(x, y, w, h), "Connect")) anim.PlayConnect(); y += h + pad;
        if (GUI.Button(new Rect(x, y, w, h), "Flow")) anim.PlayFlow(); y += h + pad;
        if (GUI.Button(new Rect(x, y, w, h), "Stop")) anim.PlayStop(); y += h + pad;
        if (GUI.Button(new Rect(x, y, w, h), "Detach")) anim.PlayDetach();
    }
}
