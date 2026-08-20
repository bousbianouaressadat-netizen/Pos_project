import { api } from '../apiClient';

export interface PosProduct {
  productId: string;
  sku: string;
  nameAR: string;
  price: number;
  taxRate: number;
  categoryNameAR: string | null;
}

interface ProductListItemDto {
  productID: string;
  sku: string;
  nameAR: string;
  nameFR: string;
  categoryNameAR: string | null;
  unitSymbol: string | null;
  price: number;
  taxRate: number;
  isActive: boolean;
}

interface ProductByBarcodeDto {
  productID: string;
  sku: string;
  nameAR: string;
  nameFR: string;
  price: number;
  taxRate: number;
}

function mapProduct(dto: ProductListItemDto): PosProduct {
  return {
    productId: dto.productID,
    sku: dto.sku,
    nameAR: dto.nameAR,
    price: dto.price,
    taxRate: dto.taxRate,
    categoryNameAR: dto.categoryNameAR,
  };
}

export async function fetchPosProducts(search?: string): Promise<PosProduct[]> {
  const query = search ? `?search=${encodeURIComponent(search)}` : '';
  const list = await api.get<ProductListItemDto[]>(`/api/products${query}`);
  return list.filter((p) => p.isActive).map(mapProduct);
}

export interface PosCategory {
  categoryId: string;
  nameAR: string;
}

interface CategoryDto {
  categoryID: string;
  nameAR: string;
  nameFR: string;
  parentCategoryID: string | null;
  isActive: boolean;
}

export async function fetchPosCategories(): Promise<PosCategory[]> {
  const list = await api.get<CategoryDto[]>('/api/categories');
  return [
    { categoryId: 'all', nameAR: 'الكل' },
    ...list.filter((c) => c.isActive).map((c) => ({ categoryId: c.categoryID, nameAR: c.nameAR })),
  ];
}

export async function fetchProductByBarcode(code: string): Promise<PosProduct | null> {
  try {
    const dto = await api.get<ProductByBarcodeDto>(`/api/products/barcode/${encodeURIComponent(code)}`);
    return { productId: dto.productID, sku: dto.sku, nameAR: dto.nameAR, price: dto.price, taxRate: dto.taxRate, categoryNameAR: null };
  } catch {
    return null;
  }
}
