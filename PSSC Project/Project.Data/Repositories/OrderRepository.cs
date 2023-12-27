﻿using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Project.Data.Models;
using Project.Domain.Models;
using Project.Domain.Repositories;
using static Project.Domain.Models.Orders;
using static LanguageExt.Prelude;

namespace Project.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ProjectContext context;

        public OrderRepository(ProjectContext context)
        {
            this.context = context;
        }

        public TryAsync<OrderNumber> TryGetExistentOrder(string orderNumberToCheck) => async () =>
        {
            var order = await context.Orders
                                       .FirstOrDefaultAsync(order => order.OrderNumber.Equals(orderNumberToCheck));

            return new OrderNumber(order.OrderNumber);
        };

        public TryAsync<List<OrderNumber>> TryGetExistentOrderNumbers() => async () =>
        {
            var orderNumbers = await context.Orders
                                      .Select(o => o.OrderNumber)
                                      .ToListAsync();

            return orderNumbers.Select(number => new OrderNumber(number))
                .ToList();
        };

        public TryAsync<List<EvaluatedOrder>> TryGetExistentOrders() => async () => (await(
                         from order in context.Orders
                         join orderDetail in context.OrderDetails on order.OrderId equals orderDetail.OrderId
                         join product in context.Products on orderDetail.ProductId equals product.ProductId
                         group new { product, orderDetail.Quantity, order.OrderNumber, order.TotalPrice, order.DeliveryAddress } by order.OrderId into grouped
                         select new
                         {
                             OrderId = grouped.Key,
                             OrderNumber = grouped.Select(x => x.OrderNumber).FirstOrDefault(),
                             OrderPrice = grouped.Select(x => x.TotalPrice).FirstOrDefault(),
                             OrderDeliveryAddress = grouped.Select(x => x.DeliveryAddress).FirstOrDefault(),
                             Products = grouped.Select(item => 
                             new EvaluatedProduct
                             (
                                 new ProductName(item.product.ProductName),
                                 new ProductQuantity(item.Quantity),
                                 new ProductPrice(item.product.Price)
                             ))                            
                            .ToList()
                         })
                        .AsNoTracking()
                        .ToListAsync())
                        .Select(result => new EvaluatedOrder(
                             new OrderNumber(result.OrderNumber),
                             new OrderPrice(result.OrderPrice),
                             new OrderDeliveryAddress(result.OrderDeliveryAddress),
                             new OrderProducts(result.Products)
                             )
                         ).ToList();

        public TryAsync<Unit> TrySaveOrder(ValidatedOrder order) => async () =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserRegistrationNumber == order.Order.UserRegistrationNumber.Value);
            var orderToSave = new Order()
            {
                UserId = user.UserId,
                OrderNumber = order.Order.OrderNumber.Value,
                TotalPrice = order.Order.OrderPrice.Price,
                DeliveryAddress = order.Order.OrderDeliveryAddress.DeliveryAddress,
                PostalCode = "aha",
                Telephone = "aha",
                OrderStatus = OrderStatus.Validated,
            };

            var res = context.Orders.Add(orderToSave);
            var orderId = context.Orders.OrderBy(o=>o.OrderId).LastOrDefault().OrderId;

            var productsToUpdate = context.Products
                                          .Where(prod => order.Order.OrderProducts.OrderProductsList.Any(p => p.ProductName.Name == prod.ProductName)).ToList();

            foreach (var productToUpdate in productsToUpdate)
            {
                var orderProduct = order.Order.OrderProducts.OrderProductsList
                    .First(p => p.ProductName.Name == productToUpdate.ProductName);

                productToUpdate.Quantity -= orderProduct.Quantity.Quantity;
            }

            order.Order.OrderProducts.OrderProductsList.ForEach(p =>
                context.OrderDetails.Add(new OrderDetails()
                {
                    OrderId = orderId,
                    ProductId = context.Products.Where(prod => prod.ProductName == p.ProductName.Name).Select(prod => prod.ProductId).FirstOrDefault(),
                    Quantity = p.Quantity.Quantity
                }
                )
            );

            await context.SaveChangesAsync();

            return unit;
        };
    }
}
