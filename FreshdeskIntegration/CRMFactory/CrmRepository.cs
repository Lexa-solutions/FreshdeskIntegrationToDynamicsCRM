using FreshdeskIntegration.Helper;
using FreshdeskIntegration.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FreshdeskIntegration.FreshdeskFactory;
using FreshdeskIntegration.MapperFactory;

namespace FreshdeskIntegration.CRMFactory
{
    class CrmRepository : ICrmRepository
    {
        private readonly IOrganizationService _service;
        private readonly FreshdeskRepository _freshdeskRepo;
        private readonly ContactMapper _contactMapper;

        public CrmRepository()
        {
            _service = CrmServiceFactory.GetOrganization();
            _freshdeskRepo = new FreshdeskRepository();
            _contactMapper = new ContactMapper();
        }

        public EntityCollection GetAccounts(int pageSize = 500, int pageNumber = 1, string pagingCookie = null)
        {
            var appSettings = ConfigurationManager.AppSettings;
            string lastSyncDateString = appSettings["AccountLastSyncDate"];
            DateTime lastSyncDate = new DateTime(2020, 4, 1);

            if (!string.IsNullOrEmpty(lastSyncDateString))
            {
                try
                {
                    lastSyncDate = DateTime.Parse(lastSyncDateString);
                }
                catch (Exception)
                {
                }
            }

            FilterExpression fe = new FilterExpression();
            ConditionExpression ce = new ConditionExpression("modifiedon", ConditionOperator.GreaterEqual, lastSyncDate);
            ConditionExpression ce1 = new ConditionExpression("isw_isobsoletedata", ConditionOperator.Equal, 0);
            ConditionExpression ce2 = new ConditionExpression("statecode", ConditionOperator.Equal, 0);
            fe.AddCondition(ce); fe.AddCondition(ce1); fe.AddCondition(ce2);

            string[] accountFields = new string[] { "name", "customertypecode", "industrycode", "accountnumber",
                "address1_composite", "accountid"};

            var accountCollection = CrmServiceFactory.RetrieveEntities("account", accountFields, fe, pageSize, pageNumber, pagingCookie);

            return accountCollection;
        }

        public EntityCollection GetContacts(int pageSize = 500, int pageNumber = 1, string pagingCookie = null)
        {
            var appSettings = ConfigurationManager.AppSettings;
            string lastSyncDateString = appSettings["ContactLastSyncDate"];
            DateTime lastSyncDate = new DateTime(2020, 4, 1);

            if (!string.IsNullOrEmpty(lastSyncDateString))
            {
                try
                {
                    lastSyncDate = DateTime.Parse(lastSyncDateString).AddHours(-1);
                }
                catch (Exception)
                {
                }
            }

            FilterExpression fe = new FilterExpression();
            ConditionExpression ce = new ConditionExpression("createdon", ConditionOperator.GreaterEqual, lastSyncDate);
            ConditionExpression ce1 = new ConditionExpression("fullname", ConditionOperator.NotNull);
            ConditionExpression ce2 = new ConditionExpression("statecode", ConditionOperator.Equal, 0);
            ConditionExpression ce3 = new ConditionExpression("isw_freshdeskid", ConditionOperator.Null);
            fe.AddCondition(ce); fe.AddCondition(ce1); fe.AddCondition(ce2); fe.AddCondition(ce3);

            string[] contactFields = new string[] { "address1_line1", "parentcustomerid", "address1_line1", "fullname",
                "telephone1", "telephone2", "emailaddress1", "jobtitle", "contactid"};

            var accountCollection = CrmServiceFactory.RetrieveEntities("contact", contactFields, fe, pageSize, pageNumber, pagingCookie);

            return accountCollection;
        }

