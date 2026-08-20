import { api, setToken } from './apiClient';
import { SessionUser, PermissionCode } from './authTypes';

interface LoginResponseDto {
  token: string;
  userID: string;
  username: string;
  fullName: string;
  roles: string[];
  permissions: PermissionCode[];
}

export async function login(username: string, password: string): Promise<SessionUser> {
  const result = await api.post<LoginResponseDto>('/api/auth/login', { username, password });

  setToken(result.token);

  return {
    userId: result.userID,
    username: result.username,
    fullName: result.fullName,
    roles: result.roles,
    permissions: result.permissions,
  };
}

export function logout() {
  setToken(null);
}
