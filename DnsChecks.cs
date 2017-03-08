using System;
using static AppService;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Dialogs;

public class DnsChecks
{
	public DnsChecks()
	{
	}

    public static async Task StartDnsChecks(IDialogContext context, AppService appService)
    {
        IList<string> txtRecords = new List<string>();
        string output;
        string pattern = string.Format(@"{0}\s*text =\s*""([\w\-\=]*)""", appService.CustomHostname);

        await context.PostAsync($"the pattern: {pattern}");

        var startInfo = new ProcessStartInfo("nslookup");
        startInfo.Arguments = string.Format("-type=TXT {0}", appService.CustomHostname);
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;

        using (var cmd = Process.Start(startInfo))
        {
            output = cmd.StandardOutput.ReadToEnd();
        }

        await context.PostAsync($"Output: {output}");

        MatchCollection matches = Regex.Matches(output, pattern, RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            if (match.Success)
                txtRecords.Add(match.Groups[1].Value);
        }
        
        foreach(string record in txtRecords)
        {
            await context.PostAsync($"Record: {record}");
        }
        
    }
}
