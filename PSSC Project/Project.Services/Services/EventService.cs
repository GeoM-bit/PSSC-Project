using Project.Dto.Models;

namespace Project.Common.Services
{
    public interface IEventService
    {
        void SetPlacedOrderEventReceived(ReceivedOrder _order);
        bool IsOrderPlaced(string orderNumber);
        public static ReceivedOrder? currentOrder{ get; set; }
    }

    public class EventService : IEventService
    {
        private List<string> orderDet = new List<string>();

        public static ReceivedOrder currentOrder { get; set; }

        public void SetPlacedOrderEventReceived(ReceivedOrder _order)
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
