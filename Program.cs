using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Exportable.Engines;
using Exportable.Engines.Excel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;

namespace MobkoiTest
{
    class Program
    {
        static void Main(string[] args)
        {

            IOrganizationService oServiceProxy;
            try
            {
                CrmServiceClient crmServiceClient = new CrmServiceClient(ConfigurationManager.AppSettings["ConnectionString"]);
                oServiceProxy = (IOrganizationService)crmServiceClient.OrganizationWebProxyClient != null ?
                    (IOrganizationService)crmServiceClient.OrganizationWebProxyClient :
                    (IOrganizationService)crmServiceClient.OrganizationServiceProxy;
              
                if (oServiceProxy != null)
                {
                    //Get the current user ID:
                    Guid userid = ((WhoAmIResponse)oServiceProxy.Execute(new WhoAmIRequest())).UserId;

                    if (userid != Guid.Empty)
                    {
                        Console.WriteLine("Connection Successful!");
                        var accounts = Account.RetrieveAccounts(oServiceProxy);
                        var rates = ExchangeRate.GetExchangeRates();
                        CalculateRate(accounts, rates);
                        SaveToExcel(accounts);
                    }
                }
                else
                {
                    Console.WriteLine("Connection failed...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
        public static void CalculateRate(List<Account> accounts,List<ExchangeRate> rates)
        {
            foreach(var a in accounts)
            {
                var rate = rates.Where(x => x.CurrencyCode == a.CompanyCurrency).Select(x => x.ToUSDRate).FirstOrDefault();
                if(rate != 0)
                {
                    a.CreditLimitBase = a.CreditLimit / rate;
                    a.RevenueBase = a.Revenue / rate;
                }
                else
                {
                    Console.Write("Rate not found in list");
                }
            }
        }
        public static void SaveToExcel(List<Account> accounts)
        {
            IExportEngine engine = new ExcelExportEngine();
            engine.AddData(accounts);
            engine.AsExcel();
            engine.Export("../../../Export.xlsx");
            Console.WriteLine("Excel exported");
        }
    }
}