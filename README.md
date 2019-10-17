# Analayse Error nServiceBus error Queues using the Azure Service Bus transport
When using nServiceBus with the Azure Service Bus transport, you will use one or more error queues (where poisoned messages are placed if handlers fail to process them).  If you are unfortunate enough to get hight numbers of errors, expecially if you use shared error queues, it can be tricky to determine exactly what kinds errors you actually have.  Sure, you can use tools like Azure Service Bus explorer to see message counts and also view individual messages, but you have to look in detail at each message yourself to see details - this is not practical for larger error numbers.

You point this tool at your service bus namespace and give it a list of error queues.  I will peek messages in those queues (will not remove / change state of the messages on those queues), and will produce an output summarising what it finds.  At the moment, you can chose one of two sinks (destinations) for the output.

+ a set of pipe delimited files (one per file checked) which you can open and analyse in Excel or similar
+ an Azure Log Analytics destination.  Which you can query directly, or setup a dashboard (such as Grafana) to view.

If you run it regularly (either manually, or change it to an Azure Function or similar), you will build up a historic view of your error queues.

## The project

**das.sfa.tools.AnalayseErrorQueues.console** A simple console project, to allow you to run the tool easily at the command line.

**das.sfa.tools.AnalayseErrorQueues.Engine** Provides the process which pulls data and sends it to outputs

**das.sfa.tools.AnalayseErrorQueues.services** Provides services which encapsulate access to Azure Service Bus, and also outputs (file generation / log analytics)

**das.sfa.tools.AnalayseErrorQueues.domain** Simple POCO objects to provide domain abstractions over the nServiceBus message properties.

## Configuration
The program will look at for two config files.

+ appsettings.json
+ appsettings.local.json

and will load settings from both.

The _appsettings.local.json_ file is excluded from Git check in (via gitignore) and it is recommended that you create one of these locally for your sensitive settings.  If you look at the included _appsettings.json_ file, it shows you what settings you need to provide.  Simply copy _appsettings.json_ to _appsettings.local.json_ and update as necessary with your information, you do not need to modify the _appsettings.json_ (which does get checked in).  This will ensure you do not check in any sensitive information.