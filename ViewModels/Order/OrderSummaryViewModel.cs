namespace UsersApp.ViewModels.Order
{
    public class OrderSummaryViewModel
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string dishname { get; set; }
        public int Quantity {  get; set; }
        public decimal UnitPrice { get; set; }
    }
}
