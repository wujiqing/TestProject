﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Test.Mobile.Gateway.Controllers
{
    [Route("api/[controller]/{action}")]
    [ApiController]
    public class TestController : ControllerBase
    { 
        public IActionResult Abc()
        {
            return Content("Test.Mobile.Gateway");
        }
    }
}
