using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayBackManager : MonoBehaviour
{
    public GameObject playBackPanel;
    public Button finishButton;
    public Button showSolutionButton;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI errorCountText;
    public TextMeshProUGUI hintCountText;
    public TextMeshProUGUI stepsText;

    private float elapsedTime = 0f;
    private bool isPlayingBack = false;

    public float ElapsedTime { get => elapsedTime; set => elapsedTime = value; }
    public bool IsPlayingBack { get => isPlayingBack; set => isPlayingBack = value; }

    private void Awake()
    {
        StateManager.OnStateChanged += SetPanelActive;
        SequenceManager.OnErrorCountChanged += IncrementErrorCount;
        HintManager.OnHintCountChanged += IncrementHintCount;
        SequenceManager.OnStepChanged += IncrementStepCount;
        if(showSolutionButton != null)
        showSolutionButton.onClick.AddListener(OnShowSolutionClicked);
        if(finishButton != null) 
        finishButton.onClick.AddListener(OnFinishClicked);
    }

    public void IncrementErrorCount(int n)
    {
        errorCountText.text = n.ToString();
    }

    public void IncrementHintCount(int n)
    {
        hintCountText.text = n.ToString();
    }
    public void IncrementStepCount(int n)
    {
        int totalSteps = Manager.Instance.AssemblySequence.Count;
        stepsText.text = $"{n}/{totalSteps}";
    }
    public void OnDestroy()
    {
        StateManager.OnStateChanged -= SetPanelActive;
        SequenceManager.OnErrorCountChanged -= IncrementErrorCount;
        HintManager.OnHintCountChanged -= IncrementHintCount;
        SequenceManager.OnStepChanged -= IncrementStepCount;

        showSolutionButton.onClick.RemoveListener(OnShowSolutionClicked);
        finishButton.onClick.RemoveListener(OnFinishClicked);
    }

    public void Update()
    {
        if (isPlayingBack)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    public void SetPanelActive(State state)
    {
        playBackPanel.SetActive(state == State.PlayBack);
        isPlayingBack = (state == State.PlayBack);
        if (isPlayingBack)
        {
            elapsedTime = 0f; // Reset timer when playback starts
        }
        else if (state == State.Finish)
        {
            Manager.Instance.FinishTime = timerText.text;
        }
    }

    private void OnFinishClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UpdateTimerDisplay()
    {
        // Format the time into minutes and seconds
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

    public void OnShowSolutionClicked()
    {
        // Call the method to place all components gradually
        Manager.Instance.PlaceAllComponentsGradually(0.75f);
    }

}
