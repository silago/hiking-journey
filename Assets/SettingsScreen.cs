using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SettingsScreen : MonoBehaviour
{
    [SerializeField] private Toggle useHealthAppToggle;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button hundredSteps;
    [SerializeField] private TMP_Text error;
    [Inject] StorageService storageService;

    private void Awake()
    {
        useHealthAppToggle.isOn = storageService.UseHealthApp;
        useHealthAppToggle.onValueChanged.AddListener(OnUseHealthApp);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        hundredSteps.onClick.AddListener(OnHundredStepsClicked);
    }

    private void OnHundredStepsClicked()
    {
        var data =storageService.CurrentQuestData;
        if (data != null)
        {
            data.NewSavedPositionSteps += 1000;
            storageService.CurrentQuestData = data;
        }
    }


    public async void OnUseHealthApp(bool useHealthApp)
    {
        var main = await MainActivityBridge.Instance();
        var prev = storageService.UseHealthApp;
        if (useHealthApp)
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


        storageService.UseHealthApp = useHealthApp;
        useHealthAppToggle.isOn = storageService.UseHealthApp;


        var newV = storageService.UseHealthApp;
        if (prev != newV)
        {
            main.StopService();
            await UniTask.NextFrame();
            main.StartService();
        }
    }
}