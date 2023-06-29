using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PetProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;
using PetProject.Common;
using Microsoft.Azure.Cosmos.Linq;
using System.Net;
using System.Linq;
using PetProject.Helper;

namespace EmployeeFunctions
{
    public static class EmployeeFunction
    {
        private static readonly string CosmosDbConnectionString =
            Environment.GetEnvironmentVariable("CosmosDBConnectionString");
        private static readonly string DatabaseName = "OrganizationDB";
        private static readonly string ContainerName = "Employees";

        //For creating employee

        [FunctionName("CreateEmployee")]
        public static async Task<Response> CreateEmployee(
            [HttpTrigger(AuthorizationLevel.Anonymous, HttpMethodTypes.POST, Route = Routes.CreateEmployeeRoute)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var employee = JsonConvert.DeserializeObject<Employee>(requestBody);

                // Validate the employee using the helper class
               
                var validationErrors = EmployeeValidation.ValidateEmployee(employee);
                if (validationErrors.Count > 0)
                    return new EntityResponse<dynamic>() {Data=validationErrors,Success=false };
                
                DateTime date;

                if (DateTime.TryParseExact(employee.DOB, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    string formattedDate = date.ToString("dd-MM-yyyy");  
                    string EmployeeDob = formattedDate.Replace("-", "/");
                    employee.DOB = EmployeeDob;
                }


               employee.Age = CalculateAge.EmployeeAge(employee.DOB);

                // Check if an employee with the same ID already exists
                using (var cosmosClient = new CosmosClient(CosmosDbConnectionString))
                {
                    var container = cosmosClient.GetContainer(DatabaseName, ContainerName);
                    var query = container.GetItemLinqQueryable<Employee>()
                        .Where(e => e.Id == employee.Id)
                        .Select(e => e.Id);

                    var iterator = query.ToFeedIterator();
                    if (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        if (response.Resource.Any())
                        {
                            return new Response { Message = "An employee with the same ID already exists. Please use another ID.", Success = false};
                        }
                    }
                }
                if (employee != null)
                {
                    using (var cosmosClient = new CosmosClient(CosmosDbConnectionString))
                    {
                        var container = cosmosClient.GetContainer(DatabaseName, ContainerName);
                        var successResponse= await container.CreateItemAsync(employee);
                    }
                }
                //var successResponse = new
                //{
                //    Message = $"Employee with ID {employee.Id} created successfully",
                //    Employee = employee
                //};
                return new EntityResponse<Employee>() { Message= $"Employee with ID {employee.Id} created successfully",Success = true };


            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error creating employee");
                return new Response { ErrorCode= StatusCodes.Status500InternalServerError,Success=false };
            }
        }

        // For getting all Employees

        //[FunctionName("GetAllEmployees")]
        //public static async Task<IActionResult> GetAllEmployees(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, HttpMethodTypes.GET, Route = Routes.GetAllEmployeesRoute)] HttpRequest req,
        //    ILogger log)
        //{
        //    try
        //    {              
        //        using (var cosmosClient = new CosmosClient(CosmosDbConnectionString))
        //        {
        //            var container = cosmosClient.GetContainer(DatabaseName, ContainerName);
        //            var employees = new List<Employee>();
        //            var query = container.GetItemLinqQueryable<Employee>();
        //            var iterator = query.ToFeedIterator();
        //            while (iterator.HasMoreResults)
        //            {
        //                var response = await iterator.ReadNextAsync();
        //                employees.AddRange(response.Resource);
        //            }

        //            return new OkObjectResult(employees);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.LogError(ex, "Error retrieving employees");
        //        return new NotFoundObjectResult(StatusCodes.Status500InternalServerError);
        //    }
        //}

        //// Getting Employee by Id

        //[FunctionName("GetEmployeeById")]
        //public static async Task<Response> GetEmployeeById(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, HttpMethodTypes.GET, Route = Routes.GetEmployeeByIdRoute)] HttpRequest req,
        //    string id,
        //    ILogger log)
        //{
        //    try
        //    {
        //        using (var cosmosClient = new CosmosClient(CosmosDbConnectionString))
        //        {
        //            var container = cosmosClient.GetContainer(DatabaseName, ContainerName);
        //            var response = await container.ReadItemAsync<Employee>(id, new PartitionKey(id));
        //            var employee = response.Resource;

        //            return new EntityResponse<Employee>() { Success = true,Data=employee };
        //        }
        //    }
        //    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        //    {
        //        return new Response { Message="Employee with the specified ID doesn't exists" };
        //    }

        //    catch (Exception ex)
        //    {
        //        log.LogError(ex, $"Error retrieving employee with ID: {id}");
        //        return new Response { ErrorCode = StatusCodes.Status500InternalServerError, Success = false };
        //    }
        //}

        //Updating Employee

        //[FunctionName("UpdateEmployee")]
        //public static async Task<Response> UpdateEmployee(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, HttpMethodTypes.PUT, Route = Routes.UpdateEmployeeRoute)] HttpRequest req,
        //    string id,
        //    ILogger log)
        //{
        //    try
        //    {
        //        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //        var updatedEmployee = JsonConvert.DeserializeObject<Employee>(requestBody);

        //        if (updatedEmployee == null)
        //        {
        //            return new Response { Message = "Invalid employee data provided" };
        //        }
        //        // Validate the employee using the helper class

        //        var validationErrors = EmployeeValidation.ValidateEmployee(updatedEmployee);
        //        if (validationErrors.Count > 0)
        //            return new EntityResponse<dynamic>() { Data = validationErrors, Success = false };

        //        //DateTime date;

        //        //if (DateTime.TryParseExact(updatedEmployee.DOB, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        //        //{
        //        //    string formattedDate = date.ToString("dd-MM-yyyy");
        //        //    string EmployeeDob = formattedDate.Replace("-", "/");
        //        //    updatedEmployee.DOB = EmployeeDob;
        //        //}
        //        updatedEmployee.Age = CalculateAge.EmployeeAge(updatedEmployee.DOB);

        //        using (var cosmosClient = new CosmosClient(CosmosDbConnectionString))
        //        {
        //            var container = cosmosClient.GetContainer(DatabaseName, ContainerName);
        //            var response = await container.ReplaceItemAsync(updatedEmployee, id, new PartitionKey(id));

        //            if (response.StatusCode == HttpStatusCode.OK)
        //            {

        //                return new EntityResponse<Employee>() {Data=response };

        //            }
        //            else
        //            {
        //                return new Response { ErrorCode = StatusCodes.Status500InternalServerError, Success = false };
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.LogError(ex, $"Error updating employee with ID: {id}");
        //        return new Response { ErrorCode= StatusCodes.Status500InternalServerError,Success=false };
        //    }
        //}

        // Deleting Employee

        [FunctionName("GetAllEmployees")]
        public static async Task<IActionResult> GetAllEmployees(
           [HttpTrigger(AuthorizationLevel.Anonymous, HttpMethodTypes.GET, Route = "employee/allemployees")] HttpRequest req,
           ILogger log)
        {
            try
            {
                using (var cosmosClient = new CosmosClient(CosmosDbConnectionString))
                {
                    var container = cosmosClient.GetContainer(DatabaseName, ContainerName);
                    var employees = new List<Employee>();
                    var query = container.GetItemLinqQueryable<Employee>();
                    var iterator = query.ToFeedIterator();
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        employees.AddRange(response.Resource);
                    }

                    return new OkObjectResult(employees);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error retrieving employees");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        // Getting Employee by Id

        [FunctionName("GetEmployeeById")]
        public static async Task<IActionResult> GetEmployeeById(
            [HttpTrigger(AuthorizationLevel.Anonymous, HttpMethodTypes.GET, Route = "employee/{id}")] HttpRequest req,
            string id,
            ILogger log)
        {
            try
            {
                using (var cosmosClient = new CosmosClient(CosmosDbConnectionString))
                {
                    var container = cosmosClient.GetContainer(DatabaseName, ContainerName);
                    var response = await container.ReadItemAsync<Employee>(id, new PartitionKey(id));
                    var employee = response.Resource;

                    return new OkObjectResult(employee);
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new NotFoundObjectResult($"Employee with the specified ID doesn't exists");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Error retrieving employee with ID: {id}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        //Updating Employee

        [FunctionName("UpdateEmployee")]
        public static async Task<Response> UpdateEmployee(
            [HttpTrigger(AuthorizationLevel.Anonymous, HttpMethodTypes.PUT, Route = "employee/edit/{id}")] HttpRequest req,
            string id,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updatedEmployee = JsonConvert.DeserializeObject<Employee>(requestBody);

                if (updatedEmployee == null)
                {
                    return new Response { Message = "Invalid employee data provided" };
                }
                // Validate the employee using the helper class
                var validationErrors = EmployeeValidation.ValidateEmployee(updatedEmployee);
                if (validationErrors.Count > 0)
                  
                return new EntityResponse<dynamic>() { Data = validationErrors, Success = false };
                updatedEmployee.Age = CalculateAge.EmployeeAge(updatedEmployee.DOB);

                using (var cosmosClient = new CosmosClient(CosmosDbConnectionString))
                {
                    var container = cosmosClient.GetContainer(DatabaseName, ContainerName);
                    var response = await container.ReplaceItemAsync(updatedEmployee, id, new PartitionKey(id));

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var updateResponse = new
                        {
                            Message = "Employee updated successfully",
                            UpdatedEmployee = updatedEmployee
                        };
                        return new Response { Success=true};
                    }
                    else
                    {
                        return new Response { ErrorCode= StatusCodes.Status500InternalServerError };
                    }
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return new Response { Message = "No employee exists with the specified ID" };
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Error updating employee with ID: {id}");
                return new Response { ErrorCode = StatusCodes.Status500InternalServerError };
            }
        }



        [FunctionName("DeleteEmployee")]
        public static async Task<Response> DeleteEmployee(
            [HttpTrigger(AuthorizationLevel.Anonymous, HttpMethodTypes.DELETE, Route = Routes.DeleteEmployeeRoute)] HttpRequest req,
            string id,
            ILogger log)
        {
            try
            {
                using (var cosmosClient = new CosmosClient(CosmosDbConnectionString))
                {
                    var container = cosmosClient.GetContainer(DatabaseName, ContainerName);
                    await container.DeleteItemAsync<Employee>(id, new PartitionKey(id));
                }

                return new Response {Message=$"Employee with ID {id} deleted successfully",Success=true};
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new Response { Message="No employee exists with the specified ID" };
            }
            //catch (Exception ex)
            //{
            //    log.LogError(ex, $"Error deleting employee with ID: {id}");
            //    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            //}
        }
    }
}
