
using FilesystemViewer.Portable;
using System.ComponentModel;

namespace FilesystemViewer.WinForms
{
    public partial class FileItemDataTemplate : UserControl
    {
        public FileItemDataTemplate()
        {
            InitializeComponent();
            Height = 50;
            PlusMinus.UseCompatibleTextRendering = true;
            PlusMinus.Font = new Font(FileAndFolderFontFamily, 16F);
            PlusMinus.Click += (sender, e)=>
            {
                if(DataContext is FilesystemItem vm)
                {
                    vm.PlusMinusToggleCommand?.Execute(DataContext);
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
        public new FilesystemItem? DataContext 
        { 
            get => (FilesystemItem?)base.DataContext;
            set
            { 
                if(DataContext is not null)
                {
                    DataContext.PropertyChanged -= localOnDataContextPropertyChanged;
                }
                base.DataContext = value;
                if(DataContext is not null)
                {
                    DataContext.PropertyChanged += localOnDataContextPropertyChanged;
                    foreach (var propertyName in new[] 
                    {
                        nameof(DataContext.Text),
                        nameof(DataContext.Space),
                        nameof(DataContext.PlusMinusGlyph),
                    })
                    {
                        localOnDataContextPropertyChanged(DataContext, new PropertyChangedEventArgs(propertyName));
                    }
                }
                void localOnDataContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
                {
                    if (DataContext is not null)
                    {
                        switch (e.PropertyName)
                        {
                            case nameof(DataContext.Text):
                                TextLabel.Text = DataContext.Text;
                                break;
                            case nameof(PlusMinus):
                                break;
                            case nameof(DataContext.Space):
                                Spacer.Width = DataContext.Space;
                                break;
                            case nameof(DataContext.PlusMinusGlyph):
                                PlusMinus.Text = DataContext.PlusMinusGlyph;
                                break;
                        }
                    }
                }
            }
        }
    }
}
