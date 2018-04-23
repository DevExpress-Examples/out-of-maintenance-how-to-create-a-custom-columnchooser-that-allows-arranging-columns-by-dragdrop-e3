Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports DevExpress.Xpf.Grid
Imports DevExpress.Xpf.Core
Imports DevExpress.Xpf.Editors
Imports DevExpress.Xpf.Core.Native
Imports System.Windows.Controls.Primitives
Imports System.Reflection
Imports System.Windows.Data

Namespace DXSample
	Partial Public Class MainWindow
		Inherits Window
		Public Sub New()
			InitializeComponent()
			Dim list As New List(Of TestData)()
			For i As Integer = 0 To 19
				list.Add(New TestData(i))
			Next i
			grid.ItemsSource = list
			grid.View.ColumnChooserFactory = New CustomColumnChooserFactory()
		End Sub
	End Class

	Public Class CustomColumnChooser
		Implements IColumnChooser

		Private privateContainer As FloatingContainer
		Private Property Container() As FloatingContainer
			Get
				Return privateContainer
			End Get
			Set(ByVal value As FloatingContainer)
				privateContainer = value
			End Set
		End Property
		Private privateOwner As DataViewBase
		Private Property Owner() As DataViewBase
			Get
				Return privateOwner
			End Get
			Set(ByVal value As DataViewBase)
				privateOwner = value
			End Set
		End Property

        Public Sub New(ByVal columnChooserOwner As DataViewBase)
            Owner = columnChooserOwner
        End Sub

		Public Sub SaveState(ByVal state As IColumnChooserState)
		End Sub

		Private Sub Container_Hidden(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Owner.IsColumnChooserVisible = False
		End Sub

		Private Sub Show() Implements IColumnChooser.Show
			If Container Is Nothing Then
                Container = FloatingContainer.ShowDialog(New CustomColumnChooserContent() With {.View = Owner}, Owner, New Size(500, 350), New FloatingContainerParameters() With {.Title = "Column Chooser", .AllowSizing = True, .CloseOnEscape = False, .ShowModal = True})
				AddHandler Container.Hidden, AddressOf Container_Hidden
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
		Private ReadOnly Property TopContainer() As UIElement Implements IColumnChooser.TopContainer
			Get
				Return CType(Container.Content, UIElement)
			End Get
		End Property

		Private Sub IColumnChooser_SaveState(ByVal state As IColumnChooserState) Implements IColumnChooser.SaveState
		End Sub
	End Class

	Public Class CustomColumnChooserContent
		Inherits ContentControl
		Public Shared ReadOnly ViewProperty As DependencyProperty = DependencyProperty.Register("View", GetType(DataViewBase), GetType(CustomColumnChooserContent), New FrameworkPropertyMetadata(Nothing))

		Public Property View() As DataViewBase
			Get
				Return CType(GetValue(ViewProperty), DataViewBase)
			End Get
			Set(ByVal value As DataViewBase)
				SetValue(ViewProperty, value)
			End Set
		End Property

		Private privateAllRightButton As Button
		Private Property AllRightButton() As Button
			Get
				Return privateAllRightButton
			End Get
			Set(ByVal value As Button)
				privateAllRightButton = value
			End Set
		End Property
		Private privateAllLeftButton As Button
		Private Property AllLeftButton() As Button
			Get
				Return privateAllLeftButton
			End Get
			Set(ByVal value As Button)
				privateAllLeftButton = value
			End Set
		End Property
		Private privateOneRightButton As Button
		Private Property OneRightButton() As Button
			Get
				Return privateOneRightButton
			End Get
			Set(ByVal value As Button)
				privateOneRightButton = value
			End Set
		End Property
		Private privateOneLeftButton As Button
		Private Property OneLeftButton() As Button
			Get
				Return privateOneLeftButton
			End Get
			Set(ByVal value As Button)
				privateOneLeftButton = value
			End Set
		End Property
		Private privateVisibleColumnsList As ColumnList
		Private Property VisibleColumnsList() As ColumnList
			Get
				Return privateVisibleColumnsList
			End Get
			Set(ByVal value As ColumnList)
				privateVisibleColumnsList = value
			End Set
		End Property
		Private privateColumnChooserList As ColumnList
		Private Property ColumnChooserList() As ColumnList
			Get
				Return privateColumnChooserList
			End Get
			Set(ByVal value As ColumnList)
				privateColumnChooserList = value
			End Set
		End Property

		Public Overrides Sub OnApplyTemplate()
			MyBase.OnApplyTemplate()

			AllRightButton = TryCast(GetTemplateChild("AllRightButton"), Button)
			AddHandler AllRightButton.Click, AddressOf AllRightButton_Click
			AllLeftButton = TryCast(GetTemplateChild("AllLeftButton"), Button)
			AddHandler AllLeftButton.Click, AddressOf AllLeftButton_Click

			OneRightButton = TryCast(GetTemplateChild("OneRightButton"), Button)
			AddHandler OneRightButton.Click, AddressOf OneRightButton_Click
			OneLeftButton = TryCast(GetTemplateChild("OneLeftButton"), Button)
			AddHandler OneLeftButton.Click, AddressOf OneLeftButton_Click

			VisibleColumnsList = TryCast(GetTemplateChild("VisibleColumnsList"), ColumnList)
			ColumnChooserList = TryCast(GetTemplateChild("ColumnChooserList"), ColumnList)
		End Sub

		Private Sub AllLeftButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			View.DataControl.BeginDataUpdate()
			For Each column As GridColumn In (CType(View, GridViewBase)).ColumnChooserColumns
				column.Visible = True
			Next column
			View.DataControl.EndDataUpdate()
		End Sub

		Private Sub AllRightButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			View.DataControl.BeginDataUpdate()
			For Each column As GridColumn In (CType(View, GridViewBase)).VisibleColumns
				column.Visible = False
			Next column
			View.DataControl.EndDataUpdate()
		End Sub

		Private Sub OneLeftButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			If ColumnChooserList.ColumnItemsControl.SelectedIndex = -1 Then
				Return
			End If
			View.DataControl.BeginDataUpdate()
			CType(View, GridViewBase).ColumnChooserColumns(ColumnChooserList.ColumnItemsControl.SelectedIndex).Visible = True
			View.DataControl.EndDataUpdate()
		End Sub

		Private Sub OneRightButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			If VisibleColumnsList.ColumnItemsControl.SelectedIndex = -1 Then
				Return
			End If
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
			MyBase.AddHandler(ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf ButtonBaseClick))
		End Sub
		Friend Sub ButtonBaseClick(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
			SelectedItem = (CType(e.OriginalSource, Button)).DataContext
		End Sub
	End Class

	Public Class ColumnList
		Inherits ContentControl
		Public Shared ReadOnly ColumnsProperty As DependencyProperty = DependencyProperty.Register("Columns", GetType(Object), GetType(ColumnList), New FrameworkPropertyMetadata(Nothing))

		Public Property Columns() As Object
			Get
				Return CObj(GetValue(ColumnsProperty))
			End Get
			Set(ByVal value As Object)
				SetValue(ColumnsProperty, value)
			End Set
		End Property

		Private privateColumnItemsControl As CustomItemsControl
		Public Property ColumnItemsControl() As CustomItemsControl
			Get
				Return privateColumnItemsControl
			End Get
			Set(ByVal value As CustomItemsControl)
				privateColumnItemsControl = value
			End Set
		End Property

		Public Overrides Sub OnApplyTemplate()
			MyBase.OnApplyTemplate()
			ColumnItemsControl = TryCast(GetTemplateChild("ItemsControl"), CustomItemsControl)

		End Sub
	End Class

	Public Class VisibleListDropTargetFactory
		Implements IGridDropTargetFactory
		Private Function CreateDropTarget(ByVal dropTargetElement As UIElement) As IDropTarget Implements IDropTargetFactory.CreateDropTarget
			Return New VisibleListDropTarget()
		End Function
	End Class

	Public Class VisibleListDropTarget
		Implements IDropTarget

		Friend Function GetColumnByDragElement(ByVal element As UIElement) As ColumnBase
			Dim gridColumnHeader As DependencyObject = LayoutHelper.FindParentObject(Of BaseGridColumnHeader)(TryCast(element, DependencyObject))
			Return CType(gridColumnHeader.GetValue(BaseGridColumnHeader.GridColumnProperty), ColumnBase)
		End Function

		Public Function PointOnColumn(ByVal parent As FrameworkElement, ByVal element As FrameworkElement, ByVal pt As Point, ByRef bottomPrevPoint As Point) As Boolean
			Dim result As Boolean = False
			Dim position As Point = element.GetPosition(parent)
			If bottomPrevPoint.Y <= pt.Y AndAlso bottomPrevPoint.Y + element.ActualHeight >= pt.Y Then
				result = True
			End If
			If (Not result) AndAlso GetColumnByDragElement(element).IsLast Then
				result = True
			End If
			bottomPrevPoint = New Point(position.X + element.ActualWidth, position.Y + element.ActualHeight)
			Return result
		End Function

		Private Sub IDropTarget_Drop(ByVal source As UIElement, ByVal pt As Point) Implements IDropTarget.Drop
			Dim dropColumn As ColumnBase = GetColumnByDragElement(source)
			If dropColumn.Visible Then
				Dim bottomLinePrevElement As New Point(0, 0)
				Dim list As ColumnList = TryCast(LayoutHelper.FindParentObject(Of ColumnList)(TryCast(source, DependencyObject)), ColumnList)
				Dim bottomColumnHeader As GridColumnHeader = Nothing
                bottomColumnHeader = CType(LayoutHelper.FindElement(list, New Predicate(Of FrameworkElement)(
                                                                    Function(element As FrameworkElement)
                                                                        Return ((TypeOf element Is GridColumnHeader) AndAlso PointOnColumn(list, element, pt, bottomLinePrevElement))
                                                                    End Function)), GridColumnHeader)
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
		Private privateNumber As Integer
		Public Property Number() As Integer
			Get
				Return privateNumber
			End Get
			Set(ByVal value As Integer)
				privateNumber = value
			End Set
		End Property
		Private privateText As String
		Public Property Text() As String
			Get
				Return privateText
			End Get
			Set(ByVal value As String)
				privateText = value
			End Set
		End Property
		Private privateBool As Boolean
		Public Property Bool() As Boolean
			Get
				Return privateBool
			End Get
			Set(ByVal value As Boolean)
				privateBool = value
			End Set
		End Property
		Public Sub New(ByVal i As Integer)
			Number = i
			Text = "Row" & i
			Bool = i Mod 3 <> 0
		End Sub
	End Class
End Namespace
