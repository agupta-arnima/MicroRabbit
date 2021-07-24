using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Banking.Application.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MicroRabbit.Banking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankingController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public BankingController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // GET: api/Banking
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_accountService.GetAccounts());
        }

        [HttpPost]
        public IActionResult Post([FromBody] AccountTransferDto accountTransfer)
        {
            _accountService.Transfer(accountTransfer);
            return Ok(accountTransfer);
        }
    }
}
