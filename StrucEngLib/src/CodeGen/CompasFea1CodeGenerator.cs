using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Rhino;
using StrucEngLib.Model;
using StrucEngLib.Model.Sm;
using StrucEngLib.Utils;

namespace StrucEngLib
{
    /// <summary>
    /// Code generator to generate python code
    /// </summary>
    public class CompasFea1CodeGenerator
    {
        public string GenerateLinFeCode(Workbench bench)
        {
            var state = new EmitState(bench);
            EmitHeaders(state, "Generate LinFe Code",
                targetPath: state.Workbench.FileName);

            EmitElements(state);
            EmitSets(state);
            EmitMaterials(state);
            EmitSections(state);
            EmitProperties(state);
            EmitDisplacements(state);
            EmitLoads(state);
            EmitSteps(state);
            EmitSummary(state);
            EmitRun(state);

            return state.Buffer.ToString();
        }

        public string GenerateSmmCode(Workbench bench)
        {
            var state = new EmitState(bench);
            const string customImports = @"
try:
    # Package installed with StrucEngLib Dependency Installer
    from Sandwichmodel import sandwichmodel_main as SMM
except Exception:
    pass
try:
    import sandwichmodel_main as SMM
except Exception:
    pass
try:
    # new packages in strucenglib-snippets starting with v0.0.15
    from strucenglib.sandwichmodel import sandwichmodel_main as SMM
except Exception:
    pass 
";
            EmitHeaders(state, "Generate Sandwich Code",
                targetPath: state.Workbench.SandwichModel.FileName,
                customImports: customImports);
            EmitElements(state);
            EmitSets(state);
            EmitMaterials(state);
            EmitSections(state);
            EmitProperties(state);
            EmitAdditionalProperties(state);
            EmitDisplacements(state);
            EmitLoads(state);
            EmitSteps(state);
            EmitSummary(state);
            EmitRun(state);
            state.Workbench.SandwichModel?.AnalysisSettings?
                .ForEach(s => EmitSmm(state, s));
            return state.Buffer.ToString();
        }

        private void EmitHeaders(EmitState s, string action, string targetPath = "C:\\Temp\\Rahmen",
            string customImports = "")
        {
            string path = "";
            string file = "";
            try
            {
                path = Path.GetDirectoryName(targetPath);
                Directory.CreateDirectory(path);
                file = Path.GetFileName(targetPath);
                path = path + "\\\\";
            }
            catch (Exception)
            {
                // ignore
            }

            string strucEngLibConnect = "";
            if (s.Workbench.ExecuteOnServer)
            {
                strucEngLibConnect = "from strucenglib_connect import connect";
            }

            string header = $@"
# This is auto generated code by StrucEngLib Plugin {StrucEngLibPlugin.Version}
# Find source at {StrucEngLibPlugin.Website}
# Code generated at {DateTime.Now.ToString("o", CultureInfo.InvariantCulture)}
# Issued by user {Environment.UserName}
# Action: {action}

from compas_fea.cad import rhino
from compas_fea.structure import ElasticIsotropic
from compas_fea.structure import ElementProperties as Properties
from compas_fea.structure import GeneralStep
from compas_fea.structure import GravityLoad
from compas_fea.structure import AreaLoad
from compas_fea.structure import PointLoad
from compas_fea.structure import GeneralDisplacement
from compas_fea.structure import FixedDisplacement
from compas_fea.structure import FixedDisplacementXX
from compas_fea.structure import FixedDisplacementYY
from compas_fea.structure import FixedDisplacementZZ
from compas_fea.structure import PinnedDisplacement
from compas_fea.structure import RollerDisplacementX
from compas_fea.structure import RollerDisplacementY
from compas_fea.structure import RollerDisplacementZ
from compas_fea.structure import RollerDisplacementXY
from compas_fea.structure import RollerDisplacementYZ
from compas_fea.structure import RollerDisplacementXZ
from compas_fea.structure import ShellSection
from compas_fea.structure import Structure

{strucEngLibConnect}
{customImports}

# Snippets based on code of Andrew Liew (github.com/andrewliew), Benjamin Berger (github.com/Beberger)

name = '{file}'
path = '{path}'
mdl = Structure(name=name, path=path)
";
            s.Buffer.Append(header);
        }

