export type ErStatus =
  | 'Draft'
  | 'Submitted'
  | 'InProgress'
  | 'Approved'
  | 'Rejected'
  | 'Resubmitted'
  | 'Closed'
  | 'AwaitingCooApproval';

export type ReasonForChange = 'CostDown' | 'AlternativeSourcing' | 'Others';
export type TestLotIdentification = 'RunAsNormal' | 'SpecialProcess' | 'Others';
export type VerificationAction = 'Approved' | 'Rejected' | 'Resubmit' | 'Comment';

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UserInfo {
  id: string;
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  departmentId?: string;
  departmentName?: string;
  roles: string[];
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: UserInfo;
}

export interface LookupItem {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
}

export interface Attachment {
  id: string;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
  isImage: boolean;
  downloadUrl: string;
  createdDate: string;
}

export interface EngineeringRequestListItem {
  id: string;
  erNumber: string;
  title: string;
  divisionName: string;
  departmentName: string;
  productName: string;
  customer: string;
  requesterName: string;
  status: ErStatus;
  statusName: string;
  submissionCount: number;
  validationDate: string;
  expiryDate: string;
  createdDate: string;
  submittedDate?: string;
}

export interface EngineeringRequestDetail extends EngineeringRequestListItem {
  divisionId: string;
  isPr: boolean;
  isEng: boolean;
  isQa: boolean;
  isInd: boolean;
  departmentId: string;
  productId: string;
  requesterId: string;
  process: string;
  detailsOfEvaluation?: string;
  reasons: ReasonForChange[];
  otherReasonDescription?: string;
  presentCondition?: string;
  newCondition?: string;
  merit?: string;
  demerit?: string;
  materialDisposition?: string;
  sampleQuantity?: number;
  testLotIdentification?: TestLotIdentification;
  testLotDescription?: string;
  applicableToChemicalOrMaterials: boolean;
  safetyDataSheet?: string;
  chemicalLabel?: string;
  chemicalClassification?: string;
  chemicalInventoryManagementSystem?: string;
  latestRejectionComments?: string;
  attachments: Attachment[];
  verificationHistories: Array<{
    id: string;
    stage: string;
    stageName: string;
    action: string;
    actionName: string;
    verifierName: string;
    comments?: string;
    actionDate: string;
  }>;
  approvalHistories: Array<{
    id: string;
    action: string;
    actionName: string;
    approverName: string;
    comments?: string;
    actionDate: string;
  }>;
  comments: Array<{
    id: string;
    content: string;
    userName: string;
    stage?: string;
    createdDate: string;
  }>;
  assignments: Array<{
    id: string;
    stage: string;
    stageName: string;
    assignedToUserName?: string;
    isCompleted: boolean;
    result?: string;
    completedDate?: string;
  }>;
}

export interface ErFormData {
  divisionId: string;
  isPr: boolean;
  isEng: boolean;
  isQa: boolean;
  isInd: boolean;
  title: string;
  departmentId: string;
  productId: string;
  customer: string;
  process: string;
  detailsOfEvaluation: string;
  reasons: ReasonForChange[];
  otherReasonDescription: string;
  presentCondition: string;
  newCondition: string;
  merit: string;
  demerit: string;
  materialDisposition: string;
  sampleQuantity: string;
  testLotIdentification: TestLotIdentification | '';
  testLotDescription: string;
  applicableToChemicalOrMaterials: boolean;
  safetyDataSheet: string;
  chemicalLabel: string;
  chemicalClassification: string;
  chemicalInventoryManagementSystem: string;
  submit: boolean;
}

export interface DashboardStats {
  totalRequests: number;
  draftCount: number;
  inProgressCount: number;
  approvedCount: number;
  rejectedCount: number;
  awaitingMyAction: number;
  closedCount: number;
  statusBreakdown: Array<{ status: ErStatus; statusName: string; count: number }>;
  monthlyTrend: Array<{ month: string; count: number }>;
  recentRequests: Array<{
    id: string;
    erNumber: string;
    title: string;
    status: ErStatus;
    statusName: string;
    createdDate: string;
  }>;
}

export interface NotificationItem {
  id: string;
  type: string;
  typeName: string;
  subject: string;
  message: string;
  isRead: boolean;
  engineeringRequestId?: string;
  erNumber?: string;
  createdDate: string;
}

export const emptyErForm = (): ErFormData => ({
  divisionId: '',
  isPr: false,
  isEng: true,
  isQa: false,
  isInd: false,
  title: '',
  departmentId: '',
  productId: '',
  customer: '',
  process: '',
  detailsOfEvaluation: '',
  reasons: [],
  otherReasonDescription: '',
  presentCondition: '',
  newCondition: '',
  merit: '',
  demerit: '',
  materialDisposition: '',
  sampleQuantity: '',
  testLotIdentification: '',
  testLotDescription: '',
  applicableToChemicalOrMaterials: false,
  safetyDataSheet: '',
  chemicalLabel: '',
  chemicalClassification: '',
  chemicalInventoryManagementSystem: '',
  submit: false,
});
