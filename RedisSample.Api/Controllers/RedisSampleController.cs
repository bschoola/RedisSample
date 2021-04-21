using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RedisSample.Api.Model;
using RedisSample.Api.Model.Response;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RedisSample.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedisSampleController : ControllerBase
    {
        private readonly ILogger<RedisSampleController> _logger;

        public RedisSampleController(ILogger<RedisSampleController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetUsersWithoutRedis")]
        public async Task<IActionResult> GetUsersWithoutRedis()
        {
            //To Get the time that we will wait to get the response
            var watch = new Stopwatch();
            watch.Start();

            //Simulate code that will get the users from the service that provides the information
            var users = await GetUsers();

            //To stop counting the time
            watch.Stop();
            var response = new ResponseObject(users, watch.Elapsed.TotalSeconds);

            return Ok(response);
        }

        [HttpGet("GetUsersWithRedis")]
        public async Task<IActionResult> GetUsersWithRedis()
        {
            //To Get the time that we will wait to get the response
            var watch = new Stopwatch();
            watch.Start();

            //Try to get in REDIS (if exists)
            var usersCache = await GetUserInRedis<List<User>>("SampleRedisUsers");

            var users = new List<User>();
            if (usersCache == null)
            {
                // If not exists, get again the data
                // Simulate code that will get the users from the service that provides the information
                users = await GetUsers();

                //Save in Redis
                await SetUserInRedis(users);
            }
            else
            {
                users = usersCache;
            }

            //To stop counting the time that we will wait to get the response
            watch.Stop();
            var response = new ResponseObject(users, watch.Elapsed.TotalSeconds);

            return Ok(response);
        }

        [HttpGet("CleanRedis")]
        public async Task<IActionResult> CleanRedis()
        {
            await DeleteInRedis("SampleRedisUsers");
            return Ok();
        }

        private async Task<Type> GetUserInRedis<Type>(string key)
        {
            using var conn = ConnectionMultiplexer.Connect("localhost");

            var output = await conn.GetDatabase().StringGetAsync(key);

            var result = JsonConvert.DeserializeObject<Type>(output.HasValue ? output.ToString() : string.Empty);

            return result;
        }

        private async Task SetUserInRedis(List<User> users)
        {
            var expire = new TimeSpan(1, 0, 0, 0); // 1 day to expire
            var input = string.Empty;
            if (users != null && users.Count > 0)
            {
                input = JsonConvert.SerializeObject(users);
            }

            using var conn = ConnectionMultiplexer.Connect("localhost");

            await conn.GetDatabase().StringSetAsync(new RedisKey("SampleRedisUsers"), new RedisValue(input), expire);
        }

        private async Task DeleteInRedis(string key)
        {
            using var conn = ConnectionMultiplexer.Connect("localhost");

            await conn.GetDatabase().KeyDeleteAsync(key);
        }

        private async Task<List<User>> GetUsers()
        {
            var users = new List<User>();

            users.Add(new User() { Id = 1, Name = "Renata Slovak", Age = 35 });
            users.Add(new User() { Id = 2, Name = "Leonardo Santos", Age = 37 });
            users.Add(new User() { Id = 3, Name = "Cleiton Havana", Age = 34 });
            users.Add(new User() { Id = 4, Name = "Michael Jordan", Age = 22 });
            users.Add(new User() { Id = 5, Name = "Carlos Alberto", Age = 62 });
            users.Add(new User() { Id = 6, Name = "Bruna Agostino", Age = 55 });
            users.Add(new User() { Id = 7, Name = "Mariana Ferreira", Age = 45 });
            users.Add(new User() { Id = 8, Name = "Alexandre Novaes", Age = 27 });
            users.Add(new User() { Id = 9, Name = "Daniel Texeira", Age = 29 });
            users.Add(new User() { Id = 10, Name = "Renan da Silva", Age = 28 });
            users.Add(new User() { Id = 11, Name = "Brian Oneil", Age = 34 });
            users.Add(new User() { Id = 12, Name = "Victoria Abrantes", Age = 43 });
            users.Add(new User() { Id = 13, Name = "Sabrina Springs", Age = 38 });
            users.Add(new User() { Id = 14, Name = "Albano Falco", Age = 37 });
            users.Add(new User() { Id = 15, Name = "Alexandre Santos", Age = 48 });

            Thread.Sleep(5000);

            return users;
        }
        
    }
}
