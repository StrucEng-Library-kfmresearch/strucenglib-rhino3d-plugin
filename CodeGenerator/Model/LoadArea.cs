using System.Collections.Generic;

namespace CodeGenerator.Model
{
    public class LoadArea : Load
    {
        public string Z { get; set; }
        public string Axes { get; set; }
        public List<Layer> Layers { get; set; } = new List<Layer>();
        public LoadType LoadType => LoadType.Area;
    }
}