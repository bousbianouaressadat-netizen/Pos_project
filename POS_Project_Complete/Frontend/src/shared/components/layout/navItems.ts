export interface NavItem {
  path: string;
  label: string;
  icon: string; // اسم أيقونة رمزي بسيط (نص/Emoji مؤقتًا، يُستبدل لاحقًا بمكتبة أيقونات حقيقية)
}

export const navItems: NavItem[] = [
  { path: '/dashboard', label: 'لوحة التحكم', icon: '◧' },
  { path: '/pos', label: 'نقطة البيع', icon: '⛁' },
  { path: '/products', label: 'المنتجات', icon: '▤' },
  { path: '/customers', label: 'العملاء', icon: '◔' },
  { path: '/purchases', label: 'المشتريات', icon: '⇩' },
  { path: '/inventory', label: 'المخزون', icon: '▦' },
  { path: '/payments', label: 'الديون والمدفوعات', icon: '◈' },
  { path: '/cash', label: 'الصندوق', icon: '▣' },
  { path: '/reports', label: 'التقارير', icon: '▲' },
  { path: '/settings', label: 'الإعدادات', icon: '◍' },
];
