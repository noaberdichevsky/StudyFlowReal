using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Service
{
    public class AlertService:IAlertService
    {
        //מקבלת שלושה פרמטרים: כותרת החלו תוכן ההודעה וטקסט כפתור לסגירה מציגה דיאלוג עם כפתור אחד בלבד ולא מחזירה תשובה.שיש שגיה 
        public Task ShowAlert(string title, string message, string cancel) =>
            Application.Current!.MainPage!.DisplayAlert(title, message, cancel);
        //מקבלת ארבעה פרמטרים: כותרת החלון תוכן ההודעה כפתור אישור כפתור ביטול מציגה דיאלוג עם שני כפתוריפ של האם למחוק חשבון ואז אישור או ביטול 
        public Task<bool> ShowAlert(string title, string message, string accept, string cancel) =>
            Application.Current!.MainPage!.DisplayAlert(title, message, accept, cancel);
    }
}
