using System;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using Rhino.Runtime;
using Bitmap = System.Drawing.Bitmap;

namespace StrucEngLib.Gui.Settings
{
    /// <summary>About The Plugin</summary>
    public class AboutStrucEngLibView : DynamicLayout
    {
        public AboutStrucEngLibView()
        {
            Padding = new Padding(5, 5, 5, 10);
            Spacing = new Size(5, 5);
            AddRow(new DynamicLayout()
                {
                    Padding = new Padding(5),
                    Spacing = new Size(5, 20),
                    Rows =
                    {
                        new ImageView()
                        {
                            Size = new Size(-1, 80),
                            Image = Rhino.UI.EtoExtensions.ToEto(GetLogo())
                        },
                    }
                }
            );
            AddRow(new Label()
            {
                Text =
                    "The StrucEng Library includes mechanical models, safety concepts, GUI's, " +
                    "load generator, etc. for the structural analysis of reinforced concrete and masonry." +
                    "\nThis is the Rhinoceros 3D Plugin for StrucEng Library."
            });

            LinkButton urlButton;

            AddRow(new Label()
            {
                Text = "Version " + StrucEngLibPlugin.Version
            });
            AddRow(urlButton = new LinkButton()
            {
                Text = "https://strucenglib.ethz.ch/"
            });
            urlButton.Click += (sender, args) =>
            {
                try
                {
                    PythonScript ps = PythonScript.Create();
                    ps.ExecuteScript(
                        $"import webbrowser; " +
                        $"webbrowser.open('https://strucenglib.ethz.ch/')");
                }
                catch (Exception)
                {
                }
            };
        }

        private Bitmap GetLogo()
        {
            return new Bitmap(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("StrucEngLib.EmbeddedResources.logo.transparent.png"));
        }
    }
}