using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModeChooserManager : MonoBehaviour
{
    [SerializeField] private GameObject modeSelectionPanel;
    [SerializeField] private Button playBackButton;
    [SerializeField] private Button recordButton;
    [SerializeField] private Button initializeButton;
    [SerializeField] private Button returnBackButton;


    private void Awake()
    {
        StateManager.OnStateChanged += SetPanelActive;
        if (playBackButton != null)
            playBackButton.onClick.AddListener(OnPlayBackButtonClicked);
        if (recordButton != null)
            recordButton.onClick.AddListener(OnRecordButtonClicked);
        if (initializeButton != null)
            initializeButton.onClick.AddListener(OnInitializeClicked);
        if (returnBackButton != null)
            returnBackButton.onClick.AddListener(OnReturnBackClicked);

    }

    private void OnDestroy()
    {
        StateManager.OnStateChanged -= SetPanelActive;
        playBackButton.onClick.RemoveListener(OnPlayBackButtonClicked);
        recordButton.onClick.RemoveListener(OnRecordButtonClicked);
        initializeButton.onClick.RemoveListener(OnInitializeClicked);
        returnBackButton.onClick.RemoveListener(OnReturnBackClicked);

    }

    public void SetPanelActive(State state)
    {
        modeSelectionPanel.SetActive(state == State.SelectingMode);
        if (state == State.SelectingMode)
        {
            UpdateButtonStates();
        }
    }

    public void UpdateButtonStates()
    {
        string modelName = Manager.Instance.Model.name;
        string initializedModelsPath = Path.Combine(Application.persistentDataPath, "InitializedModels", modelName + ".json");
        string savedBuildDataPath = Path.Combine(Application.persistentDataPath, "SavedBuildData", modelName + ".json");

        recordButton.interactable = File.Exists(initializedModelsPath);
        playBackButton.interactable = File.Exists(savedBuildDataPath);
    }

    public void OnInitializeClicked()
    {
        StateManager.Instance.UpdateState(State.Initialize);
    }

    public void OnPlayBackButtonClicked()
    {
        StateManager.Instance.UpdateState(State.PlayBack);
    }

    public void OnRecordButtonClicked()
    {
        StateManager.Instance.UpdateState(State.Record);
    }

    private void OnReturnBackClicked()
    {
        //Manager.Instance.UpdateState(State.ChoosingModel);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
