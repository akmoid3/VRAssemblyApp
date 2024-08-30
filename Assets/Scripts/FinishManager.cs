using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinishManager : MonoBehaviour
{
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private Button finishButton;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI errorCountText;
    [SerializeField] private TextMeshProUGUI hintCountText;
    private void Awake()
    {
        StateManager.OnStateChanged += SetPanelActive;
        if(finishButton != null)
        finishButton.onClick.AddListener(OnFinishClicked);
    }

    public void Update()
    {
        if (StateManager.Instance.CurrentState == State.Finish && timerText.text == "00:00")
            timerText.text = Manager.Instance.FinishTime;
    }
    private void OnDestroy()
    {
        StateManager.OnStateChanged -= SetPanelActive;
        finishButton.onClick.RemoveListener(OnFinishClicked);
    }

    public void SetPanelActive(State state)
    {
        finishPanel.SetActive(state == State.Finish);
        if (state == State.Finish)
        {
            errorCountText.text = Manager.Instance.ErrorCount.ToString();
            hintCountText.text = Manager.Instance.HintCount.ToString();
        }
    }

    public void OnFinishClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
