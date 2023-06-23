using System.Linq;
using Eto.Forms;
using Rhino.UI.Controls;
using StrucEngLib.Utils;

namespace StrucEngLib.Gui.Settings
{
    public class SettingsMainView : Scrollable
    {
        public static SettingsMainView Instance { get; private set; }

        public SettingsMainView(SettingsMainViewModel vm)
        {
            Instance = this;
            LoadUi(vm);
        }

        public void LoadUi(SettingsMainViewModel vm)
        {
            BackgroundColor = new Label().BackgroundColor;
            var layout = new DynamicLayout();
            var holder = new EtoCollapsibleSectionHolder();

            layout.AddRow(holder);
            ScrollHelper.ScrollParent(holder);
            new[]
            {
                new CollapsibleSectionHolder("About The StrucEng Library",
                    new AboutStrucEngLibView()),
                new CollapsibleSectionHolder("Install Dependencies",
                    new InstallSettingsView(vm)),
                new CollapsibleSectionHolder("Remote Server",
                    new RemoteServerSettingsView(vm)),
                new CollapsibleSectionHolder("Changelog",
                    new ChangelogSettingsView(vm)),
            }.ToList().ForEach(e => holder.Add(e));

            Content = layout;
        }

        public void DisposeUi()
        {
            Content.Dispose();
            Content = null;
        }
    }
}