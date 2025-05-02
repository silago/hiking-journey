using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Startup : MonoBehaviour
{
    [SerializeField] private Settings settings;
    private StorageService storage;
    [SerializeField]
    private List<SceneLoaderButton> sceneLoaderButtons = new List<SceneLoaderButton>();

    private async void Awake()
    {
        this.storage = StorageService.Instance;

        if (!string.IsNullOrEmpty(storage.CurrentQuest))
        {
            var questSettings = settings.Quests.FirstOrDefault(x => x.questSettings.QuestName == storage.CurrentQuest);
            var sceneName = questSettings.questSettings.Scene;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            var bridge = await MainActivityBridge.Instance();
            bridge.StartService();
            //SceneManager.LoadScene(sceneName);
        }

        foreach (var sceneLoaderButton in sceneLoaderButtons)
        {
            sceneLoaderButton.questButtonPressed += SceneLoaderButtonOnQuestButtonPressed;
        }
    }

    private async void SceneLoaderButtonOnQuestButtonPressed(string questName)
    {
        var mainActivityInstance = await MainActivityBridge.Instance();
        
        var permission = await mainActivityInstance.CheckForAutoStartPermission();
        if (!permission)
        {
            Debug.Log("Permission Denied ");
            return;
        }
        
        storage.CurrentQuest = questName;
        var questData = storage.CurrentQuestData;
        var questSettings = settings.Quests.FirstOrDefault(x => x.questSettings.QuestName == questName);
        
        storage.CurrentQuestSettingsContainer = questSettings.questSettings; 
        
        storage.CurrentQuestData ??= new QuestData() { QuestName = questName };

        Debug.Log($"loadScene: {questSettings?.questSettings.Scene}");
        SceneManager.LoadScene(questSettings.questSettings.Scene);
        
        var bridge = await MainActivityBridge.Instance();
        bridge.StartService();
    }
}