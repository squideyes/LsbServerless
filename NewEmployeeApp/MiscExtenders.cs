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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NewEmployeeApp
{
    internal static class MiscExtenders
    {
        public static async Task<(List<GridEvent<T>>, HttpResponseMessage)> GetGridEvents<T>(
            this HttpRequestMessage request) where T : class
        {
            var json = await request.Content.ReadAsStringAsync();

            var gridEvents = JsonConvert.DeserializeObject<List<GridEvent<T>>>(json);

            if (gridEvents != null)
                return (gridEvents, null);

            return (null, request.CreateErrorResponse(HttpStatusCode.BadRequest,
                $"The event could not be deserialized into a List<GridEvent<{typeof(T)}>> object!"));
        }
    }
}
