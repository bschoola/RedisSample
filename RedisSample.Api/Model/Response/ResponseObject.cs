using System;
using System.Collections.Generic;

namespace RedisSample.Api.Model.Response
{
    public class ResponseObject
    {
        public ResponseObject(List<User> users, double seconds)
        {
            Seconds = seconds;
            Users = users;
        }

        public double Seconds { get; set; }
        public List<User> Users { get; set; }
    }
}
