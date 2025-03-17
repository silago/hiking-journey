using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    private StorageService storageService;

    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
        storageService = StorageService.Instance;
    }

    private async void OnButtonClicked()
    {
        storageService.CurrentQuest = null;
        var bridge = await MainActivityBridge.Instance();
        bridge.StopService();
        SceneManager.LoadScene("Main");

    }
}