using System;
using System.IO;
using System.Text;
using Rhino;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.UI;
using StrucEngLib.Gui;
using StrucEngLib.Model;

namespace StrucEngLib
{
    /// <summary>
    /// StrucEngLibPlugin Plugin class
    /// </summary>
    public class StrucEngLibPlugin : Rhino.PlugIns.PlugIn
    {
        public new static string Version = "0.0.22";
        public static string Website = "https://github.com/kfmResearch-NumericsTeam/StrucEng_Library_Plug_in";

        private static string _modelKey = "model";

        public MainViewModel MainViewModel { get; private set; }

        public StrucEngLibPlugin()
        {
            Instance = this;
            MainViewModel = new MainViewModel(new Workbench());
        }

        public static StrucEngLibPlugin Instance { get; private set; }

        protected override bool ShouldCallWriteDocument(FileWriteOptions options) => true;

        public void ResetData()
        {
            MainViewModel.Dispose();
            MainViewModel = null;
            MainViewModel = new MainViewModel(new Workbench());
        }

        public void ResetSandwichData()
        {
            var wb = MainViewModel.Workbench;
            MainViewModel.Dispose();
            wb.SandwichModel = null;
            MainViewModel = null;
            MainViewModel = new MainViewModel(wb);
        }

        protected override void WriteDocument(
            RhinoDoc doc,
            BinaryArchiveWriter archive,
            FileWriteOptions options)
        {
            MainViewModel.UpdateModel();
            var data = WorkbenchFactory.Instance.SerializeToString(MainViewModel.Workbench);
            var dict = new Rhino.Collections.ArchivableDictionary(1, "StrucEngLib");
            dict.Set(_modelKey, data);
            archive.WriteDictionary(dict);
        }

        protected override void ReadDocument(
            RhinoDoc doc,
            BinaryArchiveReader archive,
            FileReadOptions options)
        {
            Rhino.Collections.ArchivableDictionary dict = archive.ReadDictionary();
            var data = dict.GetString(_modelKey, null);
            MainViewModel.Dispose();
            var bench = WorkbenchFactory.Instance.DeserializeFromString(data);
            MainViewModel = new MainViewModel(bench);
        }

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            RegisterPanels();
            UpdateMenubar();
            return LoadReturnCode.Success;
        }

        private void RegisterPanels()
        {
            var stream = Assembly.GetManifestResourceStream(
                "StrucEngLib.EmbeddedResources.plugin-utility.generic.ico");
            var icon = stream != null ? new System.Drawing.Icon(stream) : null;
            Panels.RegisterPanel(this, typeof(MainView), "StrucEng Library", icon);
        }

        private void UpdateMenubar()
        {
            var v = Settings.GetString("PlugInVersion", null);
            if (!string.IsNullOrEmpty(v))
            {
                if (0 != string.Compare(Version, v, StringComparison.OrdinalIgnoreCase))
                {
                    var sb = new StringBuilder();
                    sb.Append(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                    sb.Append(@"\McNeel\Rhinoceros\7.0\UI\Plug-ins\");
                    sb.AppendFormat("{0}.rui", Assembly.GetName().Name);
                    var path = sb.ToString();
                    if (File.Exists(path))
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch
                        {
                            // XXX: Ignore
                        }
                    }

                    Settings.SetString("PlugInVersion", Version);
                }
            }
        }
    }
}