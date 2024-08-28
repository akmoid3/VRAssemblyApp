using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationQuit
{
    public void QuitApplication()
    {
#if UNITY_EDITOR
        // Ferma la modalità di gioco in editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Chiudi l'applicazione quando è eseguita in build
        Application.Quit();
#endif
    }
}
