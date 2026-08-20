import { api } from '../apiClient';

// يطابق DashboardDto بالـ Backend حرفيًا (نفس أسماء الحقول بعد camelCase التلقائي من System.Text.Json)
export interface DashboardData {
  todaySales: number;
  todayInvoicesCount: number;
  todayCOGS: number;
  grossProfitToday: number;
  expensesToday: number;
  netProfitToday: number;
  totalCollectionsToday: number;
  totalCustomerDebts: number;
  totalSupplierDebts: number;
  currentCashBalance: number;
  topSellingProducts: { productID: string; nameAR: string; qtySold: number; revenue: number }[];
  lowStockProducts: { productID: string; nameAR: string; currentQuantity: number; minStock: number }[];
  recentOperations: { type: string; description: string; amount: number | null; timestamp: string }[];
}

export async function fetchDashboard(): Promise<DashboardData> {
  return api.get<DashboardData>('/api/reports/dashboard');
}
