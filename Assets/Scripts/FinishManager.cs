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
        Manager.OnStateChanged += SetPanelActive;
        finishButton.onClick.AddListener(OnFinishClicked);
    }

    private void Update()
    {
        if (Manager.Instance.State == State.Finish && timerText.text == "00:00")
            timerText.text = Manager.Instance.FinishTime;
    }
    private void OnDestroy()
    {
        Manager.OnStateChanged -= SetPanelActive;
        finishButton.onClick.RemoveListener(OnFinishClicked);
    }

    private void SetPanelActive(State state)
    {
        finishPanel.SetActive(state == State.Finish);
        if (state == State.Finish)
        {
            errorCountText.text = Manager.Instance.ErrorCount.ToString();
            hintCountText.text = Manager.Instance.HintCount.ToString();
        }
    }

    private void OnFinishClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
