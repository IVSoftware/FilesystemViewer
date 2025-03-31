using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace FilesystemViewer.Portable
{
    public class FilesystemItem : XBoundViewObjectImplementer
    {
        public FilesystemItem() { }
        public FilesystemItem(XElement xel) : base(xel) { }

        /// <summary>
        /// Intercept the command on its way to the base class.
        /// This can be used as a load-on-demand hook in response
        /// to folder expansions.
        /// </summary>
        public override ICommand PlusMinusToggleCommand
        {
            get
            {
                if (_plusMinusToggleCommand is null)
                {
                    _plusMinusToggleCommand = new CommandPCL<IXBoundViewObject>(
                        execute: (xbvo) =>
                        {
                            base.PlusMinusToggleCommand?.Execute(xbvo);
                        });
                }
                return _plusMinusToggleCommand;
            }
        }
        ICommand? _plusMinusToggleCommand = null;
    }

    public class DriveItem : FilesystemItem
    {
        public DriveItem() { }
        public DriveItem(XElement xel) : base(xel) { }

        public override string PlusMinusGlyph 
            => "\uE80F";
    }
    public class FolderItem : FilesystemItem
    {
        public FolderItem() { }
        public override string PlusMinusGlyph
        {
            get
            {
                switch (PlusMinus)
                {
                    case PlusMinus.Collapsed:
                        return "\uE803";
                    case PlusMinus.Partial:
                        return "\uE804";
                    case PlusMinus.Expanded:
                        return "\uE804";
                    default:
                    case PlusMinus.Leaf:
                        return "\uE803";
                }
            }
        }
    }

    public class FileItem : FilesystemItem
    {
        public override string PlusMinusGlyph
        {
            get
            {
                switch (PlusMinus)
                {
                    case PlusMinus.Collapsed:
                        return "\uE803";
                    case PlusMinus.Partial:
                        return "\uE804";
                    case PlusMinus.Expanded:
                        return "\uE804";
                    default:
                    case PlusMinus.Leaf:
                        return "\uE805";
                }
            }
        }
    }
}
