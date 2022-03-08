using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobkoiTest
{
    public class Account
    {
        public string Name { get; set; }
        public string ParentAccount { get; set; }
        public string PrimaryContact { get; set; }
        public string MainPhone { get; set; }
        public string Email { get; set; }
        public decimal Revenue { get; set; }
        public decimal RevenueBase { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CreditLimitBase { get; set; }
        public string CompanyCurrency { get; set; }
        public string ParentCurrency { get;set; }


        public static List<Account> RetrieveAccounts(IOrganizationService _service)
        {
            string[] columns = { "name", "parentaccountid", "primarycontactid", "telephone1", "emailaddress1", "revenue", "revenue_base", "creditlimit", "creditlimit_base", "transactioncurrencyid" };
            var queryExpression = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression()
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("revenue_base", ConditionOperator.NotNull),
                        new ConditionExpression("creditlimit_base", ConditionOperator.NotNull)
                    }
                },
                LinkEntities =
                {
                    new LinkEntity("account","transactioncurrency","transactioncurrencyid","transactioncurrencyid",JoinOperator.Inner),
                    new LinkEntity("account","account","parentaccountid","accountid",JoinOperator.LeftOuter),
                    new LinkEntity("account","contact","primarycontactid","contactid",JoinOperator.LeftOuter)
                }
            };
            queryExpression.LinkEntities[0].Columns.AddColumns("isocurrencycode", "exchangerate");
            queryExpression.LinkEntities[0].EntityAlias = "code";

            queryExpression.LinkEntities[1].Columns.AddColumn("name");
            queryExpression.LinkEntities[1].EntityAlias = "parent";

            queryExpression.LinkEntities[2].Columns.AddColumns("firstname","lastname");
            queryExpression.LinkEntities[2].EntityAlias = "contact";
            // Way to find out all the logical names of the tables

            //Dictionary<string, string> attributesData = new Dictionary<string, string>();
            //RetrieveAllEntitiesRequest metaDataRequest = new RetrieveAllEntitiesRequest();
            //RetrieveAllEntitiesResponse metaDataResponse = new RetrieveAllEntitiesResponse();
            //metaDataRequest.EntityFilters = EntityFilters.Entity;

            //// Execute the request.

            //metaDataResponse = (RetrieveAllEntitiesResponse)_service.Execute(metaDataRequest);

            //var entities = metaDataResponse.EntityMetadata;
            //var entityNames = entities.Select(x=>x.LogicalName).ToList();



            var accounts = _service.RetrieveMultiple(queryExpression).Entities.ToList();
            //Alternate way to read data with all columns
            //var context = new OrganizationServiceContext(_service);
            //var items = context.CreateQuery("account").Where(x => x.GetAttributeValue<double>("revenue") != null && x.GetAttributeValue<double>("creditlimit") != null).ToList();

            var result = new List<Account>();
            if (accounts.Count > 0)
            {
                foreach (var c in accounts)
                {
                    var account = new Account();
                    account.Name = c.Attributes["name"].ToString();
                    AliasedValue parentname;
                    c.TryGetAttributeValue<AliasedValue>("parent.name", out parentname);
                    if(parentname != null)
                        account.ParentAccount = parentname.Value.ToString();
                    account.MainPhone = c.GetAttributeValue<string>("telephone1");
                    account.Email = c.GetAttributeValue<string>("emailaddress1");
                    AliasedValue firstname;
                    AliasedValue lastname; 
                    c.TryGetAttributeValue<AliasedValue>("contact.firstname", out firstname);
                    c.TryGetAttributeValue<AliasedValue>("contact.lastname", out lastname);
                    if (firstname != null && lastname != null)
                        account.PrimaryContact = firstname.Value.ToString() + " " + lastname.Value.ToString();
                    account.Revenue = c.GetAttributeValue<Money>("revenue").Value;
                    account.RevenueBase = c.GetAttributeValue<Money>("revenue_base").Value;
                    account.CreditLimit = c.GetAttributeValue<Money>("creditlimit").Value;
                    account.CreditLimitBase = c.GetAttributeValue<Money>("creditlimit_base").Value;
                    account.CompanyCurrency = c.GetAttributeValue<AliasedValue>("code.isocurrencycode").Value.ToString();
                    account.ParentCurrency = "USD";
                    result.Add(account);
                }
            }
            return result;
        }
    }
}
