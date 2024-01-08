using Project.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Commands
{
    public record ReturnOrderCommand
    {
        public ReturnOrderCommand(ReturnOrderModel returnOrder)
        {
            ReturnOrder = returnOrder;
        }

        public ReturnOrderModel ReturnOrder { get; }
    }
}
