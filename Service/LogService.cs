using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Service
{
    public class LogService:IAppLogger
    {
        //מדפיסה הודעת דיבאג לחלון ה-Output של Visual Studio עם התג [DEBUG].
        public void LogDebug(string message) => Debug.WriteLine($"[DEBUG] {message}");//פעולות רגילות דיבאג
        public void LogError(string message) => Debug.WriteLine($"[ERROR] {message}");//כשיש שגיאה ארור
    }
}
