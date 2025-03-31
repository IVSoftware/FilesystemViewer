using FilesystemViewer.Portable;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System.Globalization;

namespace FilesystemViewer.Maui.Converters
{
    public class ColorConversions : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is FilesystemItem item && values[1] is PlusMinus plusMinus)
            {
                switch (item)
                {
                    case DriveItem:
                        switch (plusMinus)
                        {
                            case PlusMinus.Leaf:
                                return Colors.Gray;
                            case PlusMinus.Collapsed:
                                return Colors.RoyalBlue;
                            default:
                                return Colors.Green;
                        }
                    default:
                        switch (plusMinus)
                        {
                            case PlusMinus.Leaf:
                                return Colors.Gray;
                            default:
                                return Colors.Blue;
                        }
                }
            }
            else return Colors.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
