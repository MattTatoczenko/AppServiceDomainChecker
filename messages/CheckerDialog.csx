#load "..\AppService.cs"
#load "..\InputCheckers.cs"
#load "..\DnsChecks.cs"

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class CheckerDialog : IDialog<object>
{
    private AppService appService;
    private bool receivedAllCustomerInformation;

    public Task StartAsync(IDialogContext context)
    {
        try
        {
            receivedAllCustomerInformation = false;
            context.Wait(CheckUseOfAppServiceEnvironment);
        }
        catch (OperationCanceledException error)
        {
            return Task.FromCanceled(error.CancellationToken);
        }
        catch (Exception error)
        {
            return Task.FromException(error);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Starting point for the App Service Domain Checker dialog. This starts major section 1 of 4, asking the user about an App Service Environment they may or may not be using.
    /// Presents a dialog box confirming whether the user is using an App Service Environment or not.
    /// Users will choose "Yes" or "No".
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to have a confirmation dialog about the use of an App Service Environment.</param>
    /// <param name="argument">Supposed to be the argument a user puts into the chat window. Not used by this method.</param>
    /// <returns>No returns.</returns>
    public async Task CheckUseOfAppServiceEnvironment(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        await context.PostAsync("Let's check for the type of App Service you are using.");

        PromptDialog.Confirm(
            context,
            ConfirmUseOfAppServiceEnvironment,
            "Is your App Service inside of an App Service Environment?",
            promptStyle: PromptStyle.Auto);
    }

    /// <summary>
    /// Confirm whether the user is using an App Service Environment or not.
    /// If they are, we will need to get the name.
    /// If not, we will ask for information on the App Service instead.
    /// This is also where we initialize the instance variable appService to hold the user input.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to send responses back to the user about their option choice.</param>
    /// <param name="argument">A boolean value. True if the user is using an App Service Environment. False if they are not.</param>
    /// <returns>No returns.</returns>
    public async Task ConfirmUseOfAppServiceEnvironment(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        this.appService = new AppService();
        if (confirm)
        {
            await context.PostAsync("You're using an App Service Environment. We'll need the name of the App Service Environment.");
            this.appService.IsASE = true;
            AskAppServiceEnvironmentName(context);
        }
        else
        {
            await context.PostAsync("You're using a regular App Service.");

            this.appService.IsASE = false;
            this.appService.AseName = null;

            AskAppServiceName(context);
        }
    }

    /// <summary>
    /// Ask the user the name of their App Service Environment. User will input text into the chat window.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to ask the user the name of their App Service Environment.</param>
    public void AskAppServiceEnvironmentName(IDialogContext context)
    {
        PromptDialog.Text(
            context,
            GetAppServiceEnvironmentName,
            "What's the name of your App Service Environment?",
            "Please enter the name of your App Service Environment.");
    }

    /// <summary>
    /// Used to take in the App Service Environment name and validate it. Need to make sure the user inputs an acceptable name that's allowed for App Service Environment naming.
    /// If the user puts in a valid name, we will then prompt them to confirm that name is correct. The user will choose "Yes" or "No" for that.
    /// If the user puts in an invalid name, we will ask them for the name again.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to send messages back to the user based on their input and also have a confirmation dialog to confirm the App Service Environment name.</param>
    /// <param name="argument">The name that the user provides for their App Service Environment.</param>
    /// <returns>No returns.</returns>
    public async Task GetAppServiceEnvironmentName(IDialogContext context, IAwaitable<string> argument)
    {
        var message = await argument;

        // Name has to be at least 2 characters in length, can't be longer than 39 characters. Can have letters, numbers, and dashes, but it can't start or end with a dash.
        if (InputCheckers.CheckAppServiceEnvironmentName(message))
        {
            await context.PostAsync($"The name {message} is an acceptable App Service Environment name.");

            this.appService.AseName = message.ToLower();

            PromptDialog.Confirm(
                    context,
                    ConfirmAppServiceEnvironmentName,
                    $"Is {message} the correct name for your App Service Environment?",
                    $"Can you confirm if {message} is the name of your App Service Environment?",
                    promptStyle: PromptStyle.Auto);
        }
        else
        {
            await context.PostAsync($"{message} is not an acceptable App Service Environment name. Please enter a valid name.");
            AskAppServiceEnvironmentName(context);
        }
    }

    /// <summary>
    /// Confirming the App Service Environment name. Used in case a user enters the name incorrectly (mistypes).
    /// In case a user needed to adjust the App Service Environment at the end of the information gathering process, we will go from here back to showing all that collected information.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used here to provide text updates to the users.</param>
    /// <param name="argument">A boolean value. True if the user confirms that the App Service Environment is the one they want to use. False if the name needs to be adjusted.</param>
    /// <returns>No returns.</returns>
    public async Task ConfirmAppServiceEnvironmentName(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Thank you for confirming the name of your App Service Environment!");

            /* 
             * If we are back here due the user needing to change the App Service Environment name after collecting all of the information, we will send them back to the step where we present all the entered information.
             * This is to prevent the user from having to enter all of the information again after this point, which would be the App Service name, possibly the Traffic Manager name, and the hostname to check.
             * */
            if (receivedAllCustomerInformation)
            {
                await ShowAppServiceInformation(context);
            }
            else
            {
                await context.PostAsync("Let's get the name of your App Service.");
                AskAppServiceName(context);
            }
        }
        else
        {
            await context.PostAsync("Let's get the right App Service Environment name.");
            AskAppServiceEnvironmentName(context);
        }
    }

    /// <summary>
    /// This starts major section 2 of 4 of asking for user input. This section is about asking the user about the App Service.
    /// Prompt to ask the user for the name of their App Service. User will type in the name into the chat window.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. It is used to make the prompt to the user.</param>
    void AskAppServiceName(IDialogContext context)
    {
        PromptDialog.Text(
            context,
            GetAppServiceName,
            "What's the name of your App Service?",
            "Please enter the name of your App Service.");
    }

    /// <summary>
    /// Take the App Service name that the user enters into the chat window and check that it is in the acceptable form for an App Service name.
    /// If the name is acceptable, we will ask the user to confirm that the name they entered is correct.
    /// If the name is unacceptable, we will ask the user to enter an acceptable name, sending them back to the AskAppServiceName method.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used for text updates to the users and to also create a confirmation dialog box.</param>
    /// <param name="argument">The name that the user entered as their App Service name.</param>
    /// <returns>No returns.</returns>
    public async Task GetAppServiceName(IDialogContext context, IAwaitable<string> argument)
    {
        var message = await argument;

        // Name has to be at least 2 characters in length, can't be longer than 60 characters. Can have letters, numbers, and dashes, but it can't start or end with a dash.
        if (InputCheckers.CheckAppServiceName(message))
        {
            await context.PostAsync($"The name {message} is an acceptable App Service name.");

            this.appService.AppServiceName = message.ToLower();

            PromptDialog.Confirm(
                    context,
                    ConfirmAppServiceName,
                    $"Is {message} the correct name for your App Service?",
                    $"Please confirm that {message} is the name of your App Service.",
                    promptStyle: PromptStyle.Auto);
        }
        else
        {
            await context.PostAsync($"{message} is not a valid App Service name. Please try again.");
            AskAppServiceName(context);
        }
    }

    /// <summary>
    /// Confirmation to ensure the App Service name the user entered is what they meant to enter.
    /// If the user is back at this point just to adjust the App Service name, we will send them back to the point of showing all the information they entered. This avoids them having to re-enter correct information.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to send update messages to the users.</param>
    /// <param name="argument">A boolean value. True if the customer meant to enter the App Service name. False if the customer mistyped the name and needs to re-enter it.</param>
    /// <returns>No returns.</returns>
    public async Task ConfirmAppServiceName(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Thank you for confirming the name of your App Service!");

            /* 
             * If we are back here due the user needing to change the App Service name after collecting all of the information, we will send them back to the step where we present all the entered information.
             * This is to prevent the user from having to enter all of the information again after this point, which would possibly be the Traffic Manager name and the hostname to check.
             * */
            if (receivedAllCustomerInformation)
            {
                await ShowAppServiceInformation(context);
            }
            else
            {
                await context.PostAsync("Let's check if you are using Traffic Manager.");
                CheckForTrafficManager(context);
            }
        }
        else
        {
            await context.PostAsync("Let's get the right App Service name.");
            AskAppServiceName(context);
        }
    }

    /// <summary>
    /// Starting major section 3 of 4 in asking for user input. This section revolves around the potential use of Traffic Manager.
    /// Ask the user if they use Traffic Manager.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to present a confirmation prompt to the user about whether they use Traffic Manager or not.</param>
    void CheckForTrafficManager(IDialogContext context)
    {
        PromptDialog.Confirm(
                context,
                ConfirmUseOfTrafficManager,
                "Are you using Traffic Manager with the hostname you are trying to check?",
                "I'm sorry, I didn't understand that. Are you using Traffic Manager for your hostname?",
                promptStyle: PromptStyle.Auto);
    }

    /// <summary>
    /// Used to confirm the use of Traffic Manager. 
    /// If they are, we will need the name of the Traffic Manager.
    /// If not, move on to ask the user about the custom hostname.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to provide text updates to the user based on their option.</param>
    /// <param name="argument">A boolean value. If True, the user is using Traffic Manager. If False, the user is not.</param>
    /// <returns></returns>
    public async Task ConfirmUseOfTrafficManager(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("You are using Traffic Manager.");
            this.appService.UsingTM = true;
            AskTrafficManagerName(context);
        }
        else
        {
            await context.PostAsync("You are not using Traffic Manager.");
            this.appService.UsingTM = false;
            AskCustomHostname(context);
        }
    }

    /// <summary>
    /// Ask the user the name of their Traffic Manager.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to present a prompt to the user and they will respond by putting in text into the chat window.</param>
    void AskTrafficManagerName(IDialogContext context)
    {
        PromptDialog.Text(
            context,
            GetTrafficManagerName,
            "What's the name of the Traffic Manager in front of your App Service?",
            "Please enter the name of your Traffic Manager.");
    }

    /// <summary>
    /// Take in the name the user put in for their Traffic Manager name. We also check that the name is acceptable.
    /// If the name is acceptable, we will ask the user to confirm that this name is correct.
    /// If the name is unacceptable, we will ask the user to input the Traffic Manager name again.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to provide text updates to the users based on their choice and to also provide a confirmation prompt.</param>
    /// <param name="argument">The Traffic Manager name that the user put into the chat window.</param>
    /// <returns>No returns.</returns>
    public async Task GetTrafficManagerName(IDialogContext context, IAwaitable<string> argument)
    {
        var message = await argument;

        // Name has to be at least 1 character in length, can't be longer than 63 characters. Can have letters, numbers, and dashes, but it can't start or end with a dash.
        if (InputCheckers.CheckTrafficManagerName(message))
        {
            await context.PostAsync($"The name {message} is an acceptable Traffic Manager name.");

            this.appService.TmName = message.ToLower();

            PromptDialog.Confirm(
                    context,
                    ConfirmTrafficManagerName,
                    $"Is {message} the correct name for your Traffic Manager?",
                    $"Please confirm that {message} is the name of your Traffic Manager.",
                    promptStyle: PromptStyle.Auto);
        }
        else
        {
            await context.PostAsync($"{message} is not a valid Traffic Manager name.");
            AskTrafficManagerName(context);
        }       
    }

    /// <summary>
    /// Confirm that the user put in the correct Traffic Manager name.
    /// If the user is at this point but has entered all of the other information about their App Service, we will send them to the end of the input sequence to show them the information they have entered.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to provide text updates to the user.</param>
    /// <param name="argument">A boolean value. If True, they have entered in the correct Traffic Manager name. If False, the user meant to enter a different name, so we will send them back to enter the right name.</param>
    /// <returns>No returns.</returns>
    public async Task ConfirmTrafficManagerName(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Thank you for confirming the name of your Traffic Manager!");

            /* 
             * If we are back here due the user needing to change the Traffic Manager name after collecting all of the information, we will send them back to the step where we present all the entered information.
             * This is to prevent the user from having to enter the hostname to check again after this point.
             * */
            if (receivedAllCustomerInformation)
            {
                await ShowAppServiceInformation(context);
            }
            else
            {
                AskCustomHostname(context);
            }
        }
        else
        {
            await context.PostAsync("Let's get the right Traffic Manager name.");
            AskTrafficManagerName(context);
        }
    }

    /// <summary>
    /// Starting major section 4 of 4 of asking the user for input. This major section involves asking the user for the hostname to check.
    /// This method asks the user to enter their custom hostname.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Ask the user for the hostname to check.</param>
    void AskCustomHostname(IDialogContext context)
    {
        PromptDialog.Text(
            context,
            GetCustomHostname,
            "What is the custom hostname that you are looking to check?",
            "Please enter the custom hostname you are trying to check.");
    }

    /// <summary>
    /// Take in the hostname to check from the user. It also needs to meet RFC1123 standards.
    /// If the hostname meets the RFC1123 standards, we will ask the user to confirm that this is the correct hostname.
    /// If the hostname does not meet the standards, we will ask the user to input the hostname again.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to provide text updates to the user and present a confirmation dialog to the user.</param>
    /// <param name="argument">The hostname that the user entered into the chat window.</param>
    /// <returns>No returns.</returns>
    public async Task GetCustomHostname(IDialogContext context, IAwaitable<string> argument)
    {
        var message = await argument;

        /* Hostname validation checks:
         * From RFC1123: https://tools.ietf.org/html/rfc1123#page-13
         * Labels - The individual levels of a domain. For example, www.microsoft.com has 3 labels: www, microsoft, and com.
         * Each label cannot start or end with a dash. Can start with a digit or number and end with a digit or number.
         * Separate labels by periods. However, the whole hostname cannot end with a period.
         * Each label must be between 1 and 63 characters in length inclusive.
         * Max character count for the hostname is 253 characters including deliminating periods. See https://blogs.msdn.microsoft.com/oldnewthing/20120412-00/?p=7873/.
         */
        if (InputCheckers.CheckHostname(message))
        {
            await context.PostAsync($"The custom hostname of {message} is an acceptable hostname.");

            this.appService.CustomHostname = message.ToLower();

            PromptDialog.Confirm(
                    context,
                    ConfirmCustomHostname,
                    $"Is {message} the correct custom hostname?",
                    $"Please confirm that {message} is the correct custom hostname.",
                    promptStyle: PromptStyle.Auto);
        }
        else
        {
            await context.PostAsync($"The hostname of {message} is invalid. Please enter a valid hostname.");
            AskCustomHostname(context);
        }
    }

    /// <summary>
    /// Confirm that the hostname the user entered is the one they meant to enter.
    /// If they confirm it, we will finish asking the user to input data and confirm all the data entered so far is correct
    /// If they need to adjust the hostname, send them back to the prompt to have them enter the right hostname.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="argument"></param>
    /// <returns></returns>
    public async Task ConfirmCustomHostname(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Thank you for confirming your custom hostname!");
            await ShowAppServiceInformation(context);
        }
        else
        {
            await context.PostAsync("Let's get the right custom hostname.");
            AskCustomHostname(context);
        }
    }

    /// <summary>
    /// Use individual text updates to present all of the user information back to them. 
    /// This is used to have them check and confirm that all of the information is correct.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to provide all of the text updates to the user with all the information they have entered so far.</param>
    /// <returns>No returns.</returns>
    public async Task ShowAppServiceInformation(IDialogContext context)
    {
        await context.PostAsync("Here's all of the information on your App Service and custom hostname.");

        receivedAllCustomerInformation = true;

        if (this.appService.UsingTM)
        {
            await context.PostAsync($"Traffic Manager name: {this.appService.TmName}");
        }

        if (this.appService.IsASE)
        {
            await context.PostAsync($"App Service Environment name: {this.appService.AseName}");
        }

        await context.PostAsync($"App Service name: {this.appService.AppServiceName}");
        await context.PostAsync($"Custom hostname: {this.appService.CustomHostname}");
        
        AskForConfirmationOfAppServiceInformation(context);
    }

    /// <summary>
    /// Present a prompt to have the user confirm that all the entered information is correct.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to present the confirmation prompt to the user.</param>
    public void AskForConfirmationOfAppServiceInformation(IDialogContext context)
    {
        PromptDialog.Confirm(
                context,
                ConfirmAppServiceInformation,
                "Does all of the information on your App Service and custom hostname look correct?",
                "Please confirm that the information on your App Service and custom hostname is correct.",
                promptStyle: PromptStyle.Auto);
    }

    /// <summary>
    /// Confirmation section to ensure all the user entered information is correct.
    /// If it is, we will move onto the next big section of the bot, the DNS checks.
    /// If there is something that is incorrect, we will move to figure out what is incorrect.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to provide text updates based on the user's choice.</param>
    /// <param name="argument">A boolean value. If True, all the information is correct. If False, the customer sees that something's wrong, so let's figure that out.</param>
    /// <returns>No returns.</returns>
    public async Task ConfirmAppServiceInformation(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Thank you for confirming all of the information in regards to your App Service and custom hostname.");
            await DoDNSChecks(context);
        }
        else
        {
            await context.PostAsync("Let's figure out what to change then.");
            AskAppServiceInformationToChange(context);
        }
    }

    /// <summary>
    /// Build a choice prompt to ask the user which part they have to change: App Service Environment name, Traffic Manager name, App Service name, custom hostname.
    /// Also provide an option if they mistakenly got here to go on to the DNS checks.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to prompt the user with choices of options to change.</param>
    void AskAppServiceInformationToChange(IDialogContext context)
    {
        List<string> appServiceInfoOptions = new List<string>();

        if (this.appService.UsingTM)
        {
            appServiceInfoOptions.Add("Traffic Manager name");
        }

        if (this.appService.IsASE)
        {
            appServiceInfoOptions.Add("App Service Environment name");
        }
        appServiceInfoOptions.Add("App Service name");
        appServiceInfoOptions.Add("Custom hostname");
        appServiceInfoOptions.Add("None. It was all correct.");

        PromptDialog.Choice(
            context,
            GetAppServiceInformationToChange,
            appServiceInfoOptions,
            "What information on your App Service does not look right?",
            promptStyle: PromptStyle.Auto);
    }

    /// <summary>
    /// Based on what the user needs to change, we will send them back to that point to re-enter the information.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used mainly for text updates based on the user choices.</param>
    /// <param name="argument">String input based on the choice the user needs to change.</param>
    /// <returns>No returns.</returns>
    public async Task GetAppServiceInformationToChange(IDialogContext context, IAwaitable<string> argument)
    {
        var message = await argument;
        if (message == "App Service name")
        {
            await context.PostAsync("Let's get the correct App Service name.");
            AskAppServiceName(context);
        }
        else if (message == "App Service Environment name")
        {
            await context.PostAsync("Let's get the correct App Service Environment name.");
            AskAppServiceEnvironmentName(context);
        }
        else if (message == "Traffic Manager name")
        {
            await context.PostAsync("Let's get the correct Traffic Manager name.");
            AskTrafficManagerName(context);
        }
        else if (message == "Custom hostname")
        {
            await context.PostAsync("Let's get the correct custom hostname.");
            AskCustomHostname(context);
        }
        else
        {
            await context.PostAsync("All of the information is correct. Let's proceed with checking the DNS settings.");
            await DoDNSChecks(context);
        }
    }

    /// <summary>
    /// Checking DNS records for the hostname entered as well as the App Service names.
    /// Will need to ensure the hostname is set up properly and configured to use the App Service or at least the Traffic Manager.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to provide a text update to the user.</param>
    /// <returns>No returns.</returns>
    public async Task DoDNSChecks(IDialogContext context)
    {
        await context.PostAsync("Let's pull some information on the hostname entered.");

        DnsChecks.GetAppServiceIPAddress(this.appService);     

        DnsChecks.GetHostnameARecords(this.appService);

        DnsChecks.GetHostnameAwverifyRecords(this.appService);

        DnsChecks.GetHostnameCNameRecords(this.appService);

        if (this.appService.UsingTM)
        {
            DnsChecks.GetTrafficManagerCNameRecords(this.appService);
            // Print out all of the Traffic Manager CNAME records if there are any (there should be if they are using Traffic Manager)
            foreach (string trafficManagerCName in this.appService.TrafficManagerCNameRecords)
            {
                await context.PostAsync($"Traffic Manager CNAME record: {trafficManagerCName}");
            }
        }

        DnsChecks.GetHostnameTxtRecords(this.appService);

        // TODO: Remove all of the PostAsync calls here. They are used for debug/test purposes

        // Print out all of the IP addresses that correspond to the App Service URL
        foreach (string appServiceAddress in this.appService.IPAddresses)
        {
            await context.PostAsync($"App Service IP: {appServiceAddress}");
        }
        // Print out all of the A records configured for the hostname, if there are any
        foreach (string aRecord in this.appService.HostnameARecords)
        {
            await context.PostAsync($"A Record: {aRecord}");
        }
        // Print out all of the AWVERIFY CNAME records configured for the hostname, if there are any
        foreach (string awverifyRecord in this.appService.HostnameAwverifyCNameRecords)
        {
            await context.PostAsync($"AWVerify CNAME record: {awverifyRecord}");
        }
        // Print out all the CNAME records configured for the hostname, if there are any
        foreach (string cName in this.appService.HostnameCNameRecords)
        {
            await context.PostAsync($"CNAME Record: {cName}");
        }
        // Print out all of the TXT records configured for the hostname, if there are any
        foreach (string txtRecord in this.appService.HostnameTxtRecords)
        {
            await context.PostAsync($"TXT Record: {txtRecord}");
        }

        await PresentDNSInformation(context);
    }

    public async Task PresentDNSInformation(IDialogContext context)
    {
        // Check the A records
        if(this.appService.HostnameARecords.Count() > 0)
        {
            // Loop through all of the A records and check if they match the IP of the App Service
            foreach (string aRecord in this.appService.HostnameARecords)
            {
                if (this.appService.IPAddresses.Contains(aRecord))
                {
                    await context.PostAsync($"The DNS A record configured, which points to {aRecord}, matches the IP address of the Azure App Service. This A record is configured correctly.");
                    // TODO: Check if the AWVERIFY or TXT records match the App Service URL to allow the hostname to be configured. Note: If the domain is already on the App Service, we don't need to have the AWVERIFY or TXT records still around, so say that as well.
                }
                else
                {
                    // Loop through all of the App Services' IP addresses (should only be 1 inbound IP). Print out how they should configure the record.
                    foreach (string ipAddress in this.appService.IPAddresses)
                    {
                        await context.PostAsync($"The DNS A record configured, which points to {aRecord}, does not match the IP address of the Azure App Service. If you are planning to add this hostname to your App Service, consider updating the DNS A record to point to the IP address of {ipAddress}");
                    }
                    // TODO: Check if the AWVERIFY or TXT records match the App Service URL to allow the hostname to be configured. Note: If the domain is already on the App Service, we don't need to have the AWVERIFY or TXT records still around, so say that as well.
                }
            }
        }
        else if (this.appService.HostnameCNameRecords.Count() > 0)
        {
            // Loop through all of the CNAME records and check if they match the URL of the App Service OR the URL of the Traffic Manager
            foreach (string cNameRecord in this.appService.HostnameCNameRecords)
            {
                if (this.appService.UsingTM)
                {
                    string fullTrafficManagerURL = this.appService.TmName + "." + this.appService.TrafficManagerURLEnding;
                    if (cNameRecord.Equals(fullTrafficManagerURL))
                    {
                        await context.PostAsync($"The DNS CNAME record configured, which points to {cNameRecord}, matches the Traffic Manager URL that you are using. This CNAME record is configured properly to use Traffic Manager.");
                        // TODO: Check if the Traffic Manager CNAMEs point towards the App Service URL after this
                    }
                    else
                    {
                        await context.PostAsync($"The DNS CNAME record configured, which points to {cNameRecord}, does not match the Traffic Manager URL of \"{fullTrafficManagerURL}\". If you plan to use Traffic Manager, consider updating the CNAME record.");
                        // TODO: Provide an example of how to configure the DNS CNAME here.
                    }
                }
                else
                {
                    // TODO: Check that the hostname matches the App Service URL. If it doesn't, see if the AWVERIFY CNAME records matches the App Service URL, as that's used to preemptively add the hostname to the App Service
                }
            }
        }

        await context.PostAsync("Type 'restart' to restart the domain checker. Otherwise, we will echo anything you say after this point."); 
        context.Wait(MessageReceivedAsync);
    }

    /// <summary>
    /// Default ending point right now. Once the user is here, we will repeat what they enter.
    /// If the user enters "restart" exactly, we will ask them if they really want to restart and start the process all over again.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to either repeat what the user enters or provide a confirmation prompt.</param>
    /// <param name="argument">Any message the user enters into the chat window.</param>
    /// <returns>NO returns.</returns>
    public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        var message = await argument;
        if (message.Text == "restart")
        {
            PromptDialog.Confirm(
                context,
                AfterRestartAsync,
                "Are you sure you want to go back to the beginning?",
                "I'm sorry, I didn't understand that.",
                promptStyle: PromptStyle.Auto);
        }
        else
        {
            await context.PostAsync($"You said {message.Text}");
            context.Wait(MessageReceivedAsync);
        }
    }

    /// <summary>
    /// Confirm whether the user wants to restart the whole process or not. 
    /// If so, restart and ask them for information to enter.
    /// If not, keep just repeating what they enter.
    /// NOTE: This will be changed later on.
    /// </summary>
    /// <param name="context">Context needed for the convesation to occur. Used to provide text updates based on the user's choice.</param>
    /// <param name="argument">A boolean value. If True, the user wants to restart the whole process. If False, keep doing what you're doing.</param>
    /// <returns></returns>
    public async Task AfterRestartAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Let's restart.");
            receivedAllCustomerInformation = false;
            await CheckUseOfAppServiceEnvironment(context, null);
        }
        else
        {
            await context.PostAsync("We'll continue checking the domain.");
            context.Wait(MessageReceivedAsync);
        }
    }
}