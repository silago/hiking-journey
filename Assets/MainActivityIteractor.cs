using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Action = System.Action;
#if UNITY_ANDROID
using Permission = UnityEngine.Android.Permission;
#endif



public class EntryPoint : MonoBehaviour
{
    [SerializeField] public List<Story> stories = new List<Story>();
}

public class Story : ScriptableObject
{
    public string storyName;
    public float steps;
    public Image previewImage;
    public List<DialoguePoint> dialoguePoints;
    public List<AreaCaption> areaCaptions;
}

[System.Serializable]
public class AreaCaption
{
    public float steps;
    public string caption;
}

[System.Serializable]
public class DialoguePoint
{
    public float steps;
    public string dialogue;
}