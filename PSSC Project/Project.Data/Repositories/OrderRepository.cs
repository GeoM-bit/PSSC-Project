using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Project.Data.Models;
using Project.Domain.Models;
using Project.Domain.Repositories;
using static Project.Domain.Models.Orders;
using static LanguageExt.Prelude;
using Microsoft.EntityFrameworkCore.Storage;
using static Project.Domain.Models.ModidyOrders;

namespace Project.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ProjectContext context;

        public OrderRepository(ProjectContext context)
        {
            this.context = context;
        }

        public TryAsync<EvaluatedOrder> TryGetExistentOrder(string orderNumberToCheck) => async () => (await (
                         from order in context.Orders
                         join orderDetail in context.OrderDetails on order.OrderId equals orderDetail.OrderId
                         join product in context.Products on orderDetail.ProductId equals product.ProductId
                         group new { product, orderDetail.Quantity, order.OrderNumber, order.TotalPrice, order.DeliveryAddress, order.Telephone } by order.OrderId into grouped
                         select new
                         {
                             OrderId = grouped.Key,
                             OrderNumber = grouped.Select(x => x.OrderNumber).FirstOrDefault(),
                             OrderPrice = grouped.Select(x => x.TotalPrice).FirstOrDefault(),
                             OrderDeliveryAddress = grouped.Select(x => x.DeliveryAddress).FirstOrDefault(),
                             OrderTelephone = grouped.Select(x => x.Telephone).FirstOrDefault(),
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
                             new OrderTelephone(result.OrderTelephone),
                             new OrderProducts(result.Products)
                             )
                         ).ToList().FirstOrDefault(a => a.OrderNumber.Value == orderNumberToCheck);

        public TryAsync<OrderNumber> TryGetExistentOrderNumber(string orderNumberToCheck) => async () =>
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
                         group new { product, orderDetail.Quantity, order.OrderNumber, order.TotalPrice, order.DeliveryAddress, order.Telephone } by order.OrderId into grouped
                         select new
                         {
                             OrderId = grouped.Key,
                             OrderNumber = grouped.Select(x => x.OrderNumber).FirstOrDefault(),
                             OrderPrice = grouped.Select(x => x.TotalPrice).FirstOrDefault(),
                             OrderDeliveryAddress = grouped.Select(x => x.DeliveryAddress).FirstOrDefault(),
                             OrderTelephone = grouped.Select(x => x.Telephone).FirstOrDefault(),
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
                             new OrderTelephone(result.OrderTelephone),
                             new OrderProducts(result.Products)
                             )
                         ).ToList();

        public TryAsync<Unit> TrySaveOrder(ValidatedOrder order) => async () =>
        {
            using (IDbContextTransaction transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var user = await context.Users.FirstOrDefaultAsync(u => u.UserRegistrationNumber == order.Order.UserRegistrationNumber.Value);

                    if (order.Order.CardDetails.ToUpdate)
                    {
                        user.Balance = order.Order.CardDetails.UserCardBalance.Value - order.Order.OrderPrice.Price;
                        user.CVV = order.Order.CardDetails.UserCardCVV.Value;
                        user.CardNumber = order.Order.CardDetails.UserCardNumber.CardNumber;
                        user.CardExpiryDate = order.Order.CardDetails.UserCardExpiryDate.Value;
                    }
                    else
                    {
                        user.Balance -= order.Order.OrderPrice.Price;
                    }
                    var orderToSave = new Order()
                    {
                        UserId = user.UserId,
                        OrderNumber = order.Order.OrderNumber.Value,
                        TotalPrice = order.Order.OrderPrice.Price,
                        DeliveryAddress = order.Order.OrderDeliveryAddress.DeliveryAddress,
                        Telephone = order.Order.OrderTelephone.Value
                    };

                    var res = context.Orders.Add(orderToSave);
                    await context.SaveChangesAsync();

                    var savedOrder = await context.Orders.OrderBy(o => o.OrderId).LastOrDefaultAsync();

                    var productsToUpdate = context.Products.AsEnumerable()
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
                            OrderId = savedOrder.OrderId,
                            ProductId = context.Products.AsEnumerable().Where(prod => prod.ProductName == p.ProductName.Name).Select(prod => prod.ProductId).FirstOrDefault(),
                            Quantity = p.Quantity.Quantity
                        }
                        )
                    );

                    await context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error occurred.");
                }
            }

            return unit;
        };

        public TryAsync<Unit> TryUpdateOrder(ValidatedModifiedOrder orderToUpdate, EvaluatedOrder initialOrder) => async () =>
        {
            using (IDbContextTransaction transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var user = await context.Users.FirstOrDefaultAsync(u => u.UserRegistrationNumber == orderToUpdate.Order.UserRegistrationNumber.Value);

                    if (orderToUpdate.Order.CardDetails.ToUpdate)
                    {
                        user.Balance = orderToUpdate.Order.CardDetails.UserCardBalance.Value + initialOrder.OrderPrice.Price - orderToUpdate.Order.OrderPrice.Price;
                        user.CVV = orderToUpdate.Order.CardDetails.UserCardCVV.Value;
                        user.CardNumber = orderToUpdate.Order.CardDetails.UserCardNumber.CardNumber;
                        user.CardExpiryDate = orderToUpdate.Order.CardDetails.UserCardExpiryDate.Value;
                    }
                    else
                    {
                        user.Balance = user.Balance + initialOrder.OrderPrice.Price - orderToUpdate.Order.OrderPrice.Price;
                    }

                    var ord = await context.Orders.FirstOrDefaultAsync(x => x.OrderNumber == initialOrder.OrderNumber.Value);

                    ord.TotalPrice = orderToUpdate.Order.OrderPrice.Price;
                    ord.DeliveryAddress = orderToUpdate.Order.OrderDeliveryAddress.DeliveryAddress;
                    ord.Telephone = orderToUpdate.Order.OrderTelephone.Value;

                    var res = context.Orders.Update(ord);
                    await context.SaveChangesAsync();

                    var savedOrder = await context.Orders.OrderBy(o => o.OrderId).LastOrDefaultAsync();


                    var productsToUpdate = context.Products.AsEnumerable()
                                                  .Where(prod => orderToUpdate.Order.OrderProducts.OrderProductsList.Any(p => p.ProductName.Name == prod.ProductName)
                                                              || initialOrder.OrderProducts.OrderProductsList.Any(p => p.ProductName.Name == prod.ProductName)).ToList();
                    var initialOrderDetails = await context.OrderDetails.Where(x => x.OrderId == savedOrder.OrderId).ToListAsync();
                        foreach (var prod in productsToUpdate)
                        {
                        var initialOrderProd = initialOrder.OrderProducts.OrderProductsList.FirstOrDefault(x => x.ProductName.Name == prod.ProductName);
                        var modifiedOrderProd = orderToUpdate.Order.OrderProducts.OrderProductsList.FirstOrDefault(x => x.ProductName.Name == prod.ProductName);
                        var initialQuantity = initialOrderProd == null ? 0 : initialOrderProd.Quantity.Quantity;
                        var modifiedQuantity = modifiedOrderProd == null ? 0 : modifiedOrderProd.Quantity.Quantity;

                        prod.Quantity = prod.Quantity + initialQuantity - modifiedQuantity;                           
                        }

                        foreach (var item in initialOrderDetails)
                        {
                            context.OrderDetails.Remove(item);
                        }

                        orderToUpdate.Order.OrderProducts.OrderProductsList.ForEach(p =>
                           context.OrderDetails.Add(new OrderDetails()
                           {
                               OrderId = savedOrder.OrderId,
                               ProductId = context.Products.AsEnumerable().Where(prod => prod.ProductName == p.ProductName.Name).Select(prod => prod.ProductId).FirstOrDefault(),
                               Quantity = p.Quantity.Quantity
                           }
                           )
                           );

                    await context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error occurred.");
                }
            }

            return unit;
        };
    }
}
