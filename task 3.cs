using System;

abstract class OrderProcessor
{
    public void ProcessOrder()
    {
        if (!Validate())
        {
            Console.WriteLine("   - заказ отклонён\n");
            return;
        }

        decimal price = CalculatePrice();
        price = ApplyDiscount(price);
        ProcessPayment(price);
        GenerateInvoice(price);
        Console.WriteLine();
    }

    protected abstract bool Validate();
    protected abstract decimal CalculatePrice();
    protected virtual decimal ApplyDiscount(decimal price) => price;
    protected virtual void ProcessPayment(decimal price) => Console.WriteLine($"   - оплата: {price:C}");
    protected virtual void GenerateInvoice(decimal price) => Console.WriteLine($"   - счёт отправлен на {price:C}");
}

class StandardOrder : OrderProcessor
{
    public decimal Amount { get; set; } = 5000;
    protected override bool Validate() => Amount > 0;
    protected override decimal CalculatePrice() => Amount;
    protected override decimal ApplyDiscount(decimal price) => price >= 10000 ? price * 0.90m : price * 0.95m;
}

class ExpressOrder : OrderProcessor
{
    public decimal Amount { get; set; } = 8000;
    protected override bool Validate() => true;
    protected override decimal CalculatePrice() => Amount + 1500;
    protected override void ProcessPayment(decimal price) => Console.WriteLine($"   - экспресс-оплата {price:C} (картой)");
}

class SubscriptionOrder : OrderProcessor
{
    public int Months { get; set; } = 12;
    protected override bool Validate() => Months >= 1 && Months <= 36;
    protected override decimal CalculatePrice() => Months * 999;
    protected override decimal ApplyDiscount(decimal price) => Months >= 12 ? price * 0.75m : price;
    protected override void GenerateInvoice(decimal price) => Console.WriteLine($"   - подписка на {Months} мес. → {price:C}");
}

class WholesaleOrder : OrderProcessor
{
    public decimal Amount { get; set; } = 150000;
    protected override bool Validate() => Amount >= 50000;
    protected override decimal CalculatePrice() => Amount;
    protected override decimal ApplyDiscount(decimal price) => price * 0.70m;
    protected override void ProcessPayment(decimal price) => Console.WriteLine($"   → оптовая оплата {price:C} (отсрочка 30 дней)");
}

class Program
{
    static Random rnd = new();

    static OrderProcessor[] Create100Orders()
    {
        var orders = new OrderProcessor[100];
        for (int i = 0; i < 100; i++)
        {
            orders[i] = rnd.Next(4) switch
            {
                0 => new StandardOrder { Amount = rnd.Next(1000, 50000) },
                1 => new ExpressOrder { Amount = rnd.Next(2000, 40000) },
                2 => new SubscriptionOrder { Months = rnd.Next(1, 48) },
                _ => new WholesaleOrder { Amount = rnd.Next(20000, 500000) }
            };
        }
        return orders;
    }

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("обработка 100 заказов\n");

        var orders = Create100Orders();

        for (int i = 0; i < orders.Length; i++)
        {
            Console.WriteLine($"заказ {i + 1:000} — {orders[i].GetType().Name.Replace("Order", "")}");
            orders[i].ProcessOrder();
        }

        Console.WriteLine("готово все 100 заказов обработаны");
    }
}