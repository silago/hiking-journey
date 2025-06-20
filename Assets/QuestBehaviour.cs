using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using BezierSolution;
using com.cyborgAssets.inspectorButtonPro;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Newtonsoft.Json;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Splines;
using UnityEngine.UI;
using Yarn;
using Yarn.Unity;
using Zenject;
using Color = UnityEngine.Color;

public class QuestBehaviour : MonoBehaviour
{
    private static readonly int Walk = Animator.StringToHash("Walk");
    [SerializeField] private TMP_Text questTitle;
    [SerializeField] private TMP_Text stepsCountText;

    [SerializeField] private DialogueRunner runner;
    [SerializeField] private YarnProject yarnProject;

    [SerializeField] private float Speed;
    [SerializeField] private TMP_Text info;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //[SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Transform character;
    //[SerializeField] private BezierSpline bezierSpline;

    [FormerlySerializedAs("Settings")] [SerializeField]
    public QuestSettingsContainer settingsContainer;

    [SerializeField] private Vector3 cameraFollowOffset;
    [SerializeField] public Camera _camera;
    [Inject] private StorageService storageService;
    [Inject] private MainActivityBridge bridge;
    [SerializeField] TilemapPathFinder tilemapPathFinder;

    public Camera camera
    {
        get => _camera;
    }

    [SerializeField] private GameObject completePanel;
    [SerializeField] private Button completeButton;
    [SerializeField] private Animator playerAnimator;

    private bool isRunning;
    private int segments;
    private float[] tValues;
    private float[] normalizedLengths;
    [SerializeField]
    private int pathSegmentIndex = 0;

    /*
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
    */

    private void UpdateCharacterPosByDelta(Vector3 characterPos, float delta, ref int index)
    {
        var pos = tilemapPathFinder.Move(characterPos, delta, ref index);
        
        pos.z = character.transform.position.z;
        character.position = pos;

        var camPos = camera.transform.position;
        camPos.x = character.position.x;
        camPos.y = character.position.y;
        camPos += cameraFollowOffset;
        camera.transform.position = camPos;
    }

    [ProButton]
    private void UpdateCharacterPosSBySteps(float steps)
    {
        
        var unitPerStep = tilemapPathFinder.GetLength() / settingsContainer.questSettings.TotalSteps;

        var delta = steps * unitPerStep;
        var idx = 0;
        var pos = tilemapPathFinder.GetPositionByDelta(delta, ref idx);

        pos.z = character.transform.position.z;
        character.transform.position = pos;

        var camPos = camera.transform.position;
        camPos.x = character.position.x;
        camPos.y = character.position.y;
        camPos += cameraFollowOffset;
        camera.transform.position = camPos;
    }


    [ProButton]
    private void SetSteps(int steps)
    {
        var questData = storageService.CurrentQuestData;

        if (questData == null)
        {
            storageService.SetCurrentQuest(settingsContainer.questSettings);
            bridge.forceNotify();
            questData = storageService.CurrentQuestData;
        }

        questData.NewSavedPositionSteps = steps;

        storageService.CurrentQuestData = questData;
    }

    private void Update()
    {
        var questData = storageService.CurrentQuestData;
        if (questData == null)
        {
            return;
        }

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
            stepsCountText.text = "Пройдено: " + ((float) questData?.PreviousSavedPositionSteps / (float)settingsContainer.questSettings.TotalSteps * 100f).ToString("#.0") + "%";
        }

