/*using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class InitializedDataManagerTests
{
    private GameObject testObject;
    private InitializedDataManager initializedDataManager;
    private string testDirectoryPath;
    private string testFilePath;
    private Manager manager;
    private StateManager stateManager;


    [SetUp]
    public void Setup()
    {
        manager = new GameObject().AddComponent<Manager>();
        stateManager = new GameObject().AddComponent<StateManager>();

        // Crea un GameObject di test e aggiungi il componente InitializedDataManager
        testObject = new GameObject();
        initializedDataManager = testObject.AddComponent<InitializedDataManager>();

        var directoryField = typeof(InitializedDataManager).GetField("directory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        string customDirectory = "CustomInitializedModels";
        directoryField.SetValue(initializedDataManager, customDirectory);

        testDirectoryPath = Path.Combine(Application.persistentDataPath, customDirectory);


        var directoryPathField = typeof(InitializedDataManager).GetField("directoryPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        directoryPathField.SetValue(initializedDataManager, testDirectoryPath);

        // Assicurati che la directory di test esista
        if (!Directory.Exists(testDirectoryPath))
        {
            Directory.CreateDirectory(testDirectoryPath);
        }

        // Imposta i componenti di prova
        var child1 = new GameObject("Component1").transform;
        var child2 = new GameObject("Component2").transform;
        child1.gameObject.AddComponent<ComponentObject>();

        manager.Components = new List<Transform> { child1, child2 };
    }


    [TearDown]
    public void Teardown()
    {
        // Rimuovi tutti i file nella directory di test
        if (Directory.Exists(testDirectoryPath))
        {
            var files = Directory.GetFiles(testDirectoryPath);
            foreach (var file in files)
            {
                File.Delete(file);
            }

            // Elimina la directory dopo aver rimosso i file
            Directory.Delete(testDirectoryPath);
        }

        // Distruggi l'oggetto di test
        Object.DestroyImmediate(testObject);
        Object.DestroyImmediate(stateManager.gameObject);
        Object.DestroyImmediate(manager.gameObject);
    }



    [Test]
    public void Start_InitializesDirectory()
    {
        // Esegui il metodo Start
        initializedDataManager.GetType().GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(initializedDataManager, null);

        // Verifica che la directory sia stata creata
        Assert.IsTrue(Directory.Exists(testDirectoryPath), "La directory non è stata creata correttamente");
    }

    [Test]
    public void SaveComponentsData_CreatesJsonFile()
    {
        manager.Model = new GameObject("Prova");
        // Esegui il metodo SaveComponentsData
        initializedDataManager.SaveComponentsData();

        // Verifica che il file JSON sia stato creato
        testFilePath = Path.Combine(testDirectoryPath, manager.Model.name + ".json");
        Assert.IsTrue(File.Exists(testFilePath), "Il file JSON non è stato creato");

        // Verifica il contenuto del file
        string jsonContent = File.ReadAllText(testFilePath);
        Assert.IsFalse(string.IsNullOrEmpty(jsonContent), "Il contenuto del file JSON è vuoto");
    }

    [Test]
    public void SaveComponentsData_NoComponents_DoesNotCreateFile()
    {
        manager.Model = new GameObject("Prova");

        // Imposta components come null
        manager.Components = null;

        // Esegui il metodo SaveComponentsData
        initializedDataManager.SaveComponentsData();

        // Verifica che il file JSON non sia stato creato
        testFilePath = Path.Combine(testDirectoryPath, manager.Model.name + ".json");
        Assert.IsFalse(File.Exists(testFilePath), "Il file JSON non dovrebbe essere stato creato");
    }


    [Test]
    public void LoadComponentsData_LoadsDataCorrectly()
    {
        manager.Model = new GameObject("LoadsDataCorrectly");

        // Esegui il metodo SaveComponentsData per creare un file JSON
        initializedDataManager.SaveComponentsData();

        // Modifica i dati dei componenti
        var child1 = manager.Components[0];
        var child2 = manager.Components[1];

        var componentObject1 = child1.gameObject.AddComponent<ComponentObject>();

        componentObject1.SetComponentType(ComponentObject.ComponentType.None);
        componentObject1.SetGroup(ComponentObject.Group.None);

     

        // Esegui il metodo LoadComponentsData
        initializedDataManager.LoadComponentsData();

        // Verifica che i dati siano stati caricati correttamente
        Assert.AreEqual(ComponentObject.ComponentType.None, componentObject1.GetComponentType(), "Il tipo di componente 1 non è stato caricato correttamente");
        Assert.AreEqual(ComponentObject.Group.None, componentObject1.GetGroup(), "Il gruppo del componente 1 non è stato caricato correttamente");

        Assert.AreEqual(null, child2.GetComponent<ComponentObject>(), "ComponentObject non e' null");
    }

    [Test]
    public void LoadComponentsData_NoFile_DoesNotThrowException()
    {
        // Verifica che non esista un file JSON
        manager.Model = new GameObject("DoesNotThrowException");

        testFilePath = Path.Combine(testDirectoryPath, manager.Model.name + ".json");
        if (File.Exists(testFilePath))
        {
            File.Delete(testFilePath);
        }

        // Esegui il metodo LoadComponentsData e verifica che non lanci eccezioni
        Assert.DoesNotThrow(() => initializedDataManager.LoadComponentsData(), "Il metodo LoadComponentsData non dovrebbe lanciare eccezioni se il file non esiste");
    }


    [Test]
    public void FindChildByName_ReturnsCorrectChild()
    {
        // Setup: Aggiungi un componente con un nome noto
        Transform targetChild = Manager.Instance.Components[0];

        // Esegui il metodo FindChildByName
        Transform foundChild = initializedDataManager.GetType().GetMethod("FindChildByName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(initializedDataManager, new object[] { manager.Components, targetChild.name }) as Transform;

        // Verifica che il bambino trovato sia corretto
        Assert.AreEqual(targetChild, foundChild, "Il metodo FindChildByName non ha trovato il bambino corretto");
    }

    [Test]
    public void FindChildByName_ReturnsNullIfNotFound()
    {
        // Setup: Un nome di componente inesistente
        string nonExistentName = "NonExistentComponent";

        // Esegui il metodo FindChildByName
        Transform foundChild = initializedDataManager.GetType().GetMethod("FindChildByName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(initializedDataManager, new object[] { manager.Components, nonExistentName }) as Transform;

        // Verifica che il risultato sia null
        Assert.IsNull(foundChild, "Il metodo FindChildByName avrebbe dovuto restituire null per un componente inesistente");
    }

    [Test]
    public void FindChildByName_EmptyName_ReturnsNull()
    {
        // Esegui il metodo FindChildByName con un nome vuoto
        Transform foundChild = initializedDataManager.GetType().GetMethod("FindChildByName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(initializedDataManager, new object[] { manager.Components, "" }) as Transform;

        // Verifica che il risultato sia null
        Assert.IsNull(foundChild, "Il metodo FindChildByName avrebbe dovuto restituire null per un nome vuoto");
    }

    [UnityTest]
    public IEnumerator LoadComponentsData_AddsComponentObjectIfNull()
    {
        // Preparazione: Crea una directory e un file JSON con i dati di prova
        manager.Model = new GameObject("test");
        string fileName = Manager.Instance.Model.name + ".json";


        // JSON esistente che vuoi scrivere
        string jsonContent = @"
    {
        ""components"": [
            {
                ""componentName"": ""TestComponent"",
                ""componentType"": 0,
                ""componentGroup"": 0
            }
        ]
    }";

        var filePath = Path.Combine(testDirectoryPath, "test.json");
        // Scrivi il JSON direttamente nel file
        File.WriteAllText(filePath, jsonContent);

        // Imposta il GameObject senza ComponentObject
        var testChild = new GameObject("TestComponent").transform;
        Manager.Instance.Components = new List<Transform> { testChild };

        // Esegui il metodo LoadComponentsData
        initializedDataManager.LoadComponentsData();

        yield return null;
        // Verifica che il ComponentObject sia stato aggiunto e i dati siano stati correttamente impostati
        var componentObject = testChild.GetComponent<ComponentObject>();
        Assert.IsNotNull(componentObject, "ComponentObject dovrebbe essere stato aggiunto al GameObject");
        Assert.AreEqual(ComponentObject.ComponentType.None, componentObject.GetComponentType(), "Il tipo di componente non è stato impostato correttamente");
        Assert.AreEqual(ComponentObject.Group.None, componentObject.GetGroup(), "Il gruppo del componente non è stato impostato correttamente");

       
    }

}
*/