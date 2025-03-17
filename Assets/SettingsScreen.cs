using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
    [SerializeField] private Toggle useHealthAppToggle;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text error;

    private void Awake()
    {
        useHealthAppToggle.isOn = StorageService.Instance.UseHealthApp;
        useHealthAppToggle.onValueChanged.AddListener(OnUseHealthApp);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }


    public async void OnUseHealthApp(bool v)
    {
        var main = await MainActivityBridge.Instance();
        var prev = StorageService.Instance.UseHealthApp;
        if (v)
        {
            var resul = await main.CheckAllHealthPermissions();
            if (!resul.success)
            {
                error.text = resul.error;
                useHealthAppToggle.isOn = false;
                return;
            }

            useHealthAppToggle.isOn = true;
        }


        if (v)
        {
            StorageService.Instance.UseHealthApp = v;
            useHealthAppToggle.isOn = StorageService.Instance.UseHealthApp;
        }


        var newV = StorageService.Instance.UseHealthApp;
        if (prev != newV)
        {
            main.StopService();
            await UniTask.NextFrame();
            main.StartService();
        }
    }
}