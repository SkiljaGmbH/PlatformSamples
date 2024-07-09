using EDAClient.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EDAClient.Data
{
    public class MessageData
    {
        private ICommand _navigationCommand;
        public string Message { get; set; }
        public bool IsLink { get; set; }
        public int NotificationID { get; set; }

        public ICommand NavigationCommand => _navigationCommand ?? (_navigationCommand = new CommandExecutor(navigationCommand_execute));

        private void navigationCommand_execute(object o)
        {
            var url = o as string;
            //{ var.ActivityURL}?msg = The ammount on the document is above 2000.Can we approve?&an ={ wi.ActivityName}
            //&cv = Approve & dv = Deny & t =< token > &rt =< platformURL > &nt =< notification >
            if (url.LastIndexOf("<notification>") > 0)
            {
                url = url.Replace("<notification>", NotificationID.ToString());
            }

            if (url.LastIndexOf("<platformURL>") > 0)
            {
                url = url.Replace("<platformURL>",  ConfigurationManager.AppSettings["ProcessServiceAddress"]);
            }

            if (url.LastIndexOf("<token>") > 0)
            {
                url = url.Replace("<token>", "");
            }
            ProcessStartInfo sInfo = new ProcessStartInfo(url);
            Process.Start(sInfo);
        }
    }
}
