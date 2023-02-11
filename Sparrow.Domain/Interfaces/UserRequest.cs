using SparrowPlatform.Domain.Interfaces;

namespace SparrowPlatform.Domain.Interfaces
{
    public class UserRequest : RequestPages
    {
        public string Name { get; set; }
    }
}
