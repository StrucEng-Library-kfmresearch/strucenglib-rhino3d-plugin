using System;
using Rhino;
using Rhino.Commands;
using Rhino.Runtime;
using Rhino.UI;
using Command = Rhino.Commands.Command;

namespace StrucEngLib
{
    /// <summary>
    /// Command to show wiki
    /// </summary>
    public class CommandHelp : Command
    {
        public static string Url =
            "https://strucenglib.ethz.ch/strucenglib_plugin/home/";

        public override string EnglishName => "StrucEngLibHelp";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                PythonScript ps = PythonScript.Create();
                ps.ExecuteScript($"import webbrowser; webbrowser.open('{Url}')");
            }
            catch (Exception)
            {   
            }
            return Result.Success;
        }
    }
}
