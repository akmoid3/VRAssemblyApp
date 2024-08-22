using UnityEngine;
using UnityEngine.UI;

public class ModeSelectionManager : MonoBehaviour
{
    [SerializeField] private GameObject modelSelectionPanel;
    [SerializeField] private Button confirmButton;
    private void Awake()
    {
        StateManager.OnStateChanged += SetPanelActive;
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    private void OnConfirmButtonClicked()
    {
        StateManager.Instance.UpdateState(State.SelectingMode);
    }

    private void OnDestroy()
    {
        StateManager.OnStateChanged -= SetPanelActive;
        confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
    }
    private void SetPanelActive(State state)
    {
        modelSelectionPanel.SetActive(state == State.ChoosingModel);
    }


}
