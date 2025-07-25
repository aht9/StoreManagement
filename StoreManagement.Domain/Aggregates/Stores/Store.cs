﻿namespace StoreManagement.Domain.Aggregates.Stores
{
    public class Store : BaseEntity, IAggregateRoot
    {
        private string _name;
        private string? _location;
        private string? _managerName;
        private string? _contactNumber;
        private string? _email;
        private PhoneNumber _phone;
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

        public string? Location
        {
            get => _location;
            private set
            {
                _location = value;
            }
        }

        public string? ManagerName
        {
            get => _managerName;
            private set
            {
                _managerName = value;
            }
        }

        public string? ContactNumber
        {
            get => _contactNumber;
            private set
            {
                _contactNumber = value;
            }
        }

        public string? Email
        {
            get => _email;
            private set
            {
                _email = value;
            }
        }

        public PhoneNumber Phone
        {
            get => _phone;
            private set
            {
                if (value == null)
                    throw new ArgumentException("Phone number cannot be null.");
                _phone = value;
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

        // Navigation properties for related entities

        private List<PurchaseInvoice> _purchaseInvoices = new List<PurchaseInvoice>();
        public IReadOnlyCollection<PurchaseInvoice> PurchaseInvoices => _purchaseInvoices.AsReadOnly();

        public void AddPurchaseInvoice(PurchaseInvoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice), "Purchase invoice cannot be null.");
            _purchaseInvoices.Add(invoice);
            UpdateTimestamp();
        }

        public void RemovePurchaseInvoice(PurchaseInvoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice), "Purchase invoice cannot be null.");
            if (!_purchaseInvoices.Remove(invoice))
                throw new InvalidOperationException("Purchase invoice not found in the store.");
            UpdateTimestamp();
        }


        // Private constructor for EF Core
        private Store()
        {
        }

        public Store(string name, string? location, string? managerName, string? contactNumber, string? email,
            PhoneNumber phoneNumber, Address address)
        {
            Name = name;
            Location = location;
            ManagerName = managerName;
            ContactNumber = contactNumber;
            Email = email;
            Phone = phoneNumber;
            Address = address;
        }

        public void UpdateStoreDetails(string name, string? location, string? managerName, string? contactNumber,
            string? email, PhoneNumber phoneNumber, Address address)
        {
            Name = name;
            Location = location;
            ManagerName = managerName;
            ContactNumber = contactNumber;
            Email = email;
            Phone = phoneNumber;
            Address = address;
            UpdateTimestamp();
        }
    }
}
