using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class ScrewTests
{
    private GameObject screwObject;
    private Screw screw;
    private GameObject screwdriverObject;
    private BaseScrewDriver baseScrewDriver;
    private GameObject socketObject;
    private StateManager stateManager;
    private AudioManager audioManager;


    [SetUp]
    public void Setup()
    {
        audioManager = new GameObject().AddComponent<AudioManager>();

        stateManager = new GameObject().AddComponent<StateManager>();
        screwObject = new GameObject("Screw");
        screw = screwObject.AddComponent<Screw>();
        MeshRenderer mesh = screw.gameObject.AddComponent<MeshRenderer>();

        screwdriverObject = new GameObject("Screwdriver");
        baseScrewDriver = screwdriverObject.AddComponent<ElectricScrewDriver>();
        screwdriverObject.AddComponent<BoxCollider>().isTrigger = true;

        socketObject = new GameObject("Socket");
        SetPrivateField(screw, "socketTransform", socketObject.transform);

        screw.transform.position = Vector3.zero;
        baseScrewDriver.transform.position = Vector3.forward;
        screw.Tool = screwdriverObject;
        screw.CorrectToolName = screwdriverObject.name;
        SetPrivateField(screw, "initialSocketPosition", socketObject.transform.localPosition);
        SetPrivateField(screw, "initialZPosition", screw.transform.localPosition);
    }

    [UnityTest]
    public IEnumerator TestHandleInteraction_StatePlayBack()
    {
        SetPrivateField(screw, "screwdriverScript", baseScrewDriver);
        SetPrivateField(baseScrewDriver, "currentRotationSpeed", 100f);



        stateManager.CurrentState = State.PlayBack;

        yield return null;

        Vector3 initialPosition = socketObject.transform.localPosition;

        InvokeProtectedMethod(screw, "HandleInteraction", null);
        yield return null;

        Vector3 finalPosition = socketObject.transform.localPosition;

        Assert.AreNotEqual(finalPosition, initialPosition, "The socket should have moved forward when sufficient force and correct alignment are applied during playback.");
    }

    [UnityTest]
    public IEnumerator TestHandleInteraction_SufficientForce()
    {
        SetPrivateField(screw, "screwdriverScript", baseScrewDriver);
        SetPrivateField(baseScrewDriver, "currentRotationSpeed", 100f);
        SetPrivateField(screw, "isAligned", true);

      

        yield return null;

        Vector3 initialPosition = screw.transform.localPosition;
        InvokeProtectedMethod(screw, "HandleInteraction", null);

        yield return null;
        Vector3 finalPosition = screw.transform.localPosition;

        Assert.AreNotEqual(finalPosition, initialPosition, "The screw should have moved forward when sufficient force and correct alignment are applied.");
    }

    [UnityTest]
    public IEnumerator TestHandleInteraction_StatePlayBack_NotScrewing()
    {
        SetPrivateField(screw, "screwdriverScript", baseScrewDriver);
        SetPrivateField(baseScrewDriver, "currentRotationSpeed", 100f);
        SetPrivateField(screw, "isScrewing", true);



        stateManager.CurrentState = State.PlayBack;

        yield return null;

        Vector3 initialPosition = socketObject.transform.localPosition;

        InvokeProtectedMethod(screw, "HandleInteraction", null);
        yield return null;

        Vector3 finalPosition = socketObject.transform.localPosition;

        Assert.AreNotEqual(finalPosition, initialPosition, "The socket should have moved forward when sufficient force and correct alignment are applied during playback.");
    }

    [UnityTest]
    public IEnumerator TestHandleInteraction_SufficientForce_NotScrewing()
    {
        SetPrivateField(screw, "screwdriverScript", baseScrewDriver);
        SetPrivateField(baseScrewDriver, "currentRotationSpeed", 100f);
        SetPrivateField(screw, "isAligned", true);
        SetPrivateField(screw, "isScrewing", true);



        yield return null;

        Vector3 initialPosition = screw.transform.localPosition;
        InvokeProtectedMethod(screw, "HandleInteraction", null);

        yield return null;
        Vector3 finalPosition = screw.transform.localPosition;

        Assert.AreNotEqual(finalPosition, initialPosition, "The screw should have moved forward when sufficient force and correct alignment are applied.");
    }

    [Test]
    public void TestOnToolCollisionEnter()
    {
        InvokeProtectedMethod(screw, "OnToolCollisionEnter", new object[] { screwdriverObject.GetComponent<Collider>() });
        Assert.AreEqual(baseScrewDriver, GetPrivateField(screw, "screwdriverScript"), "The screwdriver script reference should be assigned on collision enter.");
    }

    [Test]
    public void TestOnToolCollisionExit()
    {
        InvokeProtectedMethod(screw, "OnToolCollisionEnter", new object[] { screwdriverObject.GetComponent<Collider>() });
        InvokeProtectedMethod(screw, "OnToolCollisionExit", new object[] { screwdriverObject.GetComponent<Collider>() });
        Assert.IsNull(GetPrivateField(screw, "screwdriverScript"), "The screwdriver script reference should be cleared on collision exit.");
    }
    

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(screwObject);
        GameObject.DestroyImmediate(screwdriverObject);
        GameObject.DestroyImmediate(socketObject);
        GameObject.DestroyImmediate(stateManager.gameObject);
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(target, value);
    }

    private object GetPrivateField(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field.GetValue(target);
    }

    private void InvokeProtectedMethod(object target, string methodName, object[] parameters)
    {
        var method = target.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(target, parameters);
    }
}
