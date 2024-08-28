using NUnit.Framework;
using System;
using UnityEngine.TestTools;
using UnityEngine;

public class ExtensionsTests
{
    class TestSource
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public readonly int ReadOnlyField = 10;
    }

    class Source : TestSource {
        public string Job { get; set; }
    }


    [Test]
    public void CopyPropertiesAndFields()
    {
        // Arrange
        var source = new TestSource();
        source.Name = "John";
        source.Age = 10;
        var destination = new TestSource();

        // Act
        source.CopyPropertiesAndFields(destination);

        // Assert
        Assert.AreEqual(source.Name,destination.Name);
        Assert.AreEqual(source.Age, destination.Age); 
    }

    [Test]
    public void CopyPropertiesAndFields_HandlesReadOnlyFields()
    {
        // Arrange
        var source = new TestSource();
        var destination = new TestSource();

        // Act
        source.CopyPropertiesAndFields(destination);

        // Assert
        Assert.AreEqual(10, destination.ReadOnlyField);
    }

    [Test]
    public void CopyPropertiesAndFields_NullReferenceThrows()
    {
        // Arrange
        TestSource source = null;
        var destination = new TestSource();

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => source.CopyPropertiesAndFields(destination));
    }

    [Test]
    public void CopyPropertiesAndFields_NotCompatibleTypes()
    {
        // Arrange
        Source source = new Source();
        source.Name = "john";
        source.Age = 10;
        source.Job = "Teacher";
        var destination = new TestSource();

        
        source.CopyPropertiesAndFields(destination);

        LogAssert.Expect(LogType.Warning, "Failed to copy property 'Job': Object does not match target type.");

        // Assert
        Assert.AreEqual(source.Name, destination.Name);
        Assert.AreEqual(source.Age, destination.Age);

    }
}
