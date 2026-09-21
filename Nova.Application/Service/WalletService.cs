using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;
using Nova.Application.Common;
using Nova.Application.Config;
using Nova.Application.Dto.Request;
using Nova.Application.Dto.Response;
using Nova.Application.Dto.Response.Nova.Application.Dto.Response;
using Nova.Application.Interfaces;
using Nova.Application.Validation;
using Nova.Domain.Entities;
using Nova.Domain.Enums;
using Nova.Domain.Exceptions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static Nova.Domain.Exceptions.NotFoundException;


namespace NovaWallet.Application.Services;

public class WalletService : IWalletService
{


    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;

    private readonly IHelperService _helperService;

    private readonly IValidator<CreditWalletRequest> _validator;
    private readonly IValidator<TransferWalletRequest> _tranValidator;
    private readonly WalletConfig _walletSetting;
    private readonly Logger logger;



    public WalletService(
        IUnitOfWork unitOfWork,
        IMapper mapper,IHelperService helperService, IOptions<WalletConfig> walletSettings, IValidator<CreditWalletRequest> validator, IValidator<TransferWalletRequest> tranValidator)
    {
        _unitOfWork = unitOfWork;
        _walletSetting = walletSettings.Value;
        _mapper = mapper;
        _helperService = helperService;
        _validator = validator;
        _tranValidator = tranValidator;
        logger = LogManager.GetCurrentClassLogger();
    }
    public async Task<ResponseModel< WalletResponse>> GetWalletAsync(Guid customerId)
    {
        logger.Info(
             "Get wallet request started. " +
             "CustomerId: {CustomerId}",
             customerId);

        var wallet =await _helperService.DoesWalletExist(customerId);

        var reponse =_mapper.Map<WalletResponse>(wallet);

        return ResponseModel<WalletResponse>.Ok("Wallet retrieved successfully",reponse);
    
    }
   

  public async Task<ResponseModel<WalletStatementResponse>> GetWalletStatementAsync(
     Guid walletId,
     Guid customerId,
     WalletStatementRequest request)
    {
       
        await _helperService.GetWalletAsync(walletId, customerId);


        var (page, pageSize) =LedgerHelpers.NormalizePagination(request);

    
        var query = _unitOfWork
            .Repository<WalletTransaction>()
            .Query()
            .AsNoTracking()
            .Where(x => x.WalletId == walletId);

        // Filters
        query =LedgerHelpers.ApplyStatementFilters(query, request);

        query = query.OrderByDescending(x => x.CreatedAtUtc);

    
        var totalCount = await query.LongCountAsync();

     
        var items = await LedgerHelpers.ProjectStatementEntries(query)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

      var response  = new WalletStatementResponse
        {
            WalletId = walletId,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };

       return ResponseModel<WalletStatementResponse>.Ok("Reversed successfuly", response);
    }
    public async Task<ResponseModel< DailyLimitStatusResponse>> GetDailyLimitStatusAsync(
        Guid walletId,
        Guid customerId)
    {
        var wallet = _helperService.GetWalletAsync(walletId, customerId);

        var businessDate = CommonUtils.GetNigeriaBusinessDate();

        var record = await _unitOfWork
            .Repository<DailyTransferLimit>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.WalletId == walletId && x.BusinessDate == businessDate);

        var limit = record?.LimitAmount ?? _walletSetting.DailyTransferLimit;
        var used = record?.UsedAmount ?? 0m;

        var response =  new DailyLimitStatusResponse
        {
            WalletId = walletId,
            BusinessDate = businessDate,
            LimitAmount = limit,
            UsedAmount = used,
            RemainingAmount = Math.Max(limit - used, 0m)
        };

        return  ResponseModel<DailyLimitStatusResponse>.Ok("Card activated successfully.", response);
    }
 
 



    

    
}