        private void EmitElements(EmitState s)
        {
            s.CommentLine("Elements");
            var elements = s.Elements();
            elements.ForEach(e => s.LayerIds.Add(e, s.ElementId(e.GetName())));

            if (elements.Count > 0)
            {
                s.Line("rhino.add_nodes_elements_from_layers(mdl, mesh_type='ShellElement', " +
                       $"layers={StringUtils.ListToPyStr(elements, el => el.GetName())})");
            }

            elements
                .Select(e => e as Element)
                .ToList()
                .Where(e => e.LoadConstraint != null)
                .ToList().ForEach(e =>
                {
                    var c = e.LoadConstraint;
                    s.Line(
                        $"mdl.elements[{c.ElementNumber}].axes.update({{'ex': [{c.Ex0}, {c.Ex1}, {c.Ex2}], 'ey': [{c.Ey0}, {c.Ey1}, {c.Ey2}], 'ez': [{c.Ez0}, {c.Ez1}, {c.Ez2}]}}) "
                        + $"# for layer: {e.GetName()}");
                });
        }

        private void EmitSets(EmitState s)
        {
            s.CommentLine("Sets");
            var sets = s.Sets();
            sets.ForEach(set => s.LayerIds.Add(set, s.SetId(set.GetName())));
            if (sets.Count > 0)
            {
                s.Line(
                    $"rhino.add_sets_from_layers(mdl, layers={StringUtils.ListToPyStr(sets, set => set.GetName())})");
            }
        }

        private void EmitMaterials(EmitState s)
        {
            s.CommentLine("Materials");
            var elements = s.Elements();
            elements.ForEach(e =>
            {
                var matId = s.MatElasticId(s.LayerIds[e]);
                s.MaterialIds.Add(e, matId);
                var mat = e.ElementMaterialElastic;
                s.Line(
                    $"mdl.add(ElasticIsotropic(name='{matId}', E={mat.E}, v={mat.V}, p={mat.P})) # for layer: {e.GetName()}");
            });
        }

        private void EmitSections(EmitState s)
        {
            s.CommentLine("Sections");
            var elements = s.Elements();
            elements.ForEach(e =>
            {
                var sectionId = s.SectionId(s.LayerIds[e]);
                s.SectionIds.Add(e, sectionId);
                s.Line(
                    $"mdl.add(ShellSection(name='{sectionId}', t={e.ElementShellSection.Thickness})) # for layer: {e.GetName()}");
            });
        }

        private void EmitProperties(EmitState s)
        {
            s.CommentLine("Properties");
            var elements = s.Elements();
            elements.ForEach(e =>
            {
                var propId = s.PropId(s.LayerIds[e]);
                s.PropertyIds.Add(e, propId);
                s.Line(
                    $"mdl.add(Properties(name='{propId}', material='{s.MaterialIds[e]}', section='{s.SectionIds[e]}', elset='{e.GetName()}'))");
            });
        }

        private string GenerateDisplacementForSet(EmitState s, Set set)
        {
            var args = new StringBuilder();
            string name;
            switch (set.SetDisplacementType)
            {
                case SetDisplacementType.GENERAL:
                    name = "GeneralDisplacement";
                    args.Append(s.EmitIfNotEmpty("x", set.SetGeneralDisplacement.Ux))
                        .Append(s.EmitIfNotEmpty("y", set.SetGeneralDisplacement.Uy))
                        .Append(s.EmitIfNotEmpty("z", set.SetGeneralDisplacement.Uz))
                        .Append(s.EmitIfNotEmpty("xx", set.SetGeneralDisplacement.Rotx))
                        .Append(s.EmitIfNotEmpty("yy", set.SetGeneralDisplacement.Roty))
                        .Append(s.EmitIfNotEmpty("zz", set.SetGeneralDisplacement.Rotz));
                    break;
                case SetDisplacementType.FIXED:
                    name = "FixedDisplacement";
                    break;
                case SetDisplacementType.PINNED:
                    name = "PinnedDisplacement";
                    break;
                case SetDisplacementType.FIXED_XX:
                    name = "FixedDisplacementXX";
                    break;
                case SetDisplacementType.FIXED_YY:
                    name = "FixedDisplacementYY";
                    break;
                case SetDisplacementType.FIXED_ZZ:
                    name = "FixedDisplacementZZ";
                    break;
                case SetDisplacementType.ROLLER_X:
                    name = "RollerDisplacementX";
                    break;
                case SetDisplacementType.ROLLER_Y:
                    name = "RollerDisplacementY";
                    break;
                case SetDisplacementType.ROLLER_Z:
                    name = "RollerDisplacementZ";
                    break;
                case SetDisplacementType.ROLLER_XY:
                    name = "RollerDisplacementXY";
                    break;
                case SetDisplacementType.ROLLER_YZ:
                    name = "RollerDisplacementYZ";
                    break;
                case SetDisplacementType.ROLLER_XZ:
                    name = "RollerDisplacementXZ";
                    break;
                case SetDisplacementType.NONE:
                // XXX: Must have a displacement
                default:
                    throw new Exception("Unknown Displacement type: " + set.SetDisplacementType.ToString());
            }

            var dispId = s.DispId(s.LayerIds[set]);
            s.DisplacementIds.Add(set, dispId);
            return $"{name}(name='{dispId}', {args} nodes='{set.GetName()}')";
        }

