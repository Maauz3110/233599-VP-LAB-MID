using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// Abstract User Class
abstract class User
{
    public string UserID { get; private set; }
    public string Name { get; private set; }
    public string PhoneNumber { get; private set; }

    public User(string userID, string name, string phoneNumber)
    {
        UserID = userID;
        Name = name;
        PhoneNumber = ValidatePhoneNumber(phoneNumber) ? phoneNumber : throw new ArgumentException("Invalid phone number format.");
    }

    private bool ValidatePhoneNumber(string phoneNumber)
    {
        return Regex.IsMatch(phoneNumber, @"^\d{3}-\d{3}-\d{4}$");
    }

    public abstract void Register();
    public abstract void Login();
}

// Rider Class
class Rider : User
{
    public List<Trip> RideHistory { get; private set; } = new List<Trip>();

    public Rider(string userID, string name, string phoneNumber) : base(userID, name, phoneNumber) { }

    public override void Register()
    {
        Console.WriteLine($"Registered successfully as a Rider");
    }

    public override void Login()
    {
        Console.WriteLine($"Rider {Name} logged in successfully.");
    }

    public void RequestRide(RideSharingSystem system, string currentLocation, string destination)
    {
        system.RequestRide(this, currentLocation, destination);
    }

    public void ViewRideHistory()
    {
        if (RideHistory.Count == 0)
        {
            Console.WriteLine("No ride history found.");
            return;
        }
        foreach (var trip in RideHistory)
        {
            trip.DisplayTripDetails();
        }
    }
}

// Driver Class
class Driver : User
{
    public string DriverID { get; private set; }
    public string VehicleDetails { get; private set; }
    public List<Trip> RideHistory { get; private set; } = new List<Trip>();
    public bool IsAvailable { get; private set; } = true;

    public Driver(string userID, string name, string phoneNumber, string driverID, string vehicleDetails)
        : base(userID, name, phoneNumber)
    {
        DriverID = driverID;
        VehicleDetails = vehicleDetails;
    }

    public override void Register()
    {
        Console.WriteLine($"Registered successfully as a Driver");
    }

    public override void Login()
    {
        Console.WriteLine($"Driver {Name} logged in successfully.");
    }

    public void AcceptRide(Trip trip)
    {
        if (IsAvailable)
        {
            trip.AssignDriver(this);
            IsAvailable = false;
            Console.WriteLine($"Driver {Name} accepted the ride request from {trip.Rider.Name}.");
        }
        else
        {
            Console.WriteLine($"Driver {Name} is not available.");
        }
    }

    public void CompleteRide(Trip trip)
    {
        trip.CompleteTrip();
        IsAvailable = true;
        RideHistory.Add(trip);
    }

    public void ViewRideHistory()
    {
        if (RideHistory.Count == 0)
        {
            Console.WriteLine("No trip history found.");
            return;
        }
        foreach (var trip in RideHistory)
        {
            trip.DisplayTripDetails();
        }
    }
}

// Trip Class
class Trip
{
    public string TripID { get; private set; }
    public Rider Rider { get; private set; }
    public Driver? Driver { get; private set; }
    public string StartLocation { get; private set; }
    public string Destination { get; private set; }
    public decimal Fare { get; private set; }
    public string Status { get; private set; }

    public Trip(string tripID, Rider rider, string startLocation, string destination)
    {
        TripID = tripID;
        Rider = rider;
        StartLocation = startLocation;
        Destination = destination;
        Fare = CalculateFare();
        Status = "Pending";
        Driver = null;
    }

    public decimal CalculateFare()
    {
        return 25.0m; // Fixed fare for simplicity
    }

    public void AssignDriver(Driver driver)
    {
        Driver = driver;
        Status = "Assigned";
    }

    public void CompleteTrip()
    {
        Status = "Completed";
        Console.WriteLine($"Trip from {StartLocation} to {Destination} has been completed. Fare: {Fare:C}");
    }

    public void DisplayTripDetails()
    {
        Console.WriteLine($"TripID: {TripID}, Rider: {Rider.Name}, Driver: {Driver?.Name ?? "Pending"}, From: {StartLocation}, To: {Destination}, Fare: {Fare:C}, Status: {Status}");
    }
}

// RideSharingSystem Class
class RideSharingSystem
{
    private List<Rider> registeredRiders = new List<Rider>();
    private List<Driver> registeredDrivers = new List<Driver>();
    private List<Trip> rides = new List<Trip>();

    public void RegisterUser(User user)
    {
        if (user is Rider rider)
        {
            registeredRiders.Add(rider);
        }
        else if (user is Driver driver)
        {
            registeredDrivers.Add(driver);
        }
        user.Register();
    }

