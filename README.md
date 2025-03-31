In addition to Stephen's excellent (and genuinely recursive) treatment of this, I wanted to simplify my own answer and leave it SxS because backing a tree-style view with portable XML is can still be a pretty decent way of going about this. Basically, it means that the same business logic can be used more universally, e.g. Maui, WinForms, WPF etc. Since there's a lot to unpack here, I'll refer you in advance to my [MAUI demo project](https://github.com/IVSoftware/IVSoftware.Demo.CrossPlatform.FilesystemViewer.git) repo and to the [XBoundObject](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject.git) code repo. The latter is especially useful for its **MSTest** suite which shows hundreds of examples of using the IP.
___

For something like a filesystem, the view doesn't need to recurse just because the data does. If the objective is representation visualization without actually coupling the data to the view, then a runtime `System.Xml.Linq.XElement` can be a good fit to hold the actual data. The first snippet belowcomes from the actual MSTest project and is a portable back-end view of things. You can really see how this works, though. It demonstrates the use of helper extensions from [XBoundObject](https://www.nuget.org/packages/IVSoftware.Portable.Xml.Linq.XBoundObject/2.0.0) to achieve three basic objectives:

1. Convert flat file paths to a 2D representation on a standard `System.Xml.Linq.XElement`.
2. Use `ViewContext.SyncList()` to synchronize an observable collection (this will be the `CollectionView` source) to the visible items in the `XElement` hierarchy.
3. For testing purposes, use the `ViewContext.ToString()` to synchronize the observable collection and be able to "print it out" as hierarchal text.

This code also exercises the `PlusMinusToggleCommand` and taken together is an actual working data model and `CollectionView` source that is suitable for any .NET platform. Then, below this example, I'll show the non-recursive `DataTemplate` that would be plug-and-play with the `FSItems` observable collection shown and is used in the Maui versions for Windows and Android in the demo project.

___

```
class DriveItem : XBoundViewObjectImplementer { }
class FolderItem : XBoundViewObjectImplementer { }
class FileItem : XBoundViewObjectImplementer { }

/// <summary>
/// Basic File System manipulations.
/// </summary>
[TestMethod]
public void Test_BasicUsageExamples_101()
{
    string actual, expected;
    FolderItem currentFolder = null;

    // Filesystem items
    var FSItems = new ObservableCollection<XBoundViewObjectImplementer>();
    var XRoot = new XElement("root");
    var ViewContext = new ViewContext(XRoot, FSItems, indent: 2, autoSyncEnabled: false);

    // Bind the ViewContext to the root element.
    XRoot.SetBoundAttributeValue(ViewContext);

    actual = XRoot.ToString();
    expected = @" 
<root viewcontext=""[ViewContext]"" />";

    Assert.AreEqual(
        expected.NormalizeResult(),
        actual.NormalizeResult(),
        "Expecting ViewContext instance is bound to Root."
    );
    Assert.IsInstanceOfType<ViewContext>(
        XRoot.To<ViewContext>(),
        "Expecting loopback of the ViewContext instance.");

    // Get Environment.SpecialFolder locations on C: drive only
    var specialFolderPaths =
        Enum
        .GetValues<Environment.SpecialFolder>()
        .Select(_ => Environment.GetFolderPath(_))
        .Where(_ => _.StartsWith("C", StringComparison.OrdinalIgnoreCase))
        .ToList();
    { }

    // Set a DriveItem at the root level.
    XRoot.Show<DriveItem>("C:");

    // Place the folder paths in the XML hierarchy.
    specialFolderPaths
        .ForEach(_ => XRoot.FindOrCreate<FolderItem>(_));

    // Now that the filesystem is populated, update the +/-
    // based on the nested (but not visible) folder items.
    DriveItem cDrive = XRoot.FindOrCreate<DriveItem>("C:");

        
    Assert.AreEqual(
        PlusMinus.Collapsed,
        cDrive.Expand(ExpandDirection.FromItems),
        "Expecting found folders result in Collapsed (not Leaf) state.");

    // Manually synchronize the observable collection.
    // This is necessary because we initialized AutoSyncEnabled to false.
    ViewContext.SyncList();

    // View the observable collection, synchronized to Visible Items.
    actual = ViewContext.ToString();
    actual.ToClipboardExpected();
    expected = @" 
+ C:"
    ;
    Assert.AreEqual(
        expected.NormalizeResult(),
        actual.NormalizeResult(),
        "Expecting collapsed C: drive"
    );

    // Perform a click on the C drive to expand the node.
    cDrive.PlusMinusToggleCommand?.Execute(cDrive);

    // View the observable collection, synchronized to Visible Items.
    ViewContext.SyncList();
    actual = ViewContext.ToString();
    expected = @" 
- C:
+ Program Files
+ Program Files (x86)
+ ProgramData
+ Users
+ WINDOWS"
    ;

    Assert.AreEqual(
        expected.NormalizeResult(),
        actual.NormalizeResult(),
        "Expecting C: is now expanded at root."
    );

    // Navigate to Program Files and "click" on it to expand.
    currentFolder = XRoot
        .FindOrCreate<FolderItem>(Path.Combine("C:", "Program Files"));
    currentFolder?.PlusMinusToggleCommand.Execute(currentFolder);

    // View the observable collection, synchronized to Visible Items.
    ViewContext.SyncList();
    actual = ViewContext.ToString();

    expected = @" 
- C:
- Program Files
    Common Files
+ Program Files (x86)
+ ProgramData
+ Users
+ WINDOWS"
    ;

    Assert.AreEqual(
        expected.NormalizeResult(),
        actual.NormalizeResult(),
        "Expecting expanded Program Files to have a child folder that is a Leaf i.e. empty."
    );
}
```

