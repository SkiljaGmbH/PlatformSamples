using System;
using System.Collections.Generic;
using System.Net.Mail;
using SampleActivity.Settings;
using STG.Common.DTO;
using STG.Common.Interfaces;
using STG.RT.API.Activity;
using STG.RT.API.Interfaces;

namespace SampleActivity
{
    public class SampleOverdueNotifier : STGSystemAgentActivityAbstract<OverdueAgentSettings>
    {
        public override void Process(ISTGProcess processLayer, ISTGConfiguration configurationLayer)
        {
            var numberOfOverdue = FindOverdueWorkItems(processLayer);
            if (numberOfOverdue > 0)
            {
                //if we have work items that are overdue, send e-mail based on configuration
                SendMail(numberOfOverdue);
            }
        }

        private int FindOverdueWorkItems(ISTGProcess processLayer)
        {
            var ret = 0;
            //search for work items that are overdue or in warning based on settings
            var searchQ = new DtoWorkItemComplexSelectQuery();

            //limit processes to current process
            searchQ.ProcessesIds = new List<int>
            {
                ActivityInfo.Process.ProcessID
            };

            //do not track deleted and Done work items
            searchQ.IncludeDeleted = false;
            searchQ.IncludeDone = false;
            searchQ.Order.Add(new DtoOrderBy() { OrderBy= "TimeToEnd" });

            //Filter based on SLAStatus based on settings
            searchQ.FilterData = new List<DtoFilterExpression<DtoWorkItemData>>();
            var slaFilter = new DtoFilterExpression<DtoWorkItemData>();
            slaFilter.FilterOperator = Operator.Or;
            slaFilter.Filters.Add(
                new DtoFilterCondition<DtoWorkItemData>
                {
                    Field = "SLAStatus",
                    Operator = SearchOperator.Equal,
                    Value = "Overdue"
                }
            );

            //Add Work items in warning if configured 
            if (ActivityConfiguration.IncludeCloseTo)
            {
                slaFilter.Filters.Add(new DtoFilterCondition<DtoWorkItemData>()
                {
                    Field = "SLAStatus",
                    Operator = SearchOperator.Equal,
                    Value = "Warning"
                });
            }

            searchQ.FilterData.Add(slaFilter);

            //Calculate from when work items must be overdue in order to count them
            var targetDate = DateTime.Now.AddDays(-1 * this.ActivityConfiguration.DaysOverdue);
            //Filter based on start date on either Warn or Expiration date based on settings
            var tteFilter = new DtoFilterExpression<DtoWorkItemData>();
            tteFilter.FilterOperator = Operator.And;
            tteFilter.Filters.Add(new DtoFilterCondition<DtoWorkItemData>()
            {
                Field = ActivityConfiguration.IncludeCloseTo ? "WarnDate" : "ExpirationDate",
                Operator = SearchOperator.Greater | SearchOperator.Equal,
                Value = targetDate
            });
            searchQ.FilterData.Add(tteFilter);

            //Get work items based on defined criteria
            var result = processLayer.GetWorkItemsFilteredOrderedPaged(searchQ);

            //if we have some work items that matches the criteria return the number
            if (result != null && result.Total > 0)
            {
                ret = result.Total;
            }

            return ret;
        }

        private void SendMail(int numberOfOverdueItems)
        {
            try
            {
                //initialize e-mail client based on settings
                var client = new SmtpClient(this.ActivityConfiguration.ServerSettings.ServerAddress);
                client.Port = this.ActivityConfiguration.ServerSettings.ServerPort;
                client.Credentials = new System.Net.NetworkCredential(this.ActivityConfiguration.ServerSettings.UserName, this.ActivityConfiguration.ServerSettings.Password);
                client.EnableSsl = this.ActivityConfiguration.ServerSettings.UseSSL;

                //create e-mail
                var mail = new MailMessage();
                mail.From = new MailAddress(this.ActivityConfiguration.ServerSettings.From == null ? this.ActivityConfiguration.ServerSettings.UserName : this.ActivityConfiguration.ServerSettings.From, "Overdue Agent Notifier");
                mail.To.Add(new MailAddress(this.ActivityConfiguration.SendToMail, this.ActivityConfiguration.SendTo));
                mail.Subject = "Work Items Overdue";
                mail.Body = $"Dear {this.ActivityConfiguration.SendTo}. " +
                    $"{Environment.NewLine} We have detected that process '{this.ActivityInfo.Process.ProcessName}' have {numberOfOverdueItems} work items in " +
                    $"{(this.ActivityConfiguration.IncludeCloseTo ? " overdue or warning " : " overdue ")} for more than {this.ActivityConfiguration.DaysOverdue} days. " +
                    $"{Environment.NewLine} Kind regards.";

                //send e-mail
                client.Send(mail);
                Log.Debug("E-mail notification sent");
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to send the e-mail", ex);
            }

        }

        public override void SetPlatformHeartbeatDelegate(PlatformHeartbeatDelegate platformHeartbeatDelegate)
        {
            //do nothing, we do not have long processing actions that require us to report heartbeats
        }

        public override void Terminate()
        {
            //Do nothing; As we do not have long running processes, it is safe to ignore termination signal
        }
    }
}
