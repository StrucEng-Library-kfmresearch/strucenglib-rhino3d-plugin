using System;
using System.Collections.Generic;
using System.Text;
using StrucEngLib.Model;

// ReSharper disable All
// @formatter:off

namespace StrucEngLib
{
    /// <summary>
    /// Code generator to generate python code based on UI input.
    /// </summary>
    public class PythonCodeGenerator
    {
        private string header = $@"
from compas_fea.cad import rhino
from compas_fea.structure import ElasticIsotropic
from compas_fea.structure import ElementProperties as Properties
from compas_fea.structure import GeneralDisplacement
from compas_fea.structure import GeneralStep
from compas_fea.structure import GravityLoad
from compas_fea.structure import AreaLoad
from compas_fea.structure import PointLoad
from compas_fea.structure import PinnedDisplacement
from compas_fea.structure import RollerDisplacementX
from compas_fea.structure import RollerDisplacementY
from compas_fea.structure import RollerDisplacementXY
from compas_fea.structure import ShellSection
from compas_fea.structure import Structure

import sandwichmodel_main as SMM

# Snippets based on code of Andrew Liew (github.com/andrewliew), Benjamin Berger (github.com/Beberger)
# Code Generated by StrucEngLib Plugin {StrucEngLibPlugin.Version}, {StrucEngLibPlugin.Website}

name = 'Rahmen'
path = 'C:/Temp/'
mdl = Structure(name=name, path=path)
";

        private const string footer = @"
# Summary

mdl.summary()

# Run

mdl.analyse_and_extract(software='abaqus', fields=['u','sf','sm'])

# rhino.plot_data(mdl, step='step_load', field='sm1',cbar_size=1)
# rhino.plot_data(mdl, step='step_load', field='sm2',cbar_size=1)
# rhino.plot_data(mdl, step='step_load', field='sm3',cbar_size=1)
# rhino.plot_data(mdl, step='step_load', field='sf4',cbar_size=1)
# rhino.plot_data(mdl, step='step_load', field='sf5',cbar_size=1)
# rhino.plot_data(mdl, step='step_load', field='sf1',cbar_size=1)
# rhino.plot_data(mdl, step='step_load', field='sf2',cbar_size=1)
# rhino.plot_data(mdl, step='step_load', field='sf3',cbar_size=1)
# rhino.plot_data(mdl, step='step_load', field='um',cbar_size=1)
# #print(mdl.elements[251])
# #print(mdl.elements[100])
# #print(mdl.elements[222])
";

        private readonly Workbench _model;

        public PythonCodeGenerator(Workbench model)
        {
            _model = model;
        }

        private int _loadIdCounter = 0;
        private string RemoveSpaces(string id) => id.Replace(" ", "_");
        private string LoadId() => "load_" + _loadIdCounter++;
        private string LayerId(string id) => RemoveSpaces(id) + "_element";
        private string SetId(string id) => RemoveSpaces(id) + "_set";
        private string SectionId(string id) => RemoveSpaces(id) + "_sec";
        private string PropId(string id) => RemoveSpaces(id) + "_prop";
        private string MatElasticId(string id) => RemoveSpaces(id) + "_mat_elastic";
        private string DispId(string id) => RemoveSpaces(id) + "_disp";

        private string ListToStr<T> (List<T> data, Func<T, string> toStr)
        {
            StringBuilder b = new StringBuilder();
            b.Append("[ ");
            int n = 0;
            foreach (var l in data)
            {
                if (n > 0)
                {
                    b.Append(", ");
                }
                b.Append($"'{toStr(l)}'");
                n++;
            }

            b.Append(" ] ");
            return b.ToString();
        }

        private string _nl = Environment.NewLine;

