using System;
using System.Net;
using Battleship.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Battleship.Api.Controllers
{
    //DO NOT TOUCH THIS FILE!!
    [Route("")]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            return RedirectPermanent("~/swagger");
        }

        [HttpGet("ping")]
        [ProducesResponseType(typeof(PingResultModel), (int)HttpStatusCode.OK)]
        public IActionResult Ping()
        {
            var model = new PingResultModel
            {
                IsAlive = true,
                Greeting = $"Hello on this fine {DateTime.Now.DayOfWeek}"
            };
            return Ok(model);
        }
    }
}