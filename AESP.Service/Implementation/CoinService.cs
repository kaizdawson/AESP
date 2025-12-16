using AESP.API.Helpers;
using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Implementation;
using AESP.Repository.Models;
using AESP.Service.Contract;
using AESP.Service.Export;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using static AESP.Service.Export.TransactionReportDocument;

namespace AESP.Service.Implementation
{
    public class CoinService : ICoinService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<ServicePackage> _packageRepository;
        private readonly PayOSService _payOSService;
        private readonly ILogger<CoinService> _logger;
        private readonly IGenericRepository<Transaction> _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<AIConversationCharge> _aiConversationChargeRepository;
        private readonly IGenericRepository<Purchase> _purchaseRepository;


        public CoinService(
            IGenericRepository<User> userRepository,
            IGenericRepository<ServicePackage> packageRepository,
            IGenericRepository<Transaction> transactionRepository,
            PayOSService payOSService,
            ILogger<CoinService> logger,
            IUnitOfWork unitOfWork,
            IGenericRepository<AIConversationCharge> aiConversationChargeRepository,
    IGenericRepository<Purchase> purchaseRepository)
        {
            _userRepository = userRepository;
            _packageRepository = packageRepository;
            _transactionRepository = transactionRepository;
            _payOSService = payOSService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _aiConversationChargeRepository = aiConversationChargeRepository; 
            _purchaseRepository = purchaseRepository;
        }

