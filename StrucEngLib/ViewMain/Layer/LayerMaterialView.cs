using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;

namespace StrucEngLib
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

            UiUtils.AddLabelTextRow(layout, _vm, "E", "E", "33700");
            UiUtils.AddLabelTextRow(layout, _vm, "V", "V", "0.0");
            UiUtils.AddLabelTextRow(layout, _vm, "P", "P", "2500/10**9");
            Add(new SelectionView("Materials", new List<string>() {"Elastic"}, new List<Control>() {layout}));
        }
    }
}