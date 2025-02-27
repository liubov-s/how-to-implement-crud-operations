Imports DevExpress.Mvvm
Imports DevExpress.Xpf.Data
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Data.Entity
Imports EntityFrameworkIssues.Issues
Imports DevExpress.Mvvm.Xpf
Imports System

Public Class MainViewModel
    Inherits ViewModelBase
    Private _ServerModeSource As DevExpress.Data.Linq.EntityServerModeSource

    Public ReadOnly Property ServerModeSource As DevExpress.Data.Linq.EntityServerModeSource
        Get
            If _ServerModeSource Is Nothing Then
                Dim context = New Issues.IssuesContext()
                _ServerModeSource = New DevExpress.Data.Linq.EntityServerModeSource With {
                    .KeyExpression = NameOf(Issues.Issue.Id),
                    .QueryableSource = context.Issues.AsNoTracking()
                }
            End If
            Return _ServerModeSource
        End Get
    End Property
    Private _Users As System.Collections.IList

    Public ReadOnly Property Users As System.Collections.IList
        Get
            If _Users Is Nothing AndAlso Not IsInDesignMode Then
                Dim context = New EntityFrameworkIssues.Issues.IssuesContext()
                _Users = context.Users.[Select](Function(user) New With {
                    .Id = user.Id,
                    .Name = user.FirstName & " " + user.LastName
                }).ToArray()
            End If
            Return _Users
        End Get
    End Property
    <DevExpress.Mvvm.DataAnnotations.Command>
    Public Sub DataSourceRefresh(ByVal args As DevExpress.Mvvm.Xpf.DataSourceRefreshArgs)
        _Users = Nothing
        RaisePropertyChanged(Nameof(Users))
    End Sub
    <DevExpress.Mvvm.DataAnnotations.Command>
    Public Sub CreateEditEntityViewModel(ByVal args As DevExpress.Mvvm.Xpf.CreateEditItemViewModelArgs)
        Dim context = New IssuesContext()
        Dim item As Issue

        If args.IsNewItem Then
            item = New Issue() With {
                .Created = Date.Now
            }
            context.Entry(item).State = EntityState.Added
        Else
            item = context.Issues.Find(args.Key)
        End If

        args.ViewModel = New EditItemViewModel(item, New EditIssueInfo(context, Users), title:=If(args.IsNewItem, "New ", "Edit ") & NameOf(Issue))
    End Sub
    <DevExpress.Mvvm.DataAnnotations.Command>
    Public Sub ValidateRow(ByVal args As DevExpress.Mvvm.Xpf.EditFormRowValidationArgs)
        Dim context = CType(args.EditOperationContext, EditIssueInfo).DbContext
        context.SaveChanges()
    End Sub
    <DevExpress.Mvvm.DataAnnotations.Command>
    Public Sub ValidateRowDeletion(ByVal args As DevExpress.Mvvm.Xpf.EditFormValidateRowDeletionArgs)
        Dim key = CInt(args.Keys.[Single]())
        Dim item = New Issue() With {
            .Id = key
        }
        Dim context = New IssuesContext()
        context.Entry(item).State = EntityState.Deleted
        context.SaveChanges()
    End Sub

End Class