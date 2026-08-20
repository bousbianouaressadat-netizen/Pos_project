// شكل البيانات هنا يطابق DashboardDto بالـ Backend حرفيًا،
// عشان لما نربط الـ API الحقيقي لاحقًا، نستبدل فقط دالة fetchDashboard() بدون تغيير أي Component.

export interface DashboardData {
  todaySales: number;
  todayInvoicesCount: number;
  grossProfitToday: number;
  netProfitToday: number;
  totalCustomerDebts: number;
  totalSupplierDebts: number;
  expensesToday: number;
  currentCashBalance: number;
  lowStockCount: number;
  topSellingProducts: { nameAR: string; qtySold: number; revenue: number }[];
  recentInvoices: { id: string; customerName: string | null; total: number; status: string; time: string }[];
  stockAlerts: { nameAR: string; currentQuantity: number; minStock: number }[];
}

const mockDashboard: DashboardData = {
  todaySales: 84500,
  todayInvoicesCount: 27,
  grossProfitToday: 21200,
  netProfitToday: 17650,
  totalCustomerDebts: 132400,
  totalSupplierDebts: 58900,
  expensesToday: 3550,
  currentCashBalance: 96200,
  lowStockCount: 6,
  topSellingProducts: [
    { nameAR: 'خيط تطريز أزرق', qtySold: 42, revenue: 12600 },
    { nameAR: 'إبرة تطريز رفيعة', qtySold: 38, revenue: 5700 },
    { nameAR: 'قماش كتان أبيض', qtySold: 21, revenue: 9450 },
    { nameAR: 'خيط ذهبي', qtySold: 19, revenue: 8360 },
  ],
  recentInvoices: [
    { id: 'F-2026-0912', customerName: 'محل الأمل', total: 4200, status: 'Completed', time: '11:40' },
    { id: 'F-2026-0911', customerName: null, total: 850, status: 'Completed', time: '11:22' },
    { id: 'F-2026-0910', customerName: 'ورشة النور', total: 12300, status: 'PartiallyPaid', time: '10:58' },
  ],
  stockAlerts: [
    { nameAR: 'خيط أحمر قرمزي', currentQuantity: 3, minStock: 10 },
    { nameAR: 'زر معدني كبير', currentQuantity: 8, minStock: 15 },
  ],
};

export async function fetchDashboard(): Promise<DashboardData> {
  // TODO(API): استبدال هذا بـ GET /api/reports/dashboard الحقيقي
  return new Promise((resolve) => setTimeout(() => resolve(mockDashboard), 200));
}
