using StoreManagement.Domain.ValueObjects;

namespace StoreManagement.Domain.Aggregates.Stores
{
    public class Store : BaseEntity, IAggregateRoot
    {
        private string _name;
        private string _location;
        private string _managerName;
        private string _contactNumber;
        private string _email;
        private PhoneNumber _phoneNumber;
        private Address _address;

        public string Name
        {
            get => _name;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Store name cannot be empty.");
                _name = value;
            }
        }

        public string Location
        {
            get => _location;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Location cannot be empty.");
                _location = value;
            }
        }

        public string ManagerName
        {
            get => _managerName;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Manager name cannot be empty.");
                _managerName = value;
            }
        }

        public string ContactNumber
        {
            get => _contactNumber;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Contact number cannot be empty.");
                _contactNumber = value;
            }
        }

        public string Email
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

        private Store()
        {
        }

        public Store(string name, string location, string managerName, string contactNumber, string email,
            PhoneNumber phoneNumber, Address address)
        {
            Name = name;
            Location = location;
            ManagerName = managerName;
            ContactNumber = contactNumber;
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address;
        }

        public void UpdateStoreDetails(string name, string location, string managerName, string contactNumber,
            string email, PhoneNumber phoneNumber, Address address)
        {
            Name = name;
            Location = location;
            ManagerName = managerName;
            ContactNumber = contactNumber;
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address;
            UpdateEntity();
        }
    }
}
