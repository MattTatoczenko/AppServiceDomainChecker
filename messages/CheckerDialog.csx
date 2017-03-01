#load "..\AppService.cs"

using System;
using System.Collections;
using System.Collections.Generic;
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

        // TODO: do some checks on the name. Shouldn't have periods. Can have dashes, letters, and numbers
        /* if (message is valid){
         *     Confirm the name
         * }
         * else {
         *   do a context.PostAsync("The App Service Environment name entered is invalid. Please re-enter it");
         *   Send them back to AskAppServiceEnvironmentName
         *   }
         *   */

        await context.PostAsync($"The name of your App Service Environment is {message}");

        // TODO: Set AseName = message

        PromptDialog.Confirm(
                context,
                ConfirmAppServiceEnvironmentName,
                $"Is {message} the correct name for your App Service Environment?",
                $"Can you confirm if {message} is the name of your App Service Environment?",
                promptStyle: PromptStyle.Auto);
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

        // TODO: do some checks on the name. Shouldn't have periods. Can have dashes, letters, and numbers. Possibly has parentheses too
        /* if (message is valid){
         *     Confirm the name
         * }
         * else {
         *   do a context.PostAsync("The App Service name entered is invalid. Please re-enter it");
         *   Send them back to AskAppServiceName
         *   }
         *   */

        await context.PostAsync($"The name of your App Service is {message}");

        // TODO: Set AppServiceName = message

        PromptDialog.Confirm(
                context,
                ConfirmAppServiceName,
                $"Is {message} the correct name for your App Service?",
                $"Please confirm that {message} is the name of your App Service.",
                promptStyle: PromptStyle.Auto);
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
            // TODO: Set usingTM to true
            // TODO: Send to the next method which asks for the Traffic Manager name
        }
        else
        {
            await context.PostAsync("Your App Service is not an endpoint of a Traffic Manager.");
            // TODO: Set usingTM to false
            // TODO: Path should go right into the domain checker prompt here
        }
        context.Wait(MessageReceivedAsync);
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