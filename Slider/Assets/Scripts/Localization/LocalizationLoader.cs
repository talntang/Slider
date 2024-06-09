using System;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

using LocalizationFile = Localization.LocalizationFile;

[RequireComponent(typeof(SettingRetriever))]
public class LocalizationLoader : Singleton<LocalizationLoader>
{
    [SerializeField]
    private TMP_FontAsset localizationFont;
    
    [SerializeField]
    private TMP_FontAsset defaultUiFont;

    public static TMP_FontAsset LocalizationFont => _instance.localizationFont;
    public static TMP_FontAsset DefaultUiFont => _instance.defaultUiFont;
    
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        RefreshLocalization(SceneManager.GetActiveScene());
    }

    public static void RefreshLocalization()
    {
        if (_instance != null)
        {
            _instance.RefreshLocalization(SceneManager.GetActiveScene()); // do not use GameObject.Scene since it will return the non destructable scene instead!
        }
    }
    
    public static string CurrentLocale => _instance == null ? LocalizationFile.DefaultLocale : _instance.GetComponent<SettingRetriever>().ReadSettingValue() as string;

    private static LocalizationFile LoadAssetAndConfigureLocaleDefaults(string locale, string sceneLocalizationFilePath) 
    {
        string localeConfigFilePath = LocalizationFile.LocaleGlobalFilePath(locale);
        var (localeConfigFile, errorLocale) = LocalizationFile.MakeLocalizationFile(locale, localeConfigFilePath);
        if (localeConfigFile == null)
        {
            LocalizationFile.PrintParserError(errorLocale, localeConfigFilePath);
            return null;
        }
        var (sceneLocalizationFile, errorScene) = LocalizationFile.MakeLocalizationFile(locale, sceneLocalizationFilePath, localeConfigFile);
        if (sceneLocalizationFile == null)
        {
            LocalizationFile.PrintParserError(errorScene, sceneLocalizationFilePath);
        }
        return sceneLocalizationFile;
    }
    
    private void RefreshLocalization(Scene scene)
    {
        var locale = CurrentLocale;
        
        LocalizableContext loaded = LocalizableContext.ForSingleScene(scene);
        LocalizableContext persistent = LocalizableContext.ForSingleScene(GameManager.instance.gameObject.scene);

        var loadedAsset = LoadAssetAndConfigureLocaleDefaults(locale, LocalizationFile.AssetPath(locale, scene));
        loaded.Localize(loadedAsset);
        persistent.Localize(loadedAsset);
    }

    public static void LocalizePrefab(GameObject prefab)
    {
        var locale = CurrentLocale;
        LocalizableContext ctx = LocalizableContext.ForSinglePrefab(prefab);
        var loadedAsset = LoadAssetAndConfigureLocaleDefaults(locale, LocalizationFile.AssetPath(locale, prefab));
        ctx.Localize(loadedAsset);
    }
}