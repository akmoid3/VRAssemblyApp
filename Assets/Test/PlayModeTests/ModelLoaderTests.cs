using NUnit.Framework;
using Moq;
using UnityEngine;
using System.Threading.Tasks;

[TestFixture]
public class ModelLoaderTests
{
    private Mock<IModelLoader> mockLoader;

    [SetUp]
    public void SetUp()
    {
        mockLoader = new Mock<IModelLoader>();
    }

    [Test]
    public async void LoadFromFile_ShouldReturnGameObject()
    {
        string filePath = "test.glb";
        var testGameObject = new GameObject();
        mockLoader.Setup(x => x.LoadFromFile(It.IsAny<string>())).ReturnsAsync(testGameObject);

        var result = await mockLoader.Object.LoadFromFile(filePath);

        Assert.IsNotNull(result);
        Assert.AreEqual(testGameObject, result);
    }
}
