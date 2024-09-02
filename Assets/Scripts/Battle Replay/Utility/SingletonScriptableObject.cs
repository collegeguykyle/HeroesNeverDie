using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using UnityEditor;

public class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
{
    private static T _instance;
    public static T instance
    {
        get
        {
            LoadInstance();

            return _instance;
        }
    }

    public virtual void Initialize()
    {

    }

    public void SaveInstance()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(instance);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public static void VerifyScriptableObject()
    {
        LoadInstance();
    }

    private static void LoadInstance()
    {
        if (_instance == null)
        {
            _instance = Resources.Load<T>(typeof(T).Name);

            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<T>();

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.CreateAsset(_instance, _instance.GetAssetPath(true));
                UnityEditor.AssetDatabase.SaveAssets();
#endif
            }

            if (_instance != null)
            {
                _instance.Initialize();
            }
        }
    }

#if UNITY_EDITOR
    public string GetAssetPath(bool fromAssetsDirectory = false)
    {
        string folderPath = GetResourcesFolderPath(fromAssetsDirectory);
        string assetPath = System.IO.Path.Combine(folderPath, $"{typeof(T).Name}.asset");
        return assetPath;
    }

    public static string GetResourcesFolderPath(bool fromAssetsDirectory = false)
    {
        T asset = ScriptableObject.CreateInstance<T>();
        UnityEditor.MonoScript scriptAsset = UnityEditor.MonoScript.FromScriptableObject(asset);

        string scriptPath = UnityEditor.AssetDatabase.GetAssetPath(scriptAsset);

        System.IO.FileInfo fi = new System.IO.FileInfo(scriptPath);
        //string rootPath = fi.Directory.Parent.ToString();
        string rootPath = fi.Directory.ToString();

        string resourcesPath = rootPath + "/Resources";

        resourcesPath = resourcesPath.Replace("//", "/");
        resourcesPath = resourcesPath.Replace("\\\\", "\\");
        resourcesPath = resourcesPath.Replace("\\", "/");

        System.IO.Directory.CreateDirectory(resourcesPath);

        if (fromAssetsDirectory)
        {
            int assetsIndex = resourcesPath.IndexOf("/Assets/");
            resourcesPath = resourcesPath.Substring(assetsIndex + 1);
        }

        return resourcesPath;
    }
#endif

    public static void CreateSelf()
    {
        var Inst = instance;
    }
}