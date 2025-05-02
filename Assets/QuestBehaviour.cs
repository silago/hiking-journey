using System;
using System.Collections.Generic;
using System.Linq;
using BezierSolution;
using com.cyborgAssets.inspectorButtonPro;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using Unity.Mathematics;
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
    [SerializeField] private BezierSpline bezierSpline;

    [FormerlySerializedAs("Settings")] [SerializeField]
    public QuestSettingsContainer settingsContainer;

    [SerializeField] private Vector3 cameraFollowOffset;
    [SerializeField] public Camera _camera;

    public Camera camera
    {
        get => _camera;
    } 
    [SerializeField] private GameObject completePanel;
    [SerializeField] private Button completeButton;
    [SerializeField] private Animator playerAnimator;
    

    public QuestData Data;
    private MainActivityBridge bridge;
    private bool isRunning;
    private StorageService storageService;
    private int segments;
    private float[] tValues;
    private float[] normalizedLengths;

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

    [ProButton]
    void BuildBezierFromSplie()
    {
        var items = splineContainer.Spline.Reverse().ToList();
        
        for (int i = 0; i < splineContainer.Spline.Count; i++)
        {
            if (i == 0)
            {
                continue;
            }
            var point = bezierSpline.InsertNewPointAt(0);
            var spliteItem = items[i]; 
            point.transform.localPosition = spliteItem.Position;
            //point.position = splineContainer.Spline[i].Position;
            point.rotation = Quaternion.identity;
            point.precedingControlPointLocalPosition = Vector3.zero;
            point.followingControlPointLocalPosition = Vector3.zero;

        }

        bezierSpline.Reverse();
    }

    [ProButton]
    private void UpdateCharacterPos(float steps)
    {
        var f = (float) steps / (float)settingsContainer.questSettings.TotalSteps;
        var pos = bezierSpline.GetPoint(f);
        pos.z = character.transform.position.z;
        character.position = pos;

        var camPos = camera.transform.position;
        camPos.x = character.position.x;
        camPos.y = character.position.y;
        camPos += cameraFollowOffset;
        camera.transform.position = camPos;
    }
    
    [ProButton]
    private void SetSteps(int steps)
    {
        var questData = StorageService.Instance.CurrentQuestData;
        questData.NewSavedPositionSteps = steps;
        StorageService.Instance.CurrentQuestData = questData;
    }

    private void Update()
    {
        var questData = StorageService.Instance.CurrentQuestData;
        
        info.text = $"is running {isRunning}\r\n";
        info.text += JsonConvert.SerializeObject(questData, Formatting.Indented);

        if (isRunning)
        {
            return;
        }
        
        if (questData.PreviousSavedPositionSteps < questData.NewSavedPositionSteps)
        {
            Progress();
        }
            
            
        if (questData.PreviousSavedPositionSteps == questData.NewSavedPositionSteps)
        {
            stepsCountText.text = questData.PreviousSavedPositionSteps.ToString();
        }

        return;
    }

    private async void Awake()
    {
        completePanel.SetActive(false);
        
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
        var pos = bezierSpline.GetPoint(f);
        pos.z = character.transform.position.z;
        character.position = pos;

        var camPos = camera.transform.position;
        camPos.x = character.position.x;
        camPos.y = character.position.y;
        camPos += cameraFollowOffset;
        camera.transform.position = camPos;
        
    }

    private void OnDrawGizmos()
    {
        if (settingsContainer != null)
        {
            foreach (var dialoguePosition in settingsContainer.questSettings.DialoguePositions)
            {
                var f = (float)dialoguePosition.StepIndex / (float)settingsContainer.questSettings.TotalSteps;
                var pos = bezierSpline.GetPoint(f);
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
        playerAnimator.SetTrigger("Walk");
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

                UpdateCharacterPos(step);
                data.PreviousSavedPositionSteps = (int)step;
                StorageService.Instance.CurrentQuestData = data;

                //var f = step / settingsContainer.questSettings.TotalSteps;
                //var pos = splineContainer.EvaluatePosition(f);
                //pos.z = character.transform.position.z;
                //character.position = pos;

                //var camPos = camera.transform.position;
                //camPos.x = character.position.x;
                //camPos.x = character.position.y;
                //camPos += cameraFollowOffset;
                //camera.transform.position = camPos;
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
            
            playerAnimator.SetTrigger("Stop");
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