// This script creates a new menu item Scripts>Create Prefab in the main menu.
// Use it to create Prefab(s) from the selected GameObject(s).
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.ComponentModel.Design;

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
            Debug.Log($"GameObject '{go.name}'");
            if (go == null || go.name == "default") continue;



            if (false && PrefabUtility.IsPartOfAnyPrefab(go))
            {
                Debug.Log($"GameObject '{go.name}' is already part of a prefab. Skipping.");
                continue;
            }

            var modelNameParts = go.name.Split('_');
            var rootFolder = Path.Combine("Assets", "_" + modelNameParts[0]);
            //Debug.LogError($"folder found {rootFolder}.");
            var basePath = Path.Combine(rootFolder, go.name);
            var prefabPath = basePath + ".prefab";
            var materialFolder = Path.Combine(rootFolder, "Materials").Replace('\\', '/');
            var resourcesFolder = Path.Combine(rootFolder, "Resources").Replace('\\', '/');
            var materialPath = Path.Combine(materialFolder, go.name + ".mat");

            // Ensure directory exists on disk
            if (!Directory.Exists(materialFolder))
                Directory.CreateDirectory(materialFolder);
            if(!Directory.Exists(resourcesFolder))
                Directory.CreateDirectory(resourcesFolder);
            



            var albedoTextureName = go.name + "_base.png";
            var normalMapTextureName = go.name + "_nor.png";
            var occlusionMapTextureName = go.name + "_occ.png";

            var renderer = go.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                Debug.LogError($"Renderer component not found on '{go.name}'.");
                continue;
            }

            var material = GetOrCreateMaterial(materialPath, materialFolder);
            renderer.sharedMaterial = material;
            //EditorUtility.SetDirty(material);

            var albedo = LoadTexture(rootFolder, albedoTextureName);
            var normal = LoadTexture(rootFolder, normalMapTextureName);
            var occlusion = LoadTexture(rootFolder, occlusionMapTextureName);

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
            material.SetInt("_SmoothnessTextureChannel", 1);
            material.SetFloat("_GlossMapScale", 0.3f);

            prefabPath = prefabPath.Replace('\\', '/');
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

            bool prefabSuccess = false;
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.AutomatedAction, out prefabSuccess);
        }
        //AssetDatabase.Refresh();
    }

    private static Material GetOrCreateMaterial(string materialPath, string materialFolder)
    {
        var normalizedPath = materialPath.Replace('\\', '/');
        var existing = AssetDatabase.LoadAssetAtPath<Material>(normalizedPath);
        if (existing != null)
            return existing;

        var mat = new Material(Shader.Find("Standard"));
        AssetDatabase.CreateAsset(mat, normalizedPath);
        return mat;
    }

    private static Texture LoadTexture(string basePath, string resource)
    {
        // basePath is expected to be an Assets/... path. We want to ensure the file
        // lives under a Resources folder and then call Resources.Load with the
        // path relative to that Resources folder (without extension).

        var assetFullPath = Path.Combine(basePath, resource).Replace('\\', '/');

        // If the asset exists outside a Resources folder, move it into basePath/Resources/
        var resourcesFolder = Path.Combine(basePath, "Resources").Replace('\\', '/');
        var destPath = Path.Combine(resourcesFolder, resource).Replace('\\', '/');

        if (File.Exists(assetFullPath) && !assetFullPath.Contains("/Resources/"))
        {
            // Ensure resources folder exists
            if (!AssetDatabase.IsValidFolder(resourcesFolder))
            {
                var parent = basePath.Replace('\\', '/');
                var folderName = "Resources";
                AssetDatabase.CreateFolder(parent, folderName);
            }

            // Try moving via AssetDatabase for proper meta handling
            var moveError = AssetDatabase.MoveAsset(assetFullPath, destPath);
            AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
        }

        // Now compute the Resources-relative path.
        // Find the substring after the last 'Resources/' segment
        var pathToCheck = assetFullPath.Contains("/Resources/") 
                        ? assetFullPath 
                        : destPath;
        var idx = pathToCheck.IndexOf("/Resources/", StringComparison.Ordinal);

        var resourcePath = pathToCheck.Substring(idx + "/Resources/".Length).Replace('\\', '/');
        var dot = resourcePath.LastIndexOf('.');
        if (dot >= 0) resourcePath = resourcePath.Substring(0, dot);

        Debug.Log($"Resources.Load path: '{resourcePath}' (from '{pathToCheck}')");

        var tex = Resources.Load<Texture>(resourcePath) as Texture;
        if (tex == null)
        {
            Debug.LogWarning($"Resources.Load failed for '{resourcePath}'. Trying AssetDatabase.LoadAssetAtPath.");
            // Try loading directly from the imported asset
            var candidatePath = pathToCheck.Replace('\\', '/');
            var loaded = AssetDatabase.LoadAssetAtPath<Texture>(candidatePath);
            return loaded;
        }

        return tex;
    }

    [MenuItem("Scripts/Create Prefab", true)]
    static bool ValidateCreatePrefab()
    {
        return Selection.gameObjects != null 
            && Selection.gameObjects.Length > 0 
            && !EditorUtility.IsPersistent(Selection.activeGameObject)
            && Selection.gameObjects[0].name != "default";
    }
}
