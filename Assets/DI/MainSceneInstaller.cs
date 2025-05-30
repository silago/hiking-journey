using UnityEngine;
using Zenject;

public class MainSceneInstaller : MonoInstaller
{
    [SerializeField] private Transform screensCanvas;
    [SerializeField] private SettingsScreen settingsScreen;
    public override void InstallBindings()
    {
        Container.BindFactory<SettingsScreen, SettingsScreenFactory>().FromComponentInNewPrefab(settingsScreen).UnderTransform(screensCanvas);
    }
}