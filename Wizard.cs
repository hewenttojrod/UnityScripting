// This script creates a new menu item Scripts>Create Prefab in the main menu.
// Use it to create Prefab(s) from the selected GameObject(s).
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public class Example
{
    [MenuItem("Scripts/Create Prefab")]
    static void CreatePrefab()
    {
        var selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0)
        {
            Debug.LogWarning("No GameObjects selected.");
            return;
        }

        foreach (var go in selected)
        {
            if (go == null) continue;

            if (false && PrefabUtility.IsPartOfAnyPrefab(go))
            {
                Debug.Log($"GameObject '{go.name}' is already part of a prefab. Skipping.");
                continue;
            }

            var modelNameParts = go.name.Split('_');
            var rootFolder = Path.Combine("Assets", "_" + modelNameParts[0]);
            var basePath = Path.Combine(rootFolder, go.name);
            var prefabPath = basePath + ".prefab";
            var materialFolder = Path.Combine(rootFolder, "Materials");
            var materialPath = Path.Combine(materialFolder, go.name + ".mat");

            var albedoTexturePath = basePath + "_base.png";
            var normalMapTexturePath = basePath + "_nor.png";
            var occlusionMapTexturePath = basePath + "_occ.png";

            var renderer = go.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                Debug.LogError($"Renderer component not found on '{go.name}'.");
                continue;
            }

            var material = GetOrCreateMaterial(materialPath, materialFolder);
            renderer.sharedMaterial = material;
            EditorUtility.SetDirty(material);

            var albedo = LoadTexture(albedoTexturePath);
            var normal = LoadTexture(normalMapTexturePath);
            var occlusion = LoadTexture(occlusionMapTexturePath);

            if (albedo != null) material.mainTexture = albedo;
            if (normal != null)
            {
                material.EnableKeyword("_NORMALMAP");
                material.SetTexture("_BumpMap", normal);
            }
            if (occlusion != null)
            {
                material.SetTexture("_OcclusionMap", occlusion);
            }

            material.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
            material.SetFloat("_GlossMapScale", 0.3f);

            //AssetDatabase.Refresh();
            var normalizedPath = materialPath.Replace('\\', '/');
            AssetDatabase.CreateAsset(material, $"{normalizedPath} - 2");
            AssetDatabase.SaveAssets();

            prefabPath = prefabPath.Replace('\\', '/');
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

            bool prefabSuccess = false;
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.AutomatedAction, out prefabSuccess);
        }
    }

    private static Material GetOrCreateMaterial(string materialPath, string materialFolder)
    {
        var normalizedPath = materialPath.Replace('\\', '/');
        var existing = AssetDatabase.LoadAssetAtPath<Material>(normalizedPath);
        if (existing != null)
            return existing;

        // Ensure directory exists on disk
        var systemFolder = materialFolder;
        if (!Directory.Exists(systemFolder))
            Directory.CreateDirectory(systemFolder);

        var mat = new Material(Shader.Find("Standard"));
        AssetDatabase.CreateAsset(mat, normalizedPath);
        return mat;
    }

    private static Texture2D LoadTexture(string path)
    {
        // Try loading from file first (raw file), otherwise try AssetDatabase (imported asset)
        if (File.Exists(path))
        {
            try
            {
                var bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                return tex;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load texture at '{path}': {e.Message}");
            }
        }

        var assetPath = path.Replace('\\', '/');
        var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        return asset;
    }

    [MenuItem("Scripts/Create Prefab", true)]
    static bool ValidateCreatePrefab()
    {
        return Selection.gameObjects != null && Selection.gameObjects.Length > 0 && !EditorUtility.IsPersistent(Selection.activeGameObject);
    }
}
// using System.IO;
// using UnityEngine;
// using UnityEditor;
// using Newtonsoft.Json.Linq;

// public class Example {
    

//     // Creates a new menu item 'Examples > Create Prefab' in the main menu.
//     [MenuItem("Scripts/Create Prefab")]
//     static void CreatePrefab() {
//         // Keep track of the currently selected GameObject(s)
//         GameObject[] objectArray = Selection.gameObjects;
//         // Loop through every GameObject in the array above
//         foreach (GameObject gameObject in objectArray) {
//             // Create folder Prefabs and set the path as within the Prefabs folder,
//             // and name it as the GameObject's name with the .Prefab format

