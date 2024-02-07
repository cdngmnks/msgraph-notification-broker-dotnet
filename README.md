# Microsoft Graph Notification Broker
This is a proof of concept for Microsoft Graph Notification Broker with function based on [Microsoft Documentation](https://learn.microsoft.com/en-us/graph/change-notifications-delivery-event-hubs?tabs=change-notifications-eventhubs-azure-cli%2Chttp#using-azure-event-hubs-to-receive-change-notifications/).

The base idea in this proof of concept is to subscribe on receiving outlook messages and renew the subscription every day automatically without the need of user interaction.

To do this we need 4 things:
1. Create an event hub resource and an event hub service insde.
2. Create a key vault that that will contain the connection to our event hub service.
3. Create a app registration.
4. Create azure cache for redis
5. Create a function app to handle subscription and receiving messages.

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

## Create azure cache for redis
Create an azure cache for redis service on azure. Go to the Authentication tab and copy the connection string. If we want to use this cache in our function app we are going to need this connection string.

## Create the function app
In this solution we have two separated, function apps. One is for managing the subscription (create, renew and delete the subscription) and one is for receiving the messages from the event hub. To connect to the event hub all we need is to use EventHubTrigger function and set the connection string that we copied from the event hub as a connection string in this event hub. If we want to use EventHubTrigger we need to install Microsoft.Azure.WebJobs.Extensions.EventHubs package. For the other functions app we need a TimerTrigger function which acts like a cron job that makes a subscription renewal. To use this TimerTrigger we need to install the Microsoft.Azure.WebJobs.Extensions package. There are few things that we need here we need the link from the keyvault that we created to send to MicrosoftGraph as a NotificationUrl, also we need the secret key that we created in the app registration for the client authentication part. Also we need the client id and the tenant id that we can get from the azure portal, and finnaly our redis connection string to use the azure cache. The details for how to connect, add and delete cache entry are in the Services folder. In our function app we only use these methods to add, read or delete a chache entry which in our case is a subscription. 

## References
- [Microsoft Documentation](https://learn.microsoft.com/en-us/graph/change-notifications-delivery-event-hubs?tabs=change-notifications-eventhubs-azure-cli%2Chttp#using-azure-event-hubs-to-receive-change-notifications/).
- [Limiting application permissions](https://learn.microsoft.com/en-us/graph/auth-limit-mailbox-access)
- [Microsoft Solution Playbook](https://playbook.microsoft.com/code-with-fusionops/Enterprise-Solutions/Collaborative-Apps/GraphNotificationBroker/)
- [Notification Broker Example Github](https://github.com/microsoft/GraphNotificationBroker)

## Issues
To view or log issues see [issues](https://github.com/cdngmnks/actionable-messages-backend-dotnet/issues).

## License
Copyright (c) codingmonkeys doo. All Rights Reserved. Licensed under the [MIT License](https://github.com/cdngmnks/msgraph-notification-broker-dotnet/blob/main/LICENSE).
