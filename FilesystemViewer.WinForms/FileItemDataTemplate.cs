
using FilesystemViewer.Portable;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System.ComponentModel;
using System.Globalization;

namespace FilesystemViewer.WinForms
{
    public partial class FileItemDataTemplate : UserControl
    {
        public FileItemDataTemplate()
        {
            InitializeComponent();
            Height = 50;
            PlusMinus.UseCompatibleTextRendering = true;
            PlusMinus.Font = new Font(FileAndFolderFontFamily, 14F);
            PlusMinus.MouseDown += (sender, e) =>
            {
                if (DataContext is FilesystemItem vm)
                {
                    BeginInvoke(() => 
                    {
                        vm.PlusMinusToggleCommand?.Execute(DataContext);
                    });
                }
            };
        }
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent?.TopLevelControl is Control form)
            {
                form.SizeChanged -= localOnSizeChanged;
                form.SizeChanged += localOnSizeChanged;
            }
            void localOnSizeChanged(object? sender, EventArgs e)
            {
                var MARGIN = new Padding(2);
                Margin = MARGIN;
                Width =
                    Parent.Width - Parent.Padding.Horizontal
                        - SystemInformation.VerticalScrollBarWidth - MARGIN.Horizontal;
            }
        }
        public FontFamily FileAndFolderFontFamily
        {
            get
            {
                if (_fileAndFolderFontFamily is null)
                {
                    _fileAndFolderFontFamily = "file-folder-drive-icons".LoadFamilyFromEmbeddedFont();
                }
                return _fileAndFolderFontFamily;
            }
        }
        FontFamily? _fileAndFolderFontFamily = null;

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if(!ReferenceEquals(_dataContextPrev, DataContext))
            {
                if (_dataContextPrev is INotifyPropertyChanged inpcB4)
                {
                    inpcB4.PropertyChanged -= localOnDataContextPropertyChanged;
                }
                if (DataContext is INotifyPropertyChanged inpcFTR)
                {
                    inpcFTR.PropertyChanged += localOnDataContextPropertyChanged;
                    foreach (var propertyName in new[]
                    {
                        nameof(FilesystemItem.Text),
                        nameof(FilesystemItem.Space),
                        nameof(FilesystemItem.PlusMinus),
                        nameof(FilesystemItem.PlusMinusGlyph),
                    })
                    {
                        localOnDataContextPropertyChanged(DataContext, new PropertyChangedEventArgs(propertyName));
                    }
                }
                void localOnDataContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
                {
                    if (DataContext is FilesystemItem fsItem)
                    {
                        switch (e.PropertyName)
                        {
                            case nameof(FilesystemItem.Text):
                                TextLabel.Text = fsItem.Text;
                                break;
                            case nameof(FilesystemItem.PlusMinus):
                                PlusMinus.ForeColor = localConvert();
                                break;
                            case nameof(FilesystemItem.Space):
                                Spacer.Width = fsItem.Space;
                                break;
                            case nameof(FilesystemItem.PlusMinusGlyph):
                                PlusMinus.Text = fsItem.PlusMinusGlyph;
                                break;
                        }
                        Color localConvert()
                        {
                            switch (fsItem)
                            {
                                case DriveItem:
                                    switch (fsItem.PlusMinus)
                                    {
                                        case IVSoftware.Portable.Xml.Linq.XBoundObject.Placement.PlusMinus.Leaf:
                                            return Color.Gray;
                                        case IVSoftware.Portable.Xml.Linq.XBoundObject.Placement.PlusMinus.Collapsed:
                                            return Color.RoyalBlue;
                                        default:
                                            return Color.Green;
                                    }
                                default:
                                    switch (fsItem.PlusMinus)
                                    {
                                        case IVSoftware.Portable.Xml.Linq.XBoundObject.Placement.PlusMinus.Leaf:
                                            return Color.Gray;
                                        default:
                                            return Color.Blue;
                                    }
                            }
                        }
                    }
                }
            }
        }
        private object? _dataContextPrev = null;
    }
}
