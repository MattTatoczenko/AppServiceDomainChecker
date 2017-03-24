using System;

[Serializable]
public class DNSCheckErrors
{
    public bool appServiceIPLookupFailed;
    public bool hostnameARecordLookupFailed;
    public bool hostnameAwverifyRecordLookupFailed;
    public bool hostnameCNameRecordLookupFailed;
    public bool trafficManagerCNameRecordLookupFailed;
    public bool hostnameTxtRecordLookupFailed;
    public int currentDNSFailures;

    public static int MaxDNSFailures = 3;

    public DNSCheckErrors()
    {
        this.appServiceIPLookupFailed = false;
        this.hostnameARecordLookupFailed = false;
        this.hostnameAwverifyRecordLookupFailed = false;
        this.hostnameCNameRecordLookupFailed = false;
        this.trafficManagerCNameRecordLookupFailed = false;
        this.hostnameTxtRecordLookupFailed = false;
        this.currentDNSFailures = 0;
    }
}
