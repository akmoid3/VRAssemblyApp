using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RecordManager : MonoBehaviour
{
    [SerializeField] private GameObject recordPanel;
    [SerializeField] private Button finishButton;


    private void Awake()
    {
        Manager.OnStateChanged += SetPanelActive;
        finishButton.onClick.AddListener(OnFinishClicked);
    }

    private void OnFinishClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        Manager.OnStateChanged -= SetPanelActive;
        finishButton.onClick.RemoveListener(OnFinishClicked);
    }

    private void SetPanelActive(State state)
    {
        recordPanel.SetActive(state == State.Record);
    }
}
