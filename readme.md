# Azure App Service Domain Checker
This GitHub repository holds the source code for the Azure Bot Service called Azure App Service Domain Checker. Read further on to find out what this Bot Service does.
For more information on Azure Bot Service, see this link: [Azure Bot Service](https://docs.botframework.com/en-us/azure-bot-service/).

## Privacy Policy
Our Azure App Service Domain Checker is enabled by Microsoft Bot Framework. The Microsoft Bot Framework is a set of web-services that enable intelligent services and connections using conversation channels you authorize. As a service provider, Microsoft will transmit content you provide to our bot/service in order to enable the service. For more information about Microsoft privacy policies please see their privacy statement here: [Microsoft Privacy Policy](http://go.microsoft.com/fwlink/?LinkId=521839). In addition, your interactions with this bot/service are also subject to the conversational channel's applicable terms of use, privacy and data collection policies. To report abuse when using a bot that uses the Microsoft Bot Framework to Microsoft, please visit the Microsoft Bot Framework website at [Bot Framework Site](https://www.botframework.com) and use the “Report Abuse” link in the menu to contact Microsoft.

## Terms of Service
To view the Terms of Service for this bot, please review the [Microsoft Services Agreement](https://www.microsoft.com/en-us/servicesagreement/).

## What does this bot do?
This bot is meant to target specific information in regards to an Azure App Service. Click the following link to learn more about [Azure App Service](https://docs.microsoft.com/en-us/azure/app-service/app-service-value-prop-what-is).

The main purpose is to check a custom hostname and see if the DNS settings are correctly configured so that this hostname can be used on an Azure App Service.

## What information does the bot need to do these checks?
The bot will ask you for the following information:

* Whether you are using an [App Service Environment](https://docs.microsoft.com/en-us/azure/app-service-web/app-service-app-service-environment-intro). If you are, it will ask for the name of the App Service Environment.
* The name of the App Service.
* Whether the App Service is an endpoint of an [Azure Traffic Manager](https://docs.microsoft.com/en-us/azure/traffic-manager/traffic-manager-overview). If so, it will ask for the name of the Traffic Manager.
* The custom hostname.

The bot will not save this information, ensuring that each check pulls the latest available public data on the App Service and the custom hostname.

## How does it check the hostname?
Hostname and DNS lookups are done using a C#/.Net library called ARSoft.Tools.Net. This library provides DNS lookup and resolution abilities through C# directly, making the process very easy to create.
For more information on the library, visit the [ARSoft.Tools.Net](http://arsofttoolsnet.codeplex.com/) page hosted on CodePlex.

## Limitations on input
The bot makes some assumptions on the various inputs it receives. 

### App Service Environment name
* The App Service Environment must be a public App Service Environment. This Bot does not check ILB App Service Environments.
* The name has to be at least 2 characters in length but it cannot be longer than 39 characters. 
* The name can have letters, numbers, and dashes, but it cannot start or end with a dash.

For example, the App Service Environment name of "matt" is valid. However, the names "m", "m-", and "-m" are not valid.

### App Service name
* The name has to be at least 2 characters in length but cannot be longer than 60 characters. 
* The name can have letters, numbers, and dashes, but it cannot start or end with a dash.

The App Service name requirement is the same as the App Service Environment requirement except for the maximum length of the App Service name.

### Traffic Manager name
* The name has to be at least 1 character in length but it cannot be longer than 63 characters. 
* The name can have letters, numbers, and dashes, but it cannot start or end with a dash.

The Traffic Manager name requirements directly follow the requirements for a hostname laid out in [RFC1123](https://tools.ietf.org/html/rfc1123#page-13).

### Custom hostname
The requirements for the hostname come directly from [RFC1123](https://tools.ietf.org/html/rfc1123#page-13) and from this blog post in regards to maximum length: https://blogs.msdn.microsoft.com/oldnewthing/20120412-00/?p=7873/.

* Each label has to be at least 1 character in length but they cannot be longer than 63 characters. 
* Each label can have letters, numbers, and dashes, but they cannot start or end with a dash.
* Each label is separated by a period ('.') character.
* The total hostname cannot be longer than 253 characters.

## Have questions or issues?
For any issues you find, please file an issue on the [GitHub Issues](https://github.com/MattTatoczenko/AppServiceDomainChecker/issues) page.
