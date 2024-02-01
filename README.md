# Microsoft Graph Notification Broker
This is a proof of concept for Microsoft Graph Notification Broker with function based on [Microsoft Documentation](https://learn.microsoft.com/en-us/graph/change-notifications-delivery-event-hubs?tabs=change-notifications-eventhubs-azure-cli%2Chttp#using-azure-event-hubs-to-receive-change-notifications/).

The base idea in this proof of concept is to subscribe on receiving outlook messages and renew the subscription every day automatically without the need of user interaction.

To do this we need 4 things:
1. Create an event hub resource and an event hub service insde.
2. Create a key vault that that will contain the connection to our event hub service.
3. Create a app registration.
4. Create a function app to handle subscription and receiving messages.

## Create an event hub
Create an event hub resource and inside create event hub. In the Shared Access Policy add a new policy and copy the connection string for later.

## Create a key vault
Create a key vault that will contain the connection string that you copied. This secret will be used by microsoft graph to send the notifications.
Create an access policy. For Secret permissions, select Get, and for Select Principal, select Microsoft Graph Change Tracking. 
Copy the link from the vault because we are going to need it in our function app.

## Create a app registration
After we create the vault we need to register our app. After the registration is created go to the "Certificates & secrets" tab and create the secret. 
Copy the value of this sicret because we are going to need it in our funtion app. After this we need to do one more thing in the registered app, we need to add api permission.
Because we don't want to have user interaction (user login and mannual approval by the user for the app to subscribe), delegated permissions won't work. 
We need to add an application permission for Mail.Read and the admin consent is required for this. 
If you are the admin you should add also access policy so that you can grant an access for specific e-mails. [Limiting application permissions](https://learn.microsoft.com/en-us/graph/auth-limit-mailbox-access)

## Create the function app
For the function app we need to create two functions one EventHubTrigger, and one TimerTrigger.

## References
- [Microsoft Documentation](https://learn.microsoft.com/en-us/graph/change-notifications-delivery-event-hubs?tabs=change-notifications-eventhubs-azure-cli%2Chttp#using-azure-event-hubs-to-receive-change-notifications/).
- [Limiting application permissions](https://learn.microsoft.com/en-us/graph/auth-limit-mailbox-access)

## Issues
To view or log issues see [issues](https://github.com/cdngmnks/actionable-messages-backend-dotnet/issues).

## License
Copyright (c) codingmonkeys doo. All Rights Reserved. Licensed under the [MIT License](https://github.com/cdngmnks/msgraph-notification-broker-dotnet/blob/main/LICENSE).
