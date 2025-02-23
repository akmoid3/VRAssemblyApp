using NUnit.Framework;
using System.Collections.Generic;

[TestFixture]
public class JsonDataTests
{
    [Test]
    public void ComponentsList_IsInitialized()
    {
        // Arrange
        JsonData jsonData = new JsonData();

        // Act
        List<ComponentTypeData> components = jsonData.components;

        // Assert
        Assert.IsNotNull(components, "La lista components non deve essere null dopo l'inizializzazione.");
        Assert.AreEqual(0, components.Count, "La lista components deve essere vuota all'inizializzazione.");
    }

    [Test]
    public void Can_Add_ComponentTypeData()
    {
        // Arrange
        JsonData jsonData = new JsonData();
        // Supponiamo che ComponentTypeData abbia un costruttore di default.
        ComponentTypeData componentData = new ComponentTypeData();

        // Act
        jsonData.components.Add(componentData);

        // Assert
        Assert.AreEqual(1, jsonData.components.Count, "La lista components deve contenere 1 elemento dopo l'aggiunta.");
        Assert.AreSame(componentData, jsonData.components[0], "L'elemento aggiunto deve essere lo stesso istanza inserita.");
    }
}