using System;
using System.Data;
using Eto.Drawing;
using Eto.Forms;
using Rhino;

namespace StrucEngLib.Utils
{
    /// <summary>Simple dialog to show text content</summary>
    sealed class MessageDialog : Dialog<DialogResult>
    {
        private readonly Button _execOk;
        private readonly Button _execCancel;

        public TextArea TextArea
        {
            get;
            private set;
        }

        public enum ResultStateEnum
        {
            Ok,
            Cancel
        };

        public new ResultStateEnum Result { get; set; }

        public MessageDialog(string title, string content)
        {
            Maximizable = true;
            Minimizable = true;
            Padding = new Padding(5);
            Resizable = true;
            ShowInTaskbar = true;
            Title = title;
            Size = new Size(700, 400);

            WindowStyle = WindowStyle.Default;
            Result = ResultStateEnum.Cancel;

            _execOk = new Button()
            {
                Text = "Ok"
            };

            _execCancel = new Button()
            {
                Text = "Cancel",
            };

            TextArea = new TextArea()
            {
                Text = content,
                ReadOnly = true,
            };
            TextArea.KeyDown += KeyDownHandel; 

            _execOk.Click += (sender, e) =>
            {
                Result = ResultStateEnum.Ok;
                Close(DialogResult.Ok);
            };

            _execCancel.Click += (sender, e) =>
            {
                Result = ResultStateEnum.Cancel;
                Close(DialogResult.Ok);
            };

            Content = new TableLayout
            {
                Padding = new Padding(5),
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow {ScaleHeight = true, Cells = {new TableCell(TextArea, true)}},
                    new TableRow(
                        new DynamicLayout()
                        {
                            Rows =
                            {
                                new TableLayout()
                                {
                                    Rows =
                                    {
                                        new TableRow(
                                            new TableCell(null, scaleWidth: true),
                                            new TableCell(TableLayout.AutoSized(new TableLayout()
                                            {
                                                Spacing = new Size(10, 10),
                                                Padding = new Padding()
                                                {
                                                    Top = 10,
                                                    Bottom = 10,
                                                },
                                                Rows =
                                                {
                                                    new TableRow(_execOk, _execCancel),
                                                }
                                            })))
                                    }
                                }
                            }
                        }
                    )
                }
            };
            KeyDown += KeyDownHandel;
        }

        private void KeyDownHandel(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Escape)
            {
                this._execCancel.PerformClick();
            } else if (e.Key == Keys.Enter || e.Key == Keys.Space)
            {
                this._execOk.PerformClick();
            }
        }
    }
}