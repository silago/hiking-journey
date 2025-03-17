using UnityEngine;

[CreateAssetMenu(menuName = "Create Settings", fileName = "Settings", order = 0)]
public class Settings : ScriptableObject
{
    public QuestSettingsContainer[] Quests;
}