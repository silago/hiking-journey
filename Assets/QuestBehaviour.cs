using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Splines;
using UnityEngine.UI;
using Yarn;
using Yarn.Unity;

public class QuestBehaviour : MonoBehaviour
{
    [SerializeField] private TMP_Text questTitle;
    [SerializeField] private TMP_Text stepsCountText;

    [SerializeField] private DialogueRunner runner;
    [SerializeField] private YarnProject yarnProject;

    [SerializeField] private float Speed;
    [SerializeField] private TMP_Text info;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Transform character;

    [FormerlySerializedAs("Settings")] [SerializeField]
    public QuestSettingsContainer settingsContainer;

    [SerializeField] public Camera camera;
    [SerializeField] private GameObject completePanel;
    [SerializeField] private Button completeButton;
    
    public bool Do = false;
    public int NewSteps;


    public QuestData Data;
    private MainActivityBridge bridge;
    private bool isRunning;
    private StorageService storageService;

    public QuestData _Data
    {
        get
        {
            var s = PlayerPrefs.GetString(settingsContainer.questSettings.QuestName, null);
            if (string.IsNullOrEmpty(s))
            {
                return new QuestData();
            }

            var result = JsonUtility.FromJson<QuestData>(s);
            return result;
        }

        set
        {
            var s = JsonUtility.ToJson(value);
            PlayerPrefs.SetString(settingsContainer.questSettings.QuestName, s);
        }
    }
    
    private void Update()
    {
        var questData = StorageService.Instance.CurrentQuestData;
        
        info.text = $"is running {isRunning}\r\n";
        info.text += JsonConvert.SerializeObject(questData, Formatting.Indented);

        if (Do)
        {
            questData.NewSavedPositionSteps = NewSteps;
            StorageService.Instance.CurrentQuestData = questData;
            Do = false;
            return;
        }
        
        if (!isRunning)
        {
            if (questData.PreviousSavedPositionSteps < questData.NewSavedPositionSteps)
            {
                Progress();
            }
            
            
            if (questData.PreviousSavedPositionSteps == questData.NewSavedPositionSteps)
            {
                stepsCountText.text = questData.PreviousSavedPositionSteps.ToString();
            }

        }

        return;
    }

    private async void Awake()
    {
        completePanel.SetActive(true);
        
        this.bridge = await MainActivityBridge.Instance();
        await bridge.CheckForAutoStartPermission();
        
        completeButton.onClick.AddListener(OnCompleteButtonClick);
        storageService = StorageService.Instance;
    }

    private async void OnCompleteButtonClick()
    {
        storageService.CurrentQuest = null;
        var bridge = await MainActivityBridge.Instance();
        bridge.StopService();
        SceneManager.LoadScene("Main");
    }

    void Start()
    {
        runner.onDialogueComplete.AddListener(OnDialogueComplete);
        questTitle.text = settingsContainer.questSettings.QuestName;
        stepsCountText.text = $"Шагов: {Data.PreviousSavedPositionSteps}";


        var f = Data.PreviousSavedPositionSteps / settingsContainer.questSettings.TotalSteps;
        var pos = splineContainer.EvaluatePosition(f);
        pos.z = character.transform.position.z;
        character.position = pos;

        var camPos = camera.transform.position;
        camPos.x = character.position.x;
        camera.transform.position = camPos;
    }

    private void OnDrawGizmos()
    {
        if (settingsContainer != null)
        {
            foreach (var dialoguePosition in settingsContainer.questSettings.DialoguePositions)
            {
                var f = (float)dialoguePosition.StepIndex / (float)settingsContainer.questSettings.TotalSteps;
                var pos = splineContainer.EvaluatePosition(f);
                pos.z = character.transform.position.z;

                Gizmos.DrawSphere(pos, 0.1f);
            }
        }
    }


    private void OnDialogueComplete()
    {
        Progress();
    }


    public async UniTask Progress()
    {
        isRunning = true;
        var data = StorageService.Instance.CurrentQuestData;

        try
        {
            var nextDialogue =
                settingsContainer.questSettings.DialoguePositions
                    .FirstOrDefault(x =>
                        x.StepIndex > data.PreviousSavedPositionSteps && !Data.CompleteDialogues.Contains(x.Name));

            var step = (float)data.PreviousSavedPositionSteps;


            while (step < data.NewSavedPositionSteps)
            {
                await UniTask.NextFrame();

                step += (Speed * Time.deltaTime);

                stepsCountText.text = $"Шагов: {(int)step}";
                if (nextDialogue != null && step > nextDialogue.StepIndex)
                {
                    data.PreviousSavedPositionSteps = (int)step;
                    var cc = data.CompleteDialogues.ToList();
                    cc.Add(nextDialogue.Name);
                    data.CompleteDialogues = cc.ToArray();
                    StorageService.Instance.CurrentQuestData = data;
                    runner.StartDialogue(new DialogueReference(yarnProject, nextDialogue.Name));

                    return;
                }

                data.PreviousSavedPositionSteps = (int)step;
                StorageService.Instance.CurrentQuestData = data;

                var f = step / settingsContainer.questSettings.TotalSteps;
                var pos = splineContainer.EvaluatePosition(f);
                pos.z = character.transform.position.z;
                character.position = pos;

                var camPos = camera.transform.position;
                camPos.x = character.position.x;
                camera.transform.position = camPos;
            }

            isRunning = false;
        }
        finally
        {
            if (data.CompleteDialogues.Length == settingsContainer.questSettings.DialoguePositions.Length
                && data.NewSavedPositionSteps >= settingsContainer.questSettings.TotalSteps
                && !isRunning)
            {
                completePanel.SetActive(true);
            }
        }
    }
}



[Serializable]
public class QuestData
{
    public string QuestName;
    public int PreviousSavedPositionSteps;
    public int NewSavedPositionSteps;
    public string[] CompleteDialogues = Array.Empty<string>();
}

/*
public class QuestsSettings : ScriptableObject {
   public QuestSettings[] Quests;
}
*/

[Serializable]
public class LocationPosition
{
    public int StepIndex;
    public string Text;
}

[Serializable]
public class DialoguePosition
{
    public int StepIndex;
    public string Name;
}