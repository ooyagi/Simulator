using System.ComponentModel.DataAnnotations;
using CommonItems.Models;

namespace WorkOrderManagement.Models;

public class WorkOrder
{
    [Key]
    public ShippingPalletID PalletID { get; set; } = ShippingPalletID.CustomPaletteID;
    public string DeliveryDate { get; set; } = "";
    public string Line { get; set; } = "";
    public string Size { get; set; } = "";
    public int PalletNumber { get; set; }
    public bool Assigned { get; set; } = false;
    public IEnumerable<OrderedItem> OrderedItems { get; set; } = new List<OrderedItem>();

    public WorkOrder() { }
    public WorkOrder(
        ShippingPalletID palletID,
        string deliveryDate,
        string line,
        string size,
        int palletNumber,
        IEnumerable<OrderedItem> orderedItems
    ) {
        PalletID = palletID;
        DeliveryDate = deliveryDate;
        Line = line;
        Size = size;
        PalletNumber = palletNumber;
        OrderedItems = orderedItems.ToList();
    }
}

public class OrderedItem
{
    public ShippingPalletID PalletID { get; set; } = ShippingPalletID.CustomPaletteID;
    public Hinban Hinban { get; set; } = Hinban.Default;
    public int Index { get; set; }

    public OrderedItem() { }
    public OrderedItem(
        ShippingPalletID palletID,
        Hinban hinban,
        int index
    ) {
        PalletID = palletID;
        Hinban = hinban;
        Index = index;
    }
}
