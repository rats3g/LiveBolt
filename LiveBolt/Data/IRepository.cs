using System.Threading.Tasks;
using LiveBolt.Models;

namespace LiveBolt.Data
{
    public interface IRepository
    {
        Task<Home> GetHomeById(int? id);
        Home GetHomeByNameAndPassword(string name, string password);
        Task<Home> AddHome(Home home);
        void RemoveHome(Home home);
        Task Commit();
    }
}
