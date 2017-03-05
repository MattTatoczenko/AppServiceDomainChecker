using System;
using System.Text.RegularExpressions;

public class InputCheckers
{
    // Values received from Azure portal when creating new resources
    private static int maxAppServiceNameLength = 60;
    private static int maxTrafficManagerNameLength = 63;
    private static int maxAppServiceEnvironmentNameLength = 39;
    // From RFC1123: https://tools.ietf.org/html/rfc1123#page-13 and https://blogs.msdn.microsoft.com/oldnewthing/20120412-00/?p=7873/
    private static int maxHostnameLength = 253;

    public static bool CheckAppServiceName(string appServiceName)
    {
        // Name has to be at least 2 characters in length, can't be longer than 60 characters. Can have letters, numbers, and dashes, but it can't start or end with a dash.
        string pattern = @"^[a-z0-9][-a-z0-9]*[a-z0-9]$";

        bool validAppServiceName = false;
        if (appServiceName.Length <= maxAppServiceNameLength)
        {
            try
            {
                validAppServiceName = Regex.IsMatch(appServiceName, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(2000));
            }
            catch (RegexMatchTimeoutException e)
            {
                Console.WriteLine($"Timed out after {e.MatchTimeout} seconds matching {e.Input}");
            }
        }
        return validAppServiceName;
    }

    public static bool CheckTrafficManagerName(string trafficManagerName)
    {
        // Name has to be at least 1 character in length, can't be longer than 63 characters. Can have letters, numbers, and dashes, but it can't start or end with a dash.
        string pattern = @"^[a-z0-9](?(-)-+[a-z0-9]|[a-z0-9])*$";

        bool validTrafficManagerName = false;
        if (trafficManagerName.Length <= maxTrafficManagerNameLength)
        {
            try
            {
                validTrafficManagerName = Regex.IsMatch(trafficManagerName, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(2000));
            }
            catch (RegexMatchTimeoutException e)
            {
                Console.WriteLine($"Timed out after {e.MatchTimeout} seconds matching {e.Input}");
            }
        }
        return validTrafficManagerName;
    }

    public static bool CheckAppServiceEnvironmentName(string appServiceEnvironmentName)
    {
        // Name has to be at least 2 characters in length, can't be longer than 39 characters. Can have letters, numbers, and dashes, but it can't start or end with a dash.
        string pattern = @"^[a-z0-9][-a-z0-9]*[a-z0-9]$";

        bool validAppServiceEnvironmentName = false;
        if(appServiceEnvironmentName.Length <= maxAppServiceEnvironmentNameLength)
        {
            try
            {
                validAppServiceEnvironmentName = Regex.IsMatch(appServiceEnvironmentName, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(2000));
            }
            catch (RegexMatchTimeoutException e)
            {
                Console.WriteLine($"Timed out after {e.MatchTimeout} seconds matching {e.Input}");
            }
        }
        return validAppServiceEnvironmentName;
    }

    
    public static bool CheckHostname(string hostname)
    {
        /* Hostname validation checks:
         * Labels - The individual levels of a domain. For example, www.microsoft.com has 3 labels: www, microsoft, and com.
         * Each label cannot start or end with a dash. Can start with a digit or number and end with a digit or number.
         * Separate labels by periods. However, the whole hostname cannot end with a period.
         * Each label must be between 1 and 63 characters in length inclusive.
         * Max character count for the hostname is 253 characters including deliminating periods. See https://blogs.msdn.microsoft.com/oldnewthing/20120412-00/?p=7873/.
         */
        string pattern = @"^(([a-z0-9](?(-)-+[a-z0-9]|[a-z0-9])*){1,63}\.)+([a-z0-9](?(-)-+[a-z0-9]|[a-z0-9])*){1,63}$";

        bool validHostname = false;
        if(hostname.Length <= maxHostnameLength)
        {
            try
            {
                validHostname = Regex.IsMatch(hostname, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(2000));
            }
            catch (RegexMatchTimeoutException e)
            {
                Console.WriteLine($"Timed out after {e.MatchTimeout} seconds matching {e.Input}");
            }
        }
        return validHostname;
    }
    
}
