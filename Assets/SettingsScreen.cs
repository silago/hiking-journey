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
    [Inject] private MainActivityBridge main;

    private void Awake()
    {
        useHealthAppToggle.isOn = storageService.UseHealthApp;
        useHealthAppToggle.onValueChanged.AddListener(OnUseHealthApp);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        hundredSteps.onClick.AddListener(OnHundredStepsClicked);
    }

    private void OnHundredStepsClicked()
    {
        if (storageService.CurrentQuest != null)
        {
            var data =storageService.CurrentQuestData;
            if (data != null)
            {
                data.NewSavedPositionSteps += 1000;
                storageService.CurrentQuestData = data;
            }
        }
        else if (storageService.PrevCurrentQuest != null)
        {
            var data =storageService.GetCurrentData(storageService.PrevCurrentQuest);
            if (data != null)
            {
                data.NewSavedPositionSteps += 1000;
                storageService.SetCurrentData(storageService.PrevCurrentQuest, data);
            }
        }
    }


    public async void OnUseHealthApp(bool useHealthApp)
    {
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