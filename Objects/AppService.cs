using System;
using System.Net;

[Serializable]
public class AppService
{
    private bool isASE;
    private bool usingTM;
    private string aseName;
    private string tmName;
    private string appServiceName;
    private string customHostname;

    private List<string> iPAddresses;
    private List<string> hostnameARecords;
    private List<string> hostnameCNameRecords;
    private List<string> hostnameTxtRecords;
    private List<string> hostnameAwverifyCNameRecords;
    private List<string> trafficManagerCNameRecords;

    public string AppServiceURLEnding = "azurewebsites.net";
    public string TrafficManagerURLEnding = "trafficmanager.net";

    public bool IsASE
    {
        get
        {
            return isASE;
        }
        set
        {
            isASE = value;
        }
    }

    public bool UsingTM
    {
        get
        {
            return usingTM;
        }
        set
        {
            usingTM = value;
        }
    }

    public string AseName
    {
        get
        {
            return aseName;
        }
        set
        {
            aseName = value;
        }
    }

    public string TmName
    {
        get
        {
            return tmName;
        }
        set
        {
            tmName = value;
        }
    }

    public string AppServiceName
    {
        get
        {
            return appServiceName;
        }
        set
        {
            appServiceName = value;
        }
    }

    public string CustomHostname
    {
        get
        {
            return customHostname;
        }
        set
        {
            customHostname = value;
        }
    }

    public List<string> IPAddresses
    {
        get
        {
            return iPAddresses;
        }
        set
        {
            iPAddresses = value;
        }
    }

    public List<string> HostnameARecords
    {
        get
        {
            return hostnameARecords;
        }
        set
        {
            hostnameARecords = value;
        }
    }

    public List<string> HostnameCNameRecords
    {
        get
        {
            return hostnameCNameRecords;
        }
        set
        {
            hostnameCNameRecords = value;
        }
    }

    public List<string> HostnameTxtRecords
    {
        get
        {
            return hostnameTxtRecords;
        }
        set
        {
            hostnameTxtRecords = value;
        }
    }

    public List<string> HostnameAwverifyCNameRecords
    {
        get
        {
            return hostnameAwverifyCNameRecords;
        }
        set
        {
            hostnameAwverifyCNameRecords = value;
        }
    }

    public List<string> TrafficManagerCNameRecords
    {
        get
        {
            return trafficManagerCNameRecords;
        }
        set
        {
            trafficManagerCNameRecords = value;
        }
    }
}
