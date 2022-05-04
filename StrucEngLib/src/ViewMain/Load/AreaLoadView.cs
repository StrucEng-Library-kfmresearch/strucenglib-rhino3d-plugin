using System;
using System.Collections.Generic;
using System.Linq;
using StrucEngLib.Model;
using StrucEngLib.Views;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.UI;

namespace StrucEngLib
{
    /// <summary>
    /// View for area load
    /// </summary>
    public class AreaLoadView : AbstractLoadView
    {
        private readonly AreaLoadViewModel _vm;

        public AreaLoadView(AreaLoadViewModel vm) : base(vm)
        {
            _vm = vm;
            BuildGui();
        }

        private void BuildGui()
        {
            UiUtils.AddLabelTextRow(this, _vm, "Pressure [N/mm^2]", "Z", "0.03");
            UiUtils.AddLabelTextRow(this, _vm, "Surface Shear x [N/mm^2]", "X", "");
            UiUtils.AddLabelTextRow(this, _vm, "Surface Shear y [N/mm^2]", "Y", "");
            UiUtils.AddLabelTextRow(this, _vm, "Axes", "Axes", "local");
        }
    }
}