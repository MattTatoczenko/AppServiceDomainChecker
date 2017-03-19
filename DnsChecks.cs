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
    /// <summary>
    /// Pulls the IP address(es) in use by the Azure App Service. This is used to check that A records are configured properly towards the App Service.
    /// If the App Service is in an ASE, the name is setup as follows: {App Service name}.{ASE name}.p.azurewebsites.net
    /// If the App Service is in the multi-tenant service, the name is setup as follows: {App Service name}.azurewebsites.net
    /// Puts the IP address list into the AppService object sent in as an argument so it can be used later.
    /// </summary>
    /// <param name="appService">Object containing all of the information on the App Service we are checking. Includes the ASE name and App Service name, which are directly used here.</param>
    public static void GetAppServiceIPAddress(AppService appService)
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

        List<string> addressesString = new List<string>();
        foreach(IPAddress address in addresses)
        {
            addressesString.Add(address.ToString()); 
        }

        appService.IPAddresses = addressesString;
    }

    /// <summary>
    /// Do DNS queries to get any A records associated with the custom hostname the user has entered.
    /// Puts the string output of the A record listing into the AppService argument so the list can be used later.
    /// </summary>
    /// <param name="appService">The object that holds all of the information the user has given, including the custom hostname.</param>
    public static void GetHostnameARecords(AppService appService)
    {
        IDnsResolver resolver = new DnsStubResolver();
        List<ARecord> aRecords = DnsResolverExtensions.Resolve<ARecord>(resolver, appService.CustomHostname, RecordType.A, RecordClass.Any);

        List<string> aRecordsStrings = new List<string>();
        foreach(ARecord aRecord in aRecords)
        {
            aRecordsStrings.Add(aRecord.Address.ToString());
        }

        appService.HostnameARecords = aRecordsStrings;
    }

    /// <summary>
    /// Do DNS queries to get any CNAME records associated with awverify.{custom hostname}.
    /// AWVerify records are the old method that accompanied A records for hostname validation. They can currently be used to preemptively add a hostname to an App Service without adjusting the existing record.
    /// For example, if the user has "www.contoso.com" configured on another server or service but wants to add it to the App Service without disrupting the hostname, they will configure a CNAME record from "awverify.www.contoso.com" to point towards the URL of the App Service to host "www.contoso.com" in the future.
    /// Puts the string output of all of the awverify records into the AppService argument so the list can be used later.
    /// </summary>
    /// <param name="appService"></param>
    public static void GetHostnameAwverifyRecords(AppService appService)
    {
        IDnsResolver resolver = new DnsStubResolver();

        string awverifyRecordURL = "awverify." + appService.CustomHostname;
        List<CNameRecord> awverifyCNameRecords = DnsResolverExtensions.Resolve<CNameRecord>(resolver, awverifyRecordURL, RecordType.CName, RecordClass.Any);

        List<string> awverifyRecords = new List<string>();
        foreach(CNameRecord awverifyCName in awverifyCNameRecords)
        {
            awverifyRecords.Add(awverifyCName.CanonicalName.ToString());
        }

        appService.HostnameAwverifyCNameRecords = awverifyRecords;
    }

    /// <summary>
    /// Do DNS queries to get any CNAME records associated with the custom hostname the user has entered.
    /// Puts the string output of all of the CNAME records into the AppService argument so the list can be used later.
    /// </summary>
    /// <param name="appService">The object that holds all of the information the user has given, including the custom hostname.</param>
    public static void GetHostnameCNameRecords(AppService appService)
    {
        IDnsResolver resolver = new DnsStubResolver();
        List<CNameRecord> cNameRecords = DnsResolverExtensions.Resolve<CNameRecord>(resolver, appService.CustomHostname, RecordType.CName, RecordClass.Any);

        List<string> cNames = new List<string>();
        foreach (CNameRecord cName in cNameRecords)
        {
            cNames.Add(cName.CanonicalName.ToString());
        }

        appService.HostnameCNameRecords = cNames;
    }

    /// <summary>
    /// Do DNS queries to get any CNAME records associated with the Traffic Manager URL the user has entered.
    /// This will get the currently available endpoint that's responding to the Traffic Manager.
    /// Puts the string output of all of the Traffic Manager CNAME records into the AppService argument so the list can be used later.
    /// </summary>
    /// <param name="appService">The object that holds all the information the user has given, including the Traffic Manager name.</param>
    public static void GetTrafficManagerCNameRecords(AppService appService)
    {
        IDnsResolver resolver = new DnsStubResolver();
        List<CNameRecord> trafficManagerCNameRecords = DnsResolverExtensions.Resolve<CNameRecord>(resolver, appService.TmName + "." + appService.TrafficManagerURLEnding, RecordType.CName, RecordClass.Any);

        List<string> trafficManagerCNames = new List<string>();
        foreach (CNameRecord trafficManagerCName in trafficManagerCNameRecords)
        {
            trafficManagerCNames.Add(trafficManagerCName.CanonicalName.ToString());
        }

        appService.TrafficManagerCNameRecords = trafficManagerCNames;
    }

    /// <summary>
    /// Do DNS queries to get any TXT records associated with the custom hostname the user has entered.
    /// Puts the string output of all of the TXT records into the AppService argument so the list can be used later.
    /// </summary>
    /// <param name="appService"></param>
    public static void GetHostnameTxtRecords(AppService appService)
    {
        IDnsResolver resolver = new DnsStubResolver();
        List<TxtRecord> txtRecords = DnsResolverExtensions.Resolve<TxtRecord>(resolver, appService.CustomHostname, RecordType.Txt, RecordClass.Any);

        List<string> txts = new List<string>();
        foreach (TxtRecord txtRecord in txtRecords)
        {
            txts.Add(txtRecord.TextData.ToString());
        }

        appService.HostnameTxtRecords = txts;
    }
}
