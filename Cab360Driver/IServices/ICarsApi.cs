using Cab360Driver.DataModels;
using Refit;
using System.Threading.Tasks;

namespace Cab360Driver.IServices
{
    [Headers("X-Parse-Application-Id: YE2MDtm13G3tLJwfLc5bKy3K9JtK0PP6m9TkEYGz", "X-Parse-REST-API-Key: 5gzlnLGlVlgPz28FpWEzQ48jusT4PcUwqMq8VcAq")]
    public interface ICarsApi
    {
        [Get("/classes/Carmodels_Car_Model_List?excludeKeys=Category")]
        Task<Welcome> GetWelcome();
    }
}