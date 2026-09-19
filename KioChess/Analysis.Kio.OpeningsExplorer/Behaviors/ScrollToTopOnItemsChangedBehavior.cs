using Microsoft.Xaml.Behaviors;
using System.Collections.Specialized;
using System.Windows.Controls;

namespace Analysis.Kio.OpeningsExplorer.Behaviors;

/// <summary>
/// Behavior that scrolls to the top when the ItemsSource changes.
/// Attach this to a ScrollViewer that contains an ItemsControl.
/// </summary>
public class ScrollToTopOnItemsChangedBehavior : Behavior<ScrollViewer>
{
    private ItemsControl _itemsControl;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += OnLoaded;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Loaded -= OnLoaded;

        if (_itemsControl != null)
        {
            if (_itemsControl.ItemsSource is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged -= OnCollectionChanged;
            }
        }
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // Find the ItemsControl within the ScrollViewer
        _itemsControl = FindItemsControl(AssociatedObject);

        if (_itemsControl?.ItemsSource is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // Scroll to top when collection changes
        AssociatedObject.ScrollToTop();
    }

    private static ItemsControl FindItemsControl(System.Windows.DependencyObject parent)
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

            if (child is ItemsControl itemsControl)
                return itemsControl;

            var result = FindItemsControl(child);
            if (result != null)
                return result;
        }
        return null;
    }
}

