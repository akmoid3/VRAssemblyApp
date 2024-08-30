using UnityEngine;
using UnityEngine.UI;

public class ModelSelectionManager : MonoBehaviour
{
    [SerializeField] private GameObject modelSelectionPanel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button loadModel;
    [SerializeField] private FileBrowserManager fileBrowserManager;


    public void Awake()
    {
        StateManager.OnStateChanged += SetPanelActive;
        if(confirmButton != null)
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        if(loadModel != null)
        loadModel.onClick.AddListener(ShowFileBrowser);
    }

    public void OnConfirmButtonClicked()
    {
        StateManager.Instance.UpdateState(State.SelectingMode);
    }

    public void OnDestroy()
    {
        StateManager.OnStateChanged -= SetPanelActive;
        confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        loadModel.onClick.RemoveListener(ShowFileBrowser);

    }
    public void SetPanelActive(State state)
    {
        modelSelectionPanel.SetActive(state == State.ChoosingModel);
    }

    public void ShowFileBrowser()
    {
        fileBrowserManager.ShowDialog("Models", ".glb");
    }

}
