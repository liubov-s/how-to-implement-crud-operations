Imports DevExpress.Xpf.Data
Imports System.Linq
Imports System.Threading.Tasks
Imports DevExpress.Xpo
Imports DevExpress.Data.Filtering
Imports XPOIssues.Issues
Imports DevExpress.Mvvm.Xpf
Imports System
Imports System.Collections
Class MainWindow
    Public Sub New()
        InitializeComponent()
        Dim properties = New ServerViewProperty() {
        New ServerViewProperty("Oid", SortDirection.Ascending, New OperandProperty("Oid")),
        New ServerViewProperty("Subject", SortDirection.None, New OperandProperty("Subject")),
        New ServerViewProperty("UserId", SortDirection.None, New OperandProperty("UserId")),
        New ServerViewProperty("Created", SortDirection.None, New OperandProperty("Created")),
        New ServerViewProperty("Votes", SortDirection.None, New OperandProperty("Votes")),
        New ServerViewProperty("Priority", SortDirection.None, New OperandProperty("Priority"))
        }
        Dim session = New Session()
        Dim source = New XPServerModeView(session, GetType(Issues.Issue), Nothing)
        source.Properties.AddRange(properties)
        grid.ItemsSource = source
        LoadLookupData()
    End Sub

    Private Sub LoadLookupData()
        Dim session = New DevExpress.Xpo.Session()
        usersLookup.ItemsSource = session.Query(Of XPOIssues.Issues.User).OrderBy(Function(user) user.Oid).[Select](Function(user) New With {
            .Id = user.Oid,
            .Name = user.FirstName & " " + user.LastName
        }).ToArray()
    End Sub

    Private Sub OnDataSourceRefresh(ByVal sender As System.Object, ByVal e As DevExpress.Xpf.Grid.DataSourceRefreshEventArgs)
        LoadLookupData()
    End Sub

    Private Sub OnCreateEditEntityViewModel(ByVal sender As System.Object, ByVal e As DevExpress.Mvvm.Xpf.CreateEditItemViewModelArgs)
        Dim unitOfWork = New UnitOfWork()
        Dim item = If(e.IsNewItem, New Issue(unitOfWork), unitOfWork.GetObjectByKey(Of Issue)(e.Key))
        e.ViewModel = New EditItemViewModel(item, New EditIssueInfo(unitOfWork, CType(usersLookup.ItemsSource, IList)), dispose:=Sub() unitOfWork.Dispose(), title:=If(e.IsNewItem, "New ", "Edit ") & NameOf(Issue))
    End Sub

    Private Sub OnValidateRow(ByVal sender As System.Object, ByVal e As DevExpress.Mvvm.Xpf.EditFormRowValidationArgs)
        Dim unitOfWork = CType(e.EditOperationContext, EditIssueInfo).UnitOfWork
        unitOfWork.CommitChanges()
    End Sub

    Private Sub OnValidateRowDeletion(ByVal sender As System.Object, ByVal e As DevExpress.Mvvm.Xpf.EditFormValidateRowDeletionArgs)
        Using unitOfWork = New UnitOfWork()
            Dim key = CInt(e.Keys.[Single]())
            Dim item = unitOfWork.GetObjectByKey(Of Issue)(key)
            unitOfWork.Delete(item)
            unitOfWork.CommitChanges()
        End Using
    End Sub

End Class
