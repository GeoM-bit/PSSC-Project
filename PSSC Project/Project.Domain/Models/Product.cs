using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Models
{
    public record Product(String productName, ProductQuantity quantity, ProductPrice price)
    {

    }
}
