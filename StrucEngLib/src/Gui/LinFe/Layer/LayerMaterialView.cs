using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using StrucEngLib.Utils;

namespace StrucEngLib.Gui.LinFe.Layer
{
    /// <summary>
    /// View for Materials
    /// </summary>
    public class LayerMaterialView : DynamicLayout
    {
        private readonly LayerMaterialViewModel _vm;

        public LayerMaterialView(LayerMaterialViewModel vm)
        {
            _vm = vm;

            var layout = new DynamicLayout()
            {
                Padding = new Padding {Top = 5, Left = 10, Bottom = 0, Right = 0},
                Spacing = new Size(5, 1)
            };

            UiUtils.AddLabelTextRow(layout, _vm, "Elastic modulus [N/mm²]", nameof(_vm.E), "33700");
            UiUtils.AddLabelTextRow(layout, _vm, "Poisson’s ratio [-]", nameof(_vm.V), "0.0");
            UiUtils.AddLabelTextRow(layout, _vm, "Specific mass [kg/m³] ", nameof(_vm.P), "2500/10**9");
            Add(new SelectionView("Materials", new List<string>() {"Elastic"}, new List<Control>() {layout}));
        }
    }
}