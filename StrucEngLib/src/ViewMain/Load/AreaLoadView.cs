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
            UiUtils.AddLabelTextRow(this, _vm, "z", "Z", "0.03");
            UiUtils.AddLabelTextRow(this, _vm, "Axes", "Axes", "local");
        }
    }
}