using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayBackManager : MonoBehaviour
{
    [SerializeField] private GameObject playBackPanel;
    [SerializeField] private Button finishButton;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI errorCountText;


    private float elapsedTime = 0f; 
    private bool isPlayingBack = false; 

    private void Awake()
    {
        Manager.OnStateChanged += SetPanelActive;
        Manager.OnErrorCountChanged += IncrementErrorCount;
        finishButton.onClick.AddListener(OnFinishClicked);
    }

    private void IncrementErrorCount(int n)
    {
        errorCountText.text = n.ToString();
    }

    private void OnDestroy()
    {
        Manager.OnStateChanged -= SetPanelActive;
        Manager.OnErrorCountChanged -= IncrementErrorCount;
        finishButton.onClick.RemoveListener(OnFinishClicked);
    }

    private void Update()
    {
        if (isPlayingBack)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    private void SetPanelActive(State state)
    {
        playBackPanel.SetActive(state == State.PlayBack);
        isPlayingBack = (state == State.PlayBack);
        if (isPlayingBack)
        {
            elapsedTime = 0f; // Reset timer when playback starts
        }
    }

    private void OnFinishClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateTimerDisplay()
    {
        // Format the time into minutes and seconds
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }
}
