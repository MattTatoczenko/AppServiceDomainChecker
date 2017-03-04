#load "..\AppService.cs"
#load "..\RegexMethods.cs"

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class CheckerDialog : IDialog<object>
{
    private AppService appService;

    public Task StartAsync(IDialogContext context)
    {
        try
        {
            context.Wait(CheckAppServiceType);
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

    // This is the entry point for the dialog, but I'm not sure if that will stay. Work in progress
    public async Task CheckAppServiceType(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        await context.PostAsync("Let's check for the type of App Service you are using.");

        List<String> AppServiceOptions = new List<String>();
        AppServiceOptions.Add("App Service Environment");
        AppServiceOptions.Add("Traditional App Service");
        PromptDialog.Choice(
            context,
            AfterAppServiceChoiceAsync,
            AppServiceOptions,
            "Is your App Service in an App Service Environment or is it a traditional App Service?",
            promptStyle: PromptStyle.Auto);
    }

    public async Task AfterAppServiceChoiceAsync(IDialogContext context, IAwaitable<string> argument)
    {
        var message = await argument;
        this.appService = new AppService();
        if (message == "App Service Environment")
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

    void AskAppServiceEnvironmentName(IDialogContext context)
    {
        PromptDialog.Text(
            context,
            GetAppServiceEnvironmentName,
            "What's the name of your App Service Environment?",
            "Please enter the name of your App Service Environment.");
    }

    public async Task GetAppServiceEnvironmentName(IDialogContext context, IAwaitable<string> argument)
    {
        var message = await argument;

        // Name has to be at least 2 characters in length, can't be longer than 39 characters. Can have letters, numbers, and dashes, but it can't start or end with a dash.
        if (RegexMethods.CheckAppServiceEnvironmentName(message))
        {
            await context.PostAsync($"The name {message} is a valid App Service Environment name.");

            this.appService.AseName = message;

            PromptDialog.Confirm(
                    context,
                    ConfirmAppServiceEnvironmentName,
                    $"Is {message} the correct name for your App Service Environment?",
                    $"Can you confirm if {message} is the name of your App Service Environment?",
                    promptStyle: PromptStyle.Auto);
        }
        else
        {
            await context.PostAsync($"{message} is not a valid App Service Environment name. Please enter a valid name.");
            AskAppServiceEnvironmentName(context);
        }
    }

    public async Task ConfirmAppServiceEnvironmentName(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Thank you for confirming the name of your App Service Environment!");
            await context.PostAsync("Let's get the name of your App Service.");
            AskAppServiceName(context);
        }
        else
        {
            await context.PostAsync("Let's get the right App Service Environment name.");
            AskAppServiceEnvironmentName(context);
        }
    }

    void AskAppServiceName(IDialogContext context)
    {
        PromptDialog.Text(
            context,
            GetAppServiceName,
            "What's the name of your App Service?",
            "Please enter the name of your App Service.");
    }

    public async Task GetAppServiceName(IDialogContext context, IAwaitable<string> argument)
    {
        var message = await argument;

        // Name has to be at least 2 characters in length, can't be longer than 60 characters. Can have letters, numbers, and dashes, but it can't start or end with a dash.
        if (RegexMethods.CheckAppServiceName(message))
        {
            await context.PostAsync($"The name {message} is a valid App Service name.");

            this.appService.AppServiceName = message;

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

    public async Task ConfirmAppServiceName(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Thank you for confirming the name of your App Service!");
            await context.PostAsync("Let's check if you are using Traffic Manager.");
            CheckForTrafficManager(context);
        }
        else
        {
            await context.PostAsync("Let's get the right App Service name.");
            AskAppServiceName(context);
        }
    }

    /* Need to figure out if I should have one of these for each type, ASE or traditional App Service. 
     * This way I can create a single instance of a class like ASEAppService, TradAppService, ASEAppServiceWithTM, and TradAppServiceWithTM. Still working on that
     * */
    void CheckForTrafficManager(IDialogContext context)
    {
        PromptDialog.Confirm(
                context,
                ConfirmUseOfTrafficManager,
                "Is your App Service an endpoint of a Traffic Manager?",
                "I'm sorry, I didn't understand that. Are you using Traffic Manager for this App Service?",
                promptStyle: PromptStyle.Auto);
    }

    public async Task ConfirmUseOfTrafficManager(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Your App Service is an endpoint of a Traffic Manager.");
            this.appService.UsingTM = true;
            AskTrafficManagerName(context);
        }
        else
        {
            await context.PostAsync("Your App Service is not an endpoint of a Traffic Manager.");
            this.appService.UsingTM = false;
            AskCustomHostname(context);
        }
    }

    void AskTrafficManagerName(IDialogContext context)
    {
        PromptDialog.Text(
            context,
            GetTrafficManagerName,
            "What's the name of the Traffic Manager in front of your App Service?",
            "Please enter the name of your Traffic Manager.");
    }

    public async Task GetTrafficManagerName(IDialogContext context, IAwaitable<string> argument)
    {
        var message = await argument;

        // Name has to be at least 1 character in length, can't be longer than 63 characters. Can have letters, numbers, and dashes, but it can't start or end with a dash.
        if (RegexMethods.CheckTrafficManagerName(message))
        {
            await context.PostAsync($"The name {message} is a valid Traffic Manager name.");

            this.appService.TmName = message;

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

    public async Task ConfirmTrafficManagerName(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Thank you for confirming the name of your Traffic Manager!");
            AskCustomHostname(context);
        }
        else
        {
            await context.PostAsync("Let's get the right Traffic Manager name.");
            AskTrafficManagerName(context);
        }
    }

    void AskCustomHostname(IDialogContext context)
    {
        PromptDialog.Text(
            context,
            GetCustomHostname,
            "What is the custom hostname that you are looking to check?",
            "Please enter the custom hostname you are trying to check.");
    }

    public async Task GetCustomHostname(IDialogContext context, IAwaitable<string> argument)
    {
        var message = await argument;

        // TODO: Check the hostname with a regular expression. Can have dashes, numbers, and letters plus deliminating periods. 
        // Labels - The individual levels of a domain. For example, www.microsoft.com has 3 labels: www, microsoft, and com.
        // Each label cannot start or end with a dash. Can start with a digit or number and end with a digit or number
        // Separate labels by periods. However, the whole hostname cannot end with a period.
        // Each label must be between 1 and 63 characters in length inclusive
        // Max character count for the hostname is 253 characters including deliminating periods

        /* if (message is valid){
         *     Confirm the name
         * }
         * else {
         *   do a context.PostAsync("The custom hostname entered is invalid. Please re-enter it");
         *   Send them back to AskCustomHostname
         *   }
         *   */

        await context.PostAsync($"The custom hostname you are checking is {message}");

        this.appService.CustomHostname = message;

        PromptDialog.Confirm(
                context,
                ConfirmCustomHostname,
                $"Is {message} the correct custom hostname?",
                $"Please confirm that {message} is the correct custom hostname.",
                promptStyle: PromptStyle.Auto);
    }

    public async Task ConfirmCustomHostname(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Thank you for confirming your custom hostname!");
            await ShowAppServiceInformation(context, null);
        }
        else
        {
            await context.PostAsync("Let's get the right custom hostname.");
            AskCustomHostname(context);
        }
    }

    public async Task ShowAppServiceInformation(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        await context.PostAsync("Here's all of the information on your App Service and custom hostname.");

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

    public void AskForConfirmationOfAppServiceInformation(IDialogContext context)
    {
        PromptDialog.Confirm(
                context,
                ConfirmAppServiceInformation,
                "Does all of the information on your App Service and custom hostname look correct?",
                "Please confirm that the information on your App Service and custom hostname is correct.",
                promptStyle: PromptStyle.Auto);
    }

    public async Task ConfirmAppServiceInformation(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Thank you for confirming all of the information in regards to your App Service and custom hostname.");
            context.Wait(MessageReceivedAsync);
        }
        else
        {
            //await context.PostAsync("Let's get the right custom hostname.");
            context.Wait(MessageReceivedAsync);
        }
    }

    // This is my default end point for now. Might have to insert this in other parts of the dialog to allow the user to restart after certain steps
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
    
    public async Task AfterRestartAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Let's restart.");
            await CheckAppServiceType(context, null);
        }
        else
        {
            await context.PostAsync("We'll continue checking the domain.");
            context.Wait(MessageReceivedAsync);
        }
    }
}