namespace StoreManagement.Application.Features.Installments.Commands;

public class PayInstallmentCommand : IRequest
{
    public long InstallmentId { get; set; }
    public decimal Amount { get; set; }
    public long BankAccountId { get; set; } // حسابی که پرداخت به آن واریز/از آن کسر می‌شود
    public DateTime PaymentDate { get; set; }
}

public class PayInstallmentCommandHandler(
    IGenericRepository<Installment> installmentRepository,
    IGenericRepository<BankAccount> bankAccountRepository)
    : IRequestHandler<PayInstallmentCommand>
{
    public async Task Handle(PayInstallmentCommand request, CancellationToken cancellationToken)
    {
        var installment = await installmentRepository.GetByIdAsync(request.InstallmentId, cancellationToken);
        if (installment == null) throw new InvalidOperationException("قسط مورد نظر یافت نشد.");
        if (installment.Status == InstallmentStatus.Paid) throw new InvalidOperationException("این قسط قبلاً به طور کامل پرداخت شده است.");

        var bankAccount = await bankAccountRepository.GetByIdForUpdateAsync(request.BankAccountId, cancellationToken);
        if (bankAccount == null) throw new InvalidOperationException("حساب بانکی مورد نظر یافت نشد.");

        installment.MarkAsPaid(request.Amount);

        var transactionDirection = installment.InvoiceType == InvoiceType.Sales ? TransactionType.Credit : TransactionType.Debit;
        var description = $"پرداخت قسط شماره {installment.InstallmentNumber} فاکتور {installment.InvoiceId}";

        var financialTx = new FinancialTransaction(
            bankAccount,
            request.Amount,
            transactionDirection,
            description,
            installment.InvoiceId,
            installment.InvoiceType
        );

        bankAccount.AddTransaction(financialTx);

        await installmentRepository.UpdateAsync(installment, cancellationToken);
        await bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
        await installmentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}