        private void EmitDisplacements(EmitState s)
        {
            s.CommentLine("Displacements");
            var sets = s.Sets();
            if (sets.Count > 0)
            {
                s.Line("mdl.add([");
                sets.ForEach(set => { s.Line(GenerateDisplacementForSet(s, set) + ","); });
                s.Line("])");
            }
        }

        private void EmitLoads(EmitState s)
        {
            s.CommentLine("Loads");
            s.Workbench.Loads.ForEach(l => { s.Line($"mdl.add({EmitLoad(s, l)})"); });
        }

        private string EmitLoad(EmitState s, Model.Load load)
        {
            switch (load.LoadType)
            {
                case LoadType.Area:
                {
                    var loadId = s.LoadId() + "_area";
                    s.LoadIds.Add(load, loadId);

                    var area = (LoadArea) load;
                    var layersList = StringUtils.ListToPyStr(load.Layers, layer => layer.GetName());
                    var z = s.EmitIfNotEmpty("z", area.Z);
                    var x = s.EmitIfNotEmpty("x", area.X);
                    var y = s.EmitIfNotEmpty("y", area.Y);

                    return $"AreaLoad(name='{loadId}', elements={layersList}, {z} {x} {y} axes='{area.Axes}')";
                }
                case LoadType.Gravity:
                {
                    var loadId = s.LoadId() + "_gravity";
                    s.LoadIds.Add(load, loadId);

                    var g = (LoadGravity) load;
                    var layersList = StringUtils.ListToPyStr(load.Layers, layer => layer.GetName());
                    var z = s.EmitIfNotEmpty("z", g.Z);
                    var x = s.EmitIfNotEmpty("x", g.X);
                    var y = s.EmitIfNotEmpty("y", g.Y);
                    return $"GravityLoad(name='{loadId}', {x} {y} {z} elements={layersList})";
                }
                case LoadType.Point:
                {
                    var loadId = s.LoadId() + "_point";
                    s.LoadIds.Add(load, loadId);

                    var p = (LoadPoint) load;
                    var layersList = StringUtils.ListToPyStr(load.Layers, layer => layer.GetName());
                    var z = s.EmitIfNotEmpty("z", p.Z);
                    var x = s.EmitIfNotEmpty("x", p.X);
                    var y = s.EmitIfNotEmpty("y", p.Y);
                    var zz = s.EmitIfNotEmpty("zz", p.ZZ);
                    var xx = s.EmitIfNotEmpty("xx", p.XX);
                    var yy = s.EmitIfNotEmpty("yy", p.YY);

                    return $"PointLoad(name='{loadId}', {x} {y} {z} {xx} {yy} {zz} nodes={layersList})";
                }
                default:
                    StrucEngLibLog.Instance.WriteLine("Ignoring unknown load: {0}", load.LoadType);
                    throw new Exception($"Unknown load: {load.LoadType}");
            }
        }


        private void EmitSteps(EmitState s)
        {
            s.Workbench.Steps.Sort((s1, s2) => string.Compare(s1.Order, s2.Order, StringComparison.Ordinal));
            s.CommentLine("Steps");

            // emit step definitions
            if (s.Workbench.Steps.Count > 0)
            {
                s.Line("mdl.add([");
                s.Workbench.Steps.ForEach(step => { s.Line(EmitStep(s, step) + ","); });
                s.Line("])");
            }

            // emit step order
            var stepIds = s.StepIds.Values.ToList();
            if (stepIds.Count > 0)
            {
                s.Line($"mdl.steps_order = {StringUtils.ListToPyStr(stepIds.OrderBy(st => st).ToList(), st => st)} ");
            }
        }

