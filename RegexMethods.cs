using System;
using System.Text.RegularExpressions;

public class RegexMethods
{
    private static int maxAppServiceNameLength = 60;
    private static int maxTrafficManagerNameLength = 63;
    private static int maxAppServiceEnvironmentNameLength = 39;

    public static bool CheckAppServiceName(string appServiceName)
    {
        string pattern = @"^[a-z0-9][-a-z0-9]*[a-z0-9]$";

        bool validAppServiceName = false;
        if (appServiceName.Length <= maxAppServiceNameLength)
        {
            try
            {
                validAppServiceName = Regex.IsMatch(appServiceName, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(2000));
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
        string pattern = @"^[a-z0-9](?(-)-+[a-z0-9]|[a-z0-9])*$";

        bool validTrafficManagerName = false;
        if (trafficManagerName.Length <= maxTrafficManagerNameLength)
        {
            try
            {
                validTrafficManagerName = Regex.IsMatch(trafficManagerName, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(2000));
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
        string pattern = @"^[a-z0-9][-a-z0-9]*[a-z0-9]$";

        bool validAppServiceEnvironmentName = false;
        if(appServiceEnvironmentName.Length <= maxAppServiceEnvironmentNameLength)
        {
            try
            {
                validAppServiceEnvironmentName = Regex.IsMatch(appServiceEnvironmentName, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(2000));
            }
            catch (RegexMatchTimeoutException e)
            {
                Console.WriteLine($"Timed out after {e.MatchTimeout} seconds matching {e.Input}");
            }
        }
        return validAppServiceEnvironmentName;
    }
}
