using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class User
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string PhoneNumber { get; set; }
    public int Age { get; set; }

    public User(string name, string email, string password, string phoneNumber, int age)
    {
        Name = name;
        Email = email;
        Password = password;
        PhoneNumber = phoneNumber;
        Age = age;
    }

    public override string ToString()
    {
        return $"{Name},{Email},{Password},{PhoneNumber},{Age}";
    }
}

public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public int AvailableQuantity { get; set; }

    public Product(string name, decimal price, decimal discount, int availableQuantity)
    {
        Name = name;
        Price = price;
        Discount = discount;
        AvailableQuantity = availableQuantity;
    }

    public decimal GetDiscountedPrice()
    {
        return Price - (Price * (Discount / 100));
    }
}

public class Shop
{
    public string Name { get; set; }
    public List<Product> Electronics { get; set; }
    public List<Product> Clothing { get; set; }
    public List<Product> Footwear { get; set; }
    public List<Product> HomeAppliances { get; set; }
    public List<Product> BeautyProducts { get; set; }
    public List<Product> SportsGear { get; set; }
    public List<Product> Toys { get; set; }
    public List<Product> Books { get; set; }
    public List<Product> Groceries { get; set; }
    public List<Product> HealthSupplements { get; set; }
    public List<Product> OfficeSupplies { get; set; }

    public Shop(string name)
    {
        Name = name;
        Electronics = new List<Product>();
        Clothing = new List<Product>();
        Footwear = new List<Product>();
        HomeAppliances = new List<Product>();
        BeautyProducts = new List<Product>();
        SportsGear = new List<Product>();
        Toys = new List<Product>();
        Books = new List<Product>();
        Groceries = new List<Product>();
        HealthSupplements = new List<Product>();
        OfficeSupplies = new List<Product>();
    }
}

public class ShoppingCart
{
    private Dictionary<string, (Product Product, int Quantity)> cart;
    private DateTime creationTime;
    private const int CartExpirationMinutes = 30;

    public ShoppingCart()
    {
        cart = new Dictionary<string, (Product Product, int Quantity)>();
        creationTime = DateTime.Now;
    }

    public void AddProduct(Product product, int quantityToAdd)
    {
        if (product.AvailableQuantity >= quantityToAdd)
        {
            if (cart.ContainsKey(product.Name))
            {
                cart[product.Name] = (cart[product.Name].Product, cart[product.Name].Quantity + quantityToAdd);
            }
            else
            {
                cart.Add(product.Name, (product, quantityToAdd));
            }

            product.AvailableQuantity -= quantityToAdd;
            Console.WriteLine($"{quantityToAdd} x {product.Name} added to the cart. Available Quantity: {product.AvailableQuantity}");
        }
        else
        {
            Console.WriteLine($"Cannot add {quantityToAdd} x {product.Name}. Only {product.AvailableQuantity} available.");
        }
    }

    public void RemoveProduct(string productName, int quantityToRemove)
    {
        if (cart.ContainsKey(productName))
        {
            var cartItem = cart[productName];
            if (cartItem.Quantity >= quantityToRemove)
            {
                cartItem.Quantity -= quantityToRemove;
                cart[productName] = (cartItem.Product, cartItem.Quantity);

                cartItem.Product.AvailableQuantity += quantityToRemove;
                if (cartItem.Quantity == 0)
                {
                    cart.Remove(productName);
                }
                Console.WriteLine($"{quantityToRemove} x {productName} removed from the cart.");
            }
            else
            {
                Console.WriteLine($"Cannot remove {quantityToRemove} x {productName}. Only {cartItem.Quantity} available in cart.");
            }
        }
        else
        {
            Console.WriteLine($"{productName} is not in the cart.");
        }
    }

    public void ViewCart()
    {
        Console.WriteLine("\nShopping Cart:");
        if (IsExpired())
        {
            Console.WriteLine("Your cart has expired. Please create a new cart.");
            return;
        }

        if (cart.Count == 0)
        {
            Console.WriteLine("Your cart is empty.");
            return;
        }

        decimal total = 0;
        foreach (var item in cart)
        {
            var discountedPrice = item.Value.Product.GetDiscountedPrice();
            Console.WriteLine($"{item.Value.Quantity} x {item.Value.Product.Name} - ${discountedPrice} each (Discount: {item.Value.Product.Discount}%)");
            total += discountedPrice * item.Value.Quantity;
        }

        decimal salesTaxRate = 0.10m;
        decimal salesTax = total * salesTaxRate;
        total += salesTax;

        Console.WriteLine($"Sales Tax: ${salesTax}");
        Console.WriteLine($"Total after tax: ${total}");
    }

