﻿using Rhino;
using Rhino.Commands;
using Rhino;
using Rhino.Commands;
using Rhino.Input.Custom;
using Rhino.UI;
using Rhino.DocObjects;
using System;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;

namespace CodeGenerator
{
    public class CodeGeneratorCommand : Command
    {
        public CodeGeneratorCommand()
        {
            Instance = this;
            Panels.RegisterPanel(PlugIn, typeof(CodeGenPanelView), "Compas Code Generator", null);
        }

        public static CodeGeneratorCommand Instance { get; private set; }

        public override string EnglishName => "CodeGeneratorCommand";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Rhino.RhinoApp.WriteLine("Running command");
            var panelId = CodeGenPanelView.PanelId;
            
            Rhino.RhinoApp.WriteLine("Before");
            Panels.OpenPanel(panelId);
            Rhino.RhinoApp.WriteLine("After");
            
            if (!CodeGenPanelView.Instance.Initialized)
            {
                Rhino.RhinoApp.WriteLine("Gluing view");
                CodeGenPanelView.Instance.RegisterController(CodeGeneratorPlugin.Instance.CodeGenPanelCtrl);
                CodeGenPanelView.Instance.RegisterModel(CodeGeneratorPlugin.Instance.CodeGenPanelModel);
                CodeGeneratorPlugin.Instance.CodeGenPanelCtrl.setView(CodeGenPanelView.Instance);
            }
            return Result.Success;
        }
    }
}