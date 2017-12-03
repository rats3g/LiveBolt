using System;
using System.Threading.Tasks;
using LiveBolt.Models;

namespace LiveBolt.Data
{
    public interface IRepository
    {
        Task<Home> GetHomeById(int? id);
        Home GetHomeByNameAndPassword(string name, string password);
        bool ContainsHome(string name);
        Task<Home> AddHome(Home home);
        void RemoveHome(Home home);
        void RemoveDlm(DLM dlm);
        void RemoveIdm(IDM idm);
        Task<DLM> GetDLMByGuid(Guid guid);
        Task<IDM> GetIDMByGuid(Guid guid);
        void AddDLM(DLM dlm);
        void AddIDM(IDM idm);
        Task Commit();
    }
}
