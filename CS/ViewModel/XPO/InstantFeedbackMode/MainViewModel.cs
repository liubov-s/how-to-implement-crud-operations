﻿using DevExpress.Mvvm;
using XPOIssues.Issues;
using DevExpress.Xpo;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Data;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.Xpf;
using System;

namespace XPOIssues {
    public class MainViewModel : ViewModelBase {
        XPInstantFeedbackView _ItemsSource;
        public XPInstantFeedbackView ItemsSource {
            get
            {
                if(_ItemsSource == null) {
                    var properties = new ServerViewProperty[] {
new ServerViewProperty("Subject", SortDirection.None, new DevExpress.Data.Filtering.OperandProperty("Subject")),
new ServerViewProperty("UserId", SortDirection.None, new DevExpress.Data.Filtering.OperandProperty("UserId")),
new ServerViewProperty("Created", SortDirection.None, new DevExpress.Data.Filtering.OperandProperty("Created")),
new ServerViewProperty("Votes", SortDirection.None, new DevExpress.Data.Filtering.OperandProperty("Votes")),
new ServerViewProperty("Priority", SortDirection.None, new DevExpress.Data.Filtering.OperandProperty("Priority")),
new ServerViewProperty("Oid", SortDirection.Ascending, new DevExpress.Data.Filtering.OperandProperty("Oid"))
    };
                    _ItemsSource = new XPInstantFeedbackView(typeof(Issue), properties, null);
                    _ItemsSource.ResolveSession += (o, e) => {
                        e.Session = new Session();
                    };
                }
                return _ItemsSource;
            }
        }
        System.Collections.IList _Users;
        public System.Collections.IList Users {
            get
            {
                if(_Users == null && !DevExpress.Mvvm.ViewModelBase.IsInDesignMode) {
                    {
                        var session = new DevExpress.Xpo.Session();
                        _Users = session.Query<XPOIssues.Issues.User>().OrderBy(user => user.Oid).Select(user => new { Id = user.Oid, Name = user.FirstName + " " + user.LastName }).ToArray();
                    }
                }
                return _Users;
            }
        }
        [DevExpress.Mvvm.DataAnnotations.Command]
        public void DataSourceRefresh(DevExpress.Mvvm.Xpf.DataSourceRefreshArgs args) {
            _Users = null;
            RaisePropertyChanged(nameof(Users));
        }
        [DevExpress.Mvvm.DataAnnotations.Command]
        public void CreateEditEntityViewModel(DevExpress.Mvvm.Xpf.CreateEditItemViewModelArgs args) {
            var unitOfWork = new UnitOfWork();
            var item = args.IsNewItem
                ? new Issue(unitOfWork)
                : unitOfWork.GetObjectByKey<Issue>(args.Key);
            args.ViewModel = new EditItemViewModel(
                item,
                new EditIssueInfo(unitOfWork, Users),
                dispose: () => unitOfWork.Dispose(),
                title: (args.IsNewItem ? "New " : "Edit ") + nameof(Issue)
            );
        }
        [DevExpress.Mvvm.DataAnnotations.Command]
        public void ValidateRow(DevExpress.Mvvm.Xpf.EditFormRowValidationArgs args) {
            var unitOfWork = ((EditIssueInfo)args.EditOperationContext).UnitOfWork;
            unitOfWork.CommitChanges();
        }
        [DevExpress.Mvvm.DataAnnotations.Command]
        public void ValidateRowDeletion(DevExpress.Mvvm.Xpf.EditFormValidateRowDeletionArgs args) {
            using(var unitOfWork = new UnitOfWork()) {
                var key = (int)args.Keys.Single();
                var item = unitOfWork.GetObjectByKey<Issue>(key);
                unitOfWork.Delete(item);
                unitOfWork.CommitChanges();
            }
        }
    }
}