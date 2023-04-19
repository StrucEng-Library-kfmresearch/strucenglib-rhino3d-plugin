using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.Runtime;
using Rhino.UI.Controls;
using StrucEngLib.Utils;

namespace StrucEngLib.Install
{
    public class Installer : Form
    {
        private string _abaqusHelp =
            "https://github.com/kfmResearch-NumericsTeam/StrucEng_Library_Plug_in/wiki/Installation";

        private string _condaWebsite = "https://www.anaconda.com/products/distribution";

        private TextBox _tbAnaconda;
        private Button _btAbaqus;
        private Button _btInstallAnsys;
        private Button _btInstallAbaqus;
        private Button _btCreateEnv;
        private Button _btRemoveEnv;
        
        private Button _btOpenCondaBin;
        private Button _btSelectConda;
        private Button _btBrowseConda;
        private Button _btTest;

        public Installer()
        {
            BuildGui();
            BindGui();
        }
        
        private string GetBatFile(string resName)
        {
            string fileName = Path.GetTempPath() + "install_" + Guid.NewGuid().ToString() + ".bat";
            var fileStream = File.Create(fileName);
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                resName);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
            fileStream.Close();
            return fileName;
        }

        private void BuildGui()
        {
            Maximizable = true;
            Minimizable = true;
            Padding = new Padding(5);
            Resizable = true;
            ShowInTaskbar = true;
            WindowStyle = WindowStyle.Default;

            Title = "StrucEngLib Dependency Installer";

            var layout = new DynamicLayout()
            {
                Padding = new Padding(5),
                Spacing = new Size(10, 10),
            };
            layout.AddRow(new GroupBox()
            {
                Text = "Install or Specify Anaconda",
                Content = new DynamicLayout()
                {
                    Padding = new Padding(10),
                    Spacing = new Size(10, 15),
                    Rows =
                    {
                        TableLayout.HorizontalScaled(
                            new Label() {Text = "Anaconda Home Directory"},
                            (_tbAnaconda = new TextBox()
                            {
                                PlaceholderText = "e.g. C:\\Temp\\Miniconda3a3\\",
                            })
                        ),
                        TableLayout.HorizontalScaled(
                            (_btSelectConda = new Button() {Text = "Select Directory"}),
                            (_btBrowseConda = new Button() {Text = "Browse Website"})
                        ),
                    }
                }
            });

            layout.AddRow(new GroupBox()
            {
                Text = "Install Python Dependencies",
                Content = new DynamicLayout()
                {
                    Padding = new Padding(10),
                    Spacing = new Size(10, 15),
                    Rows =
                    {
                        new DynamicRow()
                        {
                            new Label() {Text = "Create an environment and install the software packages."},
                        },
                        new DynamicRow() {
                        (_btCreateEnv = new Button() {Text = "Create Environment"})
                        },
                        new DynamicRow() {
                            (_btInstallAbaqus = new Button() {Text = "Install for Abaqus"})
                        },
                        new DynamicRow() {
                            (_btInstallAnsys = new Button() {Text = "Install for Ansys"}),
                        },                        
                        new DynamicRow() {
                            (_btRemoveEnv = new Button() {Text = "Delete Environment"})
                        },
                        TableLayout.HorizontalScaled(
                            (_btTest = new Button() {Text = "Test Import"}),
                            (_btOpenCondaBin = new Button() {Text = "Open condabin"})
                        ),
                        
                    }
                }
            });
            // layout.AddRow(new GroupBox()
            // {
            //     Text = "Install Abaqus",
            //     Content = new DynamicLayout()
            //     {
            //         Padding = new Padding(10),
            //         Spacing = new Size(10, 15),
            //         Rows =
            //         {
            //             new DynamicRow()
            //             {
            //                 new Label()
            //                 {
            //                     Text =
            //                         "StrucEngLib needs Abaqus installed. Ensure Abaqus is available and restart Rhino."
            //                 },
            //             },
            //             new DynamicRow()
            //             {
            //                 (_btAbaqus = new Button()
            //                 {
            //                     Text = "More Information",
            //                 })
            //             }
            //         }
            //     }
            // });
            layout.AddRow(new Label());
            Content = layout;
        }

