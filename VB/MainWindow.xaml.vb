Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports DevExpress.Xpf.Grid
Imports DevExpress.Xpf.Core
Imports DevExpress.Xpf.Core.Native
Imports System.Windows.Controls.Primitives

Namespace DXSample

    Public Partial Class MainWindow
        Inherits Window

        Public Sub New()
            Me.InitializeComponent()
            Dim list As List(Of TestData) = New List(Of TestData)()
            For i As Integer = 0 To 20 - 1
                list.Add(New TestData(i))
            Next

            Me.grid.ItemsSource = list
            Me.grid.View.ColumnChooserFactory = New CustomColumnChooserFactory()
        End Sub
    End Class

    Public Class CustomColumnChooser
        Implements IColumnChooser

        Private Property Container As FloatingContainer

        Private Property Owner As DataViewBase

        Public Sub New(ByVal ownerView As DataViewBase)
            Owner = ownerView
        End Sub

        Public Sub SaveState(ByVal state As IColumnChooserState)
        End Sub

        Private Sub Container_Hidden(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Owner.IsColumnChooserVisible = False
        End Sub

        Private Sub Show() Implements IColumnChooser.Show
            If Container Is Nothing OrElse Container.IsClosed Then
                Dim columnChooserContent As CustomColumnChooserContent = New CustomColumnChooserContent()
                columnChooserContent.View = Owner
                Container = FloatingContainer.ShowDialog(columnChooserContent, Owner, New Size(500, 350), New FloatingContainerParameters() With {.Title = "Column Chooser", .AllowSizing = True, .CloseOnEscape = False, .ShowModal = True})
                AddHandler Container.Hidden, New RoutedEventHandler(AddressOf Container_Hidden)
            Else
                Container.IsOpen = True
            End If
        End Sub

        Private Sub ApplyState(ByVal state As IColumnChooserState) Implements IColumnChooser.ApplyState
        End Sub

        Private Sub Destroy() Implements IColumnChooser.Destroy
        End Sub

        Private Sub Hide() Implements IColumnChooser.Hide
            Container.IsOpen = False
        End Sub

        Private ReadOnly Property TopContainer As UIElement Implements IColumnChooser.TopContainer
            Get
                Return CType(Container.Content, UIElement)
            End Get
        End Property

        Private Sub IColumnChooser_SaveState(ByVal state As IColumnChooserState) Implements IColumnChooser.SaveState
        End Sub
    End Class

    Public Class CustomColumnChooserContent
        Inherits ContentControl

        Public Shared ReadOnly ViewProperty As DependencyProperty = DependencyProperty.Register("View", GetType(DataViewBase), GetType(CustomColumnChooserContent), New FrameworkPropertyMetadata(CType(Nothing, PropertyChangedCallback)))

        Public Property View As DataViewBase
            Get
                Return CType(GetValue(ViewProperty), DataViewBase)
            End Get

            Set(ByVal value As DataViewBase)
                SetValue(ViewProperty, value)
            End Set
        End Property

        Private Property AllRightButton As Button

        Private Property AllLeftButton As Button

        Private Property OneRightButton As Button

        Private Property OneLeftButton As Button

        Private Property VisibleColumnsList As ColumnList

        Private Property ColumnChooserList As ColumnList

        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            AllRightButton = TryCast(GetTemplateChild("AllRightButton"), Button)
            AddHandler AllRightButton.Click, New RoutedEventHandler(AddressOf AllRightButton_Click)
            AllLeftButton = TryCast(GetTemplateChild("AllLeftButton"), Button)
            AddHandler AllLeftButton.Click, New RoutedEventHandler(AddressOf AllLeftButton_Click)
            OneRightButton = TryCast(GetTemplateChild("OneRightButton"), Button)
            AddHandler OneRightButton.Click, New RoutedEventHandler(AddressOf OneRightButton_Click)
            OneLeftButton = TryCast(GetTemplateChild("OneLeftButton"), Button)
            AddHandler OneLeftButton.Click, New RoutedEventHandler(AddressOf OneLeftButton_Click)
            VisibleColumnsList = TryCast(GetTemplateChild("VisibleColumnsList"), ColumnList)
            ColumnChooserList = TryCast(GetTemplateChild("ColumnChooserList"), ColumnList)
        End Sub

        Private Sub AllLeftButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            View.DataControl.BeginDataUpdate()
            For Each column As GridColumn In CType(View, GridViewBase).ColumnChooserColumns
                column.Visible = True
            Next

            View.DataControl.EndDataUpdate()
        End Sub

        Private Sub AllRightButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            View.DataControl.BeginDataUpdate()
            For Each column As GridColumn In CType(View, GridViewBase).VisibleColumns
                column.Visible = False
            Next

            View.DataControl.EndDataUpdate()
        End Sub

        Private Sub OneLeftButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            If ColumnChooserList.ColumnItemsControl.SelectedIndex = -1 Then Return
            View.DataControl.BeginDataUpdate()
            CType(View, GridViewBase).ColumnChooserColumns(ColumnChooserList.ColumnItemsControl.SelectedIndex).Visible = True
            View.DataControl.EndDataUpdate()
        End Sub

        Private Sub OneRightButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            If VisibleColumnsList.ColumnItemsControl.SelectedIndex = -1 Then Return
            View.DataControl.BeginDataUpdate()
            CType(View, GridViewBase).VisibleColumns(VisibleColumnsList.ColumnItemsControl.SelectedIndex).Visible = False
            View.DataControl.EndDataUpdate()
        End Sub
    End Class

    Public Class CustomColumnChooserFactory
        Implements IColumnChooserFactory

        Private Function Create(ByVal owner As Control) As IColumnChooser Implements IColumnChooserFactory.Create
            Return New CustomColumnChooser(TryCast(owner, DataViewBase))
        End Function
    End Class

    Public Class CustomItemsControl
        Inherits ListBox

        Public Sub New()
            [AddHandler](ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf ButtonBaseClick))
        End Sub

        Friend Sub ButtonBaseClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
            SelectedItem = CType(e.OriginalSource, Button).DataContext
        End Sub
    End Class

    Public Class ColumnList
        Inherits ContentControl

        Public Shared ReadOnly ColumnsProperty As DependencyProperty = DependencyProperty.Register("Columns", GetType(Object), GetType(ColumnList), New FrameworkPropertyMetadata(CType(Nothing, PropertyChangedCallback)))

        Public Property Columns As Object
            Get
                Return CObj(GetValue(ColumnsProperty))
            End Get

            Set(ByVal value As Object)
                SetValue(ColumnsProperty, value)
            End Set
        End Property

        Public Property ColumnItemsControl As CustomItemsControl

        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            ColumnItemsControl = TryCast(GetTemplateChild("ItemsControl"), CustomItemsControl)
        End Sub
    End Class

    Public Class VisibleListDropTargetFactory
        Inherits GridDropTargetFactoryBase

        Protected Overrides Function CreateDropTarget(ByVal dropTargetElement As UIElement) As IDropTarget
            Return New VisibleListDropTarget()
        End Function
    End Class

    Public Class VisibleListDropTarget
        Implements IDropTarget

        Friend Function GetColumnByDragElement(ByVal element As UIElement) As ColumnBase
            Return TryCast(CType(element, GridColumnHeader).DataContext, ColumnBase)
        End Function

        Public Function PointOnColumn(ByVal parent As FrameworkElement, ByVal element As FrameworkElement, ByVal pt As Point, ByRef bottomPrevPoint As Point) As Boolean
            Dim result As Boolean = False
            Dim position As Point = element.GetPosition(parent)
            If bottomPrevPoint.Y <= pt.Y AndAlso bottomPrevPoint.Y + element.ActualHeight >= pt.Y Then
                result = True
            End If

            If Not result AndAlso GetColumnByDragElement(element).IsLast Then
                result = True
            End If

            bottomPrevPoint = New Point(position.X + element.ActualWidth, position.Y + element.ActualHeight)
            Return result
        End Function

        Private Sub Drop(ByVal source As UIElement, ByVal pt As Point) Implements IDropTarget.Drop
            Dim dropColumn As ColumnBase = GetColumnByDragElement(source)
            If dropColumn.Visible Then
                Dim bottomLinePrevElement As Point = New Point(0, 0)
                Dim list As ColumnList = TryCast(LayoutHelper.FindParentObject(Of ColumnList)(TryCast(source, DependencyObject)), ColumnList)
                Dim bottomColumnHeader As GridColumnHeader = Nothing
                bottomColumnHeader = CType(LayoutHelper.FindElement(list, New Predicate(Of FrameworkElement)(Function(ByVal element)(TypeOf element Is GridColumnHeader) AndAlso PointOnColumn(list, element, pt, bottomLinePrevElement))), GridColumnHeader)
                Dim bottomColumn As ColumnBase = GetColumnByDragElement(bottomColumnHeader)
                dropColumn.VisibleIndex = bottomColumn.VisibleIndex
            Else
                dropColumn.Visible = True
            End If
        End Sub

        Private Sub OnDragLeave() Implements IDropTarget.OnDragLeave
        End Sub

        Private Sub OnDragOver(ByVal source As UIElement, ByVal pt As Point) Implements IDropTarget.OnDragOver
        End Sub
    End Class

    Public Class TestData

        Public Property Number As Integer

        Public Property Text As String

        Public Property Bool As Boolean

        Public Sub New(ByVal i As Integer)
            Number = i
            Text = "Row" & i
            Bool = i Mod 3 <> 0
        End Sub
    End Class
End Namespace
