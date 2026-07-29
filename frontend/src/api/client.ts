import axios from 'axios';
import type { Attachment, ErDetail, ErFormData, ErListItem, LoginResponse, User } from '../types';

const api = axios.create({
  baseURL: '/api',
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('cms_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

function toPayload(form: ErFormData) {
  return {
    division: form.division,
    title: form.title,
    department: form.department,
    product: form.product,
    customer: form.customer,
    process: form.process,
    reasonCostDown: form.reasonCostDown,
    reasonAlternativeSourcing: form.reasonAlternativeSourcing,
    reasonOthers: form.reasonOthers,
    reasonOthersText: form.reasonOthersText || null,
    presentDetails: form.presentDetails || null,
    newDetails: form.newDetails || null,
    merit: form.merit || null,
    demerit: form.demerit || null,
    materialDisposition: form.materialDisposition || null,
    sampleQuantity: form.sampleQuantity === '' ? null : Number(form.sampleQuantity),
    testLotType: form.testLotType === '' ? null : form.testLotType,
    testLotDescription: form.testLotDescription || null,
    testLotCodeSerial: form.testLotCodeSerial || null,
    applicableToChemicalOrMaterials: form.applicableToChemicalOrMaterials,
    safetyDataSheet: form.safetyDataSheet || null,
    chemicalLabel: form.chemicalLabel || null,
    chemicalClassification: form.chemicalClassification || null,
    chemicalInventorySystem: form.chemicalInventorySystem || null,
    verifiedBySafetyUserId: form.verifiedBySafetyUserId === '' ? null : form.verifiedBySafetyUserId,
    checkedByDeptHeadUserId: form.checkedByDeptHeadUserId === '' ? null : form.checkedByDeptHeadUserId,
    checkedByQaUserId: form.checkedByQaUserId === '' ? null : form.checkedByQaUserId,
    submit: form.submit,
  };
}

export const authApi = {
  login: (email: string, password: string) =>
    api.post<LoginResponse>('/auth/login', { email, password }).then((r) => r.data),
  me: () => api.get<User>('/auth/me').then((r) => r.data),
};

export const usersApi = {
  list: (role?: string) =>
    api.get<User[]>('/users', { params: role ? { role } : undefined }).then((r) => r.data),
};

export const lookupsApi = {
  all: () =>
    api.get<{ category: string; values: string[] }[]>('/lookups').then((r) => r.data),
};

export const erApi = {
  list: () => api.get<ErListItem[]>('/engineering-requests').then((r) => r.data),
  verificationQueue: () =>
    api.get<ErListItem[]>('/engineering-requests/verification-queue').then((r) => r.data),
  cooQueue: () =>
    api.get<ErListItem[]>('/engineering-requests/coo-queue').then((r) => r.data),
  get: (id: number) =>
    api.get<ErDetail>(`/engineering-requests/${id}`).then((r) => r.data),
  create: (form: ErFormData) =>
    api.post<ErDetail>('/engineering-requests', toPayload(form)).then((r) => r.data),
  update: (id: number, form: ErFormData) =>
    api.put<ErDetail>(`/engineering-requests/${id}`, toPayload(form)).then((r) => r.data),
  verify: (id: number, decision: number, comment?: string) =>
    api.post<ErDetail>(`/engineering-requests/${id}/verify`, { decision, comment }).then((r) => r.data),
  coo: (id: number, decision: number, comment?: string, outcome?: number) =>
    api.post<ErDetail>(`/engineering-requests/${id}/coo`, { decision, comment, outcome }).then((r) => r.data),
  upload: async (id: number, file: File, onProgress?: (pct: number) => void) => {
    const data = new FormData();
    data.append('file', file);
    const res = await api.post<Attachment>(`/engineering-requests/${id}/attachments`, data, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (e) => {
        if (e.total && onProgress) onProgress(Math.round((e.loaded / e.total) * 100));
      },
    });
    return res.data;
  },
  deleteAttachment: (id: number, attachmentId: number) =>
    api.delete(`/engineering-requests/${id}/attachments/${attachmentId}`),
};

export default api;
