using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class BackButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [Inject]
    private StorageService storageService;

    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    private async void OnButtonClicked()
    {
       storageService.SetCurrentQuest(null);
       if (!Application.isEditor)
       {
           var bridge = await MainActivityBridge.Instance();
           bridge.StopService();
       }
       SceneManager.LoadScene("Main");
    }
}