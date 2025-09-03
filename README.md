#EN A simple engine for any business logic (developed for chatbots)

To create an engine object, you need to select 3 entities that the engine will use.

This is the object that the engine will accept for operation.
An intermediate entity that will be used inside the engine
The entity that the engine will return after completing all procedures.
In the constructor, you must pass 2 mappings of these entities: from incoming to intermediate, and intermediate to outgoing. You can also pass a logger from Microsoft.Extensions.Logging.

In order for the engine to build a chain of procedures for processing incoming data, Nodes must be created. To do this, create an heir from the Node class and add it to the engine using AddNode. This class needs to redefine methods such as GetIdentificators (so that data can get to this particular Node) and Invoke (the message processing process itself). When redefining the Invoke method, you need to return the result of the Next or Complete method at the end in order to move on to the next Node or complete the processing process altogether.

The engine supports Middlewares. The principle of creating Middleware is similar to creating Node - you also need to create your own heir from the Middleware class, add it to the engine using AddMiddleware, and redefine some properties. Now, before sending incoming data to the selected Node, the message will first go through all Middlewares. Inside the Invoke, you have the option to go directly to another Node by calling the MoveToNove method, and then the Middlewares playback chain will end. You can also redefine the Middleware application conditions by redefining the GetCondition method.

The principle of the engine is very simple. The engine starts working when the Process() method is called.Incoming data is converted to intermediate data, and the process of applying all Middleware to this data begins (only those that are applicable due to the GetCondition() property). After passing through all Middleware, the intermediate data along the chain goes between Nodes. The routing principle is based on GetIndentificators(). If any of the Nodes has completed the process, the intermediate data is converted to outgoing data and returned in the Process method.

#RU Простой движок для любой бизнес логики (разрабатывался для чат ботов)

Для создания объекта двигателя необходимо выбрать 3 сущности, которые будет использовать двигатель.

Это объект, который двигатель будет принимать для работы.
Промежуточная сущность, которая будет использоваться внутри двигателя
Сущность, которую двигатель вернёт после исполнения всех процедур.
В конструкторе необходимо передать 2 маппинга этих сущностей: из входящую в промежуточную, и промежуточную в исходящую. Также можно передать логгер из Microsoft.Extensions.Logging.

Для того, чтобы двигатель мог выстроить цепочку из процедур для обработки входящих данных, необходимо создать Nodes. Для этого нужно создать наследника от класса Node и добавить его в двигатель с помощью AddNode. Этому классу нужно переопределить такие методы, как GetIdentificators (для того, чтобы данные могли попасть именно в эту Node) и Invoke (сам процесс обработки сообщения). При переопределении метода Invoke Вам в конце необходимо вернуть результат метода Next или Complete, чтобы перейти к следующему Node или вовсе завершить процесс обработки.

Двигатель поддерживает Middlewares. Принцип создания Middleware схож с созданием Node - вам также нужно создать своего наследника от класса Middleware, добавить его в двигатель с помощью AddMiddleware, и переопределить некоторые свойства. Теперь, перед тем как отправить входящие данные в выбранную Node, сначала сообщение пройдёт через все Middlewares. Внутри Invoke у вас есть возможность перейти сразу к другой Node вызвав метод MoveToNove, и тогда цепочка воспроизведения Middlewares завершится. Также вы можете переопределить условия применения Middleware переопределив метод GetCondition.

Принцип работы двигателя очень прост. Работа двигателя начинается при вызове метода Process().Входящие данные преобразуются в промежуточные, и начинается процесс применения всех Middleware к этим данным (только те, которые применимы благодаря свойству GetCondition()). После прохождения всех Middleware промежуточные данные по цепочке идут между Nodes. Принцип маршрутизации основывается на GetIndentificators(). Если какая-либо из Node завершила процесс, то промежуточные данные преобразуются в исходящие и возвращаются в методе Process.

Lib available on Nuget https://www.nuget.org/packages/EasyBotEngine/
