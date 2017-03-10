using System;
using static AppService;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Dialogs;
using System.Net;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

public class DnsChecks
{
	public DnsChecks()
	{
	}


    public static async Task StartDnsChecks(IDialogContext context, AppService appService)
    {
        List<IPAddress> aRecords = GetDnsARecords(appService);

        foreach (IPAddress aRecord in aRecords)
        {
            await context.PostAsync($"A Record: {aRecord.ToString()}");
        }

        List<string> cNameRecords = GetDnsCNameRecords(appService);
        foreach (string cName in cNameRecords)
        {
            await context.PostAsync($"CNAME Record: {cName}");
        }
        
    }

    private static List<IPAddress> GetDnsARecords(AppService appService)
    {
        IDnsResolver resolver = new DnsStubResolver();
        List<IPAddress> addresses = DnsResolverExtensions.ResolveHost(resolver, appService.CustomHostname);

        return addresses;
    }

    private static List<string> GetDnsCNameRecords(AppService appService)
    {
        IDnsResolver resolver = new DnsStubResolver();
        List<CNameRecord> cNameRecords = DnsResolverExtensions.Resolve<CNameRecord>(resolver, appService.CustomHostname, RecordType.CName, RecordClass.Any);

        List<string> cNames = new List<string>();
        foreach (CNameRecord cName in cNameRecords)
        {
            cNames.Add(cName.CanonicalName.ToString());
        }
        return cNames;
    }

    private static List<string> GetDnsTxtRecords(AppService appService)
    {
        List<string> txtRecords = new List<string>();
        txtRecords.Add("stuff");
        return txtRecords;
    }
}
