using FilesystemViewer.Portable;
using IVSoftware.Portable;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System.Collections.Specialized;
using System.Diagnostics;

namespace FilesystemViewer.WinForms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            base.DataContext = new FilesystemViewModel();
            Load += (sender, e) =>
            {
                DataContext.InitFileSystem();
                
                if(DataContext.XRoot.To<ViewContext>() is { } context)
                {
                    ViewContext = context;
                    ViewContext.DisableXObjectChangeEvents.BeginUsing += (sender, e) =>
                    {
                        UseWaitCursor = true;
                    };
                    ViewContext.DisableXObjectChangeEvents.FinalDispose += (sender, e) =>
                    {
                        var currentItems = ViewContext.ToString();
                        UseWaitCursor = false;
                    };
                }
            };
        }
        ViewContext? ViewContext { get; set; }
        new FilesystemViewModel DataContext => (FilesystemViewModel)base.DataContext!;
    }
    static partial class Extensions
    {
        public static void Add(this FlowLayoutPanel @this, IXBoundViewObject fileItem)
        {
#if false
            if (fileItem.DataTemplate is Control control)
            {
                if (control.Parent is FlowLayoutPanel)
                {
                    control.Show();
                    return;
                }
                else fileItem.DataTemplate = null;
            }
            var MARGIN = new Padding(2);
            fileItem.DataTemplate = new FileItemDataTemplate
            {
                AutoSize = false,
                Margin = MARGIN,
                Width =
                    @this.Width - @this.Padding.Horizontal
                    - SystemInformation.VerticalScrollBarWidth - MARGIN.Horizontal,
                DataContext = fileItem,
                Visible = false,
            };
            @this.Controls.Add((Control)fileItem.DataTemplate);

#endif
        }
        public static void Hide(this FlowLayoutPanel @this, FileItem fileItem)
        {
            // (fileItem.DataTemplate as Control)?.Hide();
        }


        public static void Remove(this FlowLayoutPanel @this, FileItem fileItem)
        {
#if false
            Debug.Fail("Expecting this method to remain unused. Has this changed?");
            if (fileItem.DataTemplate is Control control)
            {
                fileItem.DataTemplate = null;
                @this.Controls.Remove(control);
            }
#endif
        }
    }
}
