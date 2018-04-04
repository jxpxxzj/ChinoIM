using System.Threading.Tasks;

namespace ChinoIM.Common.Network
{
    public interface IUpdateable
    {
        Task<bool> Update();
    }
}
