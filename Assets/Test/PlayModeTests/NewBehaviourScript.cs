/*
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CoACDTests
{
    // Assicurati che il namespace CoACD e la classe Parameters siano definiti nel progetto.
    // Ad esempio:
    // namespace CoACD { public class Parameters { } }

    public class CoACDColliderDataTests
    {
        // Percorso temporaneo per il salvataggio dell'asset di test.
        private string assetPath = "Assets/TempTestCoACDColliderData.asset";

        [TearDown]
        public void Cleanup()
        {
            // Rimuove l'asset creato, se esiste.
            if (AssetDatabase.LoadAssetAtPath<CoACDColliderData>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        [Test]
        public void TestCreateAsset()
        {
            // Arrange
            // Creazione di un'istanza di CoACD.Parameters (si assume esista un costruttore di default).
            CoACD.Parameters parameters = new CoACD.Parameters();
            // Creazione di alcune mesh di test per computedMeshes.
            Mesh[] computedMeshes = new Mesh[2];
            for (int i = 0; i < computedMeshes.Length; i++)
            {
                computedMeshes[i] = new Mesh();
            }
            // Creazione di alcune mesh di test per baseMeshes.
            Mesh[] baseMeshes = new Mesh[1];
            baseMeshes[0] = new Mesh();

            // Act
            CoACDColliderData asset = CoACDColliderData.CreateAsset(assetPath, parameters, computedMeshes, baseMeshes);
            AssetDatabase.SaveAssets();

            // Assert
            Assert.IsNotNull(asset, "L'asset creato non deve essere null.");
            Assert.AreEqual(parameters, asset.parameters, "Il parametro deve essere impostato correttamente.");
            Assert.AreEqual(baseMeshes, asset.baseMeshes, "Le baseMeshes devono essere impostate correttamente.");
            Assert.AreEqual(computedMeshes, asset.computedMeshes, "Le computedMeshes devono essere impostate correttamente.");

            // Verifica che i nomi delle computedMeshes siano stati impostati correttamente.
            for (int i = 0; i < computedMeshes.Length; i++)
            {
                Assert.AreEqual($"Computed Mesh {i}", computedMeshes[i].name, 
                    $"Il nome della mesh in posizione {i} deve essere 'Computed Mesh {i}'.");
            }
        }

        [Test]
        public void TestUpdateAsset()
        {
            // Arrange
            // Crea l'asset iniziale
            CoACD.Parameters initialParams = new CoACD.Parameters();
            Mesh[] initialComputedMeshes = new Mesh[2];
            for (int i = 0; i < initialComputedMeshes.Length; i++)
            {
                initialComputedMeshes[i] = new Mesh();
            }
            Mesh[] initialBaseMeshes = new Mesh[1];
            initialBaseMeshes[0] = new Mesh();

            CoACDColliderData asset = CoACDColliderData.CreateAsset(assetPath, initialParams, initialComputedMeshes, initialBaseMeshes);
            AssetDatabase.SaveAssets();

            // Crea nuovi dati per l'aggiornamento
            CoACD.Parameters newParams = new CoACD.Parameters();
            Mesh[] newComputedMeshes = new Mesh[3];
            for (int i = 0; i < newComputedMeshes.Length; i++)
            {
                newComputedMeshes[i] = new Mesh();
            }
            Mesh[] newBaseMeshes = new Mesh[2];
            newBaseMeshes[0] = new Mesh();
            newBaseMeshes[1] = new Mesh();

            // Act
            asset.UpdateAsset(newParams, newComputedMeshes, newBaseMeshes);
            AssetDatabase.SaveAssets();

            // Assert
            Assert.AreEqual(newParams, asset.parameters, "I nuovi parametri devono essere impostati correttamente.");
            Assert.AreEqual(newBaseMeshes, asset.baseMeshes, "Le nuove baseMeshes devono essere impostate correttamente.");
            Assert.AreEqual(newComputedMeshes, asset.computedMeshes, "Le nuove computedMeshes devono essere impostate correttamente.");

            // Verifica che i nomi delle nuove computedMeshes siano stati aggiornati correttamente.
            for (int i = 0; i < newComputedMeshes.Length; i++)
            {
                Assert.AreEqual($"Computed Mesh {i}", newComputedMeshes[i].name,
                    $"Il nome della mesh in posizione {i} deve essere 'Computed Mesh {i}'.");
            }
        }
    }
}
*/
