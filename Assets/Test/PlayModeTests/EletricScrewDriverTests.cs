using UnityEngine;
using NUnit.Framework;
using UnityEngine.XR.Interaction.Toolkit;
using Moq;
using UnityEngine.UIElements;

public class ElectricScrewDriverTests
{
    private GameObject screwdriverObject;
    private ElectricScrewDriver electricScrewDriver;
    private AudioSource mockAudioSource;
    private Mock<XRBaseControllerInteractor> mockInteractor;
    private Mock<XRController> mockXRController;

    [SetUp]
    public void SetUp()
    {
        screwdriverObject = new GameObject();
        electricScrewDriver = screwdriverObject.AddComponent<ElectricScrewDriver>();

        mockAudioSource = screwdriverObject.AddComponent<AudioSource>();

        mockInteractor = new Mock<XRBaseControllerInteractor>();
        mockXRController = new Mock<XRController>();

        mockInteractor.Setup(interactor => interactor.xrController).Returns(mockXRController.Object);

       
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(screwdriverObject);
    }

    [Test]
    public void ElectricScrewDriver_ActivatesSound_WhenTriggerPulled()
    {
        mockXRController.Setup(controller => controller.activateInteractionState.value).Returns(1f);

        // Simulate selection using event invocation
        electricScrewDriver.onSelectEntered.Invoke(mockInteractor.Object);

        electricScrewDriver.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);

        Assert.IsTrue(mockAudioSource.isPlaying);
        Assert.AreEqual(1f, mockAudioSource.volume);
        Assert.AreEqual(3f, mockAudioSource.pitch);
    }

    [Test]
    public void ElectricScrewDriver_DeactivatesSound_WhenTriggerReleased()
    {
        mockXRController.Setup(controller => controller.activateInteractionState.value).Returns(0f);

        // Simulate selection using event invocation
        electricScrewDriver.onSelectEntered.Invoke(mockInteractor.Object);
        electricScrewDriver.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);

        electricScrewDriver.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);

        Assert.IsFalse(mockAudioSource.isPlaying);
        Assert.AreEqual(0f, mockAudioSource.volume);
    }

    [Test]
    public void ElectricScrewDriver_Rotates_WhenTriggerPulled()
    {
        var initialRotation = screwdriverObject.transform.rotation;
        mockXRController.Setup(controller => controller.activateInteractionState.value).Returns(1f);

        // Use a direct field or expose a property for testing
        electricScrewDriver.SpeedMultiplier = 100f;

        // Simulate selection using event invocation
        electricScrewDriver.onSelectEntered.Invoke(mockInteractor.Object);
        electricScrewDriver.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);

        var finalRotation = screwdriverObject.transform.rotation;
        Assert.AreNotEqual(initialRotation, finalRotation);
    }
}
