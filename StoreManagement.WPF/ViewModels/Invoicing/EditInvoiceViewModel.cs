namespace StoreManagement.WPF.ViewModels.Invoicing
{
    public partial class EditInvoiceViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly MainViewModel _mainViewModel;
        private readonly InvoiceType _invoiceType;

        // --- Properties اصلی فاکتور ---
        [ObservableProperty] private long _invoiceId;
        [ObservableProperty] private string _invoiceNumber;
        [ObservableProperty] private DateTime _invoiceDate;
        [ObservableProperty] private string _partyName;
        [ObservableProperty] private long _partyId;
        [ObservableProperty] private PaymentType _paymentType;

        // --- Properties مربوط به لیست آیتم‌ها ---
        [ObservableProperty] private ObservableCollection<InvoiceItemViewModel> _items;

        // --- Properties مربوط به افزودن آیتم جدید ---
        [ObservableProperty] private string _searchQuery;
        [ObservableProperty] private ObservableCollection<ProductSearchResultDto> _products;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(AddItemToInvoiceCommand))] private ProductSearchResultDto _selectedProduct;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(AddItemToInvoiceCommand))] private int _quantityToAdd = 1;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(AddItemToInvoiceCommand))] private decimal _priceToAdd;

        // --- Properties مربوط به خلاصه مالی و تسویه ---
        private decimal _originalTotalAmount;
        [ObservableProperty] private decimal _paidAmount;
        [ObservableProperty] private decimal _newTotalAmount;
        [ObservableProperty] private string _financialSummary;
        [ObservableProperty] private bool _isBalanceChanged;
        [ObservableProperty] private ObservableCollection<BankAccountDto> _bankAccounts;
        [ObservableProperty] private BankAccountDto _selectedBankAccountForAdjustment;

        public string PageTitle => _invoiceType == InvoiceType.Sales ? "ویرایش فاکتور فروش" : "ویرایش فاکتور خرید";

        public EditInvoiceViewModel(IMediator mediator, ISnackbarMessageQueue snackbarMessageQueue, MainViewModel mainViewModel, long invoiceId, InvoiceType invoiceType)
        {
            _mediator = mediator;
            _snackbarMessageQueue = snackbarMessageQueue;
            _mainViewModel = mainViewModel;
            _invoiceId = invoiceId;
            _invoiceType = invoiceType;

            Items = new ObservableCollection<InvoiceItemViewModel>();
            Products = new ObservableCollection<ProductSearchResultDto>();
            BankAccounts = new ObservableCollection<BankAccountDto>();

            Task.Run(LoadInitialData);
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (INotifyPropertyChanged item in e.NewItems)
                    item.PropertyChanged += Item_PropertyChanged;

            if (e.OldItems != null)
                foreach (INotifyPropertyChanged item in e.OldItems)
                    item.PropertyChanged -= Item_PropertyChanged;

            RecalculateTotals();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InvoiceItemViewModel.TotalPrice))
            {
                RecalculateTotals();
            }
        }

        private async Task LoadInitialData()
        {
            await LoadInvoiceDetails();
            await SearchProduct();
            if (_paymentType == PaymentType.Cash)
            {
                await LoadBankAccounts();
            }
        }

        private async Task LoadInvoiceDetails()
        {
            var query = new GetInvoiceForEditQuery { InvoiceId = _invoiceId, InvoiceType = _invoiceType };
            var dto = await _mediator.Send(query);
            if (dto != null)
            {
                InvoiceNumber = dto.InvoiceNumber;
                InvoiceDate = dto.InvoiceDate;
                PartyName = dto.PartyName;
                PartyId = dto.PartyId;
                PaymentType = dto.PaymentType;
                _originalTotalAmount = dto.TotalAmount;
                PaidAmount = dto.PaidAmount;

                var itemViewModels = dto.Items.Select(i => new InvoiceItemViewModel
                {
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountPercentage = i.DiscountPercentage,
                    TaxPercentage = i.TaxPercentage
                }).ToList();

                Items = new ObservableCollection<InvoiceItemViewModel>(itemViewModels);
                Items.CollectionChanged += Items_CollectionChanged;
                foreach (var item in Items)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
                RecalculateTotals();
            }
        }

        [RelayCommand]
        private async Task SearchProduct()
        {
            var productsResult = await _mediator.Send(new GetProductsForInvoiceQuery { SearchTerm = this.SearchQuery });

            // به جای ساختن نمونه جدید، لیست فعلی را مدیریت می‌کنیم
            Products.Clear();
            if (productsResult != null)
            {
                foreach (var product in productsResult)
                {
                    Products.Add(product);
                }
            }
        }

        partial void OnSearchQueryChanged(string value)
        {
            if (SearchProductCommand.CanExecute(null))
            {
                SearchProductCommand.Execute(null);
            }
        }

        private async Task LoadBankAccounts()
        {
            var accountsResult = await _mediator.Send(new GetAllBankAccountsQuery());
            if (accountsResult.IsSuccess)
            {
                BankAccounts = new ObservableCollection<BankAccountDto>(accountsResult.Value);
            }
        }

        private void RecalculateTotals()
        {
            NewTotalAmount = Items.Sum(i => i.TotalPrice);
            var balance = NewTotalAmount - _originalTotalAmount;
            FinancialSummary = $"مبلغ کل قبلی: {_originalTotalAmount:N0} | مبلغ کل جدید: {NewTotalAmount:N0} | مابه التفاوت: {balance:N0}";
            IsBalanceChanged = balance != 0;
            SaveChangesCommand.NotifyCanExecuteChanged();
        }

        private bool CanAddItemToInvoice() => SelectedProduct != null && QuantityToAdd > 0;

        [RelayCommand(CanExecute = nameof(CanAddItemToInvoice))]
        private void AddItemToInvoice()
        {
            // بررسی اینکه آیا این واریانت از قبل در لیست وجود دارد
            var existingItem = Items.FirstOrDefault(i => i.ProductVariantId == SelectedProduct.VariantId);
            if (existingItem != null)
            {
                // اگر وجود داشت، فقط تعداد را به آن اضافه می‌کنیم
                existingItem.Quantity += QuantityToAdd;
            }
            else
            {
                // در غیر این صورت، یک آیتم جدید به لیست اضافه می‌کنیم
                var newItem = new InvoiceItemViewModel
                {
                    ProductVariantId = SelectedProduct.VariantId,
                    ProductName = $"{SelectedProduct.Name} ({SelectedProduct.DisplayVariantName})", // نام کامل و با جزئیات
                    Quantity = QuantityToAdd,
                    // استفاده از قیمت وارد شده توسط کاربر، در غیر این صورت قیمت آخرین فروش
                    UnitPrice = PriceToAdd > 0 ? PriceToAdd : (SelectedProduct.LastSalePrice ?? 0),
                    DiscountPercentage = 0, // مقدار پیش فرض
                    TaxPercentage = 0       // مقدار پیش فرض
                };
                Items.Add(newItem);
            }

            // پاک کردن فیلدها برای افزودن آیتم بعدی
            SelectedProduct = null;
            QuantityToAdd = 1;
            PriceToAdd = 0;
        }

        [RelayCommand]
        private void DeleteItem(InvoiceItemViewModel item)
        {
            if (item != null) Items.Remove(item);
        }

        private bool CanSaveChanges() => IsBalanceChanged;

        [RelayCommand(CanExecute = nameof(CanSaveChanges))]
        private async Task SaveChanges()
        {
            // اگر فاکتور از نوع نقدی باشد و مانده حساب تغییر کرده باشد، باید حساب بانکی برای تسویه انتخاب شود
            if (PaymentType == PaymentType.Cash && IsBalanceChanged && SelectedBankAccountForAdjustment == null)
            {
                _snackbarMessageQueue.Enqueue("برای ذخیره تغییرات، لطفاً یک حساب بانکی جهت تسویه مابه‌التفاوت انتخاب کنید.");
                return;
            }

            try
            {
                var itemsDto = Items.Select(vm => new InvoiceItemDto
                {
                    ProductVariantId = vm.ProductVariantId,
                    Quantity = vm.Quantity,
                    UnitPrice = vm.UnitPrice,
                    DiscountPercentage = vm.DiscountPercentage,
                    TaxPercentage = vm.TaxPercentage
                }).ToList();

                if (_invoiceType == InvoiceType.Purchase)
                {
                    var command = new UpdatePurchaseInvoiceCommand
                    {
                        InvoiceId = this.InvoiceId,
                        InvoiceNumber = this.InvoiceNumber,
                        InvoiceDate = this.InvoiceDate,
                        StoreId = this.PartyId,
                        Items = itemsDto,
                        BankAccountIdForAdjustment = SelectedBankAccountForAdjustment?.Id
                    };
                    await _mediator.Send(command);
                }
                else // Sales
                {
                    var command = new UpdateSalesInvoiceCommand
                    {
                        InvoiceId = this.InvoiceId,
                        InvoiceNumber = this.InvoiceNumber,
                        InvoiceDate = this.InvoiceDate,
                        CustomerId = this.PartyId,
                        Items = itemsDto,
                        BankAccountIdForAdjustment = SelectedBankAccountForAdjustment?.Id
                    };
                    await _mediator.Send(command);
                }

                _snackbarMessageQueue.Enqueue("تغییرات با موفقیت ذخیره شد.");

                if (_invoiceType == InvoiceType.Sales)
                {
                    _mainViewModel.NavigateToListSalesInvoices();
                }
                else
                {
                    _mainViewModel.NavigateToListPurchaseInvoices();
                }
            }
            catch (Exception ex)
            {
                _snackbarMessageQueue.Enqueue($"خطا در ذخیره تغییرات: {ex.Message}");
            }
        }
    }
}