// This script creates a new menu item Examples>Create Prefab in the main menu.
// Use it to create Prefab(s) from the selected GameObject(s).
// It is placed in the root Assets folder.
using System.IO;
using UnityEngine;
using UnityEditor;

public class Example
{
    

    // Creates a new menu item 'Examples > Create Prefab' in the main menu.
    [MenuItem("Examples/Create Prefab")]
    static void CreatePrefab()
    {
        // Keep track of the currently selected GameObject(s)
        GameObject[] objectArray = Selection.gameObjects;
        // Loop through every GameObject in the array above
        foreach (GameObject gameObject in objectArray)
        {
            // Create folder Prefabs and set the path as within the Prefabs folder,
            // and name it as the GameObject's name with the .Prefab format

            string[] modelName =  gameObject.name.Split('_');
            string basePath = $"Assets/_{modelName[0]}/" + gameObject.name;
            string prefabPath = basePath + ".prefab";

            string albedoTextureName = basePath + "_base.png"; 
            string normalMapTextureName = basePath + "_nor.png"; 
            string occlusMapTextureName = basePath + "_occ.png"; 

            string materialName = $"Assets/_{modelName[0]}/Materials/" + gameObject.name + ".mat";

            // Get the Renderer component from the instantiated object
            Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
            
            if (renderer != null)
            {   
                //try and find the old material 
                Material materialInstance = new Material(Shader.Find("Standard"));
                Material existingMat = (Material)AssetDatabase.LoadAssetAtPath(materialName, typeof(Material));

                if (existingMat != null)
                {
                    //AssetDatabase.DeleteAsset(materialName);
                    Debug.Log($"material {materialName} already exixts ");
                    //AssetDatabase.MakeEditable(materialName);
                    materialInstance = existingMat; 
                    Debug.Log(materialInstance);
                }
                else
                {
                    //Debug.Log(materialName);
                    //if non found make new material 
                    AssetDatabase.CreateAsset(materialInstance, materialName);
                }
                
                renderer.sharedMaterial = materialInstance; 
                EditorUtility.SetDirty(materialInstance);
                
                //mark as dirty and change settings
                
                byte[] bytes;
                // Access the material property to create an instance specific to this object

                // Load the textures by name
                Texture2D albedoMap = new Texture2D(2, 2);
                bytes = File.ReadAllBytes(albedoTextureName);
                albedoMap.LoadImage(bytes);

                Texture2D normalMap =  new Texture2D(2, 2);
                bytes = File.ReadAllBytes(normalMapTextureName);
                normalMap.LoadImage(bytes);

                Texture2D occlusMap = new Texture2D(2, 2);
                bytes = File.ReadAllBytes(occlusMapTextureName);
                occlusMap.LoadImage(bytes);
                
                materialInstance.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
                materialInstance.SetFloat("_GlossMapScale", 0.3f);

                if (albedoMap != null)
                {
                    //Assign the albedo (main) texture
                    materialInstance.mainTexture = albedoMap;
                }

                if (normalMap != null)
                {
                   // Enable the normal map keyword for the shader (necessary for Standard shader)
                    materialInstance.EnableKeyword("_NORMALMAP");
                    // Assign the normal map to the "_BumpMap" property name
                    materialInstance.SetTexture("_BumpMap", normalMap); 
                }
                if (occlusMap != null)
                {
                    // Assign the occlusion map to the "_OcclusionMap" property name
                    materialInstance.SetTexture("_OcclusionMap", occlusMap); 
                }

                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

            }
            else
            {
                Debug.LogError("Renderer component not found on the prefab instance.");
            }

            // Make sure the file name is unique, in case an existing Prefab has the same name.
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);       
            // Create the new Prefab and log whether Prefab was saved successfully.
            bool prefabSuccess= false;
            
            if (PrefabUtility.IsPartOfAnyPrefab(gameObject))
            {
                Debug.Log("Prefab already exists");
            }
            else
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, prefabPath, InteractionMode.AutomatedAction, out prefabSuccess);
            }
        }
    }

    // Disable the menu item if no selection is in place.
    [MenuItem("Examples/Create Prefab", true)]
    static bool ValidateCreatePrefab()
    {
        return Selection.activeGameObject != null && !EditorUtility.IsPersistent(Selection.activeGameObject);
    }
}
