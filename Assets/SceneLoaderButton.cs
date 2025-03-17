using System;
using UnityEngine;
using UnityEngine.UI;

public class SceneLoaderButton : MonoBehaviour
{
    [SerializeField] MainActivityBridge mainActivityBridge;
    [SerializeField] private string questName;

    [SerializeField] private Button button;
    public event Action<string> questButtonPressed = w => { }; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        questButtonPressed.Invoke(questName);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}