    private bool IsExpired()
    {
        return (DateTime.Now - creationTime).TotalMinutes > CartExpirationMinutes;
    }
}

public class Program
{
    private static List<User> users = new List<User>();
    private static string currentUserEmail;
    private const string UserFilePath = "users.txt";
    private static List<Shop> shops = new List<Shop>();

    public static void Main(string[] args)
    {
        LoadUsers();
        LoadShops();
        bool running = true;

        while (running)
        {
            Console.WriteLine("\nWelcome to the Shopping Cart!");
            Console.WriteLine("1. Sign Up");
            Console.WriteLine("2. Log In");
            Console.WriteLine("3. Exit");
            Console.Write("Choose an option: ");

            if (!int.TryParse(Console.ReadLine(), out int mainChoice))
            {
                Console.WriteLine("Invalid input, please enter a number between 1 and 3.");
                continue;
            }

            switch (mainChoice)
            {
                case 1:
                    SignUp();
                    break;

                case 2:
                    if (LogIn())
                    {
                        ShoppingCart cart = new ShoppingCart();
                        UserMenu(cart);
                    }
                    break;

                case 3:
                    running = false;
                    break;

                default:
                    Console.WriteLine("Invalid option. Please select a valid option.");
                    break;
            }
        }
    }

    private static void LoadUsers()
    {
        if (File.Exists(UserFilePath))
        {
            var lines = File.ReadAllLines(UserFilePath);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length == 5 && int.TryParse(parts[4], out int age))
                {
                    users.Add(new User(parts[0], parts[1], parts[2], parts[3], age));
                }
            }
        }
        else
        {
            File.Create(UserFilePath).Dispose(); // Create the file if it doesn't exist
        }
    }

    private static void LoadShops()
    {
        // Sample shop and product data
        shops.Add(new Shop("Puma"));
        shops[0].Electronics.Add(new Product("Puma Smartwatch", 3499, 10, 10));
        shops[0].Electronics.Add(new Product("Puma Wireless Earbuds", 2999, 5, 5));
        shops[0].Clothing.Add(new Product("Puma T-Shirt", 1999, 15, 20));
        shops[0].Clothing.Add(new Product("Puma Jacket", 4999, 8, 10));
        shops[0].Footwear.Add(new Product("Puma Sneaker V2", 3499, 10, 10));
        shops[0].Footwear.Add(new Product("Puma Flip Flops", 1299, 15, 20));
        shops[0].HomeAppliances.Add(new Product("Puma Blender", 2499, 5, 10));
        shops[0].BeautyProducts.Add(new Product("Puma Moisturizer", 999, 10, 30));

        // Add more shops and products here...
        shops.Add(new Shop("Adidas"));
        shops[1].Electronics.Add(new Product("Adidas Smart Speaker", 2999, 10, 10));
        shops[1].Clothing.Add(new Product("Adidas T-Shirt", 1999, 15, 20));
        shops[1].Footwear.Add(new Product("Adidas Sneakers", 3499, 10, 10));
        shops[1].HomeAppliances.Add(new Product("Adidas Vacuum Cleaner", 4999, 15, 5));
        shops[1].BeautyProducts.Add(new Product("Adidas Perfume", 1999, 20, 15));

        // Add more categories and products as needed...
    }

    private static void SaveUser(User user)
    {
        using (StreamWriter writer = new StreamWriter(UserFilePath, true))
        {
            writer.WriteLine(user);
        }
    }

    private static void SignUp()
    {
        Console.Write("Enter your name: ");
        string name = Console.ReadLine();
        Console.Write("Enter your email: ");
        string email = Console.ReadLine();
        Console.Write("Enter your password: ");
        string password = Console.ReadLine();
        Console.Write("Enter your phone number: ");
        string phoneNumber = Console.ReadLine();
        Console.Write("Enter your age: ");
        if (!int.TryParse(Console.ReadLine(), out int age) || age < 0)
        {
            Console.WriteLine("Invalid age entered. Please try again.");
            return;
        }

        User newUser = new User(name, email, password, phoneNumber, age);
        users.Add(newUser);
        SaveUser(newUser);
        Console.WriteLine("Sign-up successful! You can now log in.");
    }

    private static bool LogIn()
    {
        Console.Write("Enter your email: ");
        string email = Console.ReadLine();
        Console.Write("Enter your password: ");
        string password = Console.ReadLine();

        var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.Password == password);
        if (user != null)
        {
            currentUserEmail = email;
            Console.WriteLine($"Welcome back, {user.Name}!");
            return true;
        }
        else
        {
            Console.WriteLine("Invalid email or password. Please try again.");
            return false;
        }
    }

    private static void UserMenu(ShoppingCart cart)
    {
        bool inMenu = true;

        while (inMenu)
        {
            Console.WriteLine("\nUser Menu:");
            Console.WriteLine("1. View Products");
            Console.WriteLine("2. View Cart");
            Console.WriteLine("3. Remove Product from Cart");
            Console.WriteLine("4. Log Out");
            Console.Write("Choose an option: ");

            if (!int.TryParse(Console.ReadLine(), out int userChoice))
            {
                Console.WriteLine("Invalid input, please enter a number between 1 and 4.");
                continue;
            }

            switch (userChoice)
            {
                case 1:
                    ViewProducts(cart);
                    break;

                case 2:
                    cart.ViewCart();
                    break;

                case 3:
                    RemoveProductFromCart(cart);
                    break;

                case 4:
                    inMenu = false;
                    break;

                default:
                    Console.WriteLine("Invalid option. Please select a valid option.");
                    break;
            }
        }
    }

    private static void ViewProducts(ShoppingCart cart)
    {
        ClearConsole();
        Console.WriteLine("\nAvailable Products:");
        for (int i = 0; i < shops.Count; i++)
        {
            Console.WriteLine($"\nShop: {shops[i].Name}");
            Console.WriteLine("Categories: ");
            Console.WriteLine("1. Electronics");
            Console.WriteLine("2. Clothing");
            Console.WriteLine("3. Footwear");
            Console.WriteLine("4. Home Appliances");
            Console.WriteLine("5. Beauty Products");
            Console.WriteLine("6. Sports Gear");
            Console.WriteLine("7. Toys");
            Console.WriteLine("8. Books");
            Console.WriteLine("9. Groceries");
            Console.WriteLine("10. Health Supplements");
            Console.WriteLine("11. Office Supplies");
            Console.Write("Which category do you want to explore? (1-11): ");

            if (!int.TryParse(Console.ReadLine(), out int categoryChoice) || categoryChoice < 1 || categoryChoice > 11)
            {
                Console.WriteLine("That doesn’t seem to be a valid choice. Let’s try that again.");
                return;
            }

            List<Product> products = GetProductsByCategory(shops[i], categoryChoice);
            if (products.Count == 0)
            {
                Console.WriteLine("Looks like there are no products in this category right now.");
                continue;
            }

            foreach (var product in products)
            {
                Console.WriteLine($"{product.Name} - Price: ${product.Price:F2} (Discount: {product.Discount}%)");
            }

            Console.Write("Which product would you like to add to your cart? ");
            string productName = Console.ReadLine();
            Console.Write("How many would you like to add? ");
            if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
            {
                Console.WriteLine("Hmm, that doesn’t seem like a valid quantity. Please try again.");
                return;
            }

            var productToAdd = products.FirstOrDefault(p => p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));
            if (productToAdd != null)
            {
                cart.AddProduct(productToAdd, quantity);
            }
            else
            {
                Console.WriteLine("Sorry, we couldn’t find that product.");
            }

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    private static List<Product> GetProductsByCategory(Shop shop, int categoryChoice)
    {
        switch (categoryChoice)
        {
            case 1:
                return shop.Electronics;
            case 2:
                return shop.Clothing;
            case 3:
                return shop.Footwear;
            case 4:
                return shop.HomeAppliances;
            case 5:
                return shop.BeautyProducts;
            case 6:
                return shop.SportsGear;
            case 7:
                return shop.Toys;
            case 8:
                return shop.Books;
            case 9:
                return shop.Groceries;
            case 10:
                return shop.HealthSupplements;
            case 11:
                return shop.OfficeSupplies;
            default:
                return new List<Product>();
        }
    }

    private static void RemoveProductFromCart(ShoppingCart cart)
    {
        Console.Write("Enter the product name you want to remove: ");
        string productName = Console.ReadLine();
        Console.Write("Enter the quantity to remove: ");
        if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
        {
            Console.WriteLine("Hmm, that doesn’t seem like a valid quantity. Please try again.");
            return;
        }

        cart.RemoveProduct(productName, quantity);
    }

    private static void ClearConsole()
    {
        Console.Clear();
    }
}
