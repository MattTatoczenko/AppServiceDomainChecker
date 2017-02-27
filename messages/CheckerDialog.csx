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
        if (message == "App Service Environment")
        {
            await context.PostAsync("You're using an ASE. Let's continue with that.");

            PromptDialog.Text(
                context,
                GetAppServiceEnvironmentName,
                "What's the name of your App Service Environment?",
                "Please enter the name of your App Service Environment.");
        }
        else
        {
            await context.PostAsync("You're using a regular App Service.");

            // Have a similar prompt here to ask for the App Service Name. Need to figure this out better.

            await CheckForTrafficManager(context);
        }
        
    }

    public async Task GetAppServiceEnvironmentName(IDialogContext context, IAwaitable<string> argument)
    {
        var message = await argument;

        // do some checks on the name. Shouldn't have periods. Can have dashes, letters, and numbers

        // Might want to use a "confirm" prompt here to be sure they entered it right. If not, we'll have to loop back somewhere for them to re-enter the name
        await context.PostAsync($"The name of your ASE is {message}");

        // Should probably go to "CheckForTrafficManager" next. Need to figure out the best way to do this

        context.Wait(MessageReceivedAsync);
    }

    /* Need to figure out if I should have one of these for each type, ASE or traditional App Service. 
     * This way I can create a single instance of a class like ASEAppService, TradAppService, ASEAppServiceWithTM, and TradAppServiceWithTM. Still working on that
     * */
    public async Task CheckForTrafficManager(IDialogContext context)
    {
        await context.PostAsync("Let's check if you are using Traffic Manager.");

        PromptDialog.Confirm(
                context,
                AfterCheckForTrafficManager,
                "Is your App Service an endpoint of a Traffic Manager?",
                "I'm sorry, I didn't understand that. Are you using Traffic Manager for this App Service?",
                promptStyle: PromptStyle.Auto);
    }

    public async Task AfterCheckForTrafficManager(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            await context.PostAsync("Your App Service is an endpoint of a Traffic Manager.");
            // Insert path to go to after this
        }
        else
        {
            await context.PostAsync("Your App Service is not an endpoint of a Traffic Manager.");
            // Insert path to go to after this
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
                "Are you sure you want to go back to the beginning??",
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