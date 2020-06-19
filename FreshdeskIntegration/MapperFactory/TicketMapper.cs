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
    class TicketMapper
    {
        public IOrganizationService service;
        private CrmRepository _crmRepository;

        public int[] TicketPriority { get; set; } = new int[] { 0, 3, 2, 1, 4 };
        public int[] TicketSource { get; set; } = new int[] { 0, 2, 3, 1, 1, 3986, 2483, 1, 3, 3, 2, 3, 3, 3, 3 };
        public TicketMapper()
        {
            service = CrmServiceFactory.GetOrganization();
            _crmRepository = new CrmRepository();
        }

        public Entity CreateTicket(FreshdeskTicket ticket)
        {
            Entity ticketEntity = new Entity("incident");

            //requesterId => customerid
            try
            {
                string[] columns = { "contactid" };
                ConditionExpression ce = new ConditionExpression("isw_freshdeskid", ConditionOperator.Equal, ticket.RequesterId.ToString());

                FilterExpression fe = new FilterExpression();
                fe.AddCondition(ce);

                var ec = CrmServiceFactory.RetrieveEntities("contact", columns, fe, 1);

                if (ec.Entities.Count > 0)
                {
                    ticketEntity.Attributes["customerid"] = new EntityReference(ec.Entities[0].LogicalName, ec.Entities[0].Id);
                }
                else
                {
                    var r = _crmRepository.CreateContact(ticket.RequesterId);
                    if(r != null)
                    {
                        ticketEntity.Attributes["customerid"] = new EntityReference("contact", r.Value);
                    }
                    else
                    {
                        return null;
                    }
                    
                }

            }
            catch (Exception)
            {
                return null;
            }

            //responder => ownerid
            try
            {
                string[] columns = { "systemuserid" };
                ConditionExpression ce = new ConditionExpression("isw_freshdeskid", ConditionOperator.Equal, ticket.ResponderId.ToString());

                FilterExpression fe = new FilterExpression();
                fe.AddCondition(ce);

                var ec = CrmServiceFactory.RetrieveEntities("systemuser", columns, fe, 1);

                if (ec.Entities.Count > 0)
                {
                    ticketEntity.Attributes["ownerid"] = new EntityReference(ec.Entities[0].LogicalName, ec.Entities[0].Id);
                    ticketEntity.Attributes["isw_resolvedby"] = new EntityReference(ec.Entities[0].LogicalName, ec.Entities[0].Id);
                }
            }
            catch (Exception)
            {}

            if (!string.IsNullOrEmpty(ticket.Type))
            {
                try
                {
                    string[] columns = { "isw_issueid" };
                    ConditionExpression ce = new ConditionExpression("isw_name", ConditionOperator.Equal, ticket.Type);

                    FilterExpression fe = new FilterExpression();
                    fe.AddCondition(ce);

                    var ec = CrmServiceFactory.RetrieveEntities("isw_issue", columns, fe, 1);

                    if (ec.Entities.Count > 0)
                    {
                        ticketEntity.Attributes["isw_issue"] = new EntityReference(ec.Entities[0].LogicalName, ec.Entities[0].Id);
                    }
                }
                catch (Exception)
                {
                }
            }

            if (!string.IsNullOrEmpty(ticket.CustomFields.CfIssueSubCategory))
            {
                try
                {
                    string[] columns = { "isw_issuesubcategoryid" };
                    ConditionExpression ce = new ConditionExpression("isw_name", ConditionOperator.Equal, ticket.CustomFields.CfIssueSubCategory);

                    FilterExpression fe = new FilterExpression();
                    fe.AddCondition(ce);

                    var ec = CrmServiceFactory.RetrieveEntities("isw_issuesubcategory", columns, fe, 1);

                    if (ec.Entities.Count > 0)
                    {
                        ticketEntity.Attributes["isw_issuesubcategory"] = new EntityReference(ec.Entities[0].LogicalName, 
                            ec.Entities[0].Id);
                    }
                }
                catch (Exception)
                {
                }
            }

            if (!string.IsNullOrEmpty(ticket.CustomFields.CfIssueCategory))
            {
                try
                {
                    string[] columns = { "isw_issuecategoryid" };
                    ConditionExpression ce = new ConditionExpression("isw_name", ConditionOperator.Equal, 
                        ticket.CustomFields.CfIssueCategory);

                    FilterExpression fe = new FilterExpression();
                    fe.AddCondition(ce);

                    var ec = CrmServiceFactory.RetrieveEntities("isw_issuecategory", columns, fe, 1);

                    if (ec.Entities.Count > 0)
                    {
                        ticketEntity.Attributes["isw_issuecategory"] = new EntityReference(ec.Entities[0].LogicalName,
                            ec.Entities[0].Id);
                    }
                }
                catch (Exception)
                {
                }
            }

            try
            {
                ticketEntity["prioritycode"] = new OptionSetValue(TicketPriority[ticket.Priority]);
            }
            catch (Exception)
            {}

            try
            {
                ticketEntity["caseorigincode"] = new OptionSetValue(TicketSource[ticket.Source]);
            }
            catch (Exception)
            { }

            if (!string.IsNullOrEmpty(ticket.Type))
                ticketEntity["title"] = ticket.Type;

            ticketEntity["isw_freshdeskid"] = ticket.Id.ToString();

            if (!string.IsNullOrEmpty(ticket.DescriptionText))
                ticketEntity["description"] = ticket.Subject;

            if(ticket.Stats.FirstRespondedAt != null)
            {
                ticketEntity["isw_firstresponseforautocreatedcases"] = ticket.Stats.FirstRespondedAt.Value.DateTime;
            }

            if (ticket.Stats.FirstRespondedAt != null)
            {
                ticketEntity["isw_firstresponseforautocreatedcases"] = ticket.Stats.FirstRespondedAt.Value.DateTime;
            }

            if (ticket.Stats.ResolvedAt != null)
            {
                ticketEntity["isw_actualresolvedate"] = ticket.Stats.ResolvedAt.Value.DateTime;
            }

            if (ticket.Stats.ReopenedAt != null)
            {
                ticketEntity["isw_reactivateddate"] = ticket.Stats.ReopenedAt.Value.DateTime;
            }            

            return ticketEntity;
        }
    }
}
