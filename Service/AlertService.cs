using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Service
{
    public class AlertService:IAlertService
    {
        public Task ShowAlert(string title, string message, string cancel) =>
            Application.Current!.MainPage!.DisplayAlert(title, message, cancel);

        public Task<bool> ShowAlert(string title, string message, string accept, string cancel) =>
            Application.Current!.MainPage!.DisplayAlert(title, message, accept, cancel);
    }
}
