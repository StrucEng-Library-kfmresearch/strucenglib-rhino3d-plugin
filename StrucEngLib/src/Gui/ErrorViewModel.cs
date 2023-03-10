using System;
using System.Collections.Generic;
using System.Text;
using Rhino;
using Rhino.UI;
using StrucEngLib.Utils;

namespace StrucEngLib.Gui
{
    /// <summary>Error messages vm</summary>
    public class ErrorViewModel : ViewModelBase
    {
        private string _message = "";

        public enum ViewResult
        {
            Ok, Cancel
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public ErrorViewModel()
        {
            
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(Message) && Message != null && !String.IsNullOrWhiteSpace(Message))
                {
                    StrucEngLibLog.Instance.WriteTaggedLine("error", Message);
                }
            };
        }
        public ViewResult ShowException(string info, Exception e)
        {
            StringBuilder b = new StringBuilder();
            b.Append(
                $"An Exception occured. This is likely a bug. Save this message, " +
                $"try to reproduce the bug and file an issue: {StrucEngLibPlugin.Website}. \n\n");
            b.Append($"Version: {StrucEngLibPlugin.Version} \n");
            b.Append($"Info: {info} \n\n");
            b.Append($"Exception: {e.GetType().ToString()} \n");
            b.Append($"Message: {e.Message} \n");
            b.Append($"Source: {e.Source} \n");
            b.Append($"Stacktrace: {e.StackTrace} \n");
            b.Append("\n Context:\n");
            try
            {
                b.Append(StringUtils.ToJson(StrucEngLibPlugin.Instance.MainViewModel));
            }
            catch (Exception)
            {
                // XXX: Ignore errors caused by serialization error context
            }
            try
            {
                b.Append("\n Model:\n");
                b.Append(StringUtils.ToJson(StrucEngLibPlugin.Instance.MainViewModel.Workbench));
            }
            catch (Exception)
            {
                // XXX: Ignore errors caused by serialization error context
            }

            return ShowMessage(b.ToString(), false);
        }

        public ViewResult DebugMessage(params object[] values)
        {
            var b = new StringBuilder();
            foreach (var o in values)
            {
                try
                {
                    b.Append(StringUtils.ToJson(o));
                }
                catch (Exception)
                {
                    /* XXX: Ignore */
                }

                b.Append("\n");
            }

            return ShowMessages(new List<string>() {b.ToString()}, false);
        }

        public ViewResult ShowMessage(string m, bool enumerate = true)
        {
            // XXX: For now a simple show text dialog is enough
            return ShowMessages(new List<string>() {m});
        }

        public ViewResult ShowMessages(List<string> ms, bool enumerate = true)
        {
            StringBuilder b = new StringBuilder();
            foreach (var m in ms)
            {
                if (enumerate)
                {
                    b.Append("- " + m + "\n");
                }
                else
                {
                    b.Append(m + "\n");
                }
            }

            Message = b.ToString();
            var res = ShowDialog();
            Message = "";
            return res;
        }

        private ViewResult ShowDialog()
        {
            var d = new MessageDialog("The following messages have occured:", Message)
            {   
            };
            var dialogRc = d.ShowSemiModal(RhinoDoc.ActiveDoc, RhinoEtoApp.MainWindow);
            if (dialogRc == Eto.Forms.DialogResult.Ok)
            {

                if (d.Result == MessageDialog.ResultStateEnum.Ok)
                {
                    return ViewResult.Ok;
                }
            }

            return ViewResult.Cancel;
        }

        public ViewResult ShowMessages(IEnumerable<ErrorMessageContext> ctxx, bool enumerate = true)
        {
            StringBuilder b = new StringBuilder();
            foreach (var ctx in ctxx)
            {
                PrepareErrorMessage(ctx, b, enumerate);
                b.Append("\n\n");
            }

            Message = b.ToString();
            var res = ShowDialog();
            Message = "";
            return res;
        }

        public ViewResult ShowMessages(ErrorMessageContext ctx, bool enumerate = true)
        {
            return ShowMessages(new[] {ctx}, enumerate);
        }

        private void AppendText(StringBuilder b, string m, bool enumerate = true, int indent = 1)
        {
            var indentStr = "";
            for (var i = 0; i < indent; i++)
            {
                indentStr += "    ";
            }

            if (enumerate)
            {
                b.Append($"{indentStr}- {m}\n");
            }
            else
            {
                b.Append(m + "\n");
            }
        }

        protected void PrepareErrorMessage(ErrorMessageContext ctx, StringBuilder b, bool enumerate)
        {
            if (!String.IsNullOrWhiteSpace(ctx.ContextDescription))
            {
                b.Append($"{ctx.ContextDescription}:\n");
            }

            var infoMessages = ctx.GetByType(MessageType.Info);
            if (infoMessages != null && infoMessages.Count > 0)
            {
                AppendText(b, "Info:", enumerate, 1);
                foreach (var m in infoMessages)
                {
                    AppendText(b, m.Text, enumerate, 2);
                }
            }

            var warnMessages = ctx.GetByType(MessageType.Warning);
            if (warnMessages != null && warnMessages.Count > 0)
            {
                AppendText(b, "Warning:", enumerate, 1);
                foreach (var m in warnMessages)
                {
                    AppendText(b, m.Text, enumerate, 2);
                }
            }

            var errorMsgs = ctx.GetByType(MessageType.Error);
            if (errorMsgs != null && errorMsgs.Count > 0)
            {
                AppendText(b, "Error:", enumerate, 1);
                foreach (var m in errorMsgs)
                {
                    AppendText(b, m.Text, enumerate, 2);
                }
            }
        }
    }
}