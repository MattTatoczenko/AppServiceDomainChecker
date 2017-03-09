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
        List<string> aRecords = GetDnsARecords(appService);

        foreach (string aRecord in aRecords)
        {
            await context.PostAsync($"A Record: {aRecord}");
        }
        
    }

    private static List<string> GetDnsARecords(AppService appService)
    {
        List<string> aRecords = new List<string>();
        string output;
        string pattern = @"\r\nAddress(es)*:\s*(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";
        string pattern2 = @"\r\n\t\s*(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})*";
        pattern = String.Concat(appService.CustomHostname, pattern);

        var startInfo = new ProcessStartInfo("nslookup");
        startInfo.Arguments = string.Format("-type=A {0}", appService.CustomHostname);
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;

        using (var cmd = Process.Start(startInfo))
        {
            output = cmd.StandardOutput.ReadToEnd();
        }

        MatchCollection matches = Regex.Matches(output, pattern, RegexOptions.IgnoreCase);
        MatchCollection matches2 = Regex.Matches(output, pattern2, RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            if (match.Success)
                aRecords.Add(match.Groups[2].Value);
        }
        foreach (Match match in matches2)
        {
            if (match.Success)
            {
                aRecords.Add(match.Groups[1].Value);
            }
        }

        return aRecords;
    }

    private static List<string> GetDnsTxtRecords(AppService appService)
    {
        List<string> txtRecords = new List<string>();
        string output;
        string pattern = string.Format(@"{ 0}\s*text =\s*""([\w\-\=]*)""", appService.CustomHostname);


        var startInfo = new ProcessStartInfo("nslookup");
        startInfo.Arguments = string.Format("-type=TXT {0}", appService.CustomHostname);
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;

        using (var cmd = Process.Start(startInfo))
        {
            output = cmd.StandardOutput.ReadToEnd();
        }


        MatchCollection matches = Regex.Matches(output, pattern, RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            if (match.Success)
                txtRecords.Add(match.Groups[1].Value);
        }

        return txtRecords;
    }
}
