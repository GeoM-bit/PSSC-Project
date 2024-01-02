using Project.Dto.Models;

namespace Project.Services
{
    public interface IEventService
    {
        Task<List<string>> HasPlacedOrderEvent();
        void SetPlacedOrderEventReceived(OrderDto _order);
        bool IsOrderPlaced(string orderNumber);

    }

    public class EventService : IEventService
    {
        private List<string> orderDet = new List<string>();

        public Task<List<string>> HasPlacedOrderEvent()
        {
            return Task.FromResult(orderDet);
        }

        public void SetPlacedOrderEventReceived(OrderDto _order)
        {
            orderDet.Add(_order.OrderNumber);
        }

        public bool IsOrderPlaced(string orderNumber)
        {
            return orderDet.Contains(orderNumber);
        }
    }
}
