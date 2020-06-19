using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FreshdeskIntegration.Models
{
    public partial class FreshdeskTicket
    {
        [JsonProperty("cc_emails")]
        public string[] CcEmails { get; set; }

        [JsonProperty("fwd_emails")]
        public string[] FwdEmails { get; set; }

        [JsonProperty("reply_cc_emails")]
        public string[] ReplyCcEmails { get; set; }

        [JsonProperty("ticket_cc_emails")]
        public string[] TicketCcEmails { get; set; }

        [JsonProperty("fr_escalated")]
        public bool? FrEscalated { get; set; }

        [JsonProperty("spam")]
        public bool Spam { get; set; }

        [JsonProperty("email_config_id")]
        public long? EmailConfigId { get; set; }

        [JsonProperty("group_id")]
        public long? GroupId { get; set; }

        [JsonProperty("priority")]
        public long Priority { get; set; }

        [JsonProperty("requester_id")]
        public long RequesterId { get; set; }

        [JsonProperty("responder_id")]
        public long? ResponderId { get; set; }

        [JsonProperty("source")]
        public long Source { get; set; }

        [JsonProperty("company_id")]
        public long? CompanyId { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("association_type")]
        public long? AssociationType { get; set; }

        [JsonProperty("to_emails")]
        public string[] ToEmails { get; set; }

        [JsonProperty("product_id")]
        public long? ProductId { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("due_by")]
        public DateTimeOffset DueBy { get; set; }

        [JsonProperty("fr_due_by")]
        public DateTimeOffset FrDueBy { get; set; }

        [JsonProperty("is_escalated")]
        public bool IsEscalated { get; set; }

        [JsonProperty("custom_fields")]
        public CustomFields CustomFields { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("associated_tickets_count")]
        public object AssociatedTicketsCount { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("stats")]
        public Stats Stats { get; set; }

        [JsonProperty("internal_agent_id")]
        public object InternalAgentId { get; set; }

        [JsonProperty("internal_group_id")]
        public object InternalGroupId { get; set; }

        [JsonProperty("nr_due_by")]
        public object NrDueBy { get; set; }

        [JsonProperty("nr_escalated")]
        public bool? NrEscalated { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("description_text")]
        public string DescriptionText { get; set; }
    }

    public partial class CustomFields
    {
        [JsonProperty("cf_issue_category")]
        public string CfIssueCategory { get; set; }

        [JsonProperty("cf_issue_sub_category")]
        public string CfIssueSubCategory { get; set; }

        [JsonProperty("cf_fsm_contact_name")]
        public string CfFsmContactName { get; set; }

        [JsonProperty("cf_fsm_phone_number")]
        public object CfFsmPhoneNumber { get; set; }

        [JsonProperty("cf_fsm_service_location")]
        public object CfFsmServiceLocation { get; set; }

        [JsonProperty("cf_fsm_appointment_start_time")]
        public object CfFsmAppointmentStartTime { get; set; }

        [JsonProperty("cf_fsm_appointment_end_time")]
        public object CfFsmAppointmentEndTime { get; set; }

        [JsonProperty("cf_escalate")]
        public bool? CfEscalate { get; set; }
    }

    public partial class Stats
    {
        [JsonProperty("agent_responded_at")]
        public DateTimeOffset? AgentRespondedAt { get; set; }

        [JsonProperty("requester_responded_at")]
        public DateTimeOffset? RequesterRespondedAt { get; set; }

        [JsonProperty("first_responded_at")]
        public DateTimeOffset? FirstRespondedAt { get; set; }

        [JsonProperty("status_updated_at")]
        public DateTimeOffset StatusUpdatedAt { get; set; }

        [JsonProperty("reopened_at")]
        public DateTimeOffset? ReopenedAt { get; set; }

        [JsonProperty("resolved_at")]
        public DateTimeOffset? ResolvedAt { get; set; }

        [JsonProperty("closed_at")]
        public DateTimeOffset? ClosedAt { get; set; }

        [JsonProperty("pending_since")]
        public DateTimeOffset? PendingSince { get; set; }
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
