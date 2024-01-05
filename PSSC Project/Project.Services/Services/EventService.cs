using Project.Dto.Models;

namespace Project.Services
{
    public interface IEventService
    {
        void SetPlacedOrderEventReceived(OrderDto _order);
        bool IsOrderPlaced(string orderNumber);
        public static OrderDto? currentOrder{ get; set; }

    }

    public class EventService : IEventService
    {
        private List<string> orderDet = new List<string>();

        public static OrderDto currentOrder { get; set; }

        public void SetPlacedOrderEventReceived(OrderDto _order)
        {
            currentOrder = _order;
            orderDet.Add(_order.OrderNumber);
        }

        public bool IsOrderPlaced(string orderNumber)
        {
            return orderDet.Contains(orderNumber);
        }
    }
}