    public void RequestRide(Rider rider, string startLocation, string destination)
    {
        var trip = new Trip(Guid.NewGuid().ToString(), rider, startLocation, destination);
        rides.Add(trip);
        rider.RideHistory.Add(trip);
        Console.WriteLine("Ride requested successfully!");
    }

    public void AssignDriver(Driver driver, Trip trip)
    {
        if (driver.IsAvailable)
        {
            driver.AcceptRide(trip);
            trip.AssignDriver(driver);
        }
    }

    public void CompleteTrip(Trip trip)
    {
        trip.CompleteTrip();
    }

    public void DisplayAllTrips()
    {
        if (rides.Count == 0)
        {
            Console.WriteLine("No trips found.");
            return;
        }
        foreach (var trip in rides)
        {
            trip.DisplayTripDetails();
        }
    }

    public List<Trip> GetRides()
    {
        return rides;
    }
}

// Main Program
class Program
{
    static void Main()
    {
        RideSharingSystem system = new RideSharingSystem();
        Rider? currentRider = null;
        Driver? currentDriver = null;

        while (true)
        {
            Console.WriteLine("\nWelcome to the Ride-Sharing System");
            Console.WriteLine("1. Register as Rider");
            Console.WriteLine("2. Register as Driver");
            Console.WriteLine("3. Request a Ride (Rider)");
            Console.WriteLine("4. Accept a Ride (Driver)");
            Console.WriteLine("5. Complete a Ride (Driver)");
            Console.WriteLine("6. View Ride History (Rider)");
            Console.WriteLine("7. View Ride History (Driver)");
            Console.WriteLine("8. Display All Trips");
            Console.WriteLine("9. Exit");
            Console.Write("Please choose an option: ");
            int choice = int.Parse(Console.ReadLine() ?? "9");

            switch (choice)
            {
                case 1:
                    Console.Write("Enter your name: ");
                    string riderName = Console.ReadLine();
                    Console.Write("Enter your phone number (format: XXX-XXX-XXXX): ");
                    string riderPhone = Console.ReadLine();
                    currentRider = new Rider("R" + (system.GetRides().Count + 1), riderName, riderPhone);
                    system.RegisterUser(currentRider);
                    break;

                case 2:
                    Console.Write("Enter your name: ");
                    string driverName = Console.ReadLine();
                    Console.Write("Enter your phone number (format: XXX-XXX-XXXX): ");
                    string driverPhone = Console.ReadLine();
                    Console.Write("Enter your vehicle details: ");
                    string vehicleDetails = Console.ReadLine();
                    currentDriver = new Driver("D" + (system.GetRides().Count + 1), driverName, driverPhone, "DRV" + (system.GetRides().Count + 1), vehicleDetails);
                    system.RegisterUser(currentDriver);
                    break;

                case 3:
                    if (currentRider != null)
                    {
                        Console.Write("Enter your current location: ");
                        string currentLocation = Console.ReadLine();
                        Console.Write("Enter your destination: ");
                        string destination = Console.ReadLine();
                        currentRider.RequestRide(system, currentLocation, destination);
                    }
                    else
                    {
                        Console.WriteLine("You need to register as a Rider first.");
                    }
                    break;

                case 4:
                    if (currentDriver != null && system.GetRides().Count > 0)
                    {
                        Trip trip = system.GetRides().Find(t => t.Status == "Pending");
                        if (trip != null)
                        {
                            currentDriver.AcceptRide(trip);
                        }
                        else
                        {
                            Console.WriteLine("No pending rides to accept.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("You need to register as a Driver first or there are no rides available.");
                    }
                    break;

                case 5:
                    if (currentDriver != null && system.GetRides().Count > 0)
                    {
                        Trip trip = system.GetRides().Find(t => t.Status == "Assigned" && t.Driver == currentDriver);
                        if (trip != null)
                        {
                            currentDriver.CompleteRide(trip);
                        }
                        else
                        {
                            Console.WriteLine("No assigned rides to complete.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("You need to register as a Driver first or there are no rides to complete.");
                    }
                    break;

                case 6:
                    if (currentRider != null)
                    {
                        currentRider.ViewRideHistory();
                    }
                    else
                    {
                        Console.WriteLine("You need to register as a Rider first.");
                    }
                    break;

                case 7:
                    if (currentDriver != null)
                    {
                        currentDriver.ViewRideHistory();
                    }
                    else
                    {
                        Console.WriteLine("You need to register as a Driver first.");
                    }
                    break;

                case 8:
                    system.DisplayAllTrips();
                    break;

                case 9:
                    Console.WriteLine("Exiting the system. Goodbye!");
                    return;

                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
}
