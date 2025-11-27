using System;
using System.Collections.Generic;

abstract class OrderProcessor
{
    public void ProcessOrder()
    {
        if (!Validate()) { Console.WriteLine("Заказ отклонён\n"); return; }
        decimal price = CalculatePrice();
        price = ApplyDiscount(price);
        ProcessPayment(price);
        GenerateInvoice(price);
        Console.WriteLine();
    }

    protected abstract bool Validate();
    protected abstract decimal CalculatePrice();
    protected virtual decimal ApplyDiscount(decimal price) => price;
    protected virtual void ProcessPayment(decimal price) => Console.WriteLine($"Оплата {price:C}");
    protected virtual void GenerateInvoice(decimal price) => Console.WriteLine($"Счёт на {price:C} отправлен");
}

class StandardOrder : OrderProcessor
{
    public decimal Amount { get; set; }
    protected override bool Validate() => Amount > 0;
    protected override decimal CalculatePrice() => Amount;
    protected override decimal ApplyDiscount(decimal price) => price * 0.95m;
}

class ExpressOrder : OrderProcessor
{
    public decimal Amount { get; set; }
    protected override bool Validate() => Amount >= 100;
    protected override decimal CalculatePrice() => Amount + 500;
    protected override void ProcessPayment(decimal price) => Console.WriteLine($"Экспресс-оплата {price:C}");
}

class SubscriptionOrder : OrderProcessor
{
    public int Months { get; set; }
    protected override bool Validate() => Months >= 1 && Months <= 36;
    protected override decimal CalculatePrice() => Months * 990;
    protected override decimal ApplyDiscount(decimal price)
        => Months >= 12 ? price * 0.80m : price;
    protected override void GenerateInvoice(decimal price)
        => Console.WriteLine($"Подписка на {Months} мес., к оплате {price:C}");
}

class WholesaleOrder : OrderProcessor
{
    public decimal Amount { get; set; }
    protected override bool Validate() => Amount >= 50000;
    protected override decimal CalculatePrice() => Amount;
    protected override decimal ApplyDiscount(decimal price)
        => price >= 100000 ? price * 0.70m : price * 0.85m;
    protected override void ProcessPayment(decimal price)
        => Console.WriteLine($"Оптовая оплата {price:C} (отсрочка 30 дней)");
}

class Program
{
    static Random rnd = new Random();

    static OrderProcessor CreateRandomOrder()
    {
        return rnd.Next(4) switch
        {
            0 => new StandardOrder { Amount = rnd.Next(1000, 20000) },
            1 => new ExpressOrder { Amount = rnd.Next(50, 30000) },
            2 => new SubscriptionOrder { Months = rnd.Next(1, 48) },
            _ => new WholesaleOrder { Amount = rnd.Next(10000, 300000) }
        };
    }

    static void Main()
    {
        Console.WriteLine("Обработка 100 заказов\n");

        for (int i = 1; i <= 100; i++)
        {
            Console.WriteLine($"Заказ {i:000}");
            var order = CreateRandomOrder();
            order.ProcessOrder();
        }

        Console.WriteLine("Все заказы обработаны.");
    }
}