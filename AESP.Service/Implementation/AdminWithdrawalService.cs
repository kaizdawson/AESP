using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class AdminWithdrawalService : IAdminWithdrawalService
    {
        private readonly IGenericRepository<Transaction> _transactionRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public AdminWithdrawalService(IGenericRepository<Transaction> transactionRepository, IGenericRepository<User> userRepository, IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<ResponseDTO> ApproveWithdrawalAsync(Guid transactionId)
        {
            var dto = new ResponseDTO();

            try
            {
                var transaction = await _transactionRepository.GetById(transactionId);

                if (transaction == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy giao dịch.";
                    return dto;
                }

                if (transaction.Type != "Withdrawal")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Giao dịch này không phải yêu cầu rút coin.";
                    return dto;
                }

                if (transaction.Status != "Pending")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_ERROR;
                    dto.Message = "Giao dịch đã được xử lý trước đó.";
                    return dto;
                }

                var user = await _userRepository.GetById(transaction.UserId);
                if (user == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy thông tin người dùng.";
                    return dto;
                }

                transaction.Status = "Approved";
                transaction.Description += $" | Admin duyệt lúc {DateTime.Now:dd/MM/yyyy HH:mm}";

                await _transactionRepository.Update(transaction);
                await _unitOfWork.SaveChangeAsync();

                // ==============================
                //  GỬI EMAIL CHO USER
                // ==============================
                if (!string.IsNullOrEmpty(user.Email))
                {
                    string subject = "AESP System - Yêu cầu rút coin của bạn đã được duyệt";
                    string body =
        $@"Xin chào {user.FullName},

Yêu cầu rút coin của bạn đã được quản trị viên phê duyệt.

🔹 Số coin: {transaction.AmountCoin}
🔹 Số tiền tương ứng: {transaction.AmountMoney:N0} VNĐ
🔹 Ngân hàng: {transaction.BankName}
🔹 Số tài khoản: {transaction.AccountNumber}
🔹 Mã giao dịch: {transaction.OrderCode}

Vui lòng kiểm tra tài khoản ngân hàng của bạn.

Trân trọng,
Đội ngũ Quản trị AESP.";

                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Duyệt yêu cầu rút coin thành công.";
                dto.Data = new
                {
                    transaction.TransactionId,
                    transaction.AmountCoin,
                    transaction.AmountMoney,
                    transaction.Status
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi duyệt yêu cầu rút coin: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetAllTransferTransactionsAsync(string? keyword, string? type, int pageNumber, int pageSize)
        {
            var dto = new ResponseDTO();
            var resultList = new List<object>();

            try
            {
                var db = _unitOfWork.GetDbContext();

                var query = db.Set<TransferTransaction>()
                    .Include(t => t.ReviewerProfile)
                        .ThenInclude(rp => rp.User)
                    .Include(t => t.LearnerProfile)
                        .ThenInclude(lp => lp.User)
                    .AsQueryable();

                // ===================== FILTER THEO LOẠI GIAO DỊCH =====================
                if (!string.IsNullOrWhiteSpace(type))
                {
                    query = query.Where(t => t.TransactionType == type.Trim());
                }

                // ===================== TÌM KIẾM THEO TÊN =====================
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    query = query.Where(t =>
                        (t.ReviewerProfile != null && t.ReviewerProfile.User.FullName != null && t.ReviewerProfile.User.FullName.ToLower().Contains(kw)) ||
                        (t.LearnerProfile != null && t.LearnerProfile.User.FullName != null && t.LearnerProfile.User.FullName.ToLower().Contains(kw)) ||
                        (t.Comment != null && t.Comment.ToLower().Contains(kw))
                    );
                }

                // ===================== SẮP XẾP + PHÂN TRANG =====================
                var totalItems = await query.CountAsync();

                var items = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        TransferId = t.TransferTransactionId,
                        CreatedAt = t.CreatedAt,

                        // LOẠI GIAO DỊCH
                        Type = t.TransactionType == "ReviewPayment" ? "Thanh toán review" :
                               t.TransactionType == "ReviewerTip" ? "Reviewer tip learner" : t.TransactionType,


                        AmountCoin = t.AmountCoin,
                       
                       
                        // REVIEWER: NGƯỜI NHẬN TIỀN (kiếm coin hoặc bị trừ khi tip)
                        ReviewerId = t.ReviewerProfileId,
                        ReviewerName = t.ReviewerProfile != null ? t.ReviewerProfile.User.FullName : "(Đã xóa)",
                        ReviewerEmail = t.ReviewerProfile != null ? t.ReviewerProfile.User.Email : null,

                        // LEARNER: NGƯỜI ĐƯỢC NHẬN TIP (chỉ có khi tip)
                        LearnerId = t.LearnerProfileId,
                        LearnerName = t.LearnerProfile != null ? t.LearnerProfile.User.FullName : null,
                        LearnerEmail = t.LearnerProfile != null ? t.LearnerProfile.User.Email : null,

                        // NẾU LÀ TIP → HIỂN THỊ RÕ NGƯỜI TẶNG CHO AI
                        FlowDescription = t.TransactionType == "ReviewPayment"
                     ? $"Hệ thống → {(t.ReviewerProfile != null && t.ReviewerProfile.User != null ? t.ReviewerProfile.User.FullName : "(Đã xóa)")} (+{t.AmountCoin} coin)"
                     : t.TransactionType == "ReviewerTip"
                     ? $"{(t.ReviewerProfile != null && t.ReviewerProfile.User != null ? t.ReviewerProfile.User.FullName : "(Đã xóa)")} → {(t.LearnerProfile != null && t.LearnerProfile.User != null ? t.LearnerProfile.User.FullName : "Người học")} (+{t.AmountCoin} coin)"
                     : t.Comment ?? "Chuyển coin nội bộ",

                        Comment = t.Comment,
                        Status = t.Status,
                        ReviewId = t.ReviewId
                    })
                    .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách chuyển coin nội bộ thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                    Items = items
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách chuyển coin: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetAllWithdrawalAsync(string? keyword, string? status, int pageNumber, int pageSize)
        {
            var dto = new ResponseDTO(); try
            {
                var db = _transactionRepository.GetDbContext();

                var query = db.Transactions
                    .Include(t => t.User)
                    .Where(t => t.Type == "Withdrawal")
                    .OrderByDescending(t => t.CreatedTransaction)
                    .AsQueryable();

                // =====================
                // 🔍 SEARCH
                // =====================
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();

                    query = query.Where(t =>
                        (t.User.FullName != null && t.User.FullName.ToLower().Contains(kw)) ||
                        (t.User.Email != null && t.User.Email.ToLower().Contains(kw)) ||
                        (t.OrderCode != null && t.OrderCode.ToLower().Contains(kw)) 
                    );
                }

                // =====================
                // 🔍 FILTER STATUS
                // =====================
                if (!string.IsNullOrEmpty(status))
                {
                    switch (status.ToLower())
                    {
                        case "pending":
                            query = query.Where(t => t.Status == "Pending");
                            break;

                        case "approved":
                            query = query.Where(t => t.Status == "Approved");
                            break;

                        case "rejected":
                            query = query.Where(t => t.Status == "Rejected");
                            break;

                        case "processing":
                            query = query.Where(t => t.Status == "Processing");
                            break;

                        case "all":
                        default:
                            break;
                    }
                }

                // =====================
                // 📌 PAGING
                // =====================
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        t.TransactionId,
                        t.UserId,
                        ReviewerName = t.User.FullName,
                        t.User.Email,
                        Coin = t.AmountCoin,
                        AmountMoney = t.AmountMoney,
                        t.BankName,
                        t.AccountNumber,
                        t.OrderCode,
                        Status = t.Status,
                        CreatedAt = t.CreatedTransaction
                    })
                    .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách yêu cầu rút coin thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách yêu cầu rút coin: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetPendingWithdrawalsAsync(int pageNumber, int pageSize)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _transactionRepository.GetDbContext();

                var query = db.Transactions
    .Include(t => t.User)
    .Where(t => t.Type == "Withdrawal" && t.Status == "Pending");


                var totalItems = await query.CountAsync();

                var items = await query
                    .OrderByDescending(t => t.CreatedTransaction)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách yêu cầu rút coin thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Items = items.Select(t => new
                    {
                        t.TransactionId,
                        t.UserId,
                        FullName = t.User?.FullName,
                        Email = t.User?.Email,
                        Coin = t.AmountCoin,
                        AmountMoney = t.AmountMoney,
                        t.BankName,
                        t.AccountNumber,
                        t.OrderCode,
                        Status = t.Status,
                        CreatedAt = t.CreatedTransaction
                    })
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách yêu cầu rút coin: " + ex.Message;
            }

            return dto;
        }
        //API: Lấy tổng quan 4 trạng thái (cho 4 ô trên UI)
        public async Task<ResponseDTO> GetWithdrawalSummaryAsync()
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _transactionRepository.GetDbContext();

                var pending = await db.Transactions
                    .CountAsync(t => t.Type == "Withdrawal" && t.Status == "Pending");

                var approved = await db.Transactions
                    .CountAsync(t => t.Type == "Withdrawal" && t.Status == "Approved");

                var rejected = await db.Transactions
                    .CountAsync(t => t.Type == "Withdrawal" && t.Status == "Rejected");

                var processing = await db.Transactions
                    .CountAsync(t => t.Type == "Withdrawal" && t.Status == "Processing");

                var total = pending + approved + rejected + processing;

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy thống kê thành công.";
                dto.Data = new
                {
                    Pending = pending,
                    Approved = approved,
                    Rejected = rejected,
                    Processing = processing,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy thống kê: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> RejectWithdrawalAsync(Guid transactionId, string reason)
        {
            var dto = new ResponseDTO();

            try
            {
                var transaction = await _transactionRepository.GetById(transactionId);

                if (transaction == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy giao dịch.";
                    return dto;
                }

                if (transaction.Type != "Withdrawal")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.INVALID_INPUT;
                    dto.Message = "Giao dịch này không phải yêu cầu rút coin.";
                    return dto;
                }

                if (transaction.Status != "Pending")
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_ERROR;
                    dto.Message = "Giao dịch đã được xử lý.";
                    return dto;
                }

                var user = await _userRepository.GetById(transaction.UserId);
                if (user == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy người dùng.";
                    return dto;
                }

                // trả coin về
                user.CoinBalance += Convert.ToInt32(transaction.AmountCoin);

                transaction.Status = "Rejected";
                transaction.ReasonReject = reason;
                transaction.Description +=
                    $" | Admin từ chối lúc {DateTime.Now:dd/MM/yyyy HH:mm}. Lý do: {reason}";

                await _userRepository.Update(user);
                await _transactionRepository.Update(transaction);
                await _unitOfWork.SaveChangeAsync();

                // ==============================
                //  GỬI EMAIL CHO USER
                // ==============================
                if (!string.IsNullOrEmpty(user.Email))
                {
                    string subject = "AESP System - Yêu cầu rút coin của bạn bị từ chối";
                    string body =
        $@"Xin chào {user.FullName},

Rất tiếc, yêu cầu rút coin của bạn đã bị quản trị viên từ chối.

🔹 Số coin yêu cầu rút: {transaction.AmountCoin}
🔹 Số tiền tương ứng: {transaction.AmountMoney:N0} VNĐ
🔹 Lý do từ chối: {reason}

Số coin đã được hoàn lại vào tài khoản của bạn.

Nếu bạn có thắc mắc, vui lòng phản hồi email này để được hỗ trợ.

Trân trọng,
Đội ngũ Quản trị AESP.";

                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                dto.Message = "Từ chối yêu cầu rút coin thành công.";
                dto.Data = new
                {
                    transaction.TransactionId,
                    transaction.AmountCoin,
                    transaction.AmountMoney,
                    Reason = reason,
                    transaction.Status
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi từ chối yêu cầu rút coin: " + ex.Message;
            }

            return dto;
        }
    }
}