**Non-Recursive DataTemplate for Maui.Windows and Maui.Android**


These screenshots show how a non-recursive data template that indents the text label based on the current depth in the XML tree create the "smoke-and-mirrors" illusion of depth. To review, we started with flat file path strings, we are maintaining that data in a functional `XElement` tree hierarchy that is _not_ visible, but we iterate the visible items of `XRoot.Descendants()` to maintain an observable collection that _is_ flat but _appears to be_ 2D.


[ ]    [ ]

___

Here's the shared xaml for these views.

```
<CollectionView 
    x:Name="FileCollectionView" 
    ItemsSource="{Binding FSItems}" 
    SelectionMode="None"
    SelectedItem="{Binding SelectedItem, Mode=TwoWay}"
    BackgroundColor="AliceBlue"
    Margin="1">
    <CollectionView.ItemsLayout>
        <LinearItemsLayout Orientation="Vertical" ItemSpacing="2" />
    </CollectionView.ItemsLayout>
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <Grid ColumnDefinitions="Auto,40,*" RowDefinitions="40" >
                <BoxView 
                    Grid.Column="0" 
                    WidthRequest="{Binding Space}"
                    BackgroundColor="{
                        Binding BackgroundColor, 
                        Source={x:Reference FileCollectionView}}"/>
                <Button 
                    Grid.Column="1" 
                    Text="{Binding PlusMinusGlyph}" 
                    FontSize="16"
                    FontFamily="file-folder-drive-icons"
                    BackgroundColor="Transparent"
                    Padding="0"
                    VerticalOptions="Fill"
                    HorizontalOptions="Fill"
                    MinimumHeightRequest="0"
                    MinimumWidthRequest="0"
                    CornerRadius="0"
                    Command="{Binding PlusMinusToggleCommand}"
                    CommandParameter="{Binding .}">
                    <Button.TextColor>
                        <MultiBinding Converter="{StaticResource ColorConversions}">
                            <Binding />
                            <Binding Path="PlusMinus"/>
                        </MultiBinding>
                    </Button.TextColor>
                </Button>
                <Label 
                    Grid.Column="2"
                    Text="{Binding Text}" 
                    VerticalTextAlignment="Center" Padding="2,0,0,0"/>
            </Grid>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```
