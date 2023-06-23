namespace StrucEngLib.Model
{
    /// <summary>Model for dependency installer</summary>
    public class InstallerModel
    {
        public string AnacondaHome { get; set; }
     
        /// <summary>
        /// Git paths for developer installs
        /// </summary>
        public string DevStrucenglibPath { get; set; }
        public string DevStrucenglibConnectPath { get; set; }
        public string DevCompasPath { get; set; }
        public string DevCompasFeaPath { get; set; }
    }
}