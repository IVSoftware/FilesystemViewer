using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesystemViewer.WinForms.Controls
{
    public class CollectionView : FlowLayoutPanel
    {
        public IList? ItemsSource
        {
            get => _itemsSource;
            set
            {
                if (!Equals(_itemsSource, value))
                {
                    if (_itemsSource is INotifyCollectionChanged)
                    {
                        ((INotifyCollectionChanged)_itemsSource).CollectionChanged -= localOnCollectionChanged;
                    }
                    _itemsSource = value;
                    Controls.Clear();
                    if (_itemsSource is not null)
                    {
                        foreach (var item in _itemsSource)
                        {
                            if (Activator.CreateInstance(DataTemplate) is Control control)
                            {
                                control.DataContext = item;
                            }
                            else throw new InvalidCastException();
                        }
                    }
                    if (_itemsSource is INotifyCollectionChanged)
                    {
                        ((INotifyCollectionChanged)_itemsSource).CollectionChanged += localOnCollectionChanged;
                    }
                }
                void localOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
                {
                }
            }
        }

        IList? _itemsSource = null;
        public Type DataTemplate
        {
            get => _dataTemplate ?? typeof(Control);
            set
            {
                if (!typeof(Control).IsAssignableFrom(value))
                    throw new ArgumentException("DataTemplate must be a Control type.");

                if (!Equals(_dataTemplate, value))
                {
                    _dataTemplate = value;
                }
            }
        }
        Type? _dataTemplate = null;
    }
}
