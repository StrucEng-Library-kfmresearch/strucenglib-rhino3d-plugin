using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Security.AccessControl;
using Eto.Forms;
using Rhino;
using Rhino.UI;
using StrucEngLib.Model;
using StrucEngLib.Utils;

namespace StrucEngLib.Gui.LinFe.Load
{
    using Load = StrucEngLib.Model.Load;
    
    /// <summary>
    /// Main Vm for load information
    /// </summary>
    public class ListLoadViewModel : ViewModelBase
    {
        private readonly LinFeMainViewModel _mainVm;
        
        /// <summary> Raised if a setting within a load is changed</summary> 
        public event EventHandler LoadChanged;

        public ObservableCollection<ListItem> LoadNames { get; }

        private LoadType _loadName;

        public LoadType LoadName
        {
            get { return _loadName; }
            set
            {
                _loadName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Load> Loads { get; }

        private Load _selectedLoad;

        public Load SelectedLoad
        {
            get => _selectedLoad;
            set
            {
                _selectedLoad = value;
                OnPropertyChanged();
                UpdateContentView();
                SelectLayerByName();
            }
        }

        private Control _loadView;

        public Control LoadView
        {
            get => _loadView;
            set
            {
                _loadView = value;
                OnPropertyChanged();
                LoadViewVisible = _loadView != null;
            }
        }

        private bool _loadViewVisible = false;

        public bool LoadViewVisible
        {
            get => _loadViewVisible;
            set
            {
                _loadViewVisible = value;
                OnPropertyChanged();
                CommandDeleteLoad.UpdateCanExecute();
            }
        }

        private bool _selectLoadViewVisible = false;

        public bool SelectLoadViewVisible
        {
            get => _selectLoadViewVisible;
            set
            {
                _selectLoadViewVisible = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand CommandAddLoad { get; }
        public RelayCommand CommandDeleteLoad { get; }

        public ListLoadViewModel(LinFeMainViewModel mainVm)
        {
            _mainVm = mainVm;
            CommandAddLoad = new RelayCommand(OnAddLoad);
            CommandDeleteLoad = new RelayCommand(OnLoadDelete, CanLoadDelete);
            LoadNames = new ObservableCollection<ListItem>
            {
                new ListItem {Key = LoadType.Area.ToString(), Text = "Area"},
                new ListItem {Key = LoadType.Gravity.ToString(), Text = "Gravity"},
                new ListItem {Key = LoadType.Point.ToString(), Text = "Point"},
            };
            Loads = new ObservableCollection<Load>(mainVm.Workbench.Loads);
            Loads.CollectionChanged += (sender, args) => { LoadsChanged(); };
            LoadsChanged();
        }

        private void LoadsChanged()
        {
            SelectLoadViewVisible = Loads.Count != 0;
            OnLoadSettingChanged();
        }

        private void OnAddLoad()
        {
            Load newLoad;
            if (LoadName == LoadType.Gravity)
            {
                newLoad = new LoadGravity();
            }
            else if (LoadName == LoadType.Area)
            {
                newLoad = new LoadArea();
            }
            else if (LoadName == LoadType.Point)
            {
                newLoad = new LoadPoint();
            }
            else
            {
                _mainVm.ErrorVm.ShowMessage($"Unknown Load (OnAddLoad) {LoadName}");
                return;
            }

            Loads.Add(newLoad);
            OnPropertyChanged(nameof(Loads));

            SelectedLoad = newLoad;
        }

        private void OnLoadDelete()
        {
            if (SelectedLoad == null) return;
            Loads.Remove(SelectedLoad);
            OnPropertyChanged(nameof(Loads));
            SelectedLoad = null;
        }

        private bool CanLoadDelete() => Loads.Count != 0;

        private void UpdateContentView()
        {
            if (SelectedLoad == null)
            {
                LoadView = null;
            }
            else if (SelectedLoad.LoadType == LoadType.Area)
            {
                LoadView = new AreaLoadView(new AreaLoadViewModel(_mainVm));
            }
            else if (SelectedLoad.LoadType == LoadType.Gravity)
            {
                LoadView = new GravityLoadView(new GravityLoadViewModel(_mainVm));
            }
            else if (SelectedLoad.LoadType == LoadType.Point)
            {
                LoadView = new PointLoadView(new PointLoadViewModel(_mainVm));
            }
            else
            {
                _mainVm.ErrorVm.ShowMessage($"Unknown Load (UpdateContentView): {SelectedLoad.LoadType}");
                return;
            }
        }
        
        public void OnLoadSettingChanged()
        {
            LoadChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SelectLayerByName()
        {
            if (_selectedLoad != null)
            {
                List<string> layerNames = new List<string>();
                foreach (var l in _selectedLoad.Layers)
                {
                    layerNames.Add(l.GetName());
                }

                if (layerNames.Count == 0)
                {
                    RhinoUtils.UnSelectAll(RhinoDoc.ActiveDoc);
                }
                else
                {
                    RhinoUtils.SelectLayerByNames(RhinoDoc.ActiveDoc, layerNames);
                }
            }
        }

        public override void UpdateModel()
        {
            _mainVm.Workbench.Loads.Clear();
            foreach (var load in Loads)
            {
                _mainVm.Workbench.Loads.Add(load);
            }
        }
    }
}