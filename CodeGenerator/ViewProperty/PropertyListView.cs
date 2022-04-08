using Eto.Drawing;
using Eto.Forms;

namespace CodeGenerator
{
    public class PropertyListView : TableLayout
    {
        private readonly PropertyListViewModel _vm;

        public PropertyListView(PropertyListViewModel vm)
        {
            _vm = vm;
            Padding = new Padding {Top = 0, Left = 10, Bottom = 0, Right = 0};
            Spacing = new Size(5, 1);

            foreach (var property in _vm.Group.Properties)
            {
                var tb = new TextBox();
                tb.TextBinding.Bind(() => (string) property.Value, val => property.Value = val);
                Rows.Add(TableLayout.HorizontalScaled(new Label {Text = property.Label}, tb));
                Rhino.RhinoApp.WriteLine("{0} -> {1}", property.Label, (string) property.Value);
            }
        }
    }
}