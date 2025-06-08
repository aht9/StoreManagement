using StoreManagement.Domain.Aggregates.Invoices;

namespace StoreManagement.Domain.Aggregates.Customers
{
    public class Customer : BaseEntity, IAggregateRoot
    {
        // Private fields for encapsulation
        private string _firstName;
        private string _lastName;
        private string? _email;
        private PhoneNumber _phoneNumber;
        private Address _address;
        private DateTime? _dateOfBirth;
        private long? _nationalCode;

        // Public properties with validation logic
        public string FirstName
        {
            get => _firstName;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("First name cannot be empty.");
                _firstName = value;
            }
        }

        public string LastName
        {
            get => _lastName;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Last name cannot be empty.");
                _lastName = value;
            }
        }

        public string? Email
        {
            get => _email;
            private set
            {
                if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
                    throw new ArgumentException("Invalid email address.");
                _email = value;
            }
        }

        public PhoneNumber PhoneNumber
        {
            get => _phoneNumber;
            private set
            {
                if (value == null)
                    throw new ArgumentException("Phone number cannot be null.");
                _phoneNumber = value;
            }
        }

        public Address Address
        {
            get => _address;
            private set
            {
                if (value == null)
                    throw new ArgumentException("Address cannot be null.");
                _address = value;
            }
        }

        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            private set
            {
                if (value >= DateTime.Now)
                    throw new ArgumentException("Date of birth must be in the past.");
                _dateOfBirth = value;
            }
        }

        public long? NationalCode
        {
            get => _nationalCode;
            private set
            {
                if (value <= 0)
                    throw new ArgumentException("National code must be a positive number.");
                _nationalCode = value;
            }
        }

        // Navigation properties for relationships (if any)
        private List<SalesInvoice> _salesInvoices = new List<SalesInvoice>();
        public IReadOnlyCollection<SalesInvoice> SalesInvoices => _salesInvoices.AsReadOnly();

        public void AddSalesInvoice(SalesInvoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice), "Sale invoice cannot be null.");
            _salesInvoices.Add(invoice);
        }

        public void RemoveSalesInvoice(SalesInvoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice), "Sale invoice cannot be null.");
            _salesInvoices.Remove(invoice);
        }


        // Parameterless constructor for EF Core or serialization
        private Customer() { }

        // Constructor for creating a customer
        public Customer(string firstName, string lastName, string? email, string phoneNumber, string city, string fullAddress, DateTime? dateOfBirth, long? nationalCode)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            var phoneNumberResult = PhoneNumber.Create(phoneNumber);
            if (phoneNumberResult.IsFailure)
            {
                throw new ArgumentException(phoneNumberResult.Error);
            }

            PhoneNumber = phoneNumberResult.Value;
            Address = new Address(city, fullAddress);
            DateOfBirth = dateOfBirth;
            NationalCode = nationalCode;
        }

        // Behavior methods
        public void UpdateContactInfo(string email, string phoneNumber)
        {
            Email = email;
            PhoneNumber = PhoneNumber.Create(phoneNumber).Value; 
        }

        public void UpdateAddress(Address address)
        {
            Address = address;
        }

        public void ValidateCustomer()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
                throw new InvalidOperationException("Customer must have a valid name.");
            if (DateOfBirth >= DateTime.Now)
                throw new InvalidOperationException("Customer must have a valid date of birth.");
        }

        public override string ToString()
        {
            return $"{FirstName} {LastName}, Email: {Email}, Phone: {PhoneNumber.GetFormattedNumber()}, Address: {Address}";
        }
    }
}