        private string EmitStep(EmitState s, Model.Step step)
        {
            var stepName = s.CreateStepName(step.Order);
            s.StepIds.Add(step, stepName);

            var loadNames = new List<string>();
            var dispNames = new List<string>();

            foreach (var stepEntry in step.Entries)
            {
                switch (stepEntry.Type)
                {
                    case StepType.Load:
                        loadNames.Add(s.LoadIds[stepEntry.Value as Model.Load]);
                        break;
                    case StepType.Set:
                        dispNames.Add(s.DisplacementIds[stepEntry.Value as Model.Set]);
                        break;
                    default:
                        // XXX: Ignore rest
                        break;
                }
            }

            var loadStr = "";
            var dispStr = "";
            if (loadNames.Count > 0)
            {
                loadStr = $" loads={StringUtils.ListToPyStr(loadNames, name => name)}, ";
            }

            if (dispNames.Count > 0)
            {
                dispStr = $" displacements={StringUtils.ListToPyStr(dispNames, name => name)}, ";
            }

            return $"GeneralStep(name='{stepName}', {loadStr} {dispStr} nlgeom=False)";
        }

        private void EmitSummary(EmitState s)
        {
            s.CommentLine("Summary");
            s.Line("mdl.summary()");
        }

        private void EmitRun(EmitState s)
        {
            var title = "Run";
            if (s.Workbench.ExecuteOnServer)
            {
                title += " on Sever " + s.Workbench.RemoteServer;
            }
            s.CommentLine(title);
            
            var sx = s.Workbench.Steps
                .Where(step => step.Setting != null && step.Setting.Include)
                .Select(step => step.Setting)
                .ToList();
            var fields = StringUtils.ListToPyStr<string>(new List<string>()
                {
                    sx.Select(setting => setting.Rf).Contains(true) ? "rf" : "",
                    sx.Select(setting => setting.Rm).Contains(true) ? "rm" : "",
                    sx.Select(setting => setting.U).Contains(true) ? "u" : "",
                    sx.Select(setting => setting.Ur).Contains(true) ? "ur" : "",
                    sx.Select(setting => setting.Cf).Contains(true) ? "cf" : "",
                    sx.Select(setting => setting.Cm).Contains(true) ? "cm" : "",
                    sx.Select(setting => setting.ShellForces).Contains(true) ? "sf" : "",
                    sx.Select(setting => setting.SectionMoments).Contains(true) ? "sm" : "",
                }.Where(setting => setting != "").ToList(), (id => id)
            );

            if (s.Workbench.ExecuteOnServer)
            {
                s.Line($"mdl = connect.analyse_and_extract('{s.Workbench.RemoteServer}', mdl, software='abaqus', fields={fields})");
            }
            else
            {
                s.Line($"mdl.analyse_and_extract(software='abaqus', fields={fields})");
            }
            
            sx.ForEach(setting =>
            {
                var step = s.CreateStepName(setting.StepId);
                s.CommentLine($"Plot Step {step}");

                EmitPlotData(s, step, "rfx", setting.Rf);
                EmitPlotData(s, step, "rfy", setting.Rf);
                EmitPlotData(s, step, "rfz", setting.Rf);
                EmitPlotData(s, step, "rfm", setting.Rf);

                EmitPlotData(s, step, "rmx", setting.Rm);
                EmitPlotData(s, step, "rmy", setting.Rm);
                EmitPlotData(s, step, "rmz", setting.Rm);
                EmitPlotData(s, step, "rmm", setting.Rm);

                EmitPlotData(s, step, "ux", setting.U);
                EmitPlotData(s, step, "uy", setting.U);
                EmitPlotData(s, step, "uz", setting.U);
                EmitPlotData(s, step, "um", setting.U);

                EmitPlotData(s, step, "urx", setting.Ur);
                EmitPlotData(s, step, "ury", setting.Ur);
                EmitPlotData(s, step, "urz", setting.Ur);
                EmitPlotData(s, step, "urm", setting.Ur);

                EmitPlotData(s, step, "cfx", setting.Cf);
                EmitPlotData(s, step, "cfy", setting.Cf);
                EmitPlotData(s, step, "cfz", setting.Cf);
                EmitPlotData(s, step, "cfm", setting.Cf);

                EmitPlotData(s, step, "cmx", setting.Cm);
                EmitPlotData(s, step, "cmy", setting.Cm);
                EmitPlotData(s, step, "cmz", setting.Cm);
                EmitPlotData(s, step, "cmm", setting.Cm);

                // shell forces
                EmitPlotData(s, step, "sf1", setting.ShellForces);
                EmitPlotData(s, step, "sf2", setting.ShellForces);
                EmitPlotData(s, step, "sf3", setting.ShellForces);
                EmitPlotData(s, step, "sf4", setting.ShellForces);
                EmitPlotData(s, step, "sf5", setting.ShellForces);

                // section, shell moments together, as same keys
                EmitPlotData(s, step, "sm1", setting.SectionMoments);
                EmitPlotData(s, step, "sm2", setting.SectionMoments);
                EmitPlotData(s, step, "sm3", setting.SectionMoments);
            });
        }