        public bool UpsertAccount(Entity account)
        {
            RepositoryResult repositoryResult = CreateEntity(account);

            if (repositoryResult.Result)
                return true;
            else if (repositoryResult.hasDuplicate)
            {
                string phone = account.GetAttributeValue<string>("telephone1");
                string email = account.GetAttributeValue<string>("emailaddress1");
                string name = account.GetAttributeValue<string>("name");
                string freshdeskId = String.Empty;

                if(account.Contains("isw_freshdeskid"))
                    freshdeskId = account.GetAttributeValue<string>("isw_freshdeskid");

                FilterExpression feGroup = new FilterExpression(LogicalOperator.And);

                FilterExpression fe2 = new FilterExpression();
                ConditionExpression ce1 = new ConditionExpression("isw_isobsoletedata", ConditionOperator.Equal, 0);
                ConditionExpression ce2 = new ConditionExpression("statecode", ConditionOperator.Equal, 0);
                fe2.AddCondition(ce1); fe2.AddCondition(ce2);


                FilterExpression fe = new FilterExpression(LogicalOperator.Or);

                if (!string.IsNullOrEmpty(phone))
                {
                    ConditionExpression ce = new ConditionExpression("telephone1", ConditionOperator.Equal, phone);
                    fe.AddCondition(ce);
                }

                if (!string.IsNullOrEmpty(email))
                {
                    ConditionExpression ce3 = new ConditionExpression("emailaddress1", ConditionOperator.Equal, email);
                    fe.AddCondition(ce3);
                }

                if (!string.IsNullOrEmpty(name))
                {
                    ConditionExpression ce4 = new ConditionExpression("name", ConditionOperator.Equal, name);
                    fe.AddCondition(ce4);
                }

                if (!string.IsNullOrEmpty(freshdeskId))
                {
                    ConditionExpression ce5 = new ConditionExpression("isw_freshdeskid", ConditionOperator.Equal, freshdeskId);
                    fe.AddCondition(ce5);
                }

                feGroup.AddFilter(fe); feGroup.AddFilter(fe2);

                CheckExistenceResult existenceResult = CheckExistence("account", feGroup);

                if (existenceResult.Exists)
                {
                    account.Id = existenceResult.RecordId;

                    var r = UpdateEntity(account);

                    return r.Result;
                }

                return false;
                
            }

            return false;
        }

        public RepositoryResult UpsertContact(Entity contact)
        {
            RepositoryResult repositoryResult = CreateEntity(contact);

            if (repositoryResult.Result)
                return repositoryResult;
            else if (repositoryResult.hasDuplicate)
            {
                string phone = contact.GetAttributeValue<string>("telephone1");
                string email = contact.GetAttributeValue<string>("emailaddress1");
                //string name = contact.GetAttributeValue<string>("fullname");
                string freshdeskId = contact.GetAttributeValue<string>("isw_freshdeskid");

                FilterExpression fe = new FilterExpression(LogicalOperator.Or);

                if (!string.IsNullOrEmpty(phone))
                {
                    ConditionExpression ce = new ConditionExpression("telephone1", ConditionOperator.Equal, phone);
                    fe.AddCondition(ce);
                }

                if (!string.IsNullOrEmpty(email))
                {
                    ConditionExpression ce1 = new ConditionExpression("emailaddress1", ConditionOperator.Equal, email);
                    fe.AddCondition(ce1);
                }

                /*
                if (!string.IsNullOrEmpty(name))
                {
                    ConditionExpression ce2 = new ConditionExpression("fullname", ConditionOperator.Equal, name);
                    fe.AddCondition(ce2);
                }
                */

                if (!string.IsNullOrEmpty(freshdeskId))
                {
                    ConditionExpression ce3 = new ConditionExpression("isw_freshdeskid", ConditionOperator.Equal, freshdeskId);
                    fe.AddCondition(ce3);
                }

                CheckExistenceResult existenceResult = CheckExistence("contact", fe);

                if (existenceResult.Exists)
                {
                    contact.Id = existenceResult.RecordId;

                    var r = UpdateEntity(contact);

                    r.EntityId = existenceResult.RecordId;

                    return r;
                }

                return repositoryResult;

            }

            return repositoryResult;
        }

        public bool UpsertTicket(Entity ticket)
        {
            RepositoryResult repositoryResult = CreateEntity(ticket);

            if (repositoryResult.Result)
            {
                ResolveTicket(repositoryResult.EntityId);
                return true;
            }
                
            else if (repositoryResult.hasDuplicate)
            {                
                string freshdeskId = ticket.GetAttributeValue<string>("isw_freshdeskid");

                FilterExpression fe = new FilterExpression(LogicalOperator.Or);

                if (!string.IsNullOrEmpty(freshdeskId))
                {
                    ConditionExpression ce3 = new ConditionExpression("isw_freshdeskid", ConditionOperator.Equal, freshdeskId);
                    fe.AddCondition(ce3);
                }

                CheckExistenceResult existenceResult = CheckExistence("ticket", fe);

                if (existenceResult.Exists)
                {
                    ticket.Id = existenceResult.RecordId;

                    OpenTicket(ticket.Id);

                    var r = UpdateEntity(ticket);

                    ResolveTicket(ticket.Id);

                    return r.Result;
                }

                return false;

            }

            return false;
        }

