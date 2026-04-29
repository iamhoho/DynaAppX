using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Tooling.PackageDeployment;
using McTools.Xrm.Connection;
using System.AddIn;
using System.Windows.Forms;
using XrmToolBox.Extensibility;

namespace DynaAppXPlugin
{
    public class DynaAppXPlugin : PluginControlBase<ConnectionManager>, IHelpNotifiable
    {
        private DynaAppXControl _mainControl;

        public DynaAppXPlugin()
        {
            _mainControl = new DynaAppXControl();
        }

        public override void Closing()
        {
            base.Closing();
        }

        public override UserControl GetUserControl()
        {
            return _mainControl;
        }

        public override void OnConnectionUpdated(ConnectionUpdatedEventArgs e)
        {
            base.OnConnectionUpdated(e);
            _mainControl?.SetConnection(e.ConnectionDetail.Service);
        }

        public void ShowHelp()
        {
            MessageBox.Show(
                "DynaAppX Plugin - Access Check & Flow Invocation Tool\n\n" +
                "This plugin provides:\n" +
                "- AccessCheck: Check user permissions on CRM records\n" +
                "- InvokeFlow: Execute Dynamics 365 workflows and actions",
                "DynaAppX Help",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}