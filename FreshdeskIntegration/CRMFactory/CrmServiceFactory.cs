using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System.Net.Security;
using System.Configuration;

namespace FreshdeskIntegration.CRMFactory
{
    public static class CrmServiceFactory
    {
        public static IOrganizationService service;
        public static IOrganizationService GetOrganization()
        {
            var appSettings = ConfigurationManager.AppSettings;

            //Uri organizationUri = new Uri("https://interswitch.interswitchng.com/XRMServices/2011/Organization.svc");
            Uri organizationUri = new Uri(appSettings["crmUrl"]);
            //ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType.Tls12);

            var cred = new ClientCredentials();

            
            string username = appSettings["username"];
            string password = appSettings["password"];

            cred.Windows.ClientCredential.Domain = "interswitchng";
            cred.Windows.ClientCredential.UserName = username;
            cred.Windows.ClientCredential.Password = password;
            OrganizationServiceProxy _serviceproxy = new OrganizationServiceProxy(organizationUri, null, cred, null);
            service = (IOrganizationService)_serviceproxy;
            return service;
        }

        public static EntityCollection RetrieveEntities(string source, string[] columns,
            FilterExpression fe = null, int pageSize = 200, int pageNumber = 1, string pagingCookie = null)
        {
            service = GetOrganization();

            QueryExpression query = new QueryExpression(source);

            if(columns != null)
                query.ColumnSet.AddColumns(columns);
            else
            {
                query.ColumnSet = new ColumnSet(true);
            }
            //query.Criteria.AddCondition("objectid", ConditionOperator.Equal, caseRecord.Id);
            if (fe != null)
            {
                query.Criteria = fe;
            }
            //query.TopCount = count;

            // Assign the pageinfo properties to the query expression.
            query.PageInfo = new PagingInfo() { ReturnTotalRecordCount = true };
            query.PageInfo.Count = pageSize;
            query.PageInfo.PageNumber = pageNumber;

            // The current paging cookie. When retrieving the first page, 
            // pagingCookie should be null.
            query.PageInfo.PagingCookie = pagingCookie;

            EntityCollection entities = service.RetrieveMultiple(query);

            return entities;
        }
    }
}
