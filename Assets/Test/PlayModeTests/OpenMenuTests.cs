using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class OpenMenuTests
{
    private GameObject menuObject;
    private GameObject testObject;
    private OpenMenu openMenu;

    [SetUp]
    public void Setup()
    {
        // Create the menu GameObject
        menuObject = new GameObject("Menu");
        
        // Create the test GameObject with OpenMenu component
        testObject = new GameObject("TestObject");
        openMenu = testObject.AddComponent<OpenMenu>();
        
        // Set the menu reference using reflection (since m_Menu is private)
        var menuField = typeof(OpenMenu).GetField("m_Menu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        menuField.SetValue(openMenu, menuObject);
        
        // Initially set menu to inactive
        menuObject.SetActive(false);
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(menuObject);
        Object.Destroy(testObject);
    }

    [UnityTest]
    public IEnumerator OpenMenu_PressSpaceKey_TogglesMenuVisibility()
    {
        // Verify menu is initially inactive
        Assert.IsFalse(menuObject.activeSelf);

        // Simulate pressing space key
        openMenu.ToggleMenu(true); 
        
        // Wait for one frame
        yield return null;
        
        // Verify menu is now active
        Assert.IsTrue(menuObject.activeSelf);

        // Simulate pressing space key again
        openMenu.ToggleMenu(true); 
        
        // Wait for one frame
        yield return null;

        // Verify menu is inactive again
        Assert.IsFalse(menuObject.activeSelf);
    }
    
}