        return;
    }

    private async void Awake()
    {
        
        var data = storageService.CurrentQuestData;
        storageService.CurrentQuestData = data;
        
        completePanel.SetActive(false);
        completeButton.onClick.AddListener(OnCompleteButtonClick);

        await bridge.CheckForAutoStartPermission();
        await bridge.CheckBatteryPermission();
        
    }

    private async void OnCompleteButtonClick()
    {
        storageService.CompleteCurrentQuest();
        bridge.StopService();
        SceneManager.LoadScene("Main");
    }

    void Start()
    {
        if (Application.isEditor)
        {
            var questData = storageService.CurrentQuestData;

            if (questData == null)
            {
                storageService.SetCurrentQuest(settingsContainer.questSettings);
                bridge.forceNotify();
            }
        }
        
        var data = storageService.CurrentQuestData;


        runner.onDialogueComplete.AddListener(OnDialogueComplete);
        questTitle.text = settingsContainer.questSettings.QuestName;
        stepsCountText.text = "Пройдено: " + ((float) data?.PreviousSavedPositionSteps / (float)settingsContainer.questSettings.TotalSteps * 100f).ToString("#.0") + "%";


        var unitPerStep = tilemapPathFinder.GetLength() / settingsContainer.questSettings.TotalSteps;
        var delta = data?.PreviousSavedPositionSteps ?? 0 * unitPerStep;
        var idx = 0;
        var pos = tilemapPathFinder.GetPositionByDelta(delta, ref idx);
        
        pos.z = character.transform.position.z;
        character.position = pos;

        var camPos = camera.transform.position;
        camPos.x = character.position.x;
        camPos.y = character.position.y;
        camPos += cameraFollowOffset;
        camera.transform.position = camPos;
        
        
        UpdateCharacterPosSBySteps(data.PreviousSavedPositionSteps);
        pathSegmentIndex = tilemapPathFinder.GetClosestIndex(character.transform.position);
        
        Progress();
        
    }

    private void OnDrawGizmos()
    {
        
        
        var unitPerStep = tilemapPathFinder.GetLength() / settingsContainer.questSettings.TotalSteps;
        var start = tilemapPathFinder.GetStartPoint();
        
        if (settingsContainer != null)
        {

            Gizmos.color = Color.red;

            var t = 0f;
            var prevDelta = 0f;
            var idx = 0;
            for (var index = 0; index < settingsContainer.questSettings.DialoguePositions.Length; index++)
            {
                var dialoguePosition = settingsContainer.questSettings.DialoguePositions[index];
                if (dialoguePosition.StepIndex > settingsContainer.questSettings.TotalSteps)
                {
                    continue;
                }
                var delta = dialoguePosition.StepIndex * unitPerStep;
                var pos = tilemapPathFinder.GetPositionByDelta(delta);

                Gizmos.DrawSphere(pos, 0.3f);
            }

            Gizmos.color = Color.yellow;
            
            for (var index = 0; index < settingsContainer.questSettings.LocationPositions.Length; index++)
            {
                var dialoguePosition = settingsContainer.questSettings.LocationPositions[index];
                
                if (dialoguePosition.StepIndex > settingsContainer.questSettings.TotalSteps)
                {
                    continue;
                }
                if (dialoguePosition.StepIndex > settingsContainer.questSettings.TotalSteps)
                {
                    continue;
                }
                var delta = dialoguePosition.StepIndex * unitPerStep;
                var pos = tilemapPathFinder.GetPositionByDelta(delta);

                Gizmos.DrawSphere(pos, 0.3f);
            }
        }
    }


    private void OnDialogueComplete()
    {
        bridge.forceNotify();
        Progress();
        
    }

    private DialoguePosition GetNextDialogue()
    {
        var data = storageService.CurrentQuestData;

        if (data.NewSavedPositionSteps == 0)
        {
            return settingsContainer.questSettings.DialoguePositions
                .FirstOrDefault(x =>
                    ((float)x.StepIndex <= 0)
                    && !data.CompleteDialogues.Contains(x.Name));
        }

        return settingsContainer.questSettings.DialoguePositions
            .FirstOrDefault(x =>
                ((float)x.StepIndex / settingsContainer.questSettings.TotalSteps) > data.Normalized
                && !data.CompleteDialogues.Contains(x.Name));
    }


    public async UniTask Progress()
    {
        isRunning = true;
        
        playerAnimator.SetBool(Walk , true);
        var data = storageService.CurrentQuestData;

        
        var unitPerStep = tilemapPathFinder.GetLength() / settingsContainer.questSettings.TotalSteps;
        var stepPerUnit = settingsContainer.questSettings.TotalSteps / tilemapPathFinder.GetLength();

        try
        {
            var nextDialogue = GetNextDialogue();

            var step = (float)data.PreviousSavedPositionSteps;
            if (data.PreviousSavedPositionSteps == -1)
            {
                data.PreviousSavedPositionSteps = 0;
            }

            do
            {
                await UniTask.NextFrame();
                var delta = Speed * Time.deltaTime;

                stepsCountText.text = "Пройдено: " 
                                      + ((float) data?.PreviousSavedPositionSteps / (float)settingsContainer.questSettings.TotalSteps * 100f).ToString("#.0") + "%";

                if (nextDialogue != null
                    &&
                    data.PreviousSavedPositionSteps >= nextDialogue.StepIndex)
                {
                    var cc = data.CompleteDialogues.ToList();
                    cc.Add(nextDialogue.Name);
                    data.CompleteDialogues = cc.ToArray();
                    storageService.CurrentQuestData = data;
                    
                    step = Mathf.Min(step, data.NewSavedPositionSteps);
                    data.PreviousSavedPositionSteps =
                        (int)step; //(int)(data.Normalized * (float)settingsContainer.questSettings.TotalSteps);
                    storageService.CurrentQuestData = data;
                    runner.StartDialogue(new DialogueReference(yarnProject, nextDialogue.Name));

                    playerAnimator.SetBool(Walk , false);
                    return;
                }

                UpdateCharacterPosByDelta(character.transform.position, delta, ref pathSegmentIndex);

                step += stepPerUnit * delta;
                step = Mathf.Min(step, data.NewSavedPositionSteps);

                data.PreviousSavedPositionSteps =
                    (int)step; //(int)(data.Normalized * (float)settingsContainer.questSettings.TotalSteps);
                storageService.CurrentQuestData = data;
            } while (data.PreviousSavedPositionSteps < data.NewSavedPositionSteps &&
                     data.PreviousSavedPositionSteps < settingsContainer.questSettings.TotalSteps);

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

            playerAnimator.SetBool(Walk , false);
        }
    }
}


[Serializable]
public class QuestData
{
    public string QuestName;
    public int PreviousSavedPositionSteps = -1;
    public int NewSavedPositionSteps = 0;
    public float Normalized;
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

public class LeftToRightPath
{
    public List<Vector3> points;


    public Vector3 Move(Vector3 pos, float delta, ref int index)
    {
        var current = index;
        var next = index + 1;
        
        var nextPoint =  next >= points.Count-1 ? points.Last()  : points[next];
        var distance = Vector3.Distance(nextPoint, pos);
        pos = Vector3.MoveTowards(pos, nextPoint, delta);
        if (distance < delta)
        {
            index++;
        }

        return pos;
    }
    
    public int GetClosest(Vector3 pos)
    {
        var closestDistance = Vector3.Distance(points[0], pos);
        var closestIndex = 0;
        
        for (var index =  1; index < points.Count; index++)
        {
            var point = this.points[index];
            var distance = Vector3.Distance(point, pos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = index;
            }
            else
            {
                break;
            }
        }

        return closestIndex;
    } 
}

[Serializable]
public class TileSettngs
{
    public string TileName;
    public Vector3 Offset;
}