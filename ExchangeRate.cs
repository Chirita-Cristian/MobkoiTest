using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Configuration;

namespace MobkoiTest
{

    public class ExchangeRate
    {
        [XmlElement(ElementName = "countryName")]
        public string CountryName { get; set; }
        [XmlElement(ElementName = "countryCode")]
        public string CountryCode { get;set; }
        [XmlElement(ElementName = "currencyName")]
        public string CurrencyName { get; set; }
        [XmlElement(ElementName = "currencyCode")]
        public string CurrencyCode { get; set; }
        [XmlElement(ElementName = "rateNew")]
        public decimal Rate { get; set; }
        public decimal ToUSDRate { get; set; }


        public static List<ExchangeRate> exchangeRate = new List<ExchangeRate>();

        public static List<ExchangeRate> RetrieveExchangeRates()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ExchangeRates));
                using (XmlTextReader reader = new XmlTextReader(ConfigurationManager.AppSettings["ExchangeRatesURL"]))
                {
                    ExchangeRates result = (ExchangeRates)serializer.Deserialize(reader);
                    exchangeRate.AddRange(result.ExchangeRate);
                    return exchangeRate;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        public static List<ExchangeRate> ConvertToUSD(List<ExchangeRate> rates)
        {
            var GPBToUSD = rates.Where(x => x.CurrencyCode == "USD").Select(x => x.Rate).FirstOrDefault();
            foreach (var r in rates)
            {
                if (r.CountryCode == "USD")
                    r.ToUSDRate = 1;
                else
                {
                    r.ToUSDRate = r.Rate / GPBToUSD;
                }
            }
            rates.Add(new ExchangeRate { CurrencyCode = "GBP", Rate = 1, ToUSDRate = 1 / GPBToUSD });
            return rates;
        }
        public static List<ExchangeRate> GetExchangeRates()
        {
            return ConvertToUSD(RetrieveExchangeRates());
        }

    }
    [XmlRoot(ElementName = "exchangeRateMonthList")]
    public class ExchangeRates
    {
        [XmlElement("exchangeRate")]
        public List<ExchangeRate> ExchangeRate;
    }
}
