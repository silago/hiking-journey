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

public class MainActivityIteractor : MonoBehaviour
{
    [SerializeField] private Button HeathPermissionButton;
    [SerializeField] private Button GetHelthSreps;
    [SerializeField] private Button DeviceStepsPermissionButton;
    [SerializeField] private Button StartServiceButton;
    [SerializeField] private Button SendInstance;
    [SerializeField] private Button StopServiceButton;


    [SerializeField] private TMP_Text deviceCounter;
    [SerializeField] private TMP_Text healthCounter;
    [SerializeField] private TMP_Text status;
    
    private int totalDeviceStepsCount = 0;
    private AndroidJavaClass unityPlayer;
    private AndroidJavaObject currentActivity;

    private bool hasDevicePermission = false;
    private AndroidJavaClass customClass;

    private const string PackageName = "com.nevermind.healthconnectbridge.Bridge";
    private const string UnityDefaultJavaClassName = "com.unity3d.player.UnityPlayer";
    
    private void Awake()
    {
        unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        customClass = new AndroidJavaClass(PackageName);
        Debug.Log($"custom class {customClass}");
        
        StartServiceButton.onClick.AddListener(StartService);
        StopServiceButton.onClick.AddListener(StopService);
        SendInstance.onClick.AddListener(SendActivityReference);
        
        HeathPermissionButton.onClick.AddListener(RequestStepPermission);
        GetHelthSreps.onClick.AddListener(GetStepsCount);
        DeviceStepsPermissionButton.onClick.AddListener(RequestStepsPermission);
        hasDevicePermission = Permission.HasUserAuthorizedPermission("android.permission.ACTIVITY_RECOGNITION");
        
        
        status.text = "device perm" + (hasDevicePermission ? "Yes" : "No");
        if (hasDevicePermission)
        {
            InputSystem.EnableDevice(StepCounter.current); 
            hasDevicePermission = true;
            StepCounter.current.MakeCurrent();
            status.text = "device perm" + (hasDevicePermission ? "Yes" : "No");
        }
    }

    private async void RequestStepsPermission()
    {
        var cb = new PermissionCallbacks();
        cb.PermissionGranted += (x) =>
        {
            InputSystem.EnableDevice(StepCounter.current); 
            hasDevicePermission = true;
            StepCounter.current.MakeCurrent();
            status.text = "device perm" + (hasDevicePermission ? "Yes" : "No");
        };
        
        Permission.RequestUserPermission("android.permission.ACTIVITY_RECOGNITION", cb);
    }

    public void OnStepsCountReceived(string stepsCount)
    {
        Debug.Log("Steps Count: " + stepsCount);
        healthCounter.text = $"health steps count: {stepsCount}";
    }

    private void GetStepsCount()
    {
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            currentActivity.Call("getStepsCount", "2025-01-01T00:00:00.00Z", "2025-01-12T00:00:00.00Z");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception while calling Android method: " + ex.Message);
        }
    }
    
    public void StartService()
    {
        customClass.CallStatic("StartService");
    }

    public void StopService()
    {
        customClass.CallStatic("StopService");
    }

    void RequestStepPermission()
    {
        try
        {
            currentActivity.Call("requestStepPermission");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception while calling Android method: " + ex.Message);
        }
    }

    void SendActivityReference()
    {
        Debug.Log("Send actiity");
        customClass.CallStatic("ReceiveActivityInstance", currentActivity);
    }

    void Update()
    {
        if (hasDevicePermission)
        {
            var val = StepCounter.current.stepCounter.ReadValue();
            totalDeviceStepsCount += val;
            deviceCounter.text = $"Device steps {val}";
        }
        
    }
}


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






