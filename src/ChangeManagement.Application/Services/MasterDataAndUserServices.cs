using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChangeManagement.Application.DTOs.Common;
using ChangeManagement.Application.DTOs.MasterData;
using ChangeManagement.Application.DTOs.Notifications;
using ChangeManagement.Application.DTOs.Users;
using ChangeManagement.Application.Interfaces;
using ChangeManagement.Domain.Entities;
using ChangeManagement.Domain.Enums;
using ChangeManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Application.Services;

public class MasterDataService : IMasterDataService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MasterDataService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IReadOnlyList<LookupDto>>> GetDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<Department>().Query()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ProjectTo<LookupDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<LookupDto>>.Ok(items);
    }

    public async Task<ApiResponse<IReadOnlyList<LookupDto>>> GetDivisionsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<Division>().Query()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ProjectTo<LookupDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<LookupDto>>.Ok(items);
    }

    public async Task<ApiResponse<IReadOnlyList<LookupDto>>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<Product>().Query()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ProjectTo<LookupDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<LookupDto>>.Ok(items);
    }

    public async Task<ApiResponse<IReadOnlyList<LookupDto>>> GetCustomersAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<Customer>().Query()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ProjectTo<LookupDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<LookupDto>>.Ok(items);
    }

    public async Task<ApiResponse<IReadOnlyList<RoleDto>>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<Role>().Query()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<RoleDto>>.Ok(items);
    }

    public async Task<ApiResponse<LookupDto>> CreateDepartmentAsync(CreateLookupRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Department { Code = request.Code, Name = request.Name };
        await _unitOfWork.Repository<Department>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<LookupDto>.Ok(_mapper.Map<LookupDto>(entity));
    }

    public async Task<ApiResponse<LookupDto>> CreateDivisionAsync(CreateLookupRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Division { Code = request.Code, Name = request.Name };
        await _unitOfWork.Repository<Division>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<LookupDto>.Ok(_mapper.Map<LookupDto>(entity));
    }

    public async Task<ApiResponse<LookupDto>> CreateProductAsync(CreateLookupRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Product { Code = request.Code, Name = request.Name };
        await _unitOfWork.Repository<Product>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<LookupDto>.Ok(_mapper.Map<LookupDto>(entity));
    }

    public async Task<ApiResponse<LookupDto>> CreateCustomerAsync(CreateLookupRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Customer { Code = request.Code, Name = request.Name };
        await _unitOfWork.Repository<Customer>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<LookupDto>.Ok(_mapper.Map<LookupDto>(entity));
    }
}

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(PagedQuery query, CancellationToken cancellationToken = default)
    {
        var q = _unitOfWork.Repository<User>().Query()
            .Include(u => u.Department)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(u => u.UserName.ToLower().Contains(s) || u.Email.ToLower().Contains(s) ||
                             u.FirstName.ToLower().Contains(s) || u.LastName.ToLower().Contains(s));
        }

        var total = await q.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var items = await q.OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        return ApiResponse<PagedResult<UserDto>>.Ok(new PagedResult<UserDto>
        {
            Items = _mapper.Map<List<UserDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>().Query()
            .Include(u => u.Department)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
        if (user is null)
            return ApiResponse<UserDto>.Fail("User not found.");
        return ApiResponse<UserDto>.Ok(_mapper.Map<UserDto>(user));
    }

    public async Task<ApiResponse<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>().Query()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
        if (user is null)
            return ApiResponse<UserDto>.Fail("User not found.");

        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.EmployeeId = request.EmployeeId;
        user.DepartmentId = request.DepartmentId;
        user.IsActive = request.IsActive;
        user.UpdatedDate = DateTime.UtcNow;

        user.UserRoles.Clear();
        var roles = await _unitOfWork.Repository<Role>().Query()
            .Where(r => request.Roles.Contains(r.Name))
            .ToListAsync(cancellationToken);
        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        }

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid id, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id, cancellationToken);
        if (user is null || user.IsDeleted)
            return ApiResponse<bool>.Fail("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return ApiResponse<bool>.Fail("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true, "Password changed.");
    }

    public async Task<ApiResponse<bool>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id, cancellationToken);
        if (user is null || user.IsDeleted)
            return ApiResponse<bool>.Fail("User not found.");
        user.IsActive = false;
        user.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true);
    }
}

public class NotificationAppService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService _emailService;

    public NotificationAppService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _emailService = emailService;
    }

    public async Task<ApiResponse<PagedResult<NotificationDto>>> GetMyNotificationsAsync(PagedQuery query, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
            return ApiResponse<PagedResult<NotificationDto>>.Fail("Unauthorized.");

        var q = _unitOfWork.Repository<Notification>().Query()
            .Include(n => n.EngineeringRequest)
            .Where(n => n.UserId == _currentUser.UserId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedDate);

        var total = await q.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return ApiResponse<PagedResult<NotificationDto>>.Ok(new PagedResult<NotificationDto>
        {
            Items = _mapper.Map<List<NotificationDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var n = await _unitOfWork.Repository<Notification>().GetByIdAsync(id, cancellationToken);
        if (n is null || n.UserId != _currentUser.UserId)
            return ApiResponse<bool>.Fail("Not found.");
        n.IsRead = true;
        _unitOfWork.Repository<Notification>().Update(n);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<bool>> MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<Notification>()
            .FindAsync(n => n.UserId == _currentUser.UserId && !n.IsRead && !n.IsDeleted, cancellationToken);
        foreach (var n in items)
        {
            n.IsRead = true;
            _unitOfWork.Repository<Notification>().Update(n);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true);
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null) return 0;
        return await _unitOfWork.Repository<Notification>()
            .CountAsync(n => n.UserId == _currentUser.UserId && !n.IsRead && !n.IsDeleted, cancellationToken);
    }

    public async Task NotifyAsync(Guid userId, NotificationType type, string subject, string message, Guid? erId = null, bool sendEmail = true, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            EngineeringRequestId = erId,
            Type = type,
            Subject = subject,
            Message = message
        };
        await _unitOfWork.Repository<Notification>().AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (sendEmail)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
            if (user is not null && !string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    await _emailService.SendAsync(user.Email, subject, message, cancellationToken);
                    notification.EmailSent = true;
                    notification.EmailSentDate = DateTime.UtcNow;
                    _unitOfWork.Repository<Notification>().Update(notification);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch
                {
                    // Email failures should not break workflow
                }
            }
        }
    }

    public async Task NotifyRoleAsync(string roleName, NotificationType type, string subject, string message, Guid? erId = null, bool sendEmail = true, CancellationToken cancellationToken = default)
    {
        var userIds = await _unitOfWork.Repository<User>().Query()
            .Where(u => u.IsActive && !u.IsDeleted && u.UserRoles.Any(ur => ur.Role.Name == roleName))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in userIds)
        {
            await NotifyAsync(userId, type, subject, message, erId, sendEmail, cancellationToken);
        }
    }
}
