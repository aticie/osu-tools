namespace StrainVisualizer
{
    internal class FileDetails
    {
        public string FileName { get; set; }
        public string FileImage { get; set; }
        public string FileCreation { get; set; }
        public string Path { get; set; }

        public bool IsFile = false;
        public bool IsFolder = false;
    }
}
