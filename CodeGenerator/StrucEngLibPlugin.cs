﻿using Rhino;
using System;
using System.Security.Permissions;
using CodeGenerator.Model;
using Rhino.PlugIns;

namespace CodeGenerator
{
    /// <summary>
    /// StrucEngLibPlugin Plugin class
    /// </summary>
    public class StrucEngLibPlugin : Rhino.PlugIns.PlugIn
    {
        public StrucEngLibPlugin()
        {
            Instance = this;
        }

        public static StrucEngLibPlugin Instance { get; private set; }
        

        private Workbench _model;
        public Workbench GetModel()
        {
            return _model;
        }
    }
}