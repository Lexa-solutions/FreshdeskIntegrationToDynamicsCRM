using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FreshdeskIntegration.Models;
using Newtonsoft.Json;

namespace FreshdeskIntegration.FreshdeskFactory
{
    class FreshdeskRepository : IFreshdeskRepository
    {
        private string accountlastSyncDateString;
        private string contactLastSyncDate;
        private string ticketLastSyncDate;
        private string agentLastSyncDate;
        private HttpClient _freshdeskClient;
        public FreshdeskRepository()
        {
            var appSettings = ConfigurationManager.AppSettings;
            accountlastSyncDateString = appSettings["AccountLastSyncDate"];
            accountlastSyncDateString = DateTime.Parse(accountlastSyncDateString)
                .ToString("yyyy'-'MM'-'dd");
            contactLastSyncDate = appSettings["ContactLastSyncDate"];
            ticketLastSyncDate = appSettings["TicketLastSyncDate"];
            agentLastSyncDate = appSettings["AgentLastSyncDate"];
            _freshdeskClient = FreshdeskHttpClient.GetClient();
        }

        public FreshdeskAccountDto CreateAccount(FreshdeskAccountForCreation freshdeskAccountForCreation)
        {
            FreshdeskAccountDto freshdeskAccountDto = null;

            string responseBody = String.Empty;
            HttpWebRequest request = FreshdeskHttpClient.GetRequestMessage("POST",
                $"/api/v2/companies");

            string data = JsonConvert.SerializeObject(freshdeskAccountForCreation);

            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            // Set the ContentLength property of the WebRequest. 
            request.ContentLength = byteArray.Length;

            responseBody = WriteRequest(request, byteArray);

            freshdeskAccountDto = JsonConvert.DeserializeObject<FreshdeskAccountDto>(responseBody);

            return freshdeskAccountDto;
        }

        public FreshdeskContactDto CreateContact(FreshdeskContactForCreation freshdeskContactForCreation)
        {
            FreshdeskContactDto freshdeskContactDto = null;

            string responseBody = String.Empty;
            HttpWebRequest request = FreshdeskHttpClient.GetRequestMessage("POST",
                $"/api/v2/contacts");

            string data = JsonConvert.SerializeObject(freshdeskContactForCreation);

            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            // Set the ContentLength property of the WebRequest. 
            request.ContentLength = byteArray.Length;

            responseBody = WriteRequest(request, byteArray);

            freshdeskContactDto = JsonConvert.DeserializeObject<FreshdeskContactDto>(responseBody);

            return freshdeskContactDto;
        }

