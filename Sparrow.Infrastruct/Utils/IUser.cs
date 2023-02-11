using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowPlatform.Infrastruct.Utils
{
    public interface IUser
    {
        bool IsAuthenticated();
        string GetName();
        List<string> GetUserInfoFromToken(string ClaimType);
        string GetAADID();
    }
}
