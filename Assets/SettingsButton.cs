using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using Zenject;

public class SettingsButton : MonoBehaviour
{
    [SerializeField] private Button button;
    
    private GameObject screen;
    [Inject] private SettingsScreenFactory factory; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (screen == null)
        {
            screen = factory.Create().gameObject;
            //screen.transform.parent = this.transform.parent;
            screen.transform.localPosition = Vector3.zero;
            screen.SetActive(false);
        }
        
        button.onClick.AddListener(() => screen.gameObject.SetActive(true));
    }
}