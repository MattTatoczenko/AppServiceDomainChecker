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

    /// <summary>
    /// Validate whether the name for the App Service that was entered is valid or not.
    /// Validation information gathered from the Azure portal based on the responses given when testing various App Service names.
    /// </summary>
    /// <param name="appServiceName">The App Service name the user has entered</param>
    /// <returns>A boolean value. If True, the App Service name is acceptable as an App Service name. If False, the name is not acceptable.</returns>
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

    /// <summary>
    /// Validate whether the name for the Traffic Manager that was entered is valid or not.
    /// Validation information gathered from the Azure portal based on the responses given when testing various Traffic Manager names.
    /// Traffic Manager naming follows the outline given in RFC1123 for labels of a hostname.
    /// </summary>
    /// <param name="trafficManagerName">The Traffic Manager name the user has entered.</param>
    /// <returns>A boolean value. If True, the Traffic Manager name is acceptable as an Traffic Manager name. If False, the name is not acceptable.</returns>
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

    /// <summary>
    /// Validate whether the name for the App Service Environment that was entered is valid or not.
    /// Validation information gathered from the Azure portal based on the responses given when testing various App Service Environment names.
    /// </summary>
    /// <param name="appServiceEnvironmentName">The App Service Environment name that the user has entered.</param>
    /// <returns>A boolean value. If True, the App Service Environment name is acceptable as an App Service Environment name. If False, the name is not acceptable.</returns>
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

    /// <summary>
    /// Validate if the custom hostname entered is valid based on RFC 1123. See https://tools.ietf.org/html/rfc1123#page-13 for details.
    /// </summary>
    /// <param name="hostname">The custom hostname that the user has entered.</param>
    /// <returns>A boolean value. If True, the custom hostname is acceptable. If False, the name is not acceptable.</returns>
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
