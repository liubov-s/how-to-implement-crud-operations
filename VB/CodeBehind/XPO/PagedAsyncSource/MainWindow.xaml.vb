Imports DevExpress.Xpf.Data
Imports System.Linq
Imports System.Threading.Tasks
Imports DevExpress.Xpo
Imports DevExpress.Data.Filtering
Class MainWindow
    Private _DetachedObjectsHelper As DetachedObjectsHelper(Of XPOIssues.Issues.Issue)
    Public Sub New()
        InitializeComponent()
        Using session = New Session()
            Dim classInfo = session.GetClassInfo(Of Issues.Issue)()
            Dim properties = classInfo.Members.Where(Function(member) member.IsPublic AndAlso member.IsPersistent).[Select](Function(member) member.Name).ToArray()
            _DetachedObjectsHelper = DetachedObjectsHelper(Of Issues.Issue).Create(classInfo.KeyProperty.Name, properties)
        End Using
        Dim source = New PagedAsyncSource With {
            .CustomProperties = _DetachedObjectsHelper.Properties,
            .KeyProperty = nameof(Issues.Issue.Oid),
            .PageNavigationMode = PageNavigationMode.ArbitraryWithTotalPageCount
        }
        AddHandler source.FetchPage, AddressOf OnFetchPage
        AddHandler source.GetTotalSummaries, AddressOf OnGetTotalSummaries
        grid.ItemsSource = source
        LoadLookupData()
    End Sub

    Private Sub OnFetchPage(ByVal sender As System.Object, ByVal e As DevExpress.Xpf.Data.FetchPageAsyncEventArgs)
        e.Result = Task.Run(Of DevExpress.Xpf.Data.FetchRowsResult)(Function()
                                                                        Const pageTakeCount As Integer = 5
                                                                        Using session = New DevExpress.Xpo.Session()
                                                                            Dim queryable = session.Query(Of Issues.Issue)().SortBy(e.SortOrder, defaultUniqueSortPropertyName:=NameOf(Issues.Issue.Oid)).Where(MakeFilterExpression(CType(e.Filter, DevExpress.Data.Filtering.CriteriaOperator)))
                                                                            Dim items = queryable.Skip(e.Skip).Take(e.Take * pageTakeCount).ToArray()
                                                                            Return _DetachedObjectsHelper.ConvertToDetachedObjects(items)
                                                                        End Using
                                                                    End Function)
    End Sub

    Private Sub OnGetTotalSummaries(ByVal sender As System.Object, ByVal e As DevExpress.Xpf.Data.GetSummariesAsyncEventArgs)
        e.Result = Task.Run(Function()
                                Using session = New DevExpress.Xpo.Session()
                                    Return session.Query(Of Issues.Issue)().Where(MakeFilterExpression(e.Filter)).GetSummaries(e.Summaries)
                                End Using
                            End Function)
    End Sub

    Private Function MakeFilterExpression(ByVal filter As DevExpress.Data.Filtering.CriteriaOperator) As System.Linq.Expressions.Expression(Of System.Func(Of XPOIssues.Issues.Issue, Boolean))
        Dim converter = New DevExpress.Xpf.Data.GridFilterCriteriaToExpressionConverter(Of XPOIssues.Issues.Issue)()
        Return converter.Convert(filter)
    End Function

    Private Sub OnValidateRow(ByVal sender As System.Object, ByVal e As DevExpress.Xpf.Grid.GridRowValidationEventArgs)
        Using unitOfWork = New DevExpress.Xpo.UnitOfWork()
            Dim item = If(e.IsNewItem, New Issues.Issue(unitOfWork), unitOfWork.GetObjectByKey(Of Issues.Issue)(_DetachedObjectsHelper.GetKey(e.Row)))
            _DetachedObjectsHelper.ApplyProperties(item, e.Row)
            unitOfWork.CommitChanges()

            If e.IsNewItem Then
                _DetachedObjectsHelper.SetKey(e.Row, item.Oid)
            End If
        End Using
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

End Class
