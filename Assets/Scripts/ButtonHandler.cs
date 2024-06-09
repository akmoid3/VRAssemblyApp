using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public void OnRecordButtonClicked()
    {
        // Codice per registrare una nuova sequenza di assemblaggio
        Debug.Log("Record a New Assembly Sequence button clicked");
    }

    public void OnPlaybackButtonClicked()
    {
        // Codice per riprodurre una sequenza esistente
        Debug.Log("Play an Existing Sequence button clicked");
    }
}
