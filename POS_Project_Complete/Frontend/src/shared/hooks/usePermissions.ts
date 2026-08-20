import { useSession } from '../services/SessionContext';
import { PermissionCode } from '../services/authTypes';

/**
 * ⚠️ هذا Hook يتحكم فقط بما يظهر بالواجهة (إخفاء زر، تعطيل حقل...).
 * الحماية الفعلية موجودة بالـ Backend على كل Endpoint حساس (RequirePermissionAttribute).
 * لا يجوز الاعتماد على هذا الملف وحده كطبقة أمان.
 */
export function usePermissions() {
  const { user } = useSession();

  const has = (code: PermissionCode): boolean => user?.permissions.includes(code) ?? false;
  const hasAny = (codes: PermissionCode[]): boolean => codes.some(has);
  const hasAll = (codes: PermissionCode[]): boolean => codes.every(has);

  return { has, hasAny, hasAll, permissions: user?.permissions ?? [] };
}
