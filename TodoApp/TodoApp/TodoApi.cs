using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using TodoApp.Model;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using System.Net;

public static class TodoApi

{

    private static string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=todomanager;AccountKey=Yw9+CrXByis7sn8A6OOtWsvoeDZ2bxBjvSWvxYkizy3GXUwjWEa/hnAHw2NTYqnxJfItDBYV/+1W+AStB4dUkQ==;EndpointSuffix=core.windows.net";




    [FunctionName("CreateTodo")]

    public static async Task<IActionResult> CreateTodo(

        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "todo")] HttpRequest req, ILogger log)

    {

        log.LogInformation("C# HTTP trigger function processed a request.");


        // Enable CORS

        SetCorsHeaders(req, log);


        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        var data = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);


        // Create a CloudTable instance

        CloudTable table = await GetTableAsync();


        var todo = new Todo

        {

            TaskDescription = data.TaskDescription,

            IsCompleted = false

        };


        // Insert the Todo entity into the table

        var insertOperation = TableOperation.Insert(todo);

        await table.ExecuteAsync(insertOperation);


        return new OkObjectResult(todo);

    }


    [FunctionName("GetTodos")]
    public static async Task<IActionResult> GetTodos(
[HttpTrigger(AuthorizationLevel.Function, "get", Route = "todo")] HttpRequest req,
ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");
        SetCorsHeaders(req, log);

        // Create a CloudTable instance
        CloudTable table = await GetTableAsync();

        // Query all Todo entities from the table
        TableQuery<Todo> query = new TableQuery<Todo>();
        var todos = await table.ExecuteQuerySegmentedAsync(query, null);

        // Disable caching
        var response = new OkObjectResult(todos);
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentTypes.Add("application/json");

        if (req.HttpContext.Response is HttpResponse responseMessage)
        {
            responseMessage.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            responseMessage.Headers["Pragma"] = "no-cache";
            responseMessage.Headers["Expires"] = "0";
        }

        return response;
    }


    [FunctionName("GetTodoById")]

    public static async Task<IActionResult> GetTodoById(

        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "todo/{id}")] HttpRequest req,

        ILogger log, string id)

    {

        log.LogInformation("C# HTTP trigger function processed a request.");

        SetCorsHeaders(req, log);


        // Create a CloudTable instance

        CloudTable table = await GetTableAsync();


        // Retrieve the Todo entity based on partition key ("Todos") and row key (id)

        var retrieveOperation = TableOperation.Retrieve<Todo>("Todos", id);

        var result = await table.ExecuteAsync(retrieveOperation);

        var todo = (Todo)result.Result;


        if (todo == null)

        {

            return new NotFoundResult();

        }


        return new OkObjectResult(todo);

    }


    [FunctionName("UpdateTodo")]

    public static async Task<IActionResult> UpdateTodo(

        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "todo/{id}")] HttpRequest req,

        ILogger log, string id)

    {

        log.LogInformation("C# HTTP trigger function processed a request.");

        SetCorsHeaders(req, log);


        // Create a CloudTable instance

        CloudTable table = await GetTableAsync();


        // Retrieve the Todo entity based on partition key ("Todos") and row key (id)

        var retrieveOperation = TableOperation.Retrieve<Todo>("Todos", id);

        var result = await table.ExecuteAsync(retrieveOperation);

        var todo = (Todo)result.Result;


        if (todo == null)

        {

            return new NotFoundResult();

        }


        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        var data = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);


        // Update the Todo entity with the new values

        todo.TaskDescription = data.TaskDescription;

        todo.IsCompleted = data.IsCompleted;


        // Update the Todo entity in the table

        var updateOperation = TableOperation.Replace(todo);

        await table.ExecuteAsync(updateOperation);


        return new OkObjectResult(todo);

    }


    [FunctionName("DeleteTodo")]

    public static async Task<IActionResult> DeleteTodo(

        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "todo/{id}")] HttpRequest req,

        ILogger log, string id)

    {

        log.LogInformation("C# HTTP trigger function processed a request.");

        SetCorsHeaders(req, log);


        // Create a CloudTable instance

        CloudTable table = await GetTableAsync();


        // Retrieve the Todo entity based on partition key ("Todos") and row key (id)

        var retrieveOperation = TableOperation.Retrieve<Todo>("Todos", id);

        var result = await table.ExecuteAsync(retrieveOperation);

        var todo = (Todo)result.Result;


        if (todo == null)

        {

            return new NotFoundResult();

        }


        // Delete the Todo entity from the table

        var deleteOperation = TableOperation.Delete(todo);

        await table.ExecuteAsync(deleteOperation);


        return new OkResult();

    }


    private static async Task<CloudTable> GetTableAsync()

    {

        // Parse the storage connection string

        CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=todomanager;AccountKey=Yw9+CrXByis7sn8A6OOtWsvoeDZ2bxBjvSWvxYkizy3GXUwjWEa/hnAHw2NTYqnxJfItDBYV/+1W+AStB4dUkQ==;EndpointSuffix=core.windows.net");


        // Create a CloudTableClient instance

        CloudTableClient tableClient = storageAccount.CreateCloudTableClient();


        // Get a reference to the "TodoItems" table

        CloudTable table = tableClient.GetTableReference("TodoItems");


        // Create the table if it doesn't exist

        await table.CreateIfNotExistsAsync();


        return table;

    }


    private static void SetCorsHeaders(HttpRequest req, ILogger log)

    {

        var headers = req.Headers;

        if (headers.ContainsKey("Origin"))

        {

            var origin = headers["Origin"].FirstOrDefault();

            log.LogInformation($"Setting Access-Control-Allow-Origin header: {origin}");

            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", origin);

        }

    }

}
