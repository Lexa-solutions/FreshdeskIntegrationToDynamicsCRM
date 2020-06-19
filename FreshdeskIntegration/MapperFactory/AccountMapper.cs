using FreshdeskIntegration.CRMFactory;
using FreshdeskIntegration.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshdeskIntegration.MapperFactory
{

    public class AccountMapper
    {

        public FreshdeskAccountForCreation CreateAccount(Entity account)
        {
            if (account == null)
            {
                return new FreshdeskAccountForCreation();
            }

            return new FreshdeskAccountForCreation()
            {
                Name = account.GetAttributeValue<string>("name"),
                //Relationship Type
                //AccountTier = account.Contains("customertypecode") ? account.FormattedValues["customertypecode"] : "",
                //Industry = account.Contains("industrycode") ? account.FormattedValues["industrycode"] : "",
                Description = account.GetAttributeValue<string>("accountnumber"),
                Note = account.GetAttributeValue<string>("address1_composite"),
            };

        }

        public Entity CreateAccount(FreshdeskAccountDto account)
        {
            Entity accountEntity = new Entity("account");
            if (account == null)
            {
                return accountEntity;
            }

            accountEntity["name"] = account.Name;
            accountEntity["isw_freshdeskid"] = account.Id.ToString();

            return accountEntity;

        }
    }
}
