// نفس أكواد الصلاحيات الموجودة بالـ Backend (Permission.Code) — يجب أن تبقى متطابقة حرفيًا
export type PermissionCode =
  | 'CanSell'
  | 'CanDiscount'
  | 'CanChangePrice'
  | 'CanDeleteSale'
  | 'CanReturn'
  | 'CanViewCost'
  | 'CanViewProfit'
  | 'CanModifyStock'
  | 'CanCloseCash'
  | 'CanManageUsers'
  | 'CanViewReports';

export interface SessionUser {
  userId: string;
  username: string;
  fullName: string;
  roles: string[];
  permissions: PermissionCode[];
}
