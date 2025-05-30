using System.Net.Mime;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using Zenject;

public class MainActivityBridge : MonoBehaviour
{
    [Inject] StorageService storageService;
    private static MainActivityBridge _instance;

    public static async UniTask<MainActivityBridge> Instance()
    {
        await UniTask.WaitWhile(() => _instance == null);
        return _instance;
    }

    private int totalDeviceStepsCount = 0;
    private AndroidJavaClass unityPlayer;
    private AndroidJavaObject currentActivity;

    private bool hasDevicePermission = false;
    private AndroidJavaClass bridgeClass;

    private const string PackageName = "com.nevermind.healthconnectbridge.Bridge";
    private const string UnityDefaultJavaClassName = "com.unity3d.player.UnityPlayer";

    private async void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }


        DontDestroyOnLoad(this.gameObject);

        if (!Application.isEditor)
        {
            unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            bridgeClass = new AndroidJavaClass(PackageName);
        }

        SendActivityReference();

        if (storageService.UseHealthApp)
        {
            var r = await CheckAllHealthPermissions();
            if (!r.success)
            {
                storageService.UseHealthApp = false;
            }
        }
        
        _instance = this;
    }

    /*
    public async UniTask<bool> RequestHealthPermissions()
    {
        if (!HasHealthPermissions())
        {
            bridgeClass.CallStatic("requestHealthPermissions");
            await UniTask.NextFrame();
            await AwaitAppFocused();
            await UniTask.NextFrame();
        }

        return HasHealthPermissions();
    }
    */

    public async UniTask<bool> RequestHealthStepsPermissions()
    {
        if (!await HasHealthStepsPermissions())
        {
            bridgeClass.CallStatic("requestHealthStepsPermissionsActivity");
            await UniTask.NextFrame();
            await AwaitAppFocused();
        }

        return await HasHealthStepsPermissions();
    }

    public async UniTask<bool> HasHealthStepsPermissions()
    {
        bridgeClass.CallStatic("checkHealthStepsPermissions");
        await AwaitAppFocused();
        while (!bridgeClass.GetStatic<bool>("HealthStepsPermissionUpdated"))
        {
            await UniTask.NextFrame();
        }

        return bridgeClass.GetStatic<bool>("HealthStepsPermissionGranted");
    }

    /*
    public bool HasHealthPermissions()
    {
        return bridgeClass.CallStatic<bool>("hasHealthPermissions");
    }
    */

    public bool IsHealthAppUpdateRequired()
    {
        var result = bridgeClass.CallStatic<bool>("isHealthAppUpdateRequired");
        Debug.Log($"healht app available: result: {result}");
        return result;
    }

    public bool IsHealthAppAvailable()
    {
        var result = bridgeClass.CallStatic<bool>("isHealthAppAvailable");
        Debug.Log($"healht app available: result: {result}");
        return result;
    }

    public async UniTask<bool> SetupDeviceCounter()
    {
        hasDevicePermission = Permission.HasUserAuthorizedPermission("android.permission.ACTIVITY_RECOGNITION");

        if (hasDevicePermission)
        {
            InputSystem.EnableDevice(StepCounter.current);
            hasDevicePermission = true;
            StepCounter.current.MakeCurrent();
            return true;
        }

        return false;
    }


    public bool HasAllPermissions()
    {
#if UNITY_EDITOR
        return true;
#endif
        var result = bridgeClass.CallStatic<bool>("hasAllPermissions");
        return result;
    }

    public async UniTask<bool> RequestHealthStepPermission()
    {
#if UNITY_EDITOR
        return true;
#endif


        bridgeClass.CallStatic<bool>("requestStepsPermission");
        await UniTask.NextFrame();
        await AwaitAppFocused();
        await UniTask.NextFrame();

        var result = bridgeClass.CallStatic<bool>("requestStepsPermission");
        return result;
    }

    public async UniTask RequestAllPermissions()
    {
#if UNITY_EDITOR
        return;
#endif

        bridgeClass.CallStatic("requestAllPermissions");
        await UniTask.NextFrame();
        await AwaitAppFocused();
        await UniTask.NextFrame();
    }

    public async UniTask<bool> CheckBatteryPermission()
    {
        if (Application.isEditor)
        {
            return true;
        }

        if (false == bridgeClass.CallStatic<bool>("isBatteryOptimizationEnabled"))
        {
            bridgeClass.CallStatic("requestDisableBatteryOptimization");
            await AwaitAppFocused();
            if (
                false == bridgeClass.CallStatic<bool>("isBatteryOptimizationEnabled")
            )
            {
                Debug.Log("Permission Denied battery");
                return false;
            }
        }

        return true;
    }

    public async UniTask<bool> CheckForAutoStartPermission()
    {
#if UNITY_EDITOR
        return true;
#endif

        if (false == bridgeClass.CallStatic<bool>("hasAutoStartPermission"))
        {
            bridgeClass.CallStatic("requestAutoStartPermission");
            await AwaitAppFocused();
            if (
                false == bridgeClass.CallStatic<bool>("hasAutoStartPermission")
            )
            {
                Debug.Log("Permission Denied autostart");
                return false;
            }
        }

        return true;
    }

    public async UniTask AwaitAppFocused()
    {
        do
        {
            await UniTask.NextFrame();
        } while (!Application.isFocused);
    }

    public void OnStepsCountReceived(string stepsCount)
    {
        Debug.Log("Steps Count: " + stepsCount);
    }

    public async void StartService()
    {
#if UNITY_EDITOR
        return;
#endif
        await Instance();
        bridgeClass.CallStatic("startService");
    }

    public void StopService()
    {
#if UNITY_EDITOR
        return;
#endif
        bridgeClass.CallStatic("stopService");
    }

    public async void RequestStepPermission()
    {
#if UNITY_EDITOR
        return;
#endif
        try
        {
            bridgeClass.CallStatic("requestActivityRecognitionPermission");

            await UniTask.NextFrame();
            await AwaitAppFocused();
            await UniTask.NextFrame();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception while calling Android method: " + ex.Message);
        }
    }

    public void SendActivityReference()
    {
#if UNITY_EDITOR
        return;
#endif
        Debug.Log("Send actiity");
        bridgeClass.CallStatic("receiveActivityInstance", currentActivity);
    }

    void Update()
    {
        if (hasDevicePermission)
        {
            var val = StepCounter.current.stepCounter.ReadValue();
            totalDeviceStepsCount += val;
        }
    }

    public async Task<bool> CheckAllBasicPermissions()
    {
        if (!HasAllPermissions())
        {
            await RequestAllPermissions();

            return (!HasAllPermissions());
        }

        else
        {
            return true;
        }
    }


    public async Task<CheckHelthPermissionResult> CheckAllHealthPermissions()
    {
        if (!IsHealthAppAvailable())
        {
            //useHealthAppToggle.isOn = storageService.UseHealthApp;
            return new CheckHelthPermissionResult()
            {
                success = false, error = "Health App Not Available"
            };
        }

        if (IsHealthAppUpdateRequired())
        {
            //useHealthAppToggle.isOn = storageService.UseHealthApp;
            return new CheckHelthPermissionResult()
            {
                success = false, error = "Health App Update Required"
            };
        }

        if (!HasAllPermissions())
        {
            await RequestAllPermissions();

            if (!HasAllPermissions())
            {
                return new CheckHelthPermissionResult()
                {
                    success = false, error = "Not enough all permissions"
                };
            }
        }

        var result = await CheckForAutoStartPermission();
        if (!result)
        {
            return new CheckHelthPermissionResult()
            {
                success = false, error = "Not enought enought Permissions (auto start)"
            };
        }

        result = await CheckBatteryPermission();
        if (!result)
        {
            return new CheckHelthPermissionResult()
            {
                success = false, error = "Not enought enought Permissions (battery)"
            };
        }

        /*
        result = await RequestHealthPermissions();
        if (!result)
        {
            return new CheckHelthPermissionResult()
            {
                success = false, error = "No Health Permissions"
            };
        }
        */

        result = await RequestHealthStepsPermissions();
        if (!result)
        {
            return new CheckHelthPermissionResult()
            {
                success = false, error = "No Health steps  Permissions"
            };
        }

        result = await RequestHealthStepPermission();
        if (!result)
        {
            return new CheckHelthPermissionResult()
            {
                success = false, error = "No Health Step Permissions"
            };
        }

        return new CheckHelthPermissionResult()
        {
            success = true
        };
    }

    public void OnQuestSet()
    {
        bridgeClass.CallStatic("onQuestSet");
    }
}

public class CheckHelthPermissionResult
{
    public bool success;
    public string error;
}