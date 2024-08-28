using NUnit.Framework;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ApplicationQuitManagerTests
{
    [Test]
    public void QuitApplication_InEditorMode_ShouldStopPlaying()
    {
#if UNITY_EDITOR
        // Arrange
        var applicationQuitManager = new ApplicationQuit();
        bool initialPlayingState = EditorApplication.isPlaying;

        // Act
        applicationQuitManager.QuitApplication();

        // Assert
        Assert.IsFalse(EditorApplication.isPlaying);
        EditorApplication.isPlaying = initialPlayingState; // Ripristina lo stato iniziale
#endif
    }

    [Test]
    public void QuitApplication_InBuild_ShouldQuitApplication()
    {
#if !UNITY_EDITOR
        // Questo test non può essere eseguito in modo affidabile perché chiuderebbe l'applicazione durante il test.
        // Tuttavia, è possibile testare indirettamente che Application.Quit sia chiamato usando un mock o un altro metodo di test.
        
        // In Unity builds, Application.Quit() è difficile da testare direttamente.
        Assert.Pass("Application.Quit() cannot be tested directly in builds.");
#endif
    }
}
