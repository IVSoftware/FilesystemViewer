using FilesystemViewer.Portable;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System.Diagnostics;

namespace FilesystemViewer.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
            {
                try
                {
                    BindingContext.InitFileSystem();
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.Message);
                }
            };
        }
        new FilesystemViewModel BindingContext => (FilesystemViewModel)base.BindingContext;
    }
}
