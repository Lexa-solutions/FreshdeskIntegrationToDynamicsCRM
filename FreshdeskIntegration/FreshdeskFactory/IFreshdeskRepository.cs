using FreshdeskIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshdeskIntegration.FreshdeskFactory
{
    interface IFreshdeskRepository
    {
        IEnumerable<AgentDto> GetAgents();
        FreshdeskAccounts GetCompanies();
        FreshdeskAccountDto CreateAccount(FreshdeskAccountForCreation freshdeskAccountForCreation);
        FreshdeskAccountDto UpdateAccount(long accountId, FreshdeskAccountForCreation freshdeskAccountForCreation);
        IEnumerable<FreshdeskContactDto> GetContacts();
        FreshdeskContactDto CreateContact(FreshdeskContactForCreation freshdeskAccountForCreation);
        FreshdeskContactDto UpdateContact(long contactId, FreshdeskContactForCreation freshdeskAccountForCreation);
        IEnumerable<FreshdeskTicket> GetTickets();
    }
}
