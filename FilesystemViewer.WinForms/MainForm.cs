using FilesystemViewer.Portable;
using IVSoftware.Portable;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System.Collections;
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

        /// <summary>
        /// Synchronizes the <see cref="Items"/> collection to match the current set of visible
        /// <see cref="XElement"/> nodes in <see cref="XEL"/>. Ensures each bound object is in 
        /// the correct order, inserts missing items, and removes extraneous ones.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <see cref="Items"/> is not an <see cref="ObservableCollection{T}"/> where T implements <see cref="IXBoundViewObject"/>.
        /// </exception>
        public static void SyncItems(this Control @this, FilesystemItem[] items )
        {
            var controls = @this.Controls;
            int index = 0;


            // Different than below! Relative to the current ITEMS COUNT.
            while (index < items.Length)
            {
                FilesystemItem 
                    sbAtIndex = items[index],
                    isAtIndex;

                // Different! Relative to the current CONTROL COUNT.
                if (index < controls.Count)
                {
                    isAtIndex =
                        (FilesystemItem?)controls[index]
                        .DataContext
                        ?? throw new IndexOutOfRangeException();
                    while (index < controls.Count)
                    {
                        if (isAtIndex.IsVisible)
                        {
                            break;
                        }
                        else
                        {
                            // Remove "not visible" item
                            @this.Remove(isAtIndex);
                            isAtIndex =
                                (FilesystemItem?)controls[index]
                                .DataContext 
                                ?? throw new IndexOutOfRangeException();
                        }
                    }
                    if (ReferenceEquals(isAtIndex, sbAtIndex))
                    {   /* G T K */
                        // N O O P
                        // Item is already at the correct index.
                    }
                    else
                    {
                        @this.SetChildIndex(sbAtIndex, index);
                    }
                }
                else
                {
                    @this.Insert(index, items[index]);
                }
                index++;
            }
            while (index < controls.Count)
            {
                //Items.RemoveAt(index);
            }
        }
    }
}
