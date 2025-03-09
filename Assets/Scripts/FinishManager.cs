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
    [SerializeField] private TextMeshProUGUI averagePerformanceText;

    private void Awake()
    {
        StateManager.OnStateChanged += SetPanelActive;
        if(finishButton != null)
        finishButton.onClick.AddListener(OnFinishClicked);
    }

    public void Update()
    {
        if (StateManager.Instance.CurrentState == State.Finish && timerText.text == "00:00")
            timerText.text = $"Time: {Manager.Instance.FinishTime}";
    }
    private void OnDestroy()
    {
        StateManager.OnStateChanged -= SetPanelActive;
        if (finishButton != null)
            finishButton.onClick.RemoveListener(OnFinishClicked);
    }
    

    public void SetPanelActive(State state)
    {

        finishPanel.SetActive(state == State.Finish);
        if (state == State.Finish)
        {
            errorCountText.text = Manager.Instance.ErrorCount.ToString();
            hintCountText.text = Manager.Instance.HintCount.ToString();

            errorCountText.text = $"Errors: {Manager.Instance.ErrorCount}";
            hintCountText.text = $"Hints: {Manager.Instance.HintCount}";
            averagePerformanceText.text = $"Accuracy: {CalculateAveragePerformance() * 100:F2}";
        }
    }

    public float CalculateAveragePerformance()
    {
        float performance = 0f;
        bool firstElement = true;
        int count = 0;
    
        foreach (var var in Manager.Instance.PerformaceForEachStep)
        {
            if (firstElement)
            {
                firstElement = false;
                continue;
            }
        
            performance += var;
            count++;
        }
    
        // Check to avoid division by zero
        if (count == 0)
            return 0f;
        
        float average = performance / count;
    
        return average;
    }


    public void OnFinishClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
