using FreshdeskIntegration.CRMFactory;
using FreshdeskIntegration.FreshdeskFactory;
using FreshdeskIntegration.MapperFactory;
using FreshdeskIntegration.Models;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshdeskIntegration
{
    class Program
    {
        private static FreshdeskRepository freshdesk;
        private static AccountMapper accountMapper;
        private static ContactMapper contactMapper;
        private static TicketMapper ticketMapper;
        private static CrmRepository crm;

        static void Main(string[] args)
        {
            freshdesk = new FreshdeskRepository();
            accountMapper = new AccountMapper();
            contactMapper = new ContactMapper();
            ticketMapper = new TicketMapper();
            crm = new CrmRepository();

            string dayOfWeek = DateTime.Now.DayOfWeek.ToString();

            if(dayOfWeek == "Saturday")
                AgentSync();

            AccountSync();
            ContactSync();
            TicketSync();
        }

        static void AgentSync()
        {
            IEnumerable<AgentDto> agents = freshdesk.GetAgents();

            foreach (var agent in agents)
            {
                try
                {
                    crm.UpdateAgent(agent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }

            UpdateAppsettings("AgentLastSyncDate", 3);
        }

        static void AccountSync()
        {
            int page = 1;
            var crmAccounts = crm.GetAccounts(500, page);
            
            void Execute()
            {
                foreach (var account in crmAccounts.Entities)
                {
                    try
                    {
                        FreshdeskAccountForCreation freshdeskAccount = accountMapper.CreateAccount(account);

                        if (account.Contains("isw_freshdeskid"))
                        {
                            long freshdeskId = long.Parse(account["isw_freshdeskid"].ToString());
                            freshdesk.UpdateAccount(freshdeskId, freshdeskAccount);
                        }
                        else
                        {
                            FreshdeskAccountDto createdFreshdeskAccount = freshdesk.CreateAccount(freshdeskAccount);

                            if (createdFreshdeskAccount != null)
                            {
                                Entity accountEntity = accountMapper.CreateAccount(createdFreshdeskAccount);

                                crm.UpsertAccount(accountEntity);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }                    
                }
            }

            do
            {
                Execute();
                page++;

                if (crmAccounts.MoreRecords)
                    crmAccounts = crm.GetAccounts(500, page, crmAccounts.PagingCookie);
                else
                    break;
            } while (crmAccounts.MoreRecords);

            UpdateAppsettings("AccountLastSyncDate", 2);
        }

        static void ContactSync()
        {
            DateTime? lastSyncRecord = null;

            void ExecuteFreshdesk()
            {
                try
                {
                    IEnumerable<FreshdeskContactDto> contactDtos = freshdesk.GetContacts();

                    if (contactDtos.Any())
                    {
                        lastSyncRecord = contactDtos.OrderByDescending(t => t.UpdatedAt).First().UpdatedAt.DateTime;
                    }

                    foreach (FreshdeskContactDto contact in contactDtos)
                    {
                        try
                        {
                            Entity contactEntity = contactMapper.CreateContact(contact);

                            crm.UpsertContact(contactEntity);
                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine(ex.Message);
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
               
            }

            ExecuteFreshdesk();

            /*

            int page = 1;
            var crmContacts = crm.GetContacts(500, page);

            void ExecuteCrm()
            {
                foreach (var contact in crmContacts.Entities)
                {
                    try
                    {
                        FreshdeskContactForCreation freshdeskContact = contactMapper.CreateContact(contact);


                        if (contact.Contains("isw_freshdeskid"))
                        {
                            long freshdeskId = long.Parse(contact["isw_freshdeskid"].ToString());
                            freshdesk.UpdateContact(freshdeskId, freshdeskContact);
                        }

                        FreshdeskContactDto createdFreshdeskContact = freshdesk.CreateContact(freshdeskContact);

                        Entity contactEntity = contactMapper.CreateContact(createdFreshdeskContact);

                        crm.UpsertContact(contactEntity);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            do
            {
                ExecuteCrm();
                page++;

                if (crmContacts.MoreRecords)
                    crmContacts = crm.GetAccounts(500, page, crmContacts.PagingCookie);
                else
                    break;
            } while (crmContacts.MoreRecords);

            */

            UpdateAppsettings("ContactLastSyncDate", 30, lastSyncRecord);
        }

        static void TicketSync()
        {
            DateTime? lastSyncRecord = null;

            void ExecuteFreshdesk()
            {
                try
                {
                    List<FreshdeskTicket> freshdeskTickets = freshdesk.GetTickets().ToList();
                    

                    if (freshdeskTickets.Any())
                    {
                        lastSyncRecord = freshdeskTickets.OrderByDescending(t => t.UpdatedAt).First().UpdatedAt.DateTime;
                    }

                    foreach (FreshdeskTicket ticket in freshdeskTickets)
                    {
                        try
                        {
                            Entity ticketEntity = ticketMapper.CreateTicket(ticket);

                            if(ticketEntity != null)
                                crm.UpsertTicket(ticketEntity);
                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine(ex.Message);
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

            ExecuteFreshdesk();

            UpdateAppsettings("TicketLastSyncDate", 30, lastSyncRecord);
        }

        static void UpdateAppsettings(string key, int timeDifference, DateTime? lastSync = null)
        {
            if(lastSync == null)
                lastSync = DateTime.Now.AddMinutes(-timeDifference);

            string lastSyncString = lastSync.Value.ToUniversalTime()
                         .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

            //if(key == "AccountLastSyncDate")
            //{
            //    lastSyncString = lastSync.ToUniversalTime()
            //             .ToString("yyyy'-'MM'-'dd");
            //}

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[key].Value = lastSyncString;
            config.Save(ConfigurationSaveMode.Modified);
            
            ConfigurationManager.RefreshSection("appSettings");
        }

    }
}
