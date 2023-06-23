using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Security.Policy;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.Runtime;
using Rhino.UI.Controls;
using StrucEngLib.Model;
using StrucEngLib.Utils;

namespace StrucEngLib.Install
{
    public class Installer : Form
    {
        private readonly Workbench _bench;

        private string _abaqusHelp =
            "https://strucenglib.ethz.ch/strucenglib_plugin/home/";

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

        private Button _btShowRhinoScripts;
        private Button _btInstallDevDeps;
        private TextBox _tbPyCompas;
        private TextBox _tbPyCompasFea;
        private TextBox _tbPyStrucenglib;
        private TextBox _tbPyStrucenglibConnect;
        private CheckBox _cbCompas;
        private CheckBox _cbCompasFea;
        private CheckBox _cbStrucenglib;
        private CheckBox _cbStrucenglibConnect;

        public Installer(Workbench bench)
        {
            _bench = bench;
            if (_bench.InstallerModel == null)
            {
                _bench.InstallerModel = new InstallerModel();
            }

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
                        new DynamicRow()
                        {
                            (_btCreateEnv = new Button() {Text = "Create Environment"})
                        },
                        new DynamicRow()
                        {
                            (_btInstallAbaqus = new Button() {Text = "Install for Abaqus"})
                        },
                        new DynamicRow()
                        {
                            (_btInstallAnsys = new Button() {Text = "Install for Ansys"}),
                        },
                        new DynamicRow()
                        {
                            (_btRemoveEnv = new Button() {Text = "Delete Environment"})
                        },
                        TableLayout.HorizontalScaled(
                            (_btTest = new Button() {Text = "Test Import"}),
                            (_btOpenCondaBin = new Button() {Text = "Open condabin"})
                        ),
                    }
                }
            });


            layout.AddRow(new GroupBox()
            {
                Text = "Developer",
                Content = new DynamicLayout()
                {
                    Padding = new Padding(10),
                    Spacing = new Size(10, 15),
                    Rows =
                    {
                        TableLayout.HorizontalScaled(
                            (new Label()
                            {
                                Text =
                                    "Development dependencies allow modification and test of " +
                                    "Python dependencies without a release of a new version. " +
                                    "Link the following dependencies with their git repository on disk.",
                            })),
                        TableLayout.HorizontalScaled(
                            _cbStrucenglib = new CheckBox()
                            {
                                Text = "strucenglib-snippets", Enabled = true,
                                ToolTip =
                                    ""
                            },
                            (_tbPyStrucenglib = new TextBox()
                            {
                                PlaceholderText = "Path to Git repository",
                            })),
                        TableLayout.HorizontalScaled(
                            _cbStrucenglibConnect = new CheckBox()
                            {
                                Text = "strucenglib-connect", Enabled = true,
                                ToolTip =
                                    ""
                            },
                            (_tbPyStrucenglibConnect = new TextBox()
                            {
                                PlaceholderText = "Path to Git repository",
                            })),
                        TableLayout.HorizontalScaled(
                            _cbCompas = new CheckBox()
                            {
                                Text = "compas", Enabled = true,
                                ToolTip =
                                    ""
                            },
                            (_tbPyCompas = new TextBox()
                            {
                                PlaceholderText = "Path to Git repository",
                            })),
                        TableLayout.HorizontalScaled(
                            _cbCompasFea = new CheckBox()
                            {
                                Text = "compas_fea", Enabled = true,
                                ToolTip =
                                    ""
                            },
                            (_tbPyCompasFea = new TextBox()
                            {
                                PlaceholderText = "Path to Git repository",
                            })),
                        TableLayout.HorizontalScaled(
                            (_btInstallDevDeps = new Button() {Text = "Install Dependencies"}),
                            (_btShowRhinoScripts = new Button() {Text = "Rhino Scripts Directory"})
                        )
                    }
                }
            });


            layout.AddRow(new Label());
            layout.Width = 600;

            Content = new Scrollable()
            {
                ExpandContentWidth = true,
                Width = 600,
                Content = layout
            };
            
            _tbAnaconda.TextBinding.Bind(_bench, m => m.InstallerModel.AnacondaHome);
            _tbPyCompas.TextBinding.Bind(_bench, m => m.InstallerModel.DevCompasPath);
            _tbPyCompasFea.TextBinding.Bind(_bench, m => m.InstallerModel.DevCompasFeaPath);
            _tbPyStrucenglib.TextBinding.Bind(_bench, m => m.InstallerModel.DevStrucenglibPath);
            _tbPyStrucenglibConnect.TextBinding.Bind(_bench, m => m.InstallerModel.DevStrucenglibConnectPath);
        }

        private void ExecCmdButton(string cmd, string args = "")
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

            var arguments = conda + " " + args;

            StrucEngLibLog.Instance.WriteLine(cmd, arguments);
            try
            {
                System.Diagnostics.Process.Start(cmd, arguments);
            }
            catch (Exception e)
            {
                StrucEngLibLog.Instance.WriteLine(e.Message);
            }
        }

        private void ExecInstallDevDeps()
        {
            string script = GetBatFile("StrucEngLib.EmbeddedResources.install_dev_dependency.bat");
            if (_cbStrucenglib.Checked == true
                && !string.IsNullOrEmpty(_tbPyStrucenglib.Text))
            {
                ExecCmdButton(script, "strucenglib " + _tbPyStrucenglib.Text);
            }

            if (_cbStrucenglibConnect.Checked == true
                && !string.IsNullOrEmpty(_tbPyStrucenglibConnect.Text))
            {
                ExecCmdButton(script, "strucenglib_connect " + _tbPyStrucenglibConnect.Text);
            }

            if (_cbCompasFea.Checked == true
                && !string.IsNullOrEmpty(_tbPyCompasFea.Text))
            {
                ExecCmdButton(script, "compas_fea " + _tbPyCompasFea.Text);
            }

            if (_cbCompas.Checked == true
                && !string.IsNullOrEmpty(_tbPyCompas.Text))
            {
                ExecCmdButton(script, "compas " + _tbPyCompas.Text);
            }
        }

        private void ExecShowScriptsDir()
        {
            var path = "%AppData%\\McNeel\\Rhinoceros\\7.0\\scripts";
            var cmd = $"cd /d {path} && dir /a && explorer . && pause";
            try
            {
                StrucEngLibLog.Instance.WriteLine(cmd);
                System.Diagnostics.Process.Start("C:\\Windows\\System32\\cmd.exe",
                    $"/K \"{cmd}\"");
            }
            catch (Exception e)
            {
                StrucEngLibLog.Instance.WriteLine(e.Message);
            }
        }

        private void ExecOpenConda()
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
        }

        private string GetFolderLocation()
        {
            var dialog = new SelectFolderDialog();
            var result = dialog.ShowDialog(ParentWindow);
            if (result == DialogResult.Ok)
            {
                StrucEngLibLog.Instance.WriteLine("Result: {0}, Folder: {1}", result, dialog.Directory);
                return dialog.Directory;
            }
            else
            {
                StrucEngLibLog.Instance.WriteLine("Result: {0}", result);
                return null;
            }
        }

        private void ExecTestInstall()
        {
            {
                var cmd = $@"
import imp
imp.find_module('compas')
imp.find_module('compas_fea')
imp.find_module('strucenglib')
print('success :)')";
                new PythonExecutor().Execute(cmd);
            }
            ;
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
            _btInstallDevDeps.Click += (sender, args) => ExecInstallDevDeps();
            _btShowRhinoScripts.Click += (sender, args) => ExecShowScriptsDir();

            _btSelectConda.Click += (sender, args) => _tbAnaconda.Text = GetFolderLocation();
            _cbCompas.CheckedChanged += (sender, args) =>
            {
                if (_cbCompas.Checked != null && _cbCompas.Checked.Value)
                {
                    var res = GetFolderLocation();
                    if (!string.IsNullOrEmpty(res))
                    {
                        _tbPyCompas.Text = res;
                    }
                }
            };
            _cbCompasFea.CheckedChanged += (sender, args) =>
            {
                if (_cbCompasFea.Checked != null && _cbCompasFea.Checked.Value)
                {
                    var res = GetFolderLocation();
                    if (!string.IsNullOrEmpty(res))
                    {
                        _tbPyCompasFea.Text = res;
                    }
                }
            };
            _cbStrucenglib.CheckedChanged += (sender, args) =>
            {
                if (_cbStrucenglib.Checked != null && _cbStrucenglib.Checked.Value)
                {
                    var res = GetFolderLocation();
                    if (!string.IsNullOrEmpty(res))
                    {
                        _tbPyStrucenglib.Text = res;
                    }
                }
            };
            _cbStrucenglibConnect.CheckedChanged += (sender, args) =>
            {
                if (_cbStrucenglibConnect.Checked != null && _cbStrucenglibConnect.Checked.Value)
                {
                    var res = GetFolderLocation();
                    if (!string.IsNullOrEmpty(res))
                    {
                        _tbPyStrucenglibConnect.Text = res;
                    }
                }
            };

            _btOpenCondaBin.Click += (sender, args) => ExecOpenConda();

            if (_btTest != null)
            {
                _btTest.Click += (sender, args) => ExecTestInstall();
            }
        }
    }
}