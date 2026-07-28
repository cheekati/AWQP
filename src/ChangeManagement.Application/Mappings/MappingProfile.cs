using AutoMapper;
using ChangeManagement.Application.DTOs.Auth;
using ChangeManagement.Application.DTOs.EngineeringRequests;
using ChangeManagement.Application.DTOs.MasterData;
using ChangeManagement.Application.DTOs.Notifications;
using ChangeManagement.Application.DTOs.Users;
using ChangeManagement.Domain.Entities;
using ChangeManagement.Domain.Enums;

namespace ChangeManagement.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Department, LookupDto>();
        CreateMap<Division, LookupDto>();
        CreateMap<Product, LookupDto>();
        CreateMap<Customer, LookupDto>();
        CreateMap<Role, RoleDto>();

        CreateMap<User, UserInfoDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department != null ? s.Department.Name : null))
            .ForMember(d => d.Roles, o => o.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name).ToList()));

        CreateMap<User, UserDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department != null ? s.Department.Name : null))
            .ForMember(d => d.Roles, o => o.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name).ToList()));

        CreateMap<EngineeringRequest, EngineeringRequestListDto>()
            .ForMember(d => d.DivisionName, o => o.MapFrom(s => s.Division.Name))
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department.Name))
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.RequesterName, o => o.MapFrom(s => s.Requester.FullName))
            .ForMember(d => d.StatusName, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<EngineeringRequest, EngineeringRequestDetailDto>()
            .IncludeBase<EngineeringRequest, EngineeringRequestListDto>()
            .ForMember(d => d.Reasons, o => o.MapFrom(s => s.Reasons.Select(r => r.Reason).ToList()))
            .ForMember(d => d.OtherReasonDescription, o => o.MapFrom(s =>
                s.Reasons.FirstOrDefault(r => r.Reason == ReasonForChange.Others)!.OtherDescription));

        CreateMap<EngineeringRequestAttachment, AttachmentDto>()
            .ForMember(d => d.DownloadUrl, o => o.MapFrom(s => $"/api/attachments/{s.EngineeringRequestId}/{s.Id}/download"));

        CreateMap<VerificationHistory, VerificationHistoryDto>()
            .ForMember(d => d.StageName, o => o.MapFrom(s => s.Stage.ToString()))
            .ForMember(d => d.ActionName, o => o.MapFrom(s => s.Action.ToString()))
            .ForMember(d => d.VerifierName, o => o.MapFrom(s => s.Verifier.FullName));

        CreateMap<ApprovalHistory, ApprovalHistoryDto>()
            .ForMember(d => d.ActionName, o => o.MapFrom(s => s.Action.ToString()))
            .ForMember(d => d.ApproverName, o => o.MapFrom(s => s.Approver.FullName));

        CreateMap<Comment, CommentDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName));

        CreateMap<VerificationAssignment, VerificationAssignmentDto>()
            .ForMember(d => d.StageName, o => o.MapFrom(s => s.Stage.ToString()))
            .ForMember(d => d.AssignedToUserName, o => o.MapFrom(s => s.AssignedToUser != null ? s.AssignedToUser.FullName : null));

        CreateMap<Notification, NotificationDto>()
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.ErNumber, o => o.MapFrom(s => s.EngineeringRequest != null ? s.EngineeringRequest.ErNumber : null));
    }
}
