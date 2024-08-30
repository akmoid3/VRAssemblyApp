using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RecordManager : MonoBehaviour
{
    [SerializeField] private GameObject recordPanel;
    [SerializeField] private Button finishButton;


    public virtual void Awake()
    {
        StateManager.OnStateChanged += SetPanelActive;
        if (finishButton != null)

            finishButton.onClick.AddListener(OnFinishClicked);
    }

    public void OnFinishClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log($"Loading scene {SceneManager.GetActiveScene().name}");
    }

    public virtual void OnDestroy()
    {
        StateManager.OnStateChanged -= SetPanelActive;
        if(finishButton != null )
        finishButton.onClick.RemoveListener(OnFinishClicked);
    }

    public void SetPanelActive(State state)
    {
        recordPanel.SetActive(state == State.Record);
    }
}
