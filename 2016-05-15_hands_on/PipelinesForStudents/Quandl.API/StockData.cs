
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
namespace Quandl.API {
  [DataContract]
  public class StockData {
    [DataMember(IsRequired = false)]
    public string source_code { get; set; }

    [DataMember(IsRequired = false)]
    public string code { get; set; }

    [DataMember(IsRequired = false)]
    public string name { get; set; }

    [DataMember(IsRequired = false)]
    public string urlize_name { get; set; }

    [DataMember(IsRequired = false)]
    public string description { get; set; }

    [DataMember(IsRequired = false)]
    public string updated_at { get; set; }

    [DataMember(IsRequired = false)]
    public string frequency { get; set; }

    [DataMember(IsRequired = false)]
    public string from_date { get; set; }

    [DataMember(IsRequired = false)]
    public string to_date { get; set; }

    [DataMember(IsRequired = false)]
    public string[] column_names { get; set; }

    [DataMember(Name = "private", IsRequired = false)]
    public bool @private { get; set; }

    [DataMember(IsRequired = false)]
    public string type { get; set; }

    [DataMember(IsRequired = false)]
    public string errors { get; set; }

    [DataMember(IsRequired = false)]
    public string[][] data { get; set; }

    public List<StockValue> GetValues() {
      List<StockValue> values = new List<StockValue>();
      for (int i = 0; i < data.Length; i++) {
        StockValue val = new StockValue();
        val.Date = DateTime.Parse(data[i][0], CultureInfo.InvariantCulture);
        val.Open = data[i][1] != null ? double.Parse(data[i][1], CultureInfo.InvariantCulture) : -1;
        val.High = data[i][2] != null ? double.Parse(data[i][2], CultureInfo.InvariantCulture) : -1;
        val.Low = data[i][3] != null ? double.Parse(data[i][3], CultureInfo.InvariantCulture) : -1;
        val.Close = data[i][4] != null ? double.Parse(data[i][4], CultureInfo.InvariantCulture) : -1;
        val.Volume = data[i][5] != null ? double.Parse(data[i][5], CultureInfo.InvariantCulture) : -1;

        values.Add(val);
      }
      values.Sort((x, y) => x.Date < y.Date ? -1 : 1);

      return values;
    }
  }
}
