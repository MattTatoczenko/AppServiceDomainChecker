using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class EchoDialog : IDialog<object>
{
    protected int count = 1;
    protected bool StartOfChecker;
    protected List<String> AppServiceOptions = new List<String>();


    public Task StartAsync(IDialogContext context)
    {
        try
        {
            StartOfChecker = true;
            AddItemsToAppServiceList();
            context.Wait(MessageReceivedAsync);
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
        /*
        if (message.Text == "reset")
        {
            PromptDialog.Confirm(
                context,
                AfterResetAsync,
                "Are you sure you want to reset the count?",
                "Didn't get that!",
                promptStyle: PromptStyle.Auto);
        }
        else
        {
            await context.PostAsync($"{this.count++}: You said {message.Text}");
            context.Wait(MessageReceivedAsync);
        } */
        if(StartOfChecker)
        {
            PromptDialog.Choice(
                context,
                AfterAppServiceChoiceAsync,
                AppServiceOptions,
                "Is your App Service in an App Service Environment or is it a traditional App Service?",
                promptStyle: PromptStyle.Auto);
        } else {
            await context.PostAsync($"{this.count++}: You said {message.Text}");
            context.Wait(MessageReceivedAsync);
        }
    }
    
    public void AddItemsToAppServiceList()
    {
        AppServiceOptions.Add("App Service Environment");
        AppServiceOptions.Add("Traditional App Service");
    }
    
    public async Task AfterAppServiceChoiceAsync(IDialogContext context, IAwaitable<string> argument)
    {
        var message = await argument;
        if (message == "App Service Environment")
        {
            StartOfChecker = false;
            await context.PostAsync("You're using an ASE. Let's continue with that.");
        }
        else
        {
            await context.PostAsync("You're using a regular App Service.");
        }
        context.Wait(MessageReceivedAsync);
    }

    /*
    public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            this.count = 1;
            await context.PostAsync("Reset count.");
        }
        else
        {
            await context.PostAsync("Did not reset count.");
        }
        context.Wait(MessageReceivedAsync);
    }
    */
}