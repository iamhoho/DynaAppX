using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using XrmToolBox.Extensibility;

namespace DynaAppX
{
    public class Settings
    {
        public string LastUsedOrganizationWebappUrl { get; set; }

        public Settings()
        {
        }

        public void Save()
        {
            SettingsManager.Instance.Save(GetType(), this);
        }
    }
}