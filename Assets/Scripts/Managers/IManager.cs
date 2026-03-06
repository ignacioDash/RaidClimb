using System.Threading.Tasks;

namespace Managers
{
    public interface IManager
    {
        Task Init(object[] args);
        
        void Cleanup();
    }
}