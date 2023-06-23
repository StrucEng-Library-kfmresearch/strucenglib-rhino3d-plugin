using Rhino;
using Rhino.Commands;
using Rhino.UI;
using StrucEngLib.Install;
using Command = Rhino.Commands.Command;

namespace StrucEngLib
{
    public class CommandInstall : Command
    {
        public override string EnglishName => "StrucEngLibInstallDependencies";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var wb = StrucEngLibPlugin.Instance.MainViewModel.Workbench;
            Installer i = new Installer(wb);
            i.Owner = RhinoEtoApp.MainWindow;
            i.Show();
            return Result.Success;
        }
    }
}