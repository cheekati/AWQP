import api from './client';
import type {
  ApiResponse,
  DashboardStats,
  EngineeringRequestDetail,
  EngineeringRequestListItem,
  ErFormData,
  LoginResponse,
  LookupItem,
  NotificationItem,
  PagedResult,
  UserInfo,
  VerificationAction,
} from '../types';

export const authApi = {
  login: (userName: string, password: string) =>
    api.post<ApiResponse<LoginResponse>>('/auth/login', { userName, password }),
  me: () => api.get<ApiResponse<UserInfo>>('/auth/me'),
};

export const masterDataApi = {
  departments: () => api.get<ApiResponse<LookupItem[]>>('/masterdata/departments'),
  divisions: () => api.get<ApiResponse<LookupItem[]>>('/masterdata/divisions'),
  products: () => api.get<ApiResponse<LookupItem[]>>('/masterdata/products'),
  customers: () => api.get<ApiResponse<LookupItem[]>>('/masterdata/customers'),
  roles: () => api.get<ApiResponse<{ id: string; name: string; description?: string }[]>>('/masterdata/roles'),
};

export const erApi = {
  mine: (params?: Record<string, unknown>) =>
    api.get<ApiResponse<PagedResult<EngineeringRequestListItem>>>('/engineering-requests/mine', { params }),
  all: (params?: Record<string, unknown>) =>
    api.get<ApiResponse<PagedResult<EngineeringRequestListItem>>>('/engineering-requests', { params }),
  get: (id: string) => api.get<ApiResponse<EngineeringRequestDetail>>(`/engineering-requests/${id}`),
  create: (data: ErFormData) => api.post<ApiResponse<EngineeringRequestDetail>>('/engineering-requests', toPayload(data)),
  update: (id: string, data: ErFormData) =>
    api.put<ApiResponse<EngineeringRequestDetail>>(`/engineering-requests/${id}`, toPayload(data)),
  submit: (id: string) => api.post<ApiResponse<EngineeringRequestDetail>>(`/engineering-requests/${id}/submit`),
  remove: (id: string) => api.delete<ApiResponse<boolean>>(`/engineering-requests/${id}`),
};

export const attachmentApi = {
  upload: (erId: string, files: FileList | File[]) => {
    const form = new FormData();
    Array.from(files).forEach((f) => form.append('files', f));
    return api.post<ApiResponse<import('../types').Attachment[]>>(`/attachments/${erId}`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  remove: (erId: string, attachmentId: string) =>
    api.delete<ApiResponse<boolean>>(`/attachments/${erId}/${attachmentId}`),
  downloadUrl: (erId: string, attachmentId: string) =>
    `${import.meta.env.VITE_API_URL || 'http://localhost:5080/api'}/attachments/${erId}/${attachmentId}/download`,
};

export const verificationApi = {
  safetyQueue: (params?: Record<string, unknown>) =>
    api.get<ApiResponse<PagedResult<EngineeringRequestListItem>>>('/verification/safety', { params }),
  deptHeadQueue: (params?: Record<string, unknown>) =>
    api.get<ApiResponse<PagedResult<EngineeringRequestListItem>>>('/verification/department-head', { params }),
  qaQueue: (params?: Record<string, unknown>) =>
    api.get<ApiResponse<PagedResult<EngineeringRequestListItem>>>('/verification/qa', { params }),
  act: (erId: string, stage: 'safety' | 'department-head' | 'qa', action: VerificationAction, comments?: string) =>
    api.post<ApiResponse<EngineeringRequestDetail>>(`/verification/${erId}/${stage}`, { action, comments }),
  comment: (erId: string, content: string) =>
    api.post(`/verification/${erId}/comments`, { content }),
};

export const cooApi = {
  pending: (params?: Record<string, unknown>) =>
    api.get<ApiResponse<PagedResult<EngineeringRequestListItem>>>('/coo/pending', { params }),
  act: (erId: string, action: VerificationAction, comments?: string) =>
    api.post<ApiResponse<EngineeringRequestDetail>>(`/coo/${erId}/action`, { action, comments }),
};

export const notificationApi = {
  list: (params?: Record<string, unknown>) =>
    api.get<ApiResponse<PagedResult<NotificationItem>>>('/notifications', { params }),
  unreadCount: () => api.get<{ success: boolean; data: number }>('/notifications/unread-count'),
  markRead: (id: string) => api.post(`/notifications/${id}/read`),
  markAllRead: () => api.post('/notifications/read-all'),
};

export const dashboardApi = {
  stats: () => api.get<ApiResponse<DashboardStats>>('/dashboard/stats'),
};

function toPayload(data: ErFormData) {
  return {
    ...data,
    sampleQuantity: data.sampleQuantity === '' ? null : Number(data.sampleQuantity),
    testLotIdentification: data.testLotIdentification || null,
  };
}
