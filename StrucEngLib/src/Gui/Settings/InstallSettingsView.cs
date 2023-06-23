using System;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.Runtime;
using Bitmap = System.Drawing.Bitmap;

namespace StrucEngLib.Gui.Settings
{
    /// <summary>Settings for Install Dependencies</summary>
    public class InstallSettingsView : DynamicLayout
    {
        private LinkButton _btOpenInstaller;

        public InstallSettingsView(SettingsMainViewModel vm)
        {
            Padding = new Padding(5, 5, 5, 10);
            Spacing = new Size(5, 5);
            AddRow(_btOpenInstaller = new LinkButton()
            {
                Text =
                    "Open Dependency Installer"
            });

            _btOpenInstaller.Click += (sender, args) =>
            {
                RhinoApp.ExecuteCommand(RhinoDoc.ActiveDoc, "StrucEngLibInstallDependencies");
            };
        }
    }
}