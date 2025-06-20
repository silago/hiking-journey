using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class Startup : MonoBehaviour
{
    [Inject] private Settings settings;
    [Inject] private MainActivityBridge bridge;
    [Inject] private StorageService storageService;
    [SerializeField]
    private List<SceneLoaderButton> sceneLoaderButtons = new List<SceneLoaderButton>();

    private async void Awake()
    {
        if (!string.IsNullOrEmpty(storageService.CurrentQuest))
        {
            var questSettings = settings.Quests.FirstOrDefault(x => x.questSettings.QuestName == storageService.CurrentQuest);
            if (questSettings == null)
            {
                storageService.SetCurrentQuest(null);
                bridge.forceNotify();
            }
            else
            {
                var sceneName = questSettings.questSettings.Scene;
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                bridge.StartService();
                //SceneManager.LoadScene(sceneName);
            }

        }

        foreach (var sceneLoaderButton in sceneLoaderButtons)
        {
            sceneLoaderButton.questButtonPressed += SceneLoaderButtonOnQuestButtonPressed;
        }
    }

    private async void SceneLoaderButtonOnQuestButtonPressed(string questSceneName)
    {
        var mainActivityInstance = this.bridge; 
        
        var permission = await mainActivityInstance.CheckForAutoStartPermission();
        permission = permission && (await mainActivityInstance.CheckBatteryPermission());
        permission = permission && await mainActivityInstance.CheckAllBasicPermissions();
        if (!permission)
        {
            Debug.Log("Permission Denied ");
            return;
        }
        
        //storageService.CurrentQuest = questSceneName;
        //var questData = storageService.CurrentQuestData;
        var questSettings = settings.Quests.FirstOrDefault(x => x.questSettings.Scene == questSceneName);
        
        storageService.SetCurrentQuest(questSettings.questSettings);
        bridge.forceNotify();
        
        storageService.CurrentQuestSettingsContainer = questSettings.questSettings; 
        
        storageService.CurrentQuestData ??= new QuestData() { QuestName = questSceneName };

        Debug.Log($"loadScene: {questSettings?.questSettings.Scene}");
        SceneManager.LoadScene(questSettings.questSettings.Scene);
        
        bridge.StartService();
    }
}