        public Guid? CreateContact(long contactId)
        {
            Guid? contactGuid = null;
            FreshdeskContactDto freshdeskContactDto = _freshdeskRepo.GetContact(contactId);

            if(freshdeskContactDto != null)
            {
                Entity contactEntity = _contactMapper.CreateContact(freshdeskContactDto);

                if(contactEntity != null)
                {
                    var result = UpsertContact(contactEntity);

                    contactGuid = result.EntityId;
                }
            }

            return contactGuid;
        }
        public RepositoryResult CreateEntity(Entity entity)
        {
            CreateRequest req = new CreateRequest();
            req.Parameters.Add("SuppressDuplicateDetection", false);
            req.Target = entity;
            try
            {
                CreateResponse r = (CreateResponse)_service.Execute(req);
                return new RepositoryResult() { Result = true, hasDuplicate = false, EntityId = r.id };
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (ex.Detail.ErrorCode == -2147220685)
                {
                    // Duplicate record
                    return new RepositoryResult() { Result = false, hasDuplicate = true };
                }
                else
                {
                    return new RepositoryResult() { Result = false, hasDuplicate = false, ExceptionObj = ex };
                }
            }
        }

        public RepositoryResult UpdateEntity(Entity entity)
        {
            UpdateRequest req = new UpdateRequest();
            req.Parameters.Add("SuppressDuplicateDetection", false);
            req.Target = entity;
            try
            {
                UpdateResponse r = (UpdateResponse)_service.Execute(req);
                return new RepositoryResult() { Result = true, hasDuplicate = false};
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (ex.Detail.ErrorCode == -2147220685)
                {
                    // Duplicate record
                    return new RepositoryResult() { Result = false, hasDuplicate = true };
                }
                else
                {
                    return new RepositoryResult() { Result = false, hasDuplicate = false, ExceptionObj = ex };
                }
            }
        }

        public CheckExistenceResult CheckExistence(string entity, FilterExpression fe)
        {
            EntityCollection ec = CrmServiceFactory.RetrieveEntities(entity, new string[] { entity + "id" }, fe, 1);

            if (ec.Entities.Count > 0)
            {
                return new CheckExistenceResult(true, ec.Entities[0].Id);
            }
            else
            {
                return new CheckExistenceResult(false, new Guid());
            }
        }

        public bool ResolveTicket(Guid? ticketGuid)
        {
            Entity IncidentResolution = new Entity("incidentresolution");
            IncidentResolution.Attributes["subject"] = "Subject Closed";
            IncidentResolution.Attributes["incidentid"] = new EntityReference("incident", ticketGuid.Value);
            // Create the request to close the incident, and set its resolution to the
            // resolution created above
            CloseIncidentRequest closeRequest = new CloseIncidentRequest();
            closeRequest.IncidentResolution = IncidentResolution;
            // Set the requested new status for the closed Incident
            closeRequest.Status = new OptionSetValue(5);
            // Execute the close request

            try
            {
                CloseIncidentResponse closeResponse = (CloseIncidentResponse)_service.Execute(closeRequest);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool OpenTicket(Guid ticketGuid)
        {
            SetStateRequest request = new SetStateRequest();
            request.EntityMoniker = new EntityReference("incident", ticketGuid);
            request.State = new OptionSetValue(0);
            request.Status = new OptionSetValue(1);

            try
            {
                SetStateResponse objResponse = (SetStateResponse)_service.Execute(request);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        public bool UpdateAgent(AgentDto agentDto)
        {
            Entity agentEntity = new Entity("systemuser");

            string[] columns = { "systemuserid" };
            ConditionExpression ce = new ConditionExpression("internalemailaddress",
                ConditionOperator.Equal, agentDto.Contact.Email);

            FilterExpression fe = new FilterExpression();
            fe.AddCondition(ce);

            var ec = CrmServiceFactory.RetrieveEntities("systemuser", columns, fe, 1);

            if (ec.Entities.Count > 0)
            {
                agentEntity.Id = ec.Entities[0].Id;
                agentEntity["isw_freshdeskid"] = agentDto.Id.ToString();

                var a = UpdateEntity(agentEntity);

                return a.Result;
            }

            return false;
        }
    }
}
