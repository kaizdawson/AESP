using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using AESP.Service.Export;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class AdminPurchaseService : IAdminPurchaseService
    {
        private readonly IGenericRepository<Purchase> _purchaseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdminPurchaseService(IGenericRepository<Purchase> purchaseRepository, IUnitOfWork unitOfWork)
        {
            _purchaseRepository = purchaseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetAllAsync(int pageNumber, int pageSize, string? keyword, string? type)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _purchaseRepository.GetDbContext();

                var query = db.Purchases
                    .Include(x => x.User)
                    .Include(x => x.Course).ThenInclude(c => c.Chapters)
                    .Include(x => x.ReviewFee)
                    .Include(x => x.AIConversationCharge)
                    .AsQueryable();

                // ===========================
                // 🔍 SEARCH
                // ===========================
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    string key = keyword.Trim().ToLower();

                    query = query.Where(p =>
                        p.PurchaseId.ToString().ToLower().Contains(key) ||
                        p.UserId.ToString().ToLower().Contains(key) ||
                        p.User.Email.ToLower().Contains(key) ||
                        p.User.FullName.ToLower().Contains(key)
                    );
                }

                // ===========================
                // 🔎 FILTER TYPE
                // ===========================
                if (!string.IsNullOrWhiteSpace(type))
                {
                    switch (type.ToLower())
                    {
                        case "course":
                            query = query.Where(p => p.CourseId != null);
                            break;

                        case "reviewfee":
                            query = query.Where(p => p.ReviewFeeId != null);
                            break;

                        case "aiconversation":
                            query = query.Where(p => p.AIConversationChargeId != null);
                            break;
                    }
                }

                // ===========================
                // 📌 PAGINATION
                // ===========================
                int totalItems = await query.CountAsync();

                var items = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // ===========================
                // 🧩 MAP RESPONSE (ONLY COIN)
                // ===========================
                var mapped = items.Select(p =>
                {
                    string itemType = p.CourseId != null
                        ? "Course"
                        : p.ReviewFeeId != null
                            ? "Review Fee"
                            : "AI Conversation";

                    string itemName = "";

                    // COURSE
                    if (p.CourseId != null)
                    {
                        itemName =
                            $"{p.Course.Title} | Level: {p.Course.Level} | Chapters: {p.Course.Chapters.Count}";
                    }
                    // REVIEW FEE
                    else if (p.ReviewFeeId != null)
                    {
                        itemName = $"Review Fee x{p.ReviewFee.NumberOfReview}";
                    }
                    // AI CONVERSATION
                    else if (p.AIConversationChargeId != null)
                    {
                        itemName = $"AI Conversation | {p.AIConversationCharge.AllowedMinutes} minutes";
                    }

                    return new
                    {
                        p.PurchaseId,
                        p.UserId,
                        UserName = p.User.FullName,
                        p.Status,
                        Coin = p.AmountCoin,    //  ONLY COIN
                        p.CreatedAt,
                        ItemType = itemType,
                        ItemName = itemName
                    };
                }).ToList();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách giao dịch thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Items = mapped
                };

                return dto;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách giao dịch: " + ex.Message;
                return dto;
            }
        }

        public async Task<ResponseDTO> GetDetailAsync(Guid purchaseId)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _purchaseRepository.GetDbContext();

                var p = await db.Purchases
                    .Include(x => x.User)
                    .Include(x => x.Course)
                    .Include(x => x.ReviewFee)
                        .ThenInclude(r => r.ReviewFeeDetails)
                    .Include(x => x.AIConversationCharge)
                    .FirstOrDefaultAsync(x => x.PurchaseId == purchaseId);

                if (p == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy giao dịch.";
                    return dto;
                }

                // ============================
                // Xác định loại Item
                // ============================
                string itemType = p.CourseId != null
                    ? "Course"
                    : p.ReviewFeeId != null
                        ? "Review Fee"
                        : "AI Conversation";

                object? itemDetail = null;

                // ============================
                // 🟦 Nếu là Course
                // ============================
                if (p.CourseId != null && p.Course != null)
                {
                    itemDetail = new
                    {
                        p.Course.CourseId,
                        p.Course.Title,
                        p.Course.Level,
                        NumberOfChapter = p.Course.NumberOfChapter,
                        p.Course.OrderIndex,
                        p.Course.Price
                    };
                }

                // ============================
                // 🟩 Nếu là ReviewFee
                // ============================
                if (p.ReviewFeeId != null && p.ReviewFee != null)
                {
                    var latest = p.ReviewFee.ReviewFeeDetails
                        .OrderByDescending(x => x.AppliedDate)
                        .FirstOrDefault();

                    itemDetail = new
                    {
                        p.ReviewFee.ReviewFeeId,
                        p.ReviewFee.NumberOfReview,
                        Price = latest?.PricePerReviewFee ?? 0,
                        PercentOfSystem = latest?.PercentOfSystem ?? 0,
                        PercentOfReviewer = latest?.PercentOfReviewer ?? 0
                    };
                }

                // ============================
                // 🟧 Nếu là AIConversation
                // ============================
                if (p.AIConversationChargeId != null && p.AIConversationCharge != null)
                {
                    itemDetail = new
                    {
                        p.AIConversationCharge.AIConversationChargeId,
                        p.AIConversationCharge.AmountCoin,
                        p.AIConversationCharge.AllowedMinutes,
                        p.AIConversationCharge.Status
                    };
                }

                // ============================
                // 🧩 Response
                // ============================
                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết giao dịch thành công.";

                dto.Data = new
                {
                    Info = new
                    {
                        p.PurchaseId,
                        p.UserId,
                        UserName = p.User.FullName,
                        p.AmountCoin,
                        p.Status,
                        p.CreatedAt,
                        ItemType = itemType
                    },
                    ItemDetail = itemDetail
                };

                return dto;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy chi tiết giao dịch: " + ex.Message;
                return dto;
            }
            

        }
        public async Task<byte[]> ExportPdfAsync()
        {
            var db = _purchaseRepository.GetDbContext();

            var purchases = await db.Purchases
                .Include(p => p.User)
                .Include(p => p.Course)
                .Include(p => p.ReviewFee)
                .Include(p => p.AIConversationCharge)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var list = purchases.Select(p => new PurchaseReportItem
            {
                PurchaseId = p.PurchaseId.ToString(),
                UserName = p.User.FullName,
                ItemType = p.CourseId != null ? "Course"
                         : p.ReviewFeeId != null ? "Review Fee"
                         : "AI Conversation",
                ItemName = p.CourseId != null ? p.Course.Title
                         : p.ReviewFeeId != null ? $"Review Fee - {p.ReviewFee.NumberOfReview} reviews"
                         : $"{p.AIConversationCharge.AllowedMinutes} minutes",
                AmountCoin = p.AmountCoin,
                CreatedAt = p.CreatedAt
            }).ToList();

            var pdf = new PurchaseReportDocument
            {
                Items = list,
                GeneratedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            };

            return pdf.GeneratePdf();
        }

        public async Task<ResponseDTO> GetDashboardAsync()
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _purchaseRepository.GetDbContext();

                // ✅ CHỈ TÍNH NHỮNG GIAO DỊCH THÀNH CÔNG
                var successQuery = db.Purchases
                    .Where(p => p.Status == "Success");

                var totalSuccessTransaction = await successQuery.CountAsync();

                var totalRevenueCoin = await successQuery
                    .SumAsync(p => (int?)p.AmountCoin) ?? 0;

                //  QUY ĐỔI: 1 coin = 1000 VND
                var totalRevenue = totalRevenueCoin * 1000;

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy dashboard purchase thành công.";
                dto.Data = new
                {
                    TotalSuccessTransaction = totalSuccessTransaction,
                    TotalRevenue = totalRevenue
                };

                return dto;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy dashboard purchase: " + ex.Message;
                return dto;
            }
        }

        public async Task<ResponseDTO> GetReviewFeeBuyerStatisticsAsync(int pageNumber, int pageSize)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _purchaseRepository.GetDbContext();

                var query = db.Purchases
                    .Include(p => p.User)
                    .Where(p => p.Status == "Success" && p.ReviewFeeId != null);

                var totalBuyer = await query
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync();

                var buyers = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                    {
                         p.UserId,
                         p.User.FullName,
                         p.User.Email,
                         p.CreatedAt,
                         p.AmountCoin
                    })
                     .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalBuyer = totalBuyer,
                    Buyers = buyers
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

        public async Task<ResponseDTO> GetAIConversationBuyerStatisticsAsync(int pageNumber, int pageSize)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _purchaseRepository.GetDbContext();

                var query = db.Purchases
                    .Include(p => p.User)
                    .Where(p => p.Status == "Success" && p.AIConversationChargeId != null);

                var totalBuyer = await query
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync();

                var buyers = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.UserId,
                p.User.FullName,
                p.User.Email,
                p.CreatedAt,
                p.AmountCoin
            })
            .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalBuyer = totalBuyer,
                    Buyers = buyers
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

        public async Task<ResponseDTO> GetEnrolledCourseStatisticsAsync(int pageNumber, int pageSize)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _purchaseRepository.GetDbContext();

                var query = db.LearnerCourses
                    .Include(lc => lc.LearnerProfile)
                        .ThenInclude(lp => lp.User);

                var totalLearner = await query
                    .Select(lc => lc.LearnerProfileId)
                    .Distinct()
                    .CountAsync();

                var learners = await query
                    .OrderByDescending(lc => lc.GeneratedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                 .Select(lc => new
                  {
                     lc.LearnerProfileId,
                     lc.LearnerProfile.User.FullName,
                     lc.LearnerProfile.User.Email,
                     lc.GeneratedDate,
                     lc.NumberOfCourse
                  })
            .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalEnrolledLearner = totalLearner,
                    Learners = learners
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
