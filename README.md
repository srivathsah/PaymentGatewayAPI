### Dependencies:
    To Build: .Net 6
    To Run: .Net 6 and docker for desktop or other similar local docker setups. docker-compose file is provided to run all the external dependencies like postgres, RabbitMQ and consul. Please run docker-compose up and then run the 3 projects (Payment.Api, Payment.Server and Payment.Historical.Server) from the solution either from command line using dotnet run or from visual studio. I included the projects in the docker compose as well however I ran into some issues while configuring orleans in the container. So I had to remove them. To send requests to the API, please use postman. I will provide the scripts for the same.

    Libraries: There are a number of open source libraries used in the exercise. Following are the most important.
    
    1. Microsoft Orleans: This is used in the downstream Payment.Server project to make the service highly scalable using the virtual actor concurrency model.
    2. Masstransit: This is used to orchestrate and abstract the message queues and the underlying transport and serialization mechanisms.
    3. Language-ext: This is used to introduce functional programming patterns wherever possible. I have not used it extensively as I would have liked. But, domain model validation usees this.
    4. SqlStreamStore: This is used to implement event sourcing.
    5. Okta: This is is not exactly a library. Instead of rolling out my own identity token service, I have setup an okta application that supports client credentials

    There are various other dependencies used in the project but I am not listing them here for brevity.

### Architecture:

    1. All the requests to intiate a payment needs to be sent to the API. API is protected by token based security. Only the known merchants can send the requests. The merchants are currently hard coded in okta service. But ideally there can be a separate Merchant service.
    2. API sends the request to the downstream Payment Server which is based on Micrsoft Orleans. The 2 main actors in the service are PaymentGatewayGrain and PaymentRequestGrain. The former is the main actor which is responsible for initialising a payment request and the latter is responsible for dealing with any tasks sepcific to a payment request. The number of instances of PaymentGatewayGrain is equal to the number of merchants. The number of PaymentRequestGrain is equal to the active concurrent payment processes. API queries the Historical Service to get the list of transactions.
    3. All the business logic are in the domain layer (Payment project) and the types that are shared among the projects are in Payment.Contracts project. The domain rules are specified in the form of Commands, Events and State
    4. In addition to executing the commands from the API or the internal commands, the actors also co-ordinate the event sourcing mechanism. The above mentioned actors save the domain events to the event store. Any newly added event is then read by CommitedEventsConsumerGrain and the published onto the message queue
    5. Historical Server consumes the events published in the message queue and creates a projection and saves the data in the DB that can be later queried. This is a regular stateles HTTP based REST API that follows n-tier architecture.
    6. As it is already evident, the system tries to implement eventual consistency with asynchronous task based flow from a merchant's perspective. Therefore there needs to be a mechanism of notifying the merchants after a payment is processed. I have not been able to include it in this exercise.


### Other points and assumptions:

    1. It is ideal to view the code in visual studio as I have categorized the projects into Payments (main projects along with test projects that are directly related to the exercise) and Shared (small utility projects).
    2. I have used passwords and connections trings directly in the appsettings instead of a secure vault for simplicity.
    3. Instead of writing extensive set of test cases and scenarios, I have tried to cover as many varieities of tests possible.
    4. With more time in hand, I could have tried to do add caching to the transactions, adding Polly to add retry/circuit breaker logic while calling the Historical Service from API and also while calling Bank service from Payment Server, etc. Also, I thought of adding more XML comments but I have run out of time.
    5. Most of the utility code has been reused from my side projects although I have changed few things quite a bit.
    6. The domain models and the logic are very simplistic and I understand they will no way represent anything evn close the real world models. But I felt it is a good way to demonstrate simple domain modelling with the use of records.
    7. With the current architecture and projects in place, I will use kubernetes to install the system, terraform to maintain the infrastructure so that there wont be any vendor lock-in. Depending on where the system is deployed, we can use the message queues and database accordingly. For example, if the system is run on AWS, then we can SQS and SNS and if it is run on Azure we can use Azure Service bus. Masstransit helps us in abstracting these technologies easily.


### Snippets:

    Sample request body
``` {
    "shopperId": {
        "value": "test-shopper"
    },
    "card": {
        "number": {
            "value": "1234567891234567"
        },
        "expiry": {
            "month": {
                "value": 10
            },
            "year": {
                "value": 2022
            }
        },
        "cvv": {
            "value": 100
        }
    },
    "amount": {
        "value": 10.0
    },
    "currency": {
        "value": "GBP"
    }
}
```

ClientId: 0oa2k4h6bd86aevzf5d7
ClientSecret: rhR7QpC_QsRyen_fQdw8-2jQB6AhZ_w0xxaWNNH8
TokenURL: https://dev-2224837.okta.com/oauth2/default/v1/token
Scope: access_token