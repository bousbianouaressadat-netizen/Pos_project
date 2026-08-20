export interface PosProduct {
  productId: string;
  sku: string;
  nameAR: string;
  price: number;
  taxRate: number;
  categoryId: string;
  stock: number;
  imageEmoji: string; // بديل مؤقت عن صورة حقيقية
}

export interface PosCategory {
  categoryId: string;
  nameAR: string;
}

export const mockCategories: PosCategory[] = [
  { categoryId: 'all', nameAR: 'الكل' },
  { categoryId: 'thread', nameAR: 'خيوط' },
  { categoryId: 'fabric', nameAR: 'أقمشة' },
  { categoryId: 'tools', nameAR: 'أدوات' },
  { categoryId: 'accessories', nameAR: 'إكسسوارات' },
];

export const mockProducts: PosProduct[] = [
  { productId: 'p1', sku: 'THR-001', nameAR: 'خيط تطريز أزرق', price: 300, taxRate: 19, categoryId: 'thread', stock: 42, imageEmoji: '🧵' },
  { productId: 'p2', sku: 'THR-002', nameAR: 'خيط أحمر قرمزي', price: 300, taxRate: 19, categoryId: 'thread', stock: 3, imageEmoji: '🧵' },
  { productId: 'p3', sku: 'THR-003', nameAR: 'خيط ذهبي', price: 440, taxRate: 19, categoryId: 'thread', stock: 19, imageEmoji: '🧵' },
  { productId: 'p4', sku: 'FAB-001', nameAR: 'قماش كتان أبيض', price: 450, taxRate: 19, categoryId: 'fabric', stock: 21, imageEmoji: '🧶' },
  { productId: 'p5', sku: 'FAB-002', nameAR: 'قماش حرير أسود', price: 620, taxRate: 19, categoryId: 'fabric', stock: 12, imageEmoji: '🧶' },
  { productId: 'p6', sku: 'TL-001', nameAR: 'إبرة تطريز رفيعة', price: 150, taxRate: 19, categoryId: 'tools', stock: 38, imageEmoji: '📍' },
  { productId: 'p7', sku: 'TL-002', nameAR: 'مقص تفصيل', price: 900, taxRate: 19, categoryId: 'tools', stock: 7, imageEmoji: '✂️' },
  { productId: 'p8', sku: 'ACC-001', nameAR: 'زر معدني كبير', price: 40, taxRate: 19, categoryId: 'accessories', stock: 8, imageEmoji: '🔘' },
];

export async function fetchPosProducts(): Promise<PosProduct[]> {
  // TODO(API): استبدال بـ GET /api/products
  return new Promise((resolve) => setTimeout(() => resolve(mockProducts), 150));
}

export async function fetchPosCategories(): Promise<PosCategory[]> {
  // TODO(API): استبدال بـ GET /api/categories
  return new Promise((resolve) => setTimeout(() => resolve(mockCategories), 100));
}

export async function fetchProductByBarcode(code: string): Promise<PosProduct | null> {
  // TODO(API): استبدال بـ GET /api/products/barcode/{code}
  const found = mockProducts.find((p) => p.sku === code);
  return new Promise((resolve) => setTimeout(() => resolve(found ?? null), 100));
}
