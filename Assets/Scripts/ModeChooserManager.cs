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
        Manager.OnStateChanged += SetPanelActive;
        playBackButton.onClick.AddListener(OnPlayBackButtonClicked);
        recordButton.onClick.AddListener(OnRecordButtonClicked);
        initializeButton.onClick.AddListener(OnInitializeClicked);
        returnBackButton.onClick.AddListener(OnReturnBackClicked);

    }

    private void OnDestroy()
    {
        Manager.OnStateChanged -= SetPanelActive;
        playBackButton.onClick.RemoveListener(OnPlayBackButtonClicked);
        recordButton.onClick.RemoveListener(OnRecordButtonClicked);
        initializeButton.onClick.RemoveListener(OnInitializeClicked);
        returnBackButton.onClick.RemoveListener(OnReturnBackClicked);

    }

    private void SetPanelActive(State state)
    {
        modeSelectionPanel.SetActive(state == State.SelectingMode);
        if (state == State.SelectingMode)
        {
            UpdateButtonStates();
        }
    }

    private void UpdateButtonStates()
    {
        string modelName = Manager.Instance.Model.name;
        string initializedModelsPath = Path.Combine(Application.persistentDataPath, "InitializedModels", modelName + ".json");
        string savedBuildDataPath = Path.Combine(Application.persistentDataPath, "SavedBuildData", modelName + ".json");

        recordButton.interactable = File.Exists(initializedModelsPath);
        playBackButton.interactable = File.Exists(savedBuildDataPath);
    }

    private void OnInitializeClicked()
    {
        Manager.Instance.UpdateState(State.Initialize);
    }

    private void OnPlayBackButtonClicked()
    {
        Manager.Instance.UpdateState(State.PlayBack);
    }

    private void OnRecordButtonClicked()
    {
        Manager.Instance.UpdateState(State.Record);
    }

    private void OnReturnBackClicked()
    {
        //Manager.Instance.UpdateState(State.ChoosingModel);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
