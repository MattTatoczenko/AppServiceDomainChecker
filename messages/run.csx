#r "Newtonsoft.Json"
#load "..\Dialog\CheckerDialog.csx"

using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    // Initialize the azure bot
    using (BotService.Initialize())
    {
        // Deserialize the incoming activity
        string jsonContent = await req.Content.ReadAsStringAsync();
        var activity = JsonConvert.DeserializeObject<Activity>(jsonContent);
        
        // authenticate incoming request and add activity.ServiceUrl to MicrosoftAppCredentials.TrustedHostNames
        // if request is authenticated
        if (!await BotService.Authenticator.TryAuthenticateAsync(req, new [] {activity}, CancellationToken.None))
        {
            return BotAuthenticator.GenerateUnauthorizedResponse(req);
        }
        
        if (activity != null)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new CheckerDialog());
            }
            else if (activity.Type == ActivityTypes.DeleteUserData)
            {
                // User Data Deletion not implemented

            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (activity.MembersAdded.Any(o => o.Id == activity.Recipient.Id))
                {
                    ConnectorClient client = new ConnectorClient(new Uri(activity.ServiceUrl));

                    var reply = activity.CreateReply();

                    reply.Text = "Hello! I am the Azure App Service Domain Checker.\n\n";
                    reply.Text += $"First, by using this bot, you agree to my Privacy Statement and Terms of Service here: https://matttatoczenko.github.io/AppServiceDomainChecker/ \n\n";
                    reply.Text += "Type anything to start our interaction.";

                    await client.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else if (activity.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists not implemented
            }
            else if (activity.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing not implemented
            }
            else if (activity.Type == ActivityTypes.Ping)
            {
                // Handle pings not implemented
            }
        }
        return req.CreateResponse(HttpStatusCode.Accepted);
    }    
}
