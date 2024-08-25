using UnityEngine;
using UnityEngine.UI;

public class ModelSelectionManager : MonoBehaviour
{
    [SerializeField] private GameObject modelSelectionPanel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button loadModel;
    [SerializeField] private FileBrowserManager fileBrowserManager;


    private void Awake()
    {
        StateManager.OnStateChanged += SetPanelActive;
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        loadModel.onClick.AddListener(ShowFileBrowser);
    }

    private void OnConfirmButtonClicked()
    {
        StateManager.Instance.UpdateState(State.SelectingMode);
    }

    private void OnDestroy()
    {
        StateManager.OnStateChanged -= SetPanelActive;
        confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        loadModel.onClick.RemoveListener(ShowFileBrowser);

    }
    private void SetPanelActive(State state)
    {
        modelSelectionPanel.SetActive(state == State.ChoosingModel);
    }

    private void ShowFileBrowser()
    {
        fileBrowserManager.ShowDialog("Models", ".glb");
    }

}