        private void ExecCmdButton(string cmd)
        {
            if (String.IsNullOrWhiteSpace(_tbAnaconda.Text))
            {
                StrucEngLibLog.Instance.WriteLine("Anaconda home directory is null");
                return;
            }

            var conda = _tbAnaconda.Text + "\\condabin\\conda.bat";
            if (!File.Exists(conda))
            {
                StrucEngLibLog.Instance.WriteLine("{0} does not exist!", conda);
                return;
            }

            StrucEngLibLog.Instance.WriteLine(cmd, conda);
            try
            {
                System.Diagnostics.Process.Start(cmd, conda);
            }
            catch (Exception e)
            {
                StrucEngLibLog.Instance.WriteLine(e.Message);
            }
        }

        private void BindGui()
        {
            // _btAbaqus.Click += (sender, args) => { System.Diagnostics.Process.Start(_abaqusHelp); };
            _btBrowseConda.Click += (sender, args) => { System.Diagnostics.Process.Start(_condaWebsite); };
            string cmdCreateEnv = GetBatFile("StrucEngLib.EmbeddedResources.create_env.bat");
            string cmdInstallAnsys = GetBatFile("StrucEngLib.EmbeddedResources.install_ansys.bat");
            string cmdInstallAbaqus = GetBatFile("StrucEngLib.EmbeddedResources.install_abaqus.bat");
            string cmdRemoveEnv = GetBatFile("StrucEngLib.EmbeddedResources.remove_env.bat");
            
            _btInstallAnsys.Click += (sender, args) => ExecCmdButton(cmdInstallAnsys);
            _btInstallAbaqus.Click += (sender, args) => ExecCmdButton(cmdInstallAbaqus);
            _btCreateEnv.Click += (sender, args) => ExecCmdButton(cmdCreateEnv);
            _btRemoveEnv.Click += (sender, args) => ExecCmdButton(cmdRemoveEnv);
            
            _btSelectConda.Click += (sender, args) =>
            {
                var dialog = new SelectFolderDialog();
                var result = dialog.ShowDialog(ParentWindow);
                if (result == DialogResult.Ok)
                {
                    StrucEngLibLog.Instance.WriteLine("Result: {0}, Folder: {1}", result, dialog.Directory);
                    _tbAnaconda.Text = dialog.Directory;
                }
                else
                {
                    StrucEngLibLog.Instance.WriteLine("Result: {0}", result);
                }
            };

            _btOpenCondaBin.Click += (sender, args) =>
            {
                if (String.IsNullOrWhiteSpace(_tbAnaconda.Text))
                {
                    StrucEngLibLog.Instance.WriteLine("Anaconda home directory is null");
                    return;
                }

                var conda = _tbAnaconda.Text + "\\condabin\\";
                if (!Directory.Exists(conda))
                {
                    StrucEngLibLog.Instance.WriteLine("{0} does not exist!", conda);
                    return;
                }
                try
                {
                    StrucEngLibLog.Instance.WriteLine("cmd {0}", conda);
                    conda = conda.Replace(" ", "^ ");
                    System.Diagnostics.Process.Start("C:\\Windows\\System32\\cmd.exe", $"/K \"cd /d {conda} \"");
                }
                catch (Exception e)
                {
                    StrucEngLibLog.Instance.WriteLine(e.Message);
                }
            };

            if (_btTest != null)
            {
                _btTest.Click += (sender, args) =>
                {
                    var cmd = $@"
import imp
imp.find_module('compas')
imp.find_module('compas_fea')
imp.find_module('strucenglib')
print('success :)')";
                    new PythonExecutor().Execute(cmd);
                };
            }
        }
    }
}