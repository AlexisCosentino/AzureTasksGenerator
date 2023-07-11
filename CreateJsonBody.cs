using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;


namespace CreateTaskFromIteration
{
    public class CreateJsonBody
    {
        public dynamic ticket { get; set; }

        public string createJsonWithPBIToPostFromDynamic()
        {
            string areaPath = ticket.fields["System.AreaPath"].ToString().Replace("\\", "\\\\");
            string iterationPath = ticket.fields["System.IterationPath"].ToString().Replace("\\", "\\\\");
            string title = cleanJson(ticket.fields["System.Title"].ToString());
            string description = cleanJson(ticket.fields["System.Description"].ToString());

            string jsonToPost = "[{ \"op\": \"add\", \"path\": \"/fields/System.Title\", \"from\": null, \"value\": \"" + title + "\"}";
            jsonToPost += ", { \"op\": \"add\", \"path\": \"/fields/System.Description\", \"from\": null, \"value\": \"" + description + "\"} ";
            jsonToPost += ", { \"op\": \"add\", \"path\": \"/fields/System.State\", \"from\": null, \"value\": \"To Do\"}";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/System.CreatedBy\", \"value\": \"" + ticket.fields["System.CreatedBy"].uniqueName + "\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/System.AreaPath\", \"value\": \"" + areaPath + "\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/System.IterationPath\", \"value\": \"" + iterationPath + "\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/Microsoft.VSTS.Scheduling.RemainingWork\", \"value\": \"" + ticket.fields["Microsoft.VSTS.Scheduling.RemainingWork"] + "\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/Microsoft.VSTS.Scheduling.OriginalEstimate\", \"value\": \"" + ticket.fields["Microsoft.VSTS.Scheduling.OriginalEstimate"] + "\" }";
            jsonToPost += "]";
            Console.WriteLine(jsonToPost);
            return jsonToPost;
        }

        public string cleanJson(string toformat)
        {
            toformat = toformat.Replace("{code:java}", "<code>");
            toformat = toformat.Replace("{code:java}", "<code>");
            toformat = toformat.Replace("{code}", "</code>");
            toformat = toformat.Replace("\r\n *****", "<br>&emsp;&emsp;&emsp;&emsp;&emsp;\t■");
            toformat = toformat.Replace("\r\n ****", "<br>&emsp;&emsp;&emsp;&emsp;\t■");
            toformat = toformat.Replace("\r\n ***", "<br>&emsp;&emsp;&emsp;\t■");
            toformat = toformat.Replace("\r\n **", "<br>&emsp;&emsp;\t■");
            toformat = toformat.Replace("\r\n *", "<br>&emsp;\t■");
            toformat = toformat.Replace("\r\n", "<br>"); //Transate line breaker
            toformat = toformat.Replace("\"", " "); // Remove every double quote of the text
            toformat = toformat.Replace("\\", "");  // Remove every backslash of the text
            toformat = toformat.Replace("*[", "<strong>[");
            toformat = toformat.Replace("]*", "]</strong>");
            return toformat;
        }
    }
}