        public string Generate()
        {
            _loadIdCounter = 0;

            StringBuilder b = new StringBuilder();

            b.Append(header);

            foreach (var layer in _model.Layers)
            {
                if (layer.LayerType == LayerType.ELEMENT)
                {
                    var element = (Element) layer;
                    var layerId = LayerId(element.GetName());
                    var layerName = element.GetName();
                    b.Append(_nl + $@"# == Element {layerName}" + _nl);
                    b.Append($@"rhino.add_nodes_elements_from_layers(mdl, mesh_type='ShellElement', layers=['{layerName}'])" + _nl);

                    var mat = element.ElementMaterialElastic;
                    var matId = MatElasticId(layerId);
                    b.Append($@"mdl.add(ElasticIsotropic(name='{matId}', E={mat.E}, v={mat.V}, p={mat.P}))" + _nl);
                    var sectionId = SectionId(layerId);
                    b.Append($@"mdl.add(ShellSection(name='{sectionId}', t={element.ElementShellSection.Thickness})) #[mm] " + _nl);
                    var propId = PropId(layerId);
                    b.Append($@"mdl.add(Properties(name='{propId}', material='{matId}', section='{sectionId}', elset='{layerName}'))" + _nl);
                }

                if (layer.LayerType == LayerType.SET)
                {
                    var set = (Set) layer;
                    var setName = set.GetName();
                    var setId = SetId(setName);
                    b.Append(_nl + $@"# == Set {set.GetName()}" + _nl);
                    b.Append($@"rhino.add_sets_from_layers(mdl, layers=['{setName}'] ) " + _nl);
                    var dispId = DispId(setId);
                    b.Append($@"mdl.add([PinnedDisplacement(name='{dispId}', nodes='{setName}')]) " + _nl);
                }
            }
            
            Dictionary<Load, string> loadNameMap = new Dictionary<Load, string>();
            foreach (var load in _model.Loads)
            {
                var loadId = "";
                if (load.LoadType == LoadType.Area)
                {
                    var area = (LoadArea) load;
                    string layersList = ListToStr(load.Layers, layer => layer.GetName());
                    b.Append(_nl + $@"# == Load Area {layersList}" + _nl);
                    loadId = LoadId() + "_area";
                    b.Append($@"mdl.add(AreaLoad(name='{loadId}', elements={layersList}, z={area.Z}, axes='{area.Axes}')) " + _nl);
                }
                else if (load.LoadType == LoadType.Gravity)
                {
                    string layersList = ListToStr(load.Layers, layer => layer.GetName());
                    b.Append(_nl + $@"#== Load Gravity {layersList}" + _nl);
                    loadId = LoadId() + "_gravity";
                    b.Append($@"mdl.add(GravityLoad(name='{loadId}', elements={layersList}))" + _nl);
                }
                loadNameMap.Add(load, loadId);
            }
            
            b.Append(_nl + $@"# == Steps" + _nl);
            var stepCounter = 0;
            /*
             * stepPair: <float order as key, List<Steps> which belong to same order
             * Order is already by key (asc)
             */   
            var stepOrderList = new Dictionary<string, List<Model.Step>>();
            foreach (var stepPair in GroupSteps(_model))
            {
                var stepName = "step_" + stepCounter++;
                stepOrderList.Add(stepName, stepPair.Value);
                List<string> loadsNames = new List<string>();
                List<string> dispNames = new List<string>();
                foreach (var stepEntry in stepPair.Value)
                {
                    if (stepEntry.StepType == StepType.Load) {
                        loadsNames.Add(loadNameMap[stepEntry.Load]);
                    }
                    else if (stepEntry.StepType == StepType.Set)
                    {
                        dispNames.Add(stepEntry.Set.Name);
                    }
                }
                var loadStr = "";
                var dispStr = "";
                if (loadsNames.Count > 0) {
                    loadStr = $" loads={ListToStr(loadsNames, name => name)}, ";
                }
                if (dispNames.Count > 0) {
                    dispStr = $" displacements={ListToStr(dispNames, name => name)}, ";
                }
                b.Append($@"mdl.add(GeneralStep(name='{stepName}', {loadStr} {dispStr} nlgeom=False))" + _nl);
            }
            b.Append($@"mdl.steps_order = {ListToStr(stepOrderList.Keys.ToList(), s => s)} " + _nl);
            
            b.Append(footer);
            return b.ToString();
        }
        
        /*
         * Given A workbench, group steps according to their order (float).
         * Result is a List of steps belonging to the same order-id, ordered by order-id
         */
        private SortedDictionary<float, List<Model.Step>> GroupSteps(Workbench model) {
            var steps = new SortedDictionary<float, List<Model.Step>>();
            foreach (var step in model.Steps)
            {
                try{
                    var order = float.Parse(step.Order);
                    if (String.IsNullOrEmpty(step.Order)) continue;
                    if (steps.ContainsKey(order)) {
                        steps[order].Add(step);
                    } else {
                        steps.Add(order, new List<Model.Step>(){step});
                    }
                } catch(Exception e) {
                    // XXX: We ignore invalid step numbers
                }
            }
            
            return steps;
        }
    }
}
