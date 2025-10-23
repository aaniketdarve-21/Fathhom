using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StepHintUI : MonoBehaviour
{
    [Header("Assign EITHER TMP or UGUI text")]
    public TMP_Text tmpHintText;     // if using TextMeshProUGUI
    public Text uguiHintText;        // if using legacy Text

    [Header("Buttons (all are children of this panel)")]
    public Button prevButton;
    public Button doStepButton;
    public Button nextButton;
    public Button backButton;        // <- NEW: “Go Back” inside the panel

    public System.Action OnPrev, OnDoStep, OnNext, OnBack;

    void Awake()
    {
        if (prevButton) prevButton.onClick.AddListener(() => OnPrev?.Invoke());
        if (doStepButton) doStepButton.onClick.AddListener(() => OnDoStep?.Invoke());
        if (nextButton) nextButton.onClick.AddListener(() => OnNext?.Invoke());
        if (backButton) backButton.onClick.AddListener(() => OnBack?.Invoke());
    }

    public void SetHint(string msg)
    {
        if (tmpHintText) tmpHintText.text = msg;
        if (uguiHintText) uguiHintText.text = msg;
    }

    public void SetVisible(bool v) => gameObject.SetActive(v);
    public void SetButtonsInteractable(bool v)
    {
        if (prevButton) prevButton.interactable = v;
        if (doStepButton) doStepButton.interactable = v;
        if (nextButton) nextButton.interactable = v;
        if (backButton) backButton.interactable = v;
    }
}
