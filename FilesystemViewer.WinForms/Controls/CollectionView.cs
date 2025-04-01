using FilesystemViewer.Portable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FilesystemViewer.WinForms.Controls
{
    public class CollectionView : FlowLayoutPanel
    {
        protected override CreateParams CreateParams 
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // WS_EX_COMPOSITED
                base.CreateParams.ExStyle |= 0x02000000; 
                return cp;
            }
        }
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
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            for (int i = 0; i < e.NewItems?.Count; i++)
                            {
                                var item = e.NewItems[i];
                                if (Activator.CreateInstance(DataTemplate) is Control control)
                                {
                                    control.DataContext = item;
                                    Controls.Add(control);
                                    Controls.SetChildIndex(control, e.NewStartingIndex + i);
                                }
                                else throw new InvalidCastException();
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            for (int i = 0; i < e.OldItems?.Count; i++)
                            {
                                Controls.RemoveAt(e.OldStartingIndex);
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            for (int i = 0; i < e.NewItems?.Count; i++)
                            {
                                Controls.RemoveAt(e.OldStartingIndex + i);
                                if (Activator.CreateInstance(DataTemplate) is Control control)
                                {
                                    control.DataContext = e.NewItems[i];
                                    Controls.Add(control);
                                    Controls.SetChildIndex(control, e.NewStartingIndex + i);
                                }
                                else throw new InvalidCastException();
                            }
                            break;

                        case NotifyCollectionChangedAction.Move:
                            for (int i = 0; i < e.OldItems?.Count; i++)
                            {
                                var control = Controls[e.OldStartingIndex];
                                Controls.SetChildIndex(control, e.NewStartingIndex);
                            }
                            break;

                        case NotifyCollectionChangedAction.Reset:
                            Controls.Clear();
                            foreach (var item in _itemsSource!)
                            {
                                if (Activator.CreateInstance(DataTemplate) is Control control)
                                {
                                    control.DataContext = item;
                                    Controls.Add(control);
                                }
                                else throw new InvalidCastException();
                            }
                            break;
                    }
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
        public void SyncItems(IEnumerable items)
            => ItemsSource?.SyncItems(items);
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if(e.Control is not null) SetItemWidth(e.Control);
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            foreach(Control control in Controls)
            {
                SetItemWidth(control);
            }
        }
        void SetItemWidth(Control item)
        {
            var MARGIN = new Padding(2);
            Margin = MARGIN;
            item.Width =
                Width - Padding.Horizontal
                    - SystemInformation.VerticalScrollBarWidth - MARGIN.Horizontal;
        }
    }
    static class Extensions
    {
        /// <summary>
        /// Synchronizes the contents of the list with the specified
        /// enumerable by reference equality, inserting or removing 
        /// items as needed to match the sequence.
        /// </summary>
        public static void SyncItems(this IList @this, IEnumerable items)
        {
            if (@this is null) throw new NullReferenceException(
                $"{nameof(@this)} must be set before calling {nameof(SyncItems)}.");
            object[] sbItems =
                items as object[]
                ?? items
                .OfType<object>()
                .ToArray();

            int index = 0;

            // Different than below! Relative to the current ITEMS COUNT.
            while (index < sbItems.Length)
            {
                object?
                    sbAtIndex = sbItems[index],
                    isAtIndex;

                // Different! Relative to the current CONTROL COUNT.
                if (index < @this?.Count)
                {
                    isAtIndex = @this[index];
                    if (ReferenceEquals(isAtIndex, sbAtIndex))
                    {   /* G T K */
                        // N O O P
                        // Item is already at the correct index.
                    }
                    else
                    {
                        @this?.Insert(index, sbAtIndex);
                    }
                }
                else
                {
                    @this?.Insert(index, sbItems[index]);
                }
                index++;
            }
            while (index < @this?.Count)
            {
                @this.RemoveAt(index);
            }
        }
    }
}
