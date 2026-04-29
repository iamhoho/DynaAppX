using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using XrmToolBox.Extensibility;

namespace DynaAppX
{
    public class Settings : SettingsBase
    {
        public string LastUsedOrganizationWebappUrl { get; set; }

        public Settings()
        {
        }

        public override void Save(IOrganizationService service = null)
        {
            SettingsManager.Instance.Save(GetType(), this);
        }
    }
}