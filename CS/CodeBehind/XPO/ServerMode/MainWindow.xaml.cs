using System.Windows;
using DevExpress.Xpf.Data;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using XPOIssues.Issues;
using DevExpress.Mvvm.Xpf;
using System;
using System.Collections;

namespace XPOIssues {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            var properties = new ServerViewProperty[] {
new ServerViewProperty("Oid", SortDirection.Ascending, new OperandProperty("Oid")),
new ServerViewProperty("Subject", SortDirection.None, new OperandProperty("Subject")),
new ServerViewProperty("UserId", SortDirection.None, new OperandProperty("UserId")),
new ServerViewProperty("Created", SortDirection.None, new OperandProperty("Created")),
new ServerViewProperty("Votes", SortDirection.None, new OperandProperty("Votes")),
new ServerViewProperty("Priority", SortDirection.None, new OperandProperty("Priority"))
};
            var session = new Session();
            var source = new XPServerModeView(session, typeof(XPOIssues.Issues.Issue), null);
            source.Properties.AddRange(properties);
            grid.ItemsSource = source;
            LoadLookupData();
        }

        void LoadLookupData() {
            var session = new DevExpress.Xpo.Session();
            usersLookup.ItemsSource = session.Query<XPOIssues.Issues.User>().OrderBy(user => user.Oid).Select(user => new { Id = user.Oid, Name = user.FirstName + " " + user.LastName }).ToArray();
        }

        void OnDataSourceRefresh(System.Object sender, DevExpress.Xpf.Grid.DataSourceRefreshEventArgs e) {
            LoadLookupData();
        }

        void OnCreateEditEntityViewModel(System.Object sender, DevExpress.Mvvm.Xpf.CreateEditItemViewModelArgs e) {
            var unitOfWork = new UnitOfWork();
            var item = e.IsNewItem
                ? new Issue(unitOfWork)
                : unitOfWork.GetObjectByKey<Issue>(e.Key);
            e.ViewModel = new EditItemViewModel(
                item,
                new EditIssueInfo(unitOfWork, (IList)usersLookup.ItemsSource),
                dispose: () => unitOfWork.Dispose(),
                title: (e.IsNewItem ? "New " : "Edit ") + nameof(Issue)
            );
        }

        void OnValidateRow(System.Object sender, DevExpress.Mvvm.Xpf.EditFormRowValidationArgs e) {
            var unitOfWork = ((EditIssueInfo)e.EditOperationContext).UnitOfWork;
            unitOfWork.CommitChanges();
        }

        void OnValidateRowDeletion(System.Object sender, DevExpress.Mvvm.Xpf.EditFormValidateRowDeletionArgs e) {
            using(var unitOfWork = new UnitOfWork()) {
                var key = (int)e.Keys.Single();
                var item = unitOfWork.GetObjectByKey<Issue>(key);
                unitOfWork.Delete(item);
                unitOfWork.CommitChanges();
            }
        }
    }
}
