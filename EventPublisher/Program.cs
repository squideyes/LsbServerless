// Copyright 2018 Louis S. Berman
//
// This file is part of LsbServerless.
//
// LsbServerless is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation, either version 3 of the License, 
// or (at your option) any later version.
//
// LsbServerless is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with LsbServerless.  If not, see <http://www.gnu.org/licenses/>.

using Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EventPublisher
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();

            var key = configuration["EventGrid:Key"];
            var endpoint = configuration["EventGrid:EndPoint"];

            var fred = new EmployeeInfo()
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeName = "Fred C. Muggs",
                EmployeeEmail = "fred@someco.com",
                ManagerId = Guid.NewGuid(),
                ManagerName = "Louis Berman",
                ManagerEmail = "lberman@microsoft.com"
            };

            await SendEvent(key, endpoint, fred);

            Console.WriteLine();
            Console.Write("Press any key to terminate the application...");

            Console.ReadKey(true);
        }

        private static async Task SendEvent(string key, string endpoint, EmployeeInfo data)
        {
            var gridEvent = new GridEvent<EmployeeInfo>()
            {
                Id = Guid.NewGuid().ToString("N"),
                EventType = "EmployeeAdded",
                Subject = "Department/Engineering",
                EventTime = DateTimeOffset.Now.ToString("o"),
                Data = data
            };

            var gridEvents = new List<GridEvent<EmployeeInfo>> { gridEvent };

            var json = JsonConvert.SerializeObject(gridEvents);

            Console.WriteLine(json);

            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("aeg-sas-key", key);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var result = await client.PostAsync(endpoint, content);

            Console.WriteLine();
            Console.WriteLine($"Response: {result.ReasonPhrase}.");
        }
    }
}