        private void EmitPlotData(EmitState s, string step, string field, bool take = true)
        {
            if (!take)
                return;
            s.Line($"rhino.plot_data(mdl, step='{step}', field='{field}', cbar_size=1)");
        }

        private string ValueOrDefault(string value, string defaultValue)
        {
            return String.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private void EmitAdditionalProperties(EmitState s)
        {
            s.CommentLine("Additional Properties");
            s.Line("data = {}");
            s.Workbench.SandwichModel.AdditionalProperties.ForEach(p =>
            {
                var name = s.PropertyIds[p.Layer];
                var data = new StringBuilder();
                var def = "0";
                data.Append($"prop_name='{name}', ");
                data.Append($"d_strich_bot = {ValueOrDefault(p.DStrichBot, def)}, ");
                data.Append($"d_strich_top = {ValueOrDefault(p.DStrichTop, def)}, ");
                data.Append($"fc_k = {ValueOrDefault(p.FcK, def)}, ");
                data.Append($"theta_grad_kern = {ValueOrDefault(p.FcThetaGradKern, def)}, ");
                data.Append($"fs_d = {ValueOrDefault(p.FsD, def)}, ");
                data.Append($"alpha_bot = {ValueOrDefault(p.AlphaBot, def)}, ");
                data.Append($"beta_bot = {ValueOrDefault(p.BetaBot, def)}, ");
                data.Append($"alpha_top = {ValueOrDefault(p.AlphaTop, def)}, ");
                data.Append($"beta_top = {ValueOrDefault(p.BetaTop, def)}");
                s.Line($"SMM.additionalproperty(data, {data})");
            });
        }


        private void EmitSmmPlotData(EmitState s, string stepName, string fieldName, bool emit = true)
        {
            if (emit)
            {
                s.Line($"rhino.plot_data(mdl, step='{stepName}', field='{fieldName}', cbar_size=1)");
            }
        }

        private void EmitSmm(EmitState s, SmAnalysisSetting settings)
        {
            if (!settings.Include)
            {
                return;
            }

            var sm = settings;
            var stepName = s.CreateStepName(sm.Step.Order);
            s.CommentLine($"SM for {stepName}");
            var data = new StringBuilder();
            data.Append("SMM.Hauptfunktion(structure = mdl, data = data, ");
            data.Append($"step = '{stepName}', ");
            data.Append($"Mindestbewehrung = {s.PythonBoolean(sm.MindestBewehrung)}, ");
            data.Append($"Druckzoneniteration = {s.PythonBoolean(sm.DruckzonenIteration)}, ");
            data.Append($"Schubnachweis = '{sm.Schubnachweis}', ");
            data.Append($"code = '{sm.Code}', ");
            data.Append($"axes_scale = {sm.AxesScale}");
            data.Append(")");
            s.Line(data.ToString());

            EmitSmmPlotData(s, stepName, "as_xi_bot", sm.AsXiBot);
            EmitSmmPlotData(s, stepName, "as_xi_top", sm.AsXiTop);
            EmitSmmPlotData(s, stepName, "as_eta_bot", sm.AsEtaBot);
            EmitSmmPlotData(s, stepName, "as_eta_top", sm.AsEtaTop);
            EmitSmmPlotData(s, stepName, "as_z", sm.AsZ);
            EmitSmmPlotData(s, stepName, "CC_bot", sm.CCBot);
            EmitSmmPlotData(s, stepName, "CC_top", sm.CCTop);
            EmitSmmPlotData(s, stepName, "k_bot", sm.KBot);
            EmitSmmPlotData(s, stepName, "k_top", sm.KTop);
            EmitSmmPlotData(s, stepName, "t_bot", sm.TBot);
            EmitSmmPlotData(s, stepName, "t_top", sm.TTop);
            EmitSmmPlotData(s, stepName, "psi_bot", sm.PsiBot);
            EmitSmmPlotData(s, stepName, "psi_top", sm.PsiTop);
            EmitSmmPlotData(s, stepName, "Fall_bot", sm.FallBot);
            EmitSmmPlotData(s, stepName, "Fall_top", sm.FallTop);
            EmitSmmPlotData(s, stepName, "m_cc_bot", sm.MCcBot);
            EmitSmmPlotData(s, stepName, "m_cc_top", sm.MCcTop);
            EmitSmmPlotData(s, stepName, "m_shear_c", sm.MShearC);
            EmitSmmPlotData(s, stepName, "m_c_total", sm.MCTotal);
            s.Line($"SMM.max_values(mdl,'{stepName}')");
        }
    }
}