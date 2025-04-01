using FilesystemViewer.Portable;
using IVSoftware.Portable;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using static System.Windows.Forms.Control;

namespace FilesystemViewer.WinForms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            base.DataContext = new FilesystemViewModel();
            FileCollectionView.DataTemplate = typeof(FileItemDataTemplate);
            FileCollectionView.ItemsSource = new ObservableCollection<FilesystemItem>();
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
                    ViewContext.DisableXObjectChangeEvents.FinalDispose += async(sender, e) =>
                    {
                        await (_reentrancyCheck.WaitAsync());
                        try
                        {
                            var visibleFilesytemItems = 
                            ViewContext.Items?
                            .OfType<FilesystemItem>()
                            .ToArray() ?? Array.Empty<FilesystemItem>();
#if true
                            FileCollectionView.SyncItems(visibleFilesytemItems);
#else
                            FileCollectionView.Controls.Clear();
                            int index = 0;
                            foreach (var fileItem in visibleFilesytemItems)
                            {

                                FileCollectionView.Add(fileItem);
                                index++;
                            }
#endif
                        }
                        finally
                        {
                            _reentrancyCheck.Release();
                        }
                        var currentItems = ViewContext.ToString();
                        UseWaitCursor = false;
                    };
                }
            };
        }
        SemaphoreSlim _reentrancyCheck = new SemaphoreSlim(1, 1);
        ViewContext? ViewContext { get; set; }
        new FilesystemViewModel DataContext => (FilesystemViewModel)base.DataContext!;
    }
    static partial class Extensions
    {
        public static FileItemDataTemplate Add(this Control @this, FilesystemItem fileItem)
        {
            var MARGIN = new Padding(2);
            var dataTemplate = new FileItemDataTemplate
            {
                AutoSize = false,
                Margin = MARGIN,
                Width =
                    @this.Width - @this.Padding.Horizontal
                    - SystemInformation.VerticalScrollBarWidth - MARGIN.Horizontal,
                DataContext = fileItem,
                Visible = true,
            };
            @this.Controls.Add(dataTemplate);
            return dataTemplate;
        }
        public static void Insert(this Control @this, int index, FilesystemItem fileItem)
        {
            var dataTemplate = @this.Add(fileItem);
            @this.Controls.SetChildIndex(dataTemplate, index);
        }
        public static void SetChildIndex(this Control @this, FilesystemItem fileItem, int index)
        {
            if( @this
                .Controls
                .OfType<FileItemDataTemplate>()
                .FirstOrDefault(_=>ReferenceEquals(_.DataContext, fileItem)) is { } exists)
            {
                @this.Controls.SetChildIndex(exists, index);
            }
            else
            {
                @this.Insert(index, fileItem);
            }
        }


        public static void Remove(this Control @this, FilesystemItem fileItem)
        {
            @this
                .Controls
                .Remove(
                    @this
                    .Controls
                    .OfType<FileItemDataTemplate>()
                    .First(_ => ReferenceEquals(_.DataContext, fileItem)));
        }
    }
}
