using System;
using static AppService;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Dialogs;
using System.Net;
// See http://arsofttoolsnet.codeplex.com/ for information on the ARSoft.Tools.Net library
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

public class DnsChecks
{
    public static async Task StartDnsChecks(IDialogContext context, AppService appService)
    {
        List<IPAddress> appServiceAddresses = GetAppServiceIPAddress(appService);
        foreach (IPAddress appServiceAddress in appServiceAddresses)
        {
            await context.PostAsync($"App Service IP: {appServiceAddress.ToString()}");
        }

        List<string> aRecords = GetHostnameARecords(appService);
        foreach (string aRecord in aRecords)
        {
            await context.PostAsync($"A Record: {aRecord}");
        }

        List<string> cNameRecords = GetHostnameCNameRecords(appService);
        foreach (string cName in cNameRecords)
        {
            await context.PostAsync($"CNAME Record: {cName}");
        }
        
    }

    private static List<IPAddress> GetAppServiceIPAddress(AppService appService)
    {
        string fullAppServiceURL = "";

        if (appService.IsASE)
        { 
            fullAppServiceURL = appService.AppServiceName + "." + appService.AseName + ".p." + appService.AppServiceURLEnding;
        }
        else
        {
            fullAppServiceURL = appService.AppServiceName + "." + appService.AppServiceURLEnding;
        }


        IDnsResolver resolver = new DnsStubResolver();
        List<IPAddress> addresses = DnsResolverExtensions.ResolveHost(resolver, fullAppServiceURL);

        return addresses;
    }

    /// <summary>
    /// Do DNS queries to get any A records associated with the custom hostname the user has entered.
    /// </summary>
    /// <param name="appService">The object that holds all of the information the user has given, including the custom hostname.</param>
    /// <returns>A list of strings of all of the A records associated with custom hostname.</returns>
    private static List<string> GetHostnameARecords(AppService appService)
    {
        IDnsResolver resolver = new DnsStubResolver();
        List<ARecord> aRecords = DnsResolverExtensions.Resolve<ARecord>(resolver, appService.CustomHostname, RecordType.A, RecordClass.Any);

        List<string> aRecordsStrings = new List<string>();
        foreach(ARecord aRecord in aRecords)
        {
            aRecordsStrings.Add(aRecord.Address.ToString());
        }

        return aRecordsStrings;
    }

    /// <summary>
    /// Do DNS queries to get any CNAME records associated with the custom hostname the user has entered.
    /// </summary>
    /// <param name="appService">The object that holds all of the information the user has given, including the custom hostname.</param>
    /// <returns>A list of strings of all the CNAME records associated with the custom hostname.</returns>
    private static List<string> GetHostnameCNameRecords(AppService appService)
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
}
