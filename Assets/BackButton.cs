using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class BackButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [Inject]
    private StorageService storageService;
    
    [Inject]
    private MainActivityBridge bridge;

    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
       storageService.SetCurrentQuest(null);
       bridge.forceNotify();
       
       
       if (!Application.isEditor)
       {
           bridge.StopService();
       }
       
       SceneManager.LoadScene("Main");
    }
}