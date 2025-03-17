using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Create QuestSettings", fileName = "QuestSettings", order = 0)]
[Serializable]
public class QuestSettingsContainer : ScriptableObject
{
    public QuestSettings questSettings;
}

[Serializable]
public class QuestSettings
{
    public int TotalSteps;
    public string QuestName;
    public string Scene;
    public DialoguePosition[] DialoguePositions;
    public LocationPosition[] LocationPositions;
}