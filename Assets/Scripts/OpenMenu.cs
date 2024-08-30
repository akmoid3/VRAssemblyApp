using UnityEngine;

public class OpenMenu : MonoBehaviour
{
    [SerializeField] private GameObject m_Menu;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Menu.SetActive(!m_Menu.activeSelf);
        }
    }
}
