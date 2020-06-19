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
    public class ContactMapper
    {
        public IOrganizationService service;
        public ContactMapper()
        {
            service = CrmServiceFactory.GetOrganization();
        }
        public FreshdeskContactForCreation CreateContact(Entity contact)
        {
            long? companyId = null;
            if (contact.Contains("parentcustomerid"))
            {
               EntityReference companyRef =  (EntityReference)contact["parentcustomerid"];


                Entity companyProfile = service.Retrieve("account", companyRef.Id,
                    new ColumnSet("isw_freshdeskid"));

                if(contact.Contains("isw_freshdeskid"))
                    companyId = long.Parse(contact.GetAttributeValue<string>("isw_freshdeskid"));
            }

            return new FreshdeskContactForCreation()
            {
                CompanyId = companyId,
                Address = contact.GetAttributeValue<string>("address1_line1"),
                Name = contact.GetAttributeValue<string>("fullname"),
                Phone = contact.GetAttributeValue<string>("telephone1"),
                Mobile = contact.GetAttributeValue<string>("telephone2"),
                Email = contact.GetAttributeValue<string>("emailaddress1"),
                JobTitle = contact.GetAttributeValue<string>("jobtitle"),
            };

        }
        public Entity CreateContact(FreshdeskContactDto freshdeskContactDto)
        {
            Entity contact = new Entity("contact");

            contact["isw_freshdeskid"] = freshdeskContactDto.Id.ToString();

            if (freshdeskContactDto.CompanyId != null)
            {
                string[] columns = { "accountid" };
                ConditionExpression ce = new ConditionExpression("isw_freshdeskid", ConditionOperator.Equal, freshdeskContactDto.CompanyId.ToString());

                FilterExpression fe = new FilterExpression();
                fe.AddCondition(ce);

                var ec = CrmServiceFactory.RetrieveEntities("account", columns, fe, 1);

                if (ec.Entities.Count > 0)
                {
                    contact.Attributes["parentcustomerid"] = new EntityReference(ec.Entities[0].LogicalName, ec.Entities[0].Id);
                }
            }

            if (!string.IsNullOrEmpty(freshdeskContactDto.Email))
            {
                contact["emailaddress1"] = freshdeskContactDto.Email;
            }

            if (!string.IsNullOrEmpty(freshdeskContactDto.Name))
            {
                string[] nameSplit = freshdeskContactDto.Name.Split(' ');

                contact["firstname"] = nameSplit[0];

                if (nameSplit.Length > 1)
                    contact["lastname"] = nameSplit[1];
                
                
            }
                contact["telephone1"] = freshdeskContactDto.Phone;

            if (!string.IsNullOrEmpty(freshdeskContactDto.Description))
                contact["description"] = freshdeskContactDto.Description;
            if (!string.IsNullOrEmpty(freshdeskContactDto.Address))
                contact["address1_line1"] = freshdeskContactDto.Address;
            if (!string.IsNullOrEmpty(freshdeskContactDto.JobTitle))
                contact["jobtitle"] = freshdeskContactDto.JobTitle;
            if (!string.IsNullOrEmpty(freshdeskContactDto.Mobile))
                contact["telephone2"] = freshdeskContactDto.Mobile;
            if (!string.IsNullOrEmpty(freshdeskContactDto.Name))
                contact["fullname"] = freshdeskContactDto.Name;
            

            return contact;
        }
    }
}
