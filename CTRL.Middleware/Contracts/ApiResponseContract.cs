﻿using System.Net;

namespace CTRL.Middleware.Contracts
{
    public class ApiResponseContract
    {
        public HttpStatusCode Status { get; set; }
        public string Message { get; set; }
    }
}
