using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Service
{
    public interface IGoogleAuthService
    {
        Task<string> SignInAsync();
    }
}
