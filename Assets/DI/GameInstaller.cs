using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<StorageService>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<MainActivityBridge>().AsSingle().NonLazy();
    }
}

public class SettingsScreenFactory :  PlaceholderFactory<SettingsScreen>
{
}