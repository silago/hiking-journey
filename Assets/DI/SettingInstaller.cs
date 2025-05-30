using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "UntitledInstaller", menuName = "Installers/SettingsUntitledInstaller")]
public class SettingInstaller : ScriptableObjectInstaller<SettingInstaller>
{
    [SerializeField] private Settings _settings;
    public override void InstallBindings()
    {
        Container.BindInstance(_settings).AsSingle().NonLazy();
    }
}