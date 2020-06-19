using FreshdeskIntegration.Helper;
using FreshdeskIntegration.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshdeskIntegration.CRMFactory
{
    public interface ICrmRepository
    {
        EntityCollection GetContacts(int pageSize, int pageNumber, string pagingCookie);
        EntityCollection GetAccounts(int pageSize, int pageNumber, string pagingCookie);
        RepositoryResult UpsertContact(Entity contact);
        bool UpsertAccount(Entity account);
        bool UpsertTicket(Entity Ticket);
        RepositoryResult CreateEntity(Entity entity);
        RepositoryResult UpdateEntity(Entity entity);
        CheckExistenceResult CheckExistence(string entity, FilterExpression fe);
        bool UpdateAgent(AgentDto entity);
    }
}
