using LanguageExt;
using Project.Domain.Models;
using static Project.Domain.Models.ModidyOrders;

namespace Project.Domain.Operations
{
    public static class ModifyOrderOperation
    {
        public static Task<IModifyOrder> ValidateModifyOrder(UnvalidatedModifiedOrder unvalidatedModifiedOrder,
                                                                            EvaluatedOrder order,
                                                                            Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist,
                                                                            Func<UnvalidatedModifiedOrder, Option<CardDetailsDto>> checkUserPaymentDetails,
                                                                            Func<UnvalidatedModifiedOrder, CardDetailsDto, Option<UnvalidatedModifiedOrder>> checkUserBalance) =>
          ValidateOrder(unvalidatedModifiedOrder, order, checkProductsExist, checkUserPaymentDetails, checkUserBalance)
           .MatchAsync(
              Right: validatedModifiedOrder => new ValidatedModifiedOrder(validatedModifiedOrder),
              LeftAsync: errorMessage => Task.FromResult((IModifyOrder)new InvalidModifiedOrder(unvalidatedModifiedOrder.Order, errorMessage))
              );

        private static EitherAsync<string, EvaluatedModifiedOrder> ValidateOrder(UnvalidatedModifiedOrder unvalidatedModifiedOrder,
                                                                            EvaluatedOrder order,
                                                                            Func<List<UnvalidatedProduct>, Option<List<EvaluatedProduct>>> checkProductsExist,
                                                                            Func<UnvalidatedModifiedOrder, Option<CardDetailsDto>> checkUserPaymentDetails,
                                                                            Func<UnvalidatedModifiedOrder, CardDetailsDto, Option<UnvalidatedModifiedOrder>> checkUserBalance) =>           
            from productsExist in checkProductsExist(unvalidatedModifiedOrder.Order.OrderProducts)
                                    .ToEitherAsync($"Invalid product list for order ({unvalidatedModifiedOrder.Order}).")
            from checkedUserPaymentDetails in checkUserPaymentDetails(unvalidatedModifiedOrder)
                                    .ToEitherAsync("Invalid or missing payment details.")
            from checkedBalance in checkUserBalance(unvalidatedModifiedOrder, checkedUserPaymentDetails)
                                    .ToEitherAsync($"Insufficient funds for paying order ({unvalidatedModifiedOrder.Order}).")
            select new EvaluatedModifiedOrder(order.OrderNumber, new OrderPrice(0), new OrderDeliveryAddress(unvalidatedModifiedOrder.Order.OrderDeliveryAddress), new OrderTelephone(unvalidatedModifiedOrder.Order.OrderTelephone), new OrderProducts(productsExist))
            {
                UserRegistrationNumber = new UserRegistrationNumber(unvalidatedModifiedOrder.Order.UserRegistrationNumber),
                CardDetails = new CardDetails(
                    new UserCardNumber(checkedUserPaymentDetails.CardNumber),
                    new UserCardCVV(checkedUserPaymentDetails.CVV),
                    new UserCardExpiryDate(checkedUserPaymentDetails.CardExpiryDate),
                    new UserCardBalance(checkedUserPaymentDetails.Balance),
                    checkedUserPaymentDetails.ToUpdate
                    )
            };
    }
}
