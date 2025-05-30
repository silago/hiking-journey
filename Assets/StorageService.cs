using System;
using Newtonsoft.Json;
using UnityEngine;

public class StorageService
{


    public void CompleteCurrentQuest()
    {
        CurrentQuestData = null;
        CurrentQuest = null;
    }
    
    public async void SetCurrentQuest(QuestSettings questSettings)
    {
            if (questSettings == null)
            {
                CurrentQuest = null;
                return;
            }
            
            CurrentQuest = questSettings.QuestName;
            CurrentQuestData ??= new QuestData()
            {
                QuestName = questSettings.QuestName
            };

            (await MainActivityBridge.Instance()).OnQuestSet();
    }

    public string CurrentQuest
    {
        get => PlayerPrefs.GetString("CurrentQuest");
        private set
        {
            if (value == null)
            {
                PlayerPrefs.DeleteKey("CurrentQuest");
            }
            else
            {
                PlayerPrefs.SetString("CurrentQuest", value);
            }
        }
    }

    public QuestData CurrentQuestData
    {
        get =>
            GetData<QuestData>($"CurrentQuestData_{CurrentQuest}", null);
        set => SetData($"CurrentQuestData_{CurrentQuest}", (value));
    }

    public QuestSettings CurrentQuestSettingsContainer
    {
        private get => GetData<QuestSettings>("CurrentQuestSettings", null);
        set => SetData("CurrentQuestSettings", value);
    }

    public bool UseHealthApp
    {
        get => PlayerPrefs.GetInt("UseHealthApp", 0) == 1 ? true : false;
        set { PlayerPrefs.SetInt("UseHealthApp", value ? 1 : 0); }
    }

    private T GetData<T>(string key, T defaultValue = default)
    {
        return PlayerPrefs.HasKey(key)
            ? JsonConvert.DeserializeObject<T>(PlayerPrefs.GetString(key))
            : defaultValue;
    }

    private void SetData<T>(string key, T value)
    {
        PlayerPrefs.SetString(key, JsonConvert.SerializeObject(value));
    }
}