        public async Task<decimal> GetUserCoinBalanceAsync(Guid userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null)
                throw new Exception("User not found.");
            return user.CoinBalance;
        }

        public async Task<object> AddCoinAsync(Guid servicePackageId, Guid userId)
        {
            var package = await _packageRepository.GetById(servicePackageId)
                ?? throw new Exception("Service package not found.");

            if (package.Status != "Active")
                throw new Exception("Gói dịch vụ hiện không khả dụng.");

            var user = await _userRepository.GetById(userId)?? throw new Exception("Không tìm thấy User này.");
            var orderCode = new Random().Next(100000, 999999);

            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                UserId = userId,
                UserName = user.FullName,
                ServicePackageName = package.Name,
                ServicePackageId = package.ServicePackageId,
                OrderCode = orderCode.ToString(),
                AmountMoney = package.Price,
                AmountCoin = package.NumberOfCoin,
                Status = "Pending",
                Type = "Deposit",
                Description = $"Nạp {package.NumberOfCoin} coin"
            };

            await _transactionRepository.Insert(transaction);
            await _unitOfWork.SaveChangeAsync();

            var description = orderCode.ToString();
            var (checkoutUrl, orderCodeStr, qrCode, qrBase64) = await _payOSService.CreatePaymentAsync(
                package.Price, userId.ToString(), package.NumberOfCoin, description, orderCode
            );

            _logger.LogInformation("💳 Đã tạo giao dịch Pending cho User {UserId}, OrderCode {OrderCode}", userId, orderCode);

            return new
            {
                checkoutUrl,
                orderCode = orderCodeStr,
                qrCode,
                qrBase64
            };
        }




        public async Task AddBalanceFromPayOSAsync(string orderCode, decimal amount, string payosOrderCode)
        {
            var transaction = _transactionRepository
                .AsQueryable()
                .FirstOrDefault(t => t.OrderCode == orderCode);

            if (transaction == null)
                throw new Exception($"Không tìm thấy giao dịch với OrderCode {orderCode}");

            if (transaction.Status == "Paid")
            {
                _logger.LogInformation("⚠️ Giao dịch {OrderCode} đã xử lý trước đó", orderCode);
                return;
            }

            var user = await _userRepository.GetById(transaction.UserId);
            if (user == null)
                throw new Exception($"Không tìm thấy user {transaction.UserId}");

            user.CoinBalance += (int)transaction.AmountCoin;

        
            transaction.Status = "Paid";
            transaction.Description += $" | Thanh toán thành công lúc {DateTime.Now:dd/MM/yyyy HH:mm}";
            transaction.OrderCode = orderCode;

            await _userRepository.Update(user);
            await _transactionRepository.Update(transaction);
            await _unitOfWork.SaveChangeAsync();

            _logger.LogInformation("✅ User {UserId} đã nạp thành công {Coin} coin (Order {OrderCode})", user.UserId, transaction.AmountCoin, orderCode);
        }


        public async Task CancelTransactionByOrderCodeAsync(string orderCode)
        {
            var transaction = _transactionRepository.AsQueryable()
                .FirstOrDefault(t => t.OrderCode == orderCode);

            if (transaction == null)
                throw new Exception("Không tìm thấy giao dịch.");

            if (transaction.Status == "Paid")
                throw new Exception("Giao dịch đã thanh toán, không thể hủy.");

            if (transaction.Status == "Cancelled")
                throw new Exception("Giao dịch đã bị hủy trước đó.");

            transaction.Status = "Cancelled";
            transaction.Description += $" | Hủy giao dịch lúc {DateTime.Now:dd/MM/yyyy HH:mm}";
            await _transactionRepository.Update(transaction);
            await _unitOfWork.SaveChangeAsync();

            _logger.LogInformation("❌ Giao dịch OrderCode={OrderCode} đã bị hủy.", orderCode);
        }


        public async Task<string> GetTransactionStatusAsync(string orderCode)
        {
            var transaction = _transactionRepository.AsQueryable()
                .FirstOrDefault(t => t.OrderCode == orderCode);

            if (transaction == null)
                throw new Exception("Không tìm thấy giao dịch.");

            return transaction.Status;
        }



        public async Task<int> PayCoinAsync(Guid userId, Guid aiChargeId)
        {
            var user = await _userRepository.GetById(userId)
                ?? throw new Exception("Không tìm thấy người dùng.");

            var charge = await _aiConversationChargeRepository.GetById(aiChargeId);

            if (charge == null)
                throw new Exception("Gói AI không tồn tại.");

            if (charge.Status != "Active")
                throw new Exception("Gói AI hiện không khả dụng.");

            var payCoin = charge.AmountCoin;

            if (user.CoinBalance < payCoin)
                return 0;

            user.CoinBalance -= payCoin;

            var purchase = new Purchase
            {
                UserId = userId,
                AmountCoin = payCoin,
                AIConversationChargeId = aiChargeId,
                Status = "Success",
                CreatedAt = DateTimeHelper.NowVN()
            };

            await _purchaseRepository.Insert(purchase);
            await _userRepository.Update(user);
            await _unitOfWork.SaveChangeAsync();

            return 1;
        }





        public async Task<object> WithdrawCoinAsync(Guid userId, int coin, string bankName, string accountNumber)
        {
            if (coin <= 0)
                throw new Exception("Số coin phải > 0.");

            var user = await _userRepository.GetById(userId)
                ?? throw new Exception("Không tìm thấy người dùng.");

            if (user.CoinBalance < coin)
                throw new Exception("Số dư coin không đủ để rút.");

            var amountMoney = coin * 1000;


            var orderCode = new Random().Next(100000, 999999).ToString();

            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                UserId = userId,
                Type = "Withdrawal",
                Status = "Pending",
                AmountCoin = coin,
                AmountMoney = amountMoney,
                CreatedTransaction = DateTime.Now,
                BankName = bankName,
                AccountNumber = accountNumber,
                Description = $"Rút {coin} coin tương ứng {amountMoney} vnđ",
                OrderCode = orderCode,
                UserName = user.FullName,
                ServicePackageName = null
            };

            user.CoinBalance -= coin;

            await _transactionRepository.Insert(transaction);
            await _userRepository.Update(user);
            await _unitOfWork.SaveChangeAsync();

            _logger.LogInformation("User {UserId} - {FullName} tạo yêu cầu rút {Coin} coin → {AmountMoney} VNĐ - Order: {OrderCode}",
         userId, user.FullName, coin, amountMoney, orderCode);

            return new
            {
                orderCode,
                status = "Pending",
                coin,
                amountMoney,
                message = "Yêu cầu rút coin đã được gửi. Vui lòng chờ admin duyệt."
            };
        }


        public async Task<IEnumerable<object>> GetDepositHistoryAsync(Guid userId)
        {
            var list = _transactionRepository.AsQueryable()
                .Where(t => t.UserId == userId && t.Type == "Deposit")
                .OrderByDescending(t => t.CreatedTransaction)
                .Select(t => new
                {
                    t.OrderCode,
                    t.AmountMoney,
                    t.AmountCoin,
                    t.Status,
                    t.Description,
                    CreatedAt = t.CreatedTransaction
                })
                .ToList();

            return list;
        }

        public async Task<IEnumerable<object>> GetWithdrawHistoryAsync(Guid userId)
        {
            var list = _transactionRepository.AsQueryable()
                .Where(t => t.UserId == userId && t.Type == "Withdrawal")
                .OrderByDescending(t => t.CreatedTransaction)
                .Select(t => new
                {
                    t.OrderCode,
                    t.AmountMoney,
                    t.AmountCoin,
                    t.Status,
                    t.BankName,
                    t.AccountNumber,
                    t.Description,
                    CreatedAt = t.CreatedTransaction
                })
                .ToList();

            return list;
        }


        public async Task<IEnumerable<object>> GetActiveAIConversationPackagesAsync()
        {
            var list = _aiConversationChargeRepository.AsQueryable()
                .Where(x => x.Status == "Active" && x.IsDeleted == false)
                .OrderBy(x => x.AmountCoin)
                .Select(x => new
                {
                    x.AIConversationChargeId,
                    x.AmountCoin,
                    x.AllowedMinutes
                })
                .ToList();

            return list;
        }

        public async Task<ResponseDTO> UpdateWithdrawalAsync(Guid transactionId, Guid userId, int newAmountMoney, string bankName, string accountNumber)
        {
            var dto = new ResponseDTO();

            try
            {
                if (newAmountMoney <= 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Số tiền rút phải lớn hơn 0.";
                    return dto;
                }

                var db = _transactionRepository.GetDbContext();

                var transaction = await db.Transactions
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

                if (transaction == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy giao dịch.";
                    return dto;
                }

                // 1️⃣ Kiểm tra quyền sửa
                if (transaction.UserId != userId)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.ACCESS_DENIED;
                    dto.Message = "Bạn không có quyền sửa giao dịch này.";
                    return dto;
                }

                // 2️⃣ Chỉ được sửa khi Pending
                if (transaction.Status != "Pending")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_ACTION;
                    dto.Message = "Chỉ có thể sửa giao dịch đang ở trạng thái Pending.";
                    return dto;
                }

                var user = transaction.User;

                // 3️⃣ Tính số coin mới (1 coin = 1000 VNĐ)
                decimal newCoin = (decimal)newAmountMoney / 1000m;

                if (newCoin <= 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Số tiền quá nhỏ.";
                    return dto;
                }

                // 4️⃣ Tính coin chênh lệch
                decimal oldCoin = transaction.AmountCoin;
                decimal diffCoin = newCoin - oldCoin;

                if (diffCoin > 0)
                {
                    // RÚT NHIỀU HƠN → TRỪ COIN
                    if (user.CoinBalance < diffCoin)
                    {
                        dto.IsSucess = false;
                        dto.BusinessCode = BusinessCode.INVALID_ACTION;
                        dto.Message = "Số dư coin không đủ để tăng số tiền rút.";
                        return dto;
                    }

                    user.CoinBalance -= (int)diffCoin;
                }
                else if (diffCoin < 0)
                {
                    // RÚT ÍT LẠI → HOÀN COIN
                    user.CoinBalance += (int)Math.Abs(diffCoin);
                }

                // 5️⃣ Cập nhật transaction
                transaction.AmountCoin = newCoin;
                transaction.AmountMoney = newAmountMoney;
                transaction.BankName = bankName;
                transaction.AccountNumber = accountNumber;
                transaction.Description =
                    $"Cập nhật yêu cầu rút: {newCoin} coin tương ứng {newAmountMoney:N0} VNĐ | Lúc {DateTime.Now:dd/MM/yyyy HH:mm}";

                db.Users.Update(user);
                db.Transactions.Update(transaction);

                await _unitOfWork.SaveChangeAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Cập nhật yêu cầu rút coin thành công.";
                dto.Data = new
                {
                    transaction.TransactionId,
                    transaction.AmountCoin,
                    transaction.AmountMoney,
                    transaction.BankName,
                    transaction.AccountNumber,
                    transaction.Status
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = ex.Message;
            }

            return dto;
        }



        public async Task<ResponseDTO> GetAllTransactionsAsync(int pageNumber = 1, int pageSize = 10, string? status = null, string? type = null, string? search = null)
        {
            var dto = new ResponseDTO();

            try
            {
                // Validate phân trang
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var db = _transactionRepository.GetDbContext();

                var query = db.Transactions
            .Include(t => t.User) // BẮT BUỘC Include để lấy được User.FullName
            .AsQueryable();

                // === FILTER ===

                if (!string.IsNullOrWhiteSpace(type))
                {
                    type = type.Trim();
                    query = query.Where(t => t.Type == type);
                }
                if (!string.IsNullOrWhiteSpace(status))
                {
                    status = status.Trim();

                    query = query.Where(t =>
                        // NẠP
                        (t.Type == "Deposit" &&
                            (status == "Pending" || status == "Paid" || status == "Cancelled") &&
                            t.Status == status)

                        ||

                        // RÚT
                        (t.Type == "Withdrawal" &&
                            (status == "Pending" || status == "Approved" || status == "Rejected") &&
                            t.Status == status)
                    );
                }
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    query = query.Where(t =>
                        t.UserName.ToLower().Contains(keyword) ||
                        t.OrderCode.ToLower().Contains(keyword));
                }
                // === ĐẾM TỔNG SỐ BẢN GHI SAU KHI FILTER ===
                var totalItems = await query.CountAsync();

                var items = await query
             .OrderByDescending(t => t.CreatedTransaction)
             .Skip((pageNumber - 1) * pageSize)
             .Take(pageSize)
             .Select(t => new
             {
                 t.TransactionId,
                 t.UserId,
                 UserName = t.User != null ? t.User.FullName : "(Đã xóa tài khoản)",
                 t.ServicePackageId,
                 t.ServicePackageName,
                 t.AmountMoney,
                 t.AmountCoin,
                 t.OrderCode,
                 t.BankName,
                 t.AccountNumber,
                 t.Description,
                 Type = t.Type,
                 Status = t.Status,
                 t.ReasonReject,
                 CreatedTransaction = t.CreatedTransaction
             })
             .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách giao dịch thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                    Items = items
                };

                return dto;
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucess = false,
                    BusinessCode = BusinessCode.EXCEPTION,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
        public async Task<byte[]> ExportTransactionPdfAsync()
        {
            var db = _transactionRepository.GetDbContext();

            var list = await db.Transactions
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedTransaction)
                .Select(t => new TransactionReportItem
                {
                    TransactionId = t.TransactionId.ToString(),
                    UserName = t.User != null ? t.User.FullName : "(đã xóa)",
                    Type = t.Type,
                    AmountMoney = t.AmountMoney,
                    AmountCoin = t.AmountCoin,
                    Status = t.Status,
                    OrderCode = t.OrderCode,
                    CreatedAt = t.CreatedTransaction
                })
                .ToListAsync();

            var doc = new TransactionReportDocument
            {
                Items = list,
                GeneratedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            };

            return doc.GeneratePdf();
        }

        public async Task<ResponseDTO> GetTransactionDashboardAsync()
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _transactionRepository.GetDbContext();

                // ✅ Thành công:
                // - Deposit: Paid
                // - Withdrawal: Approved
                var totalPaid = await db.Transactions
                    .CountAsync(t => t.Type == "Deposit" && t.Status == "Paid");

                var totalApproved = await db.Transactions
                    .CountAsync(t => t.Type == "Withdrawal" && t.Status == "Approved");

                // ❌ Thất bại:
                // - Deposit: Cancelled
                // - Withdrawal: Rejected
                var totalFailTransaction = await db.Transactions.CountAsync(t =>
                    (t.Type == "Deposit" && t.Status == "Cancelled") ||
                    (t.Type == "Withdrawal" && t.Status == "Rejected")
                );

                // ⏳ Đang xử lý: Pending
                var totalPendingTransaction = await db.Transactions.CountAsync(t =>
                    t.Status == "Pending"
                );
                var totalDepositAmount = await db.Transactions
                    .Where(t => t.Type == "Deposit" && t.Status == "Paid")
                    .SumAsync(t => (decimal?)t.AmountMoney) ?? 0;

                // ✅ TỔNG TIỀN RÚT RA KHỎI HỆ THỐNG
                var totalWithdrawalAmount = await db.Transactions
                    .Where(t => t.Type == "Withdrawal" && t.Status == "Approved")
                    .SumAsync(t => (decimal?)t.AmountMoney) ?? 0;

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy dashboard giao dịch thành công.";
                dto.Data = new
                {
                    totalPaid = totalPaid,
                    totalApproved = totalApproved,
                    totalFailTransaction = totalFailTransaction,
                    totalPendingTransaction = totalPendingTransaction,
                    totalDepositAmount,
                    totalWithdrawalAmount
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = ex.Message;
            }

            return dto;
        }
    }
}