//             string[] modelName =  gameObject.name.Split('_');
//             string basePath = $"Assets/_{modelName[0]}/" + gameObject.name;
//             string prefabPath = $"{basePath}.prefab";

//             var TexturePath = new JObject {
//                 ["albedo"] = basePath + "_base.png",
//                 ["normal"] = basePath + "_nor.png",
//                 ["occlus"] = basePath + "_occ.png"
//             };
//             // string albedoTextureName = basePath + "_base.png"; 
//             // string normalMapTextureName = basePath + "_nor.png"; 
//             // string occlusMapTextureName = basePath + "_occ.png"; 

//             string materialName = $"Assets/_{modelName[0]}/Materials/" + gameObject.name + ".mat";

//             // Get the Renderer component from the instantiated object
//             Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
            
//             if (renderer != null) {   
//                 //try and find the old material 
//                 Material materialInstance = new Material(Shader.Find("Standard"));
//                 Material existingMat = (Material)AssetDatabase.LoadAssetAtPath(materialName, typeof(Material));

//                 if (existingMat != null) {
//                     //AssetDatabase.DeleteAsset(materialName);
//                     Debug.Log($"material {materialName} already exists ");
//                     //AssetDatabase.MakeEditable(materialName);
//                     materialInstance = existingMat; 
//                     Debug.Log(materialInstance);
//                 }
//                 else {
//                     //Debug.Log(materialName);
//                     //if non found make new material 
//                     AssetDatabase.CreateAsset(materialInstance, materialName);
//                 }
                
//                 renderer.sharedMaterial = materialInstance; 
//                 EditorUtility.SetDirty(materialInstance);
                
//                 //mark as dirty and change settings
                
//                 byte[] bytes;
//                 // Access the material property to create an instance specific to this object

//                 // Load the textures by name
//                 Texture2D albedoMap = new Texture2D(2, 2);
//                 bytes = File.ReadAllBytes(TexturePath["albedo"]);
//                 albedoMap.LoadImage(bytes);

//                 Texture2D normalMap =  new Texture2D(2, 2);
//                 bytes = File.ReadAllBytes(TexturePath["normal"]);
//                 normalMap.LoadImage(bytes);

//                 Texture2D occlusMap = new Texture2D(2, 2);
//                 bytes = File.ReadAllBytes(TexturePath["occlus"]);
//                 occlusMap.LoadImage(bytes);
                
//                 materialInstance.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
//                 materialInstance.SetFloat("_GlossMapScale", 0.3f);

//                 if (albedoMap != null) {
//                     //Assign the albedo (main) texture
//                     materialInstance.mainTexture = albedoMap;
//                 }

//                 if (normalMap != null) {
//                    // Enable the normal map keyword for the shader (necessary for Standard shader)
//                     materialInstance.EnableKeyword("_NORMALMAP");
//                     // Assign the normal map to the "_BumpMap" property name
//                     materialInstance.SetTexture("_BumpMap", normalMap); 
//                 }
//                 if (occlusMap != null) {
//                     // Assign the occlusion map to the "_OcclusionMap" property name
//                     materialInstance.SetTexture("_OcclusionMap", occlusMap); 
//                 }

//                 AssetDatabase.Refresh();
//                 AssetDatabase.SaveAssets();

//             }
//             else {
//                 Debug.LogError("Renderer component not found on the prefab instance.");
//             }

//             // Make sure the file name is unique, in case an existing Prefab has the same name.
//             prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);       
//             // Create the new Prefab and log whether Prefab was saved successfully.
//             bool prefabSuccess= false;
            
//             if (PrefabUtility.IsPartOfAnyPrefab(gameObject)) {
//                 Debug.Log("Prefab already exists");
//             }
//             else {
//                 PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, prefabPath, InteractionMode.AutomatedAction, out prefabSuccess);
//             }
//         }
//     }

//     // Disable the menu item if no selection is in place.
//     [MenuItem("Scripts/Create Prefab", true)]
//     static bool ValidateCreatePrefab() {
//         return Selection.activeGameObject != null && !EditorUtility.IsPersistent(Selection.activeGameObject);
//     }
// }
