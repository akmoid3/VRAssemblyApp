using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;

public class SimpleHammerTest
{
    private GameObject hammerObject;
    private SimpleHammer simpleHammer;

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }

    [SetUp]
    public void SetUp()
    {
        hammerObject = new GameObject();
        simpleHammer = hammerObject.AddComponent<SimpleHammer>();
        SetPrivateField(simpleHammer, "forceMultiplier", 10f);
        SetPrivateField(simpleHammer, "minImpactForce", 5f);
        SetPrivateField(simpleHammer, "maxImpactForce", 50f);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(hammerObject);
    }

    [Test]
    public void Start_SetsLastPosition()
    {
        simpleHammer.Start();
        Vector3 lastPosition = (Vector3)typeof(SimpleHammer).GetField("lastPosition", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(simpleHammer);
        Assert.AreEqual(hammerObject.transform.position, lastPosition);
    }

    [Test]
    public void CalculateImpactForce_UpdatesImpactForceAndDirection()
    {
        simpleHammer.Start();
        hammerObject.transform.position += new Vector3(1f, 0f, 0f);
        simpleHammer.CalculateImpactForce();

        float expectedImpactForce = 50f;
        float currentImpactForce = (float)typeof(SimpleHammer).GetField("currentImpactForce", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(simpleHammer);
        Assert.AreEqual(expectedImpactForce, currentImpactForce);
        Assert.AreEqual(Vector3.right, simpleHammer.GetImpactDirection());
    }

}
