using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Eto.Forms;
using Rhino;
using Rhino.UI;
using StrucEngLib.Model;

namespace StrucEngLib
{
    /// <summary>Abstract vm for load related views</summary>
    public abstract class AbstractLoadViewModel : ViewModelBase
    {
        protected readonly ListLayerViewModel ListLayerVm;
        protected readonly ListLoadViewModel ListLoadVm;
        public readonly RelayCommand CommandConnectLayer;
        
        protected string _connectLayersLabels;

        public string ConnectLayersLabels
        {
            get => _connectLayersLabels;
            set
            {
                _connectLayersLabels = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Layer> Layers { get; }

        public AbstractLoadViewModel(MainViewModel mainVm)
        {
            CommandConnectLayer = new RelayCommand(OnConnectLayer);
            ListLayerVm = mainVm.ListLayerVm;
            ListLoadVm = mainVm.ListLoadVm;
            Layers = new ObservableCollection<Layer>();

            PropertyChanged += ((sender, args) => DoStoreVmToModel());
            Layers.CollectionChanged += (sender, args) =>
            {
                var b = new StringBuilder();
                foreach (var layer in Layers)
                {
                    b.Append(layer.GetName() + "; ");
                }

                ConnectLayersLabels = b.ToString();
            };
            DoStoreModelToVm();
        }

        /// <summary>
        /// Overwrite this method to store vm data to model 
        /// </summary>
        protected virtual void StoreVmToModel()
        {
        }

        /// <summary>
        /// Overwrite this method to store model data to vm
        /// </summary>
        protected virtual void StoreModelToVm()
        {
        }

        private void DoStoreVmToModel()
        {
            if (_ignoreStoreVmToModel) return;
            var l = ListLoadVm.SelectedLoad;
            l.Layers = Layers.ToList();
            StoreVmToModel();
            ListLoadVm.OnLoadSettingChanged();
        }

        private bool _ignoreStoreVmToModel = false;

        private void DoStoreModelToVm()
        {
            _ignoreStoreVmToModel = true;
            StoreModelToVm();
            var l = ListLoadVm.SelectedLoad;
            Layers.Clear();
            foreach (var layer in l.Layers)
            {
                Layers.Add(layer);
            }

            _ignoreStoreVmToModel = false;
        }

        protected void OnConnectLayer()
        {
            var dialog = new SelectLayerDialog(ListLayerVm.Layers.ToList());
            var dialogRc = dialog.ShowSemiModal(RhinoDoc.ActiveDoc, RhinoEtoApp.MainWindow);
            if (dialogRc == Eto.Forms.DialogResult.Ok)
            {
                Layers.Clear();

                foreach (var l in dialog.SelectedLayers)
                {
                    Layers.Add(l);
                }
            }
        }
    }
}