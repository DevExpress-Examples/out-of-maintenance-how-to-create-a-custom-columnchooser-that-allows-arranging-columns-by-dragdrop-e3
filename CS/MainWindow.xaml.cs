using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Core.Native;
using System.Windows.Controls.Primitives;
using System.Reflection;
using System.Windows.Data;

namespace DXSample {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            List<TestData> list = new List<TestData>();
            for(int i = 0; i < 20; i++) {
                list.Add(new TestData(i));
            }
            grid.ItemsSource = list;
            grid.View.ColumnChooserFactory = new CustomColumnChooserFactory();
        }
    }

    public class CustomColumnChooser : IColumnChooser {

        FloatingContainer Container { get; set; }
        DataViewBase Owner { get; set; }

        public CustomColumnChooser(DataViewBase ownerView) {
            Owner = ownerView;
        }

        public void SaveState(IColumnChooserState state) { }

        void Container_Hidden(object sender, RoutedEventArgs e) {
            Owner.IsColumnChooserVisible = false;
        }

        void IColumnChooser.Show() {
            if(Container == null) {
                CustomColumnChooserContent columnChooserContent = new CustomColumnChooserContent();
                columnChooserContent.View = Owner;

                Container = FloatingContainer.ShowDialog(columnChooserContent, Owner, new Size(500, 350), new FloatingContainerParameters() {
                    Title = "Column Chooser",
                    AllowSizing = true,
                    CloseOnEscape = false,
                    ShowModal = true
                });
                Container.Hidden += new RoutedEventHandler(Container_Hidden);
            } else {
                Container.IsOpen = true;
            }
        }

        void IColumnChooser.ApplyState(IColumnChooserState state) { }

        void IColumnChooser.Destroy() { }

        void IColumnChooser.Hide() {
            Container.IsOpen = false;
        }
        UIElement IColumnChooser.TopContainer {
            get { return (UIElement)Container.Content; }
        }

        void IColumnChooser.SaveState(IColumnChooserState state) { }
    }

    public class CustomColumnChooserContent : ContentControl {
        public static readonly DependencyProperty ViewProperty = DependencyProperty.Register("View", typeof(DataViewBase), typeof(CustomColumnChooserContent), new FrameworkPropertyMetadata(null));

        public DataViewBase View {
            get { return (DataViewBase)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }

        Button AllRightButton { get; set; }
        Button AllLeftButton { get; set; }
        Button OneRightButton { get; set; }
        Button OneLeftButton { get; set; }
        ColumnList VisibleColumnsList { get; set; }
        ColumnList ColumnChooserList { get; set; }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            AllRightButton = GetTemplateChild("AllRightButton") as Button;
            AllRightButton.Click += new RoutedEventHandler(AllRightButton_Click);
            AllLeftButton = GetTemplateChild("AllLeftButton") as Button;
            AllLeftButton.Click += new RoutedEventHandler(AllLeftButton_Click);

            OneRightButton = GetTemplateChild("OneRightButton") as Button;
            OneRightButton.Click += new RoutedEventHandler(OneRightButton_Click);
            OneLeftButton = GetTemplateChild("OneLeftButton") as Button;
            OneLeftButton.Click += new RoutedEventHandler(OneLeftButton_Click);

            VisibleColumnsList = GetTemplateChild("VisibleColumnsList") as ColumnList;
            ColumnChooserList = GetTemplateChild("ColumnChooserList") as ColumnList;
        }

        void AllLeftButton_Click(object sender, RoutedEventArgs e) {
            View.DataControl.BeginDataUpdate();
            foreach(GridColumn column in ((GridViewBase)View).ColumnChooserColumns) {
                column.Visible = true;
            }
            View.DataControl.EndDataUpdate();
        }

        void AllRightButton_Click(object sender, RoutedEventArgs e) {
            View.DataControl.BeginDataUpdate();
            foreach(GridColumn column in ((GridViewBase)View).VisibleColumns) {
                column.Visible = false;
            }
            View.DataControl.EndDataUpdate();
        }

        void OneLeftButton_Click(object sender, RoutedEventArgs e) {
            if(ColumnChooserList.ColumnItemsControl.SelectedIndex == -1)
                return;
            View.DataControl.BeginDataUpdate();
            ((GridViewBase)View).ColumnChooserColumns[ColumnChooserList.ColumnItemsControl.SelectedIndex].Visible = true;
            View.DataControl.EndDataUpdate();
        }

        void OneRightButton_Click(object sender, RoutedEventArgs e) {
            if(VisibleColumnsList.ColumnItemsControl.SelectedIndex == -1)
                return;
            View.DataControl.BeginDataUpdate();
            ((GridViewBase)View).VisibleColumns[VisibleColumnsList.ColumnItemsControl.SelectedIndex].Visible = false;
            View.DataControl.EndDataUpdate();
        }
    }

    public class CustomColumnChooserFactory : IColumnChooserFactory {
        IColumnChooser IColumnChooserFactory.Create(Control owner) {
            return new CustomColumnChooser(owner as DataViewBase);
        }
    }

    public class CustomItemsControl : ListBox {
        public CustomItemsControl() {
            AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ButtonBaseClick));
        }
        internal void ButtonBaseClick(object sender, System.Windows.RoutedEventArgs e) {
            SelectedItem = ((Button)e.OriginalSource).DataContext;
        }
    }

    public class ColumnList : ContentControl {
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(object), typeof(ColumnList), new FrameworkPropertyMetadata(null));

        public object Columns {
            get { return (object)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public CustomItemsControl ColumnItemsControl { get; set; }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            ColumnItemsControl = GetTemplateChild("ItemsControl") as CustomItemsControl;

        }
    }

    public class VisibleListDropTargetFactory : IGridDropTargetFactory {
        IDropTarget IDropTargetFactory.CreateDropTarget(UIElement dropTargetElement) {
            return new VisibleListDropTarget();
        }
    }

    public class VisibleListDropTarget : IDropTarget {

        internal ColumnBase GetColumnByDragElement(UIElement element) {
            FrameworkElement columnList = element as ColumnList;
            if(columnList != null) {
                DataViewBase view = ((CustomColumnChooserContent)columnList.DataContext).View;
                return (view.DataControl as GridControl).Columns[((IDropTarget)this).Index];
            } else {
                return (element as GridColumnHeader).DataContext as GridColumn;
            }
        }

        public bool PointOnColumn(FrameworkElement parent, FrameworkElement element, Point pt, ref Point bottomPrevPoint) {
            bool result = false;
            Point position = element.GetPosition(parent);
            if(bottomPrevPoint.Y <= pt.Y && bottomPrevPoint.Y + element.ActualHeight >= pt.Y) {
                result = true;
            }
            if(!result && GetColumnByDragElement(element).IsLast) {
                result = true;
            }
            bottomPrevPoint = new Point(position.X + element.ActualWidth, position.Y + element.ActualHeight);
            return result;
        }

        void IDropTarget.Drop(UIElement source, Point pt) {
            ColumnBase dropColumn = GetColumnByDragElement(source);
            if(dropColumn.Visible) {
                Point bottomLinePrevElement = new Point(0, 0);
                ColumnList list = LayoutHelper.FindParentObject<ColumnList>(source as DependencyObject) as ColumnList;
                GridColumnHeader bottomColumnHeader = null;
                bottomColumnHeader = (GridColumnHeader)LayoutHelper.FindElement(list, new Predicate<FrameworkElement>(delegate(FrameworkElement element) {
                    return (element is GridColumnHeader) && PointOnColumn(list, element, pt, ref bottomLinePrevElement);
                }));
                ColumnBase bottomColumn = GetColumnByDragElement(bottomColumnHeader);
                dropColumn.VisibleIndex = bottomColumn.VisibleIndex;
            } else {
                dropColumn.Visible = true;
            }
        }

        void IDropTarget.OnDragLeave() { }

        void IDropTarget.OnDragOver(UIElement source, Point pt) { }

        int IDropTarget.Index { get; set; }
    }

    public class TestData {
        public int Number { get; set; }
        public string Text { get; set; }
        public bool Bool { get; set; }
        public TestData(int i) {
            Number = i;
            Text = "Row" + i;
            Bool = i % 3 != 0;
        }
    }
}
