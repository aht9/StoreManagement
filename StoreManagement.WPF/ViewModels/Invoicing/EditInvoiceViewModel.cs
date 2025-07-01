namespace StoreManagement.WPF.ViewModels.Invoicing;

public partial class EditInvoiceViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly ISnackbarMessageQueue _snackbarMessageQueue; // برای نمایش پیام
    private readonly MainViewModel _mainViewModel; // برای ناوبری
    private readonly InvoiceType _invoiceType;

    // --- Properties اصلی فاکتور ---
    [ObservableProperty] private long _invoiceId;
    [ObservableProperty] private string _invoiceNumber;
    [ObservableProperty] private DateTime _invoiceDate;
    [ObservableProperty] private string _partyName; // نام طرف حساب (غیرقابل تغییر)
    [ObservableProperty] private long _partyId; // شناسه طرف حساب (برای ارسال Command)

    // --- Properties مربوط به لیست آیتم‌ها ---
    [ObservableProperty]
    private ObservableCollection<InvoiceItemViewModel> _items;

    // --- Properties مربوط به جستجو و افزودن آیتم جدید ---
    [ObservableProperty] private string _searchQuery;
    [ObservableProperty] private ObservableCollection<ProductSearchResultDto> _products;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(AddItemToInvoiceCommand))] private ProductSearchResultDto _selectedProduct;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(AddItemToInvoiceCommand))] private int _quantityToAdd = 1;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(AddItemToInvoiceCommand))] private decimal _priceToAdd;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(AddItemToInvoiceCommand))] private int _discountPercentageToAdd;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(AddItemToInvoiceCommand))] private int _taxPercentageToAdd;

    public string PageTitle => _invoiceType == InvoiceType.Sales ? "ویرایش فاکتور فروش" : "ویرایش فاکتور خرید";

    public EditInvoiceViewModel(IMediator mediator, ISnackbarMessageQueue snackbarMessageQueue, MainViewModel mainViewModel, long invoiceId, InvoiceType invoiceType)
    {
        _mediator = mediator;
        _snackbarMessageQueue = snackbarMessageQueue;
        _mainViewModel = mainViewModel;
        _invoiceId = invoiceId;
        _invoiceType = invoiceType;

        Task.Run(LoadInitialData);
    }

    private async Task LoadInitialData()
    {
        await LoadInvoiceDetails();
        await SearchProduct();
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
            Items = new ObservableCollection<InvoiceItemViewModel>(
                dto.Items.Select(i => new InvoiceItemViewModel
                {
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountPercentage = i.DiscountPercentage,
                    TaxPercentage = i.TaxPercentage
                })
            );
        }
    }

    [RelayCommand]
    private async Task SearchProduct()
    {
        var productsResult = await _mediator.Send(new GetProductsForInvoiceQuery { SearchTerm = this.SearchQuery });
        Products = new ObservableCollection<ProductSearchResultDto>(productsResult);
    }

    private bool CanAddItemToInvoice()
    {
        return SelectedProduct != null && QuantityToAdd > 0 && PriceToAdd > 0 &&
               DiscountPercentageToAdd >= 0 && TaxPercentageToAdd >= 0;
    }

    [RelayCommand(CanExecute = nameof(CanAddItemToInvoice))]
    private void AddItemToInvoice()
    {
        var newItem = new InvoiceItemViewModel
        {
            ProductVariantId = SelectedProduct.VariantId, 
            ProductName = SelectedProduct.Name,
            Quantity = QuantityToAdd,
            UnitPrice = PriceToAdd,
            DiscountPercentage = DiscountPercentageToAdd,
            TaxPercentage = TaxPercentageToAdd
        };
        Items.Add(newItem);
    }

    [RelayCommand]
    private void RemoveItem(InvoiceItemViewModel item)
    {
        if (item != null) Items.Remove(item);
    }

    [RelayCommand]
    private async Task SaveChanges()
    {
        try
        {
            var itemsDto = new List<InvoiceItemDto>(Items.Select(vm => new InvoiceItemDto
            {
                ProductVariantId = vm.ProductVariantId,
                Quantity = vm.Quantity,
                UnitPrice = vm.UnitPrice,
                DiscountPercentage = vm.DiscountPercentage,
                TaxPercentage = vm.TaxPercentage
            }));


            if (_invoiceType == InvoiceType.Purchase)
            {
                var command = new UpdatePurchaseInvoiceCommand
                {
                    InvoiceId = this.InvoiceId,
                    InvoiceNumber = this.InvoiceNumber,
                    InvoiceDate = this.InvoiceDate,
                    StoreId = this.PartyId,
                    Items = new List<InvoiceItemDto>(Items.Select(vm => new InvoiceItemDto
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
            else // Sales
            {
                var command = new UpdateSalesInvoiceCommand
                {
                    InvoiceId = this.InvoiceId,
                    InvoiceNumber = this.InvoiceNumber,
                    InvoiceDate = this.InvoiceDate,
                    CustomerId = this.PartyId,
                    Items = itemsDto
                };
                await _mediator.Send(command);
            }

            _snackbarMessageQueue.Enqueue("تغییرات با موفقیت ذخیره شد.");
            // بازگشت به صفحه لیست فاکتورها
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