
using System.ServiceModel;
namespace Quandl.API
{
    public class QuandlService : ClientBase<QuandlAPI>, QuandlAPI
    {
        public StockData GetData(string identifier)
        {
            return base.Channel.GetData(identifier);
        }
    }
}
