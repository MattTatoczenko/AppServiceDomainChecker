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
    protected bool StartOfChecker;

    public Task StartAsync(IDialogContext context)
    {
        try
        {
            StartOfChecker = true;
            context.Wait(CheckAppServiceType);
            //context.Wait(MessageReceivedAsync);
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

    public async Task CheckAppServiceType(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
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
        this.StartOfChecker = false;
        if (message == "App Service Environment")
        { 
            await context.PostAsync("You're using an ASE. Let's continue with that.");
        }
        else
        {
            await context.PostAsync("You're using a regular App Service.");
        }
        context.Wait(MessageReceivedAsync);
    }

    
    public async Task AfterRestartAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            this.StartOfChecker = true;
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