using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject screen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.onClick.AddListener(() => screen.SetActive(true));
    }
}