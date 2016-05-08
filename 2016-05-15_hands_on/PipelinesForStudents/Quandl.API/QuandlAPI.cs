
using System.ServiceModel;
using System.ServiceModel.Web;
namespace Quandl.API
{
    [ServiceContract]
    public interface QuandlAPI
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/datasets/GOOG/{identifier}.json")]
        StockData GetData(string identifier);
    }
}
