using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Service
{
    public interface IAlertService
    {
        Task ShowAlert(string title, string message, string cancel);
        Task<bool> ShowAlert(string title, string message, string accept, string cancel);
    }
}
