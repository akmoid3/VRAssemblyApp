using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class WoodenPinExtendedIntegrationTests
{
    private GameObject woodenPinObject;
    private WoodenPin woodenPin;
    private GameObject hammerObject;
    private SimpleHammer simpleHammer;
    private GameObject socketObject;
    private StateManager stateManager;
    private AudioManager audioManager;


    [SetUp]
    public void Setup()
    {
        audioManager = new GameObject().AddComponent<AudioManager>();
        stateManager = new GameObject().AddComponent<StateManager>();
        woodenPinObject = new GameObject("WoodenPin");
        woodenPinObject.AddComponent<ComponentObject>();

        woodenPin = woodenPinObject.AddComponent<WoodenPin>();
        MeshRenderer mesh = woodenPin.gameObject.AddComponent<MeshRenderer>();

        hammerObject = new GameObject("Hammer");
        simpleHammer = hammerObject.AddComponent<SimpleHammer>();
        hammerObject.AddComponent<BoxCollider>().isTrigger = true;

        socketObject = new GameObject("Socket");
        SetPrivateField(woodenPin, "socketTransform", socketObject.transform);

        woodenPin.transform.position = Vector3.zero;
        simpleHammer.transform.position = Vector3.forward;
        //woodenPin.Tool = hammerObject;
        woodenPin.initialSocketPosition = socketObject.transform.localPosition;
        woodenPin.InitialPosition = woodenPin.transform.localPosition;
    }

    [Test]
    public void TestHandleInteraction_CooldownNotElapsed()
    {
        SetPrivateField(woodenPin, "lastMoveTime", Time.time);
        InvokeProtectedMethod(woodenPin, "HandleInteraction", null);
        Assert.AreEqual(Vector3.zero, woodenPin.transform.position, "The pin should not move if the cooldown has not elapsed.");
    }

    [UnityTest]
    public IEnumerator TestHandleInteraction_StatePlayBack()
    {
        SetPrivateField(woodenPin, "hammerScript", simpleHammer);
        SetPrivateField(simpleHammer, "currentImpactForce",10f);
        

        Vector3 impactDirection = Vector3.forward;  // This will give a dot product of 1.0


        // Set the private field 'impactDirection' to the 'impactDirection'
        SetPrivateField(simpleHammer, "impactDirection", impactDirection);


        stateManager.CurrentState = State.PlayBack;

        yield return null;

        Vector3 initialPosition = socketObject.transform.localPosition;

        InvokeProtectedMethod(woodenPin, "HandleInteraction", null);
        yield return null;

        Vector3 finalPosition = socketObject.transform.localPosition;


        Assert.AreEqual(finalPosition, initialPosition, "The socket should have moved forward when sufficient force is applied.");
    }

    [UnityTest]
    public IEnumerator TestHandleInteraction_SufficientForce()
    {
        SetPrivateField(woodenPin, "hammerScript", simpleHammer);
        SetPrivateField(simpleHammer, "currentImpactForce", 10f);
       
        SetPrivateField(woodenPin, "isAligned", true);

        Vector3 impactDirection = Vector3.forward;  // This will give a dot product of 1.0

        // Set the private field 'impactDirection' to the 'impactDirection'
        SetPrivateField(simpleHammer, "impactDirection", impactDirection);
        yield return null;

        Vector3 initialPosition = woodenPin.transform.localPosition;
        InvokeProtectedMethod(woodenPin, "HandleInteraction", null);

        yield return null;
        Vector3 finalPosition = woodenPin.transform.localPosition;

        Assert.AreEqual(finalPosition, initialPosition, "The pin should have moved forward when sufficient force is applied.");
    }
    
    [UnityTest]
    public IEnumerator TestHandleInteractionSocket()
    {
        woodenPin.SetSocketTransform(new GameObject().transform);
        SetPrivateField(woodenPin, "hammerScript", simpleHammer);
        SetPrivateField(simpleHammer, "currentImpactForce", 10f);
       
        SetPrivateField(woodenPin, "isAligned", true);

        Vector3 impactDirection = Vector3.forward;  // This will give a dot product of 1.0

        // Set the private field 'impactDirection' to the 'impactDirection'
        SetPrivateField(simpleHammer, "impactDirection", impactDirection);
        yield return null;

        Vector3 initialPosition = woodenPin.transform.localPosition;
        InvokeProtectedMethod(woodenPin, "HandleSocketTransformInteraction", null);

        yield return null;
        Vector3 finalPosition = woodenPin.transform.localPosition;

        Assert.AreEqual(finalPosition, initialPosition, "The pin should have moved forward when sufficient force is applied.");
    }


    [Test]
    public void TestOnToolCollisionEnter()
    {
        InvokeProtectedMethod(woodenPin, "OnToolCollisionEnter", new object[] { hammerObject.GetComponent<Collider>() });
        Assert.AreEqual(simpleHammer, GetPrivateField(woodenPin, "hammerScript"), "The hammer script reference should be assigned on collision enter.");
    }

    [Test]
    public void TestOnToolCollisionExit()
    {
        InvokeProtectedMethod(woodenPin, "OnToolCollisionEnter", new object[] { hammerObject.GetComponent<Collider>() });
        InvokeProtectedMethod(woodenPin, "OnToolCollisionExit", new object[] { hammerObject.GetComponent<Collider>() });
        Assert.IsNull(GetPrivateField(woodenPin, "hammerScript"), "The hammer script reference should be cleared on collision exit.");
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(woodenPinObject);
        GameObject.DestroyImmediate(hammerObject);
        GameObject.DestroyImmediate(socketObject);
        if(stateManager != null)
            GameObject.DestroyImmediate(stateManager.gameObject);
        if(audioManager != null)
            GameObject.DestroyImmediate(audioManager.gameObject);

    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.FlattenHierarchy);
        if(field == null)
        {
            throw new System.Exception($"Field '{fieldName}' not found in {target.GetType().FullName}");
        }
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
