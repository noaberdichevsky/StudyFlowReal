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
        public void LogDebug(string message) => Debug.WriteLine($"[DEBUG] {message}");
        public void LogError(string message) => Debug.WriteLine($"[ERROR] {message}");
    }
}
