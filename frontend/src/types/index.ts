export type UserRole = 'Requester' | 'Safety' | 'DeptHead' | 'QA' | 'COO' | 'Admin';

export interface User {
  id: number;
  email: string;
  fullName: string;
  role: UserRole;
  department?: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface ErListItem {
  id: number;
  erNumber: string;
  submissionNumber: number;
  displayErNumber: string;
  title: string;
  division: string;
  department: string;
  product: string;
  status: string;
  outcome: string;
  requesterName: string;
  createdAt: string;
  submittedAt?: string;
  validationDate: string;
  submissionValidUntil: string;
  daysRemaining: number;
}

export interface Attachment {
  id: number;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  isImage: boolean;
  url: string;
  uploadedAt: string;
}

export interface Comment {
  id: number;
  userName: string;
  action: string;
  comment: string;
  createdAt: string;
}

export interface ErDetail extends ErListItem {
  customer: string;
  process: string;
  reasonCostDown: boolean;
  reasonAlternativeSourcing: boolean;
  reasonOthers: boolean;
  reasonOthersText?: string;
  presentDetails?: string;
  newDetails?: string;
  merit?: string;
  demerit?: string;
  materialDisposition?: string;
  sampleQuantity?: number;
  testLotType?: string;
  testLotDescription?: string;
  testLotCodeSerial?: string;
  applicableToChemicalOrMaterials: boolean;
  safetyDataSheet?: string;
  chemicalLabel?: string;
  chemicalClassification?: string;
  chemicalInventorySystem?: string;
  verifiedBySafetyUserId?: number;
  checkedByDeptHeadUserId?: number;
  checkedByQaUserId?: number;
  verifiedBySafetyName?: string;
  checkedByDeptHeadName?: string;
  checkedByQaName?: string;
  safetyDecision: string;
  deptHeadDecision: string;
  qaDecision: string;
  cooDecision: string;
  safetyComment?: string;
  deptHeadComment?: string;
  qaComment?: string;
  cooComment?: string;
  resubmitDeadline?: string;
  requesterId: number;
  updatedAt: string;
  attachments: Attachment[];
  comments: Comment[];
  canEdit: boolean;
  canVerify: boolean;
  canCooAct: boolean;
}

export interface ErFormData {
  division: number;
  title: string;
  department: string;
  product: string;
  customer: string;
  process: string;
  reasonCostDown: boolean;
  reasonAlternativeSourcing: boolean;
  reasonOthers: boolean;
  reasonOthersText: string;
  presentDetails: string;
  newDetails: string;
  merit: string;
  demerit: string;
  materialDisposition: string;
  sampleQuantity: string;
  testLotType: number | '';
  testLotDescription: string;
  testLotCodeSerial: string;
  applicableToChemicalOrMaterials: boolean;
  safetyDataSheet: string;
  chemicalLabel: string;
  chemicalClassification: string;
  chemicalInventorySystem: string;
  verifiedBySafetyUserId: number | '';
  checkedByDeptHeadUserId: number | '';
  checkedByQaUserId: number | '';
  submit: boolean;
}

export const emptyForm = (): ErFormData => ({
  division: 2,
  title: '',
  department: '',
  product: '',
  customer: '',
  process: '',
  reasonCostDown: false,
  reasonAlternativeSourcing: false,
  reasonOthers: false,
  reasonOthersText: '',
  presentDetails: '',
  newDetails: '',
  merit: '',
  demerit: '',
  materialDisposition: '',
  sampleQuantity: '',
  testLotType: '',
  testLotDescription: '',
  testLotCodeSerial: '',
  applicableToChemicalOrMaterials: false,
  safetyDataSheet: '',
  chemicalLabel: '',
  chemicalClassification: '',
  chemicalInventorySystem: '',
  verifiedBySafetyUserId: '',
  checkedByDeptHeadUserId: '',
  checkedByQaUserId: '',
  submit: false,
});

export const DIVISIONS = [
  { value: 1, label: 'PR' },
  { value: 2, label: 'ENG' },
  { value: 3, label: 'QA' },
  { value: 4, label: 'IND' },
];

export const TEST_LOT_TYPES = [
  { value: 1, label: 'Run as Normal' },
  { value: 2, label: 'Special Process' },
  { value: 3, label: 'Others' },
];
