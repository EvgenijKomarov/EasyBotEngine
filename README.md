A very simple class for processing messages in text bots. 
To get started, simply create your own Node class descendants, which must be placed in the engine using the AddNode method. To process a message, call the ProcessMessage method and pass an instance of the abstract MessageInput class.

To create an engine, you need to pass a Node subclass to the constructor, which will be processed by default if no verificators are found.

Any Nodes inside the engine have verificators, which ensure that the message is correctly passed to the handler. When processing a message, the Nextverificator within the Node can be used to pass the message context to the next Node. 
The process ends only if the last Node's verificator is empty.

When creating a Node subclass, you must override the Getverificators methods. This is necessary so that the engine can properly handle the message processing chain. Note that a Node may have more than one verificator.
You also need to override the Invoke. method, which is the method that handles the message and populates attributes such as Text and Buttons. 
If you want to pass the processing to another Node, you need to set the verificator for the new Node and optionally pass the context to NextData.

You can also create your own MessageInput descendants that override the Verificator and Data methods.
