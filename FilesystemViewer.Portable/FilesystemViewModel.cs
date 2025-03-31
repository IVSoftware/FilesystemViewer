using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace FilesystemViewer.Portable
{
    public class FilesystemViewModel
    {
        public ObservableCollection<FilesystemItem> FSItems { get; } = new();

        public XElement XRoot
        {
            get
            {
                if (_xroot is null)
                {
                    _xroot = new XElement("root").WithXBoundView(FSItems);
                }
                return _xroot;
            }
        }
        XElement? _xroot = null;

        public void InitFileSystem()
        {
            // Create root drives
            foreach (var drive in Directory.GetLogicalDrives())
            {
                var trim = drive.Trim(@"\/".ToCharArray());
                if (!string.IsNullOrWhiteSpace(trim))
                {
                    if (trim.Split(Path.DirectorySeparatorChar).Length == 1)
                    {
                        XRoot.Show<DriveItem>(trim);
                    }
                    else
                    {
                        XRoot.FindOrCreate<FolderItem>(trim);
                    }
                }
            }
            // Create system folders.
            foreach (var path in
                     Enum.GetValues<Environment.SpecialFolder>()
                     .Select(_ => Environment.GetFolderPath(_))
                     .Where(_ => !string.IsNullOrWhiteSpace(_) && Directory.Exists(_)))
            {
                XRoot.FindOrCreate<FolderItem>(path);
            }
            // Identify which root drives have nested content.
            foreach (var drive in XRoot.Elements().Select(_ => _.To<DriveItem>()))
            {
                drive.Expand(ExpandDirection.FromItems);
            }
        }
    }
}
