namespace StoreManagement.Application.Features.Parties.Queries;

public class GetPartiesQuery : IRequest<IEnumerable<PartyDto>>
{
    public string SearchTerm { get; set; }
    public PartyTypeToQuery Type { get; set; }
}

public class GetPartiesQueryHandler : IRequestHandler<GetPartiesQuery, IEnumerable<PartyDto>>
{
    private readonly IDapperRepository _dapper;

    public GetPartiesQueryHandler(IDapperRepository dapper)
    {
        _dapper = dapper ?? throw new ArgumentNullException(nameof(dapper));
    }

    public async Task<IEnumerable<PartyDto>> Handle(GetPartiesQuery request, CancellationToken cancellationToken)
    {
        var searchTermParam = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : $"%{request.SearchTerm}%";
        var parameters = new { SearchTerm = searchTermParam };
        string sql;

        if (request.Type == PartyTypeToQuery.Customers)
        {
            sql = @"
                    SELECT 
                        Id, 
                        (FirstName + ' ' + LastName) as Name, 
                        CAST(NationalCode AS VARCHAR(20)) as DisplayCode, 
                        'مشتری' as PartyType 
                    FROM 
                        Customers
                    WHERE 
                        @SearchTerm IS NULL 
                        OR (FirstName + ' ' + LastName) LIKE @SearchTerm 
                        OR CAST(NationalCode AS VARCHAR(20)) LIKE @SearchTerm
                    ORDER BY 
                        LastName, FirstName;";
        }
        else // Store
        {
            sql = @"
                    SELECT 
                        Id, 
                        Name, 
                        '' as DisplayCode, -- فروشگاه‌ها کد مشخصی برای نمایش ندارند
                        'فروشگاه' as PartyType 
                    FROM 
                        Stores
                    WHERE 
                        @SearchTerm IS NULL OR Name LIKE @SearchTerm
                    ORDER BY 
                        Name;";
        }

        return await _dapper.QueryAsync<PartyDto>(sql, parameters, cancellationToken: cancellationToken);
    }
}