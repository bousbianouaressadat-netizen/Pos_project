import { useEffect, useState } from 'react';
import PageHeader from '../../shared/components/layout/PageHeader';
import StatCard from '../../shared/components/data/StatCard';
import DataTable from '../../shared/components/data/DataTable';
import LoadingState from '../../shared/components/data/LoadingState';
import { usePermissions } from '../../shared/hooks/usePermissions';
import { fetchDashboard, DashboardData } from '../../shared/services/api/dashboardService';
import './Dashboard.css';

export default function DashboardPage() {
  const { has } = usePermissions();
  const [data, setData] = useState<DashboardData | null>(null);

  useEffect(() => {
    fetchDashboard().then(setData);
  }, []);

  if (!data) return <LoadingState message="جاري تحميل لوحة التحكم..." />;

  return (
    <div>
      <PageHeader title="لوحة التحكم" subtitle="نظرة عامة على أداء اليوم" />

      <div className="dashboard__stats-grid">
        <StatCard label="مبيعات اليوم" value={`${data.todaySales.toLocaleString()} دج`} tone="success" icon="◒" />
        <StatCard label="عدد الفواتير" value={String(data.todayInvoicesCount)} icon="▤" />
        {has('CanViewProfit') && (
          <StatCard label="الربح الصافي" value={`${data.netProfitToday.toLocaleString()} دج`} tone="success" icon="◔" />
        )}
        <StatCard label="ديون العملاء" value={`${data.totalCustomerDebts.toLocaleString()} دج`} tone="warning" icon="◈" />
        <StatCard label="ديون الموردين" value={`${data.totalSupplierDebts.toLocaleString()} دج`} tone="danger" icon="⇩" />
        <StatCard label="المصاريف اليوم" value={`${data.expensesToday.toLocaleString()} دج`} icon="▣" />
        <StatCard label="رصيد الصندوق" value={`${data.currentCashBalance.toLocaleString()} دج`} tone="success" icon="▦" />
        <StatCard label="منتجات منخفضة المخزون" value={String(data.lowStockProducts.length)} tone="warning" icon="⚠" />
      </div>

      <div className="dashboard__panels">
        <section className="dashboard__panel">
          <h2 className="dashboard__panel-title">أكثر المنتجات مبيعًا</h2>
          <DataTable
            rowKey={(r) => r.productID}
            columns={[
              { key: 'name', header: 'المنتج', render: (r) => r.nameAR },
              { key: 'qty', header: 'الكمية المباعة', render: (r) => r.qtySold, align: 'end' },
              { key: 'revenue', header: 'الإيراد', render: (r) => `${r.revenue.toLocaleString()} دج`, align: 'end' },
            ]}
            rows={data.topSellingProducts}
          />
        </section>

        <section className="dashboard__panel">
          <h2 className="dashboard__panel-title">تنبيهات المخزون</h2>
          <DataTable
            rowKey={(r) => r.productID}
            columns={[
              { key: 'name', header: 'المنتج', render: (r) => r.nameAR },
              { key: 'current', header: 'الكمية الحالية', render: (r) => r.currentQuantity, align: 'end' },
              { key: 'min', header: 'الحد الأدنى', render: (r) => r.minStock, align: 'end' },
            ]}
            rows={data.lowStockProducts}
            emptyMessage="لا توجد منتجات منخفضة المخزون حاليًا"
          />
        </section>
      </div>

      <section className="dashboard__panel">
        <h2 className="dashboard__panel-title">آخر العمليات</h2>
        <DataTable
          rowKey={(r) => r.timestamp + r.description}
          columns={[
            { key: 'desc', header: 'العملية', render: (r) => r.description },
            { key: 'amount', header: 'المبلغ', render: (r) => (r.amount != null ? `${r.amount.toLocaleString()} دج` : '—'), align: 'end' },
            { key: 'time', header: 'الوقت', render: (r) => new Date(r.timestamp).toLocaleTimeString('ar-DZ', { hour: '2-digit', minute: '2-digit' }) },
          ]}
          rows={data.recentOperations}
        />
      </section>
    </div>
  );
}