        public async Task<IEnumerable<AgentDto>> GetAgents2()
        {
            IEnumerable<AgentDto> agentDtos = new List<AgentDto>();

            HttpRequestMessage request = FreshdeskHttpClient.GetRequestMessage(HttpMethod.Get, 
                $"/api/v2/agents?per_page=100");

            try
            {
                var response = await _freshdeskClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    try
                    {
                        IEnumerable<AgentDto> approvalSummaryJira = JsonConvert.DeserializeObject<IEnumerable<AgentDto>>(content);
                        return approvalSummaryJira;
                    }
                    catch (Exception)
                    {
                        //Debug.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }            

            return agentDtos;
        }

        public IEnumerable<AgentDto> GetAgents()
        {
            string responseBody = String.Empty;
            List<AgentDto> agents = new List<AgentDto>();

            int page = 1;

            bool Execute()
            {
                HttpWebRequest request = FreshdeskHttpClient.GetRequestMessage("GET",
                $"/api/v2/agents?per_page=100&page={page}");

                try
                {
                    responseBody = GetRequest(request);

                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        var agentss = JsonConvert.DeserializeObject<IEnumerable<AgentDto>>(responseBody);

                        if (agentss.ToList().Count > 0)
                        {
                            agents.AddRange(agentss);
                            return true;
                        }

                        else
                            return false;

                    }
                    else
                    {
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            for (int i = 0; i < 10000000; i++)
            {
                if (!Execute())
                    break;

                page++;
            }

            return agents;
        }

        public FreshdeskAccounts GetCompanies()
        {
            string responseBody = String.Empty;
            FreshdeskAccounts accounts = new FreshdeskAccounts();
            FreshdeskAccounts accountsCollection = new FreshdeskAccounts();

            int page = 1;
            string dateFilter = $"\"created_at:>%27{accountlastSyncDateString}%27\"";

            void Execute()
            {
                HttpWebRequest request = FreshdeskHttpClient.GetRequestMessage("GET",
                $"/api/v2/search/companies?page={page}&query={dateFilter}");

                responseBody = GetRequest(request);

                if (!string.IsNullOrEmpty(responseBody))
                {
                    if(page == 1)
                    {
                        accountsCollection = JsonConvert.DeserializeObject<FreshdeskAccounts>(responseBody);
                    }
                    else
                    {
                        accounts = JsonConvert.DeserializeObject<FreshdeskAccounts>(responseBody);

                        if(accounts.Results.Count > 0)
                            accountsCollection.Results.AddRange(accounts.Results);
                    }
                }
            }

            try
            {
                Execute();
                decimal noOfPages = Math.Ceiling(((decimal)accountsCollection.Total / (decimal)accountsCollection.Results.Count));

                for (int i = 1; i < noOfPages; i++)
                {
                    ++page;

                    Execute();
                }

                return accountsCollection;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return accountsCollection;
        }

        public IEnumerable<FreshdeskContactDto> GetContacts()
        {
            string responseBody = String.Empty;
            List<FreshdeskContactDto> contacts = new List<FreshdeskContactDto>();

            int page = 1;

            bool Execute()
            {
                HttpWebRequest request = FreshdeskHttpClient.GetRequestMessage("GET",
                $"/api/v2/contacts?per_page=100&page={page}&_updated_since={contactLastSyncDate}");

                try
                {
                    responseBody = GetRequest(request);

                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        var agentss = JsonConvert.DeserializeObject<IEnumerable<FreshdeskContactDto>>(responseBody);

                        if (agentss.ToList().Count > 0)
                        {
                            contacts.AddRange(agentss);
                            return true;
                        }

                        else
                            return false;

                    }
                    else
                    {
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            for (int i = 0; i < 10000000; i++)
            {                
                if (!Execute())
                {
                    break;
                }                    

                page++;
            }

            return contacts;
        }

        public FreshdeskContactDto GetContact(long contactId)
        {
            string responseBody = String.Empty;
            FreshdeskContactDto contact = null;


            HttpWebRequest request = FreshdeskHttpClient.GetRequestMessage("GET",
            $"/api/v2/contacts/{contactId}");

            try
            {
                responseBody = GetRequest(request);

                if (!string.IsNullOrEmpty(responseBody))
                {
                    contact = JsonConvert.DeserializeObject<FreshdeskContactDto>(responseBody);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return contact;
        }

        public IEnumerable<FreshdeskTicket> GetTickets()
        {
            string responseBody = String.Empty;
            List<FreshdeskTicket> tickets = new List<FreshdeskTicket>();

            int page = 1;

            bool Execute()
            {
                HttpWebRequest request = FreshdeskHttpClient.GetRequestMessage("GET",
                $"/api/v2/tickets?updated_since={ticketLastSyncDate}&include=stats,description&per_page=100&page={page}&order_by=updated_at&order_type=asc");

                try
                {
                    responseBody = GetRequest(request);

                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        var agentss = JsonConvert.DeserializeObject<IEnumerable<FreshdeskTicket>>(responseBody);

                        if (agentss.ToList().Count > 0)
                        {
                            tickets.AddRange(agentss);
                            return true;
                        }

                        else
                            return false;

                    }
                    else
                    {
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            for (int i = 0; i < 10000000; i++)
            {
                if (!Execute())
                    break;

                page++;
            }

            return from r in tickets
                   where r.Status == 4 || r.Status == 5
                   select r;
        }

        public FreshdeskAccountDto UpdateAccount(long accountId, FreshdeskAccountForCreation freshdeskAccountForCreation)
        {
            FreshdeskAccountDto freshdeskAccountDto = null;

            string responseBody = String.Empty;
            HttpWebRequest request = FreshdeskHttpClient.GetRequestMessage("PUT",
                $"/api/v2/companies/{accountId}");

            string data = JsonConvert.SerializeObject(freshdeskAccountForCreation);

            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            // Set the ContentLength property of the WebRequest. 
            request.ContentLength = byteArray.Length;

            responseBody = WriteRequest(request, byteArray);

            freshdeskAccountDto = JsonConvert.DeserializeObject<FreshdeskAccountDto>(responseBody);

            return freshdeskAccountDto;
        }

        public FreshdeskContactDto UpdateContact(long contactId, FreshdeskContactForCreation freshdeskContactForCreation)
        {
            FreshdeskContactDto freshdeskContactDto = null;

            string responseBody = String.Empty;
            HttpWebRequest request = FreshdeskHttpClient.GetRequestMessage("POST",
                $"/api/v2/contacts/{contactId}");

            string data = JsonConvert.SerializeObject(freshdeskContactForCreation);

            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            // Set the ContentLength property of the WebRequest. 
            request.ContentLength = byteArray.Length;

            responseBody = WriteRequest(request, byteArray);

            freshdeskContactDto = JsonConvert.DeserializeObject<FreshdeskContactDto>(responseBody);

            return freshdeskContactDto;
        }

        public string GetRequest(HttpWebRequest request)
        {
            string responseBody = String.Empty;
            try
            {
                Console.WriteLine("Submitting Request");
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    responseBody = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                    //return status code
                    Console.WriteLine("Status Code: {1} {0}", ((HttpWebResponse)response).StatusCode, (int)((HttpWebResponse)response).StatusCode);

                    int rateRemaining = int.Parse(response.Headers["x-ratelimit-remaining"]);

                    if(rateRemaining < 50)
                    {
                        System.Threading.Thread.Sleep(new TimeSpan(0, 1, 10));
                    }                  

                    return responseBody;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("API Error: Your request is not successful. If you are not able to debug this error properly, mail us at support@freshdesk.com with the follwing X-Request-Id");
                Console.WriteLine("X-Request-Id: {0}", ex.Response.Headers["X-Request-Id"]);
                Console.WriteLine("Error Status Code : {1} {0}", ((HttpWebResponse)ex.Response).StatusCode, (int)((HttpWebResponse)ex.Response).StatusCode);
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.Write("Error Response: ");
                    Console.WriteLine(reader.ReadToEnd());
                }

                try
                {
                    int retryAfter = int.Parse(ex.Response.Headers["retry-after"]);

                    if (retryAfter > 0)
                    {
                        System.Threading.Thread.Sleep((retryAfter + 10) * 1000);
                        GetRequest(request);
                    }
                }
                catch (Exception)
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(ex.Message);
            }

            return responseBody;
        }

        public string WriteRequest(HttpWebRequest request, byte[] data)
        {
            string responseBody = String.Empty;
            //Get the stream that holds request data by calling the GetRequestStream method. 
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream. 
            dataStream.Write(data, 0, data.Length);
            // Close the Stream object. 
            dataStream.Close();
            try
            {
                Console.WriteLine("Submitting Request");
                WebResponse response = request.GetResponse();
                // Get the stream containing content returned by the server.
                //Send the request to the server by calling GetResponse. 
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access. 
                StreamReader reader = new StreamReader(dataStream);
                // Read the content. 
                responseBody = reader.ReadToEnd();
                //return status code
                Console.WriteLine("Status Code: {1} {0}", ((HttpWebResponse)response).StatusCode, (int)((HttpWebResponse)response).StatusCode);
                //return location header
                Console.WriteLine("Location: {0}", response.Headers["Location"]);
                //return the response 
                Console.Out.WriteLine(responseBody);

                int rateRemaining = int.Parse(response.Headers["x-ratelimit-remaining"]);

                if (rateRemaining < 50)
                {
                    System.Threading.Thread.Sleep(new TimeSpan(0, 1, 10));
                }

                return responseBody;
            }
            catch (WebException ex)
            {
                Console.WriteLine("API Error: Your request is not successful. If you are not able to debug this error properly, mail us at support@freshdesk.com with the follwing X-Request-Id");
                Console.WriteLine("X-Request-Id: {0}", ex.Response.Headers["X-Request-Id"]);
                Console.WriteLine("Error Status Code : {1} {0}", ((HttpWebResponse)ex.Response).StatusCode, (int)((HttpWebResponse)ex.Response).StatusCode);
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.Write("Error Response: ");
                    Console.WriteLine(reader.ReadToEnd());
                }

                try
                {
                    int retryAfter = int.Parse(ex.Response.Headers["retry-after"]);

                    if (retryAfter > 0)
                    {
                        System.Threading.Thread.Sleep((retryAfter + 10) * 1000);
                        WriteRequest(request, data);
                    }
                }
                catch (Exception)
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(ex.Message);
            }

            return responseBody;
        }
    }
}
