using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeSelectionManager : MonoBehaviour
{
    [SerializeField] private GameObject modelSelectionPanel;
    [SerializeField] private Button confirmButton;
    private void Awake()
    {
        Manager.OnStateChanged += SetPanelActive;
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    private void OnConfirmButtonClicked()
    {
        Manager.Instance.UpdateState(State.SelectingMode);
    }

    private void OnDestroy()
    {
        Manager.OnStateChanged -= SetPanelActive;
        confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
    }
    private void SetPanelActive(State state)
    {
        modelSelectionPanel.SetActive(state == State.ChoosingModel);
    }


}
