using System;
using System.Collections.Generic;

namespace OrderManagement.Models;

public partial class Order
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? ProductId { get; set; }

    public int Quantity { get; set; }

    public DateTime OrderDate { get; set; }

    public virtual Product Product { get; set; }

    public virtual User User { get; set; }
}
