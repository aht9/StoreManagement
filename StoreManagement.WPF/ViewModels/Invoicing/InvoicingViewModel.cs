namespace StoreManagement.WPF.ViewModels.Invoicing;

public partial class InvoicingViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly ISnackbarMessageQueue _snackbarMessageQueue;

    [ObservableProperty]
    private InvoiceType _currentInvoiceType;
    public bool IsSalesMode => CurrentInvoiceType == InvoiceType.Sales;

    public string PageTitle => CurrentInvoiceType == InvoiceType.Sales ? "صدور فاکتور فروش" : "ثبت فاکتور خرید";
    public string PartySelectionTitle => CurrentInvoiceType == InvoiceType.Sales ? "انتخاب مشتری" : "انتخاب فروشگاه";

    [ObservableProperty]
    private string _searchQuery;

    [ObservableProperty]
    private ObservableCollection<ProductSearchResultDto> _products;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddItemToInvoiceCommand))]
    private ProductSearchResultDto _selectedProduct;

    [ObservableProperty]
    private ObservableCollection<InvoiceItemViewModel> _invoiceItems = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowPaymentDialogCommand))]
    private PartyDto _selectedParty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddItemToInvoiceCommand))]
    private int _quantityToAdd = 1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddItemToInvoiceCommand))]
    private decimal _priceToAdd;

    [ObservableProperty]
    private int _discountPercentageToAdd;

    [ObservableProperty]
    private int _taxPercentageToAdd;

    [ObservableProperty]
    private decimal? _salePriceForPurchase;

    /// <summary>
    /// مبلغ کل نهایی فاکتور که به صورت خودکار به‌روز می‌شود.
    /// </summary>
    public decimal GrandTotal => InvoiceItems.Sum(item => item.TotalPrice);

    public InvoicingViewModel(InvoiceType invoiceType, IMediator mediator, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _currentInvoiceType = invoiceType;
        _mediator = mediator;
        _snackbarMessageQueue = snackbarMessageQueue;
        _invoiceItems.CollectionChanged += OnInvoiceItemsChanged;
        Task.Run(SearchProduct);
    }

    /// <summary>
    /// این متد هر زمان که آیتمی به لیست اضافه یا از آن حذف شود، فراخوانی خواهد شد.
    /// </summary>
    private void OnInvoiceItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
     
        OnPropertyChanged(nameof(GrandTotal));
        ShowPaymentDialogCommand.NotifyCanExecuteChanged();

        if (e.NewItems != null)
        {
            foreach (InvoiceItemViewModel item in e.NewItems)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }
        if (e.OldItems != null)
        {
            foreach (InvoiceItemViewModel item in e.OldItems)
            {
                item.PropertyChanged -= Item_PropertyChanged;
            }
        }
    }

    /// <summary>
    /// این متد زمانی اجرا می‌شود که یکی از پراپرتی‌های آیتم‌های داخل لیست تغییر کند.
    /// </summary>
    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(InvoiceItemViewModel.TotalPrice))
        {
            OnPropertyChanged(nameof(GrandTotal));
        }
    }


    [RelayCommand]
    private async Task SearchProduct()
    {
        var productsResult = await _mediator.Send(new GetProductsForInvoiceQuery { SearchTerm = this.SearchQuery });
        Products = new ObservableCollection<ProductSearchResultDto>(productsResult);
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (SearchProductCommand.CanExecute(null))
        {
            SearchProductCommand.Execute(null);
        }
    }

    private bool CanAddItemToInvoice()
    {
        return SelectedProduct != null &&
               QuantityToAdd > 0 &&
               PriceToAdd > 0 &&
               DiscountPercentageToAdd >= 0 && 
               TaxPercentageToAdd >= 0;        
    }

    [RelayCommand(CanExecute = nameof(CanAddItemToInvoice))]
    private void AddItemToInvoice()
    {
        var newItem = new InvoiceItemViewModel
        {
            ProductVariantId = SelectedProduct.VariantId,
            ProductName = $"{SelectedProduct.Name} ({SelectedProduct.DisplayVariantName})",
            Quantity = QuantityToAdd,
            UnitPrice = PriceToAdd,
            DiscountPercentage = DiscountPercentageToAdd,
            TaxPercentage = TaxPercentageToAdd,
            SalePriceForPurchase = SalePriceForPurchase
        };
        InvoiceItems.Add(newItem);
        FinalizeAndSubmitCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void RemoveItem(InvoiceItemViewModel item)
    {
        if (item != null)
        {
            InvoiceItems.Remove(item);
            FinalizeAndSubmitCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanFinalizeAndSubmit() => InvoiceItems.Any() && SelectedParty != null;

    [RelayCommand(CanExecute = nameof(CanFinalizeAndSubmit))]
    private async Task FinalizeAndSubmit()
    {
        try
        {
            if (CurrentInvoiceType == InvoiceType.Purchase)
            {
                var command = new CreatePurchaseInvoiceCommand
                {
                    StoreId = SelectedParty.Id,
                    InvoiceNumber = Guid.NewGuid().ToString(),
                    InvoiceDate = DateTime.Now,
                    PaymentType = PaymentType.Cash, 
                    Items = new List<InvoiceItemDto>(InvoiceItems.Select(vm => new InvoiceItemDto
                    {
                        ProductVariantId = vm.ProductVariantId,
                        Quantity = vm.Quantity,
                        UnitPrice = vm.UnitPrice,
                        DiscountPercentage = vm.DiscountPercentage,
                        TaxPercentage = vm.TaxPercentage,
                        SalePriceForPurchase = vm.SalePriceForPurchase
                    }))
                };
                await _mediator.Send(command);
            }
            else // Sales
            {
                var command = new CreateSalesInvoiceCommand
                {
                    CustomerId = SelectedParty.Id,
                    InvoiceNumber = Guid.NewGuid().ToString(),
                    InvoiceDate = DateTime.Now,
                    PaymentType = PaymentType.Cash,
                    Items = new List<InvoiceItemDto>(InvoiceItems.Select(vm => new InvoiceItemDto
                    {
                        ProductVariantId = vm.ProductVariantId,
                        Quantity = vm.Quantity,
                        UnitPrice = vm.UnitPrice,
                        DiscountPercentage = vm.DiscountPercentage,
                        TaxPercentage = vm.TaxPercentage
                    }))
                };
                await _mediator.Send(command);
            }
            // نمایش پیام موفقیت و پاک کردن فرم
        }
        catch (Exception ex)
        {
            // نمایش خطا به کاربر
        }
    }

    [RelayCommand]
    private async Task ShowAddProductDialog()
    {
        var dialogViewModel = new AddProductDialogViewModel(_mediator);

        var dialogView = new AddProductDialogView
        {
            DataContext = dialogViewModel
        };

        var result = await DialogHost.Show(dialogView, "RootDialog");

        if (result is ProductSearchResultDto newProduct)
        {
            await SearchProduct();
            this.SelectedProduct = Products.FirstOrDefault(p => p.VariantId == newProduct.VariantId);
        }
    }


    [RelayCommand(CanExecute = nameof(CanProceedToPayment))]
    private async Task ShowPaymentDialog()
    {
        var paymentDialogViewModel = new PaymentDialogViewModel(_mediator, this.GrandTotal);
        var paymentDialogView = new PaymentDialogView { DataContext = paymentDialogViewModel };

        var resultViewModel = await DialogHost.Show(paymentDialogView, "RootDialog") as PaymentDialogViewModel;

        if (resultViewModel != null)
        {
            var paymentResult = resultViewModel.GetPaymentResult();
            await FinalizeAndSubmit(paymentResult);
        }
    }

    private bool CanProceedToPayment() => InvoiceItems.Any() && SelectedParty != null;

    private async Task FinalizeAndSubmit(PaymentResult paymentResult)
    {
        try
        {
            if (CurrentInvoiceType == InvoiceType.Purchase)
            {
                var command = new CreatePurchaseInvoiceCommand
                {
                    StoreId = this.SelectedParty.Id,
                    InvoiceNumber = Guid.NewGuid().ToString(),
                    InvoiceDate = DateTime.Now,

                    Items = new List<InvoiceItemDto>(InvoiceItems.Select(vm => new InvoiceItemDto
                    {
                        ProductVariantId = vm.ProductVariantId,
                        Quantity = vm.Quantity,
                        UnitPrice = vm.UnitPrice,
                        DiscountPercentage = vm.DiscountPercentage,
                        TaxPercentage = vm.TaxPercentage,
                        SalePriceForPurchase = vm.SalePriceForPurchase
                    })),

                    PaymentType = paymentResult.PaymentType,
                    BankAccountId = paymentResult.BankAccountId,
                    InstallmentDetails = paymentResult.InstallmentDetails
                };
                await _mediator.Send(command);
            }
            else // Sales
            {
                var command = new CreateSalesInvoiceCommand
                {
                    CustomerId = this.SelectedParty.Id,
                    InvoiceNumber = Guid.NewGuid().ToString(),
                    InvoiceDate = DateTime.Now,

                    Items = new List<InvoiceItemDto>(InvoiceItems.Select(vm => new InvoiceItemDto
                    {
                        ProductVariantId = vm.ProductVariantId,
                        Quantity = vm.Quantity,
                        UnitPrice = vm.UnitPrice,
                        DiscountPercentage = vm.DiscountPercentage,
                        TaxPercentage = vm.TaxPercentage
                    })),

                    PaymentType = paymentResult.PaymentType,
                    BankAccountId = paymentResult.BankAccountId,
                    InstallmentDetails = paymentResult.InstallmentDetails
                };
                await _mediator.Send(command);
            }

            _snackbarMessageQueue.Enqueue("فاکتور با موفقیت ثبت شد.");
            ClearForm();
        }
        catch (Exception ex)
        {
            _snackbarMessageQueue.Enqueue($"خطا در ثبت فاکتور: {ex.Message}");
        }
    }

    private void ClearForm()
    {
        InvoiceItems.Clear();
        SelectedParty = null;
        SelectedProduct = null;
        QuantityToAdd = 1;
        PriceToAdd = 0;
        DiscountPercentageToAdd = 0;
        TaxPercentageToAdd = 0;
        SalePriceForPurchase = null;
    }


    [RelayCommand]
    private async Task ShowSelectPartyDialog()
    {
        bool shouldAddNew;
        do
        {
            shouldAddNew = false; // ریست کردن فلگ در ابتدای هر حلقه

            var partyType = (CurrentInvoiceType == InvoiceType.Sales)
                ? PartyTypeToQuery.Customers
                : PartyTypeToQuery.Stores;

            var selectDialogViewModel = new SelectPartyDialogViewModel(_mediator, partyType);
            var selectDialogView = new SelectPartyDialogView { DataContext = selectDialogViewModel };

            // دیالوگ انتخاب را باز می‌کنیم و منتظر نتیجه می‌مانیم
            var result = await DialogHost.Show(selectDialogView, "RootDialog");

            if (result is PartyDto selectedParty)
            {
                // اگر کاربر یک طرف حساب را انتخاب کرد، آن را ذخیره کرده و از حلقه خارج شو
                this.SelectedParty = selectedParty;
                return;
            }

            if (result is string str && str == "AddNew")
            {
                // اگر کاربر روی دکمه "افزودن جدید" کلیک کرده بود
                shouldAddNew = true; // فلگ را برای تکرار حلقه ست می‌کنیم

                var addDialogViewModel = new AddPartyDialogViewModel(_mediator); // ViewModel دیالوگ افزودن
                var addDialogView = new AddPartyDialogView { DataContext = addDialogViewModel };

                // دیالوگ افزودن را باز می‌کنیم و منتظر نتیجه می‌مانیم
                // (این دیالوگ پس از بسته شدن دیالوگ قبلی باز می‌شود)
                await DialogHost.Show(addDialogView, "RootDialog");
            }
            else
            {
                // اگر کاربر دیالوگ را با دکمه "لغو" یا از راه دیگری بسته بود، از حلقه خارج شو
                return;
            }

        } while (shouldAddNew); // تا زمانی که کاربر در حال افزودن آیتم جدید است، حلقه تکرار می‌شود
    }

    partial void OnSelectedProductChanged(ProductSearchResultDto value)
    {
        if (CurrentInvoiceType == InvoiceType.Sales && value != null)
        {
            PriceToAdd = value.LastSalePrice ?? 0;
        }
    }

    partial void OnCurrentInvoiceTypeChanged(InvoiceType value)
    {
        OnPropertyChanged(nameof(IsSalesMode));
    }
}