import { api } from '../apiClient';

export interface Warehouse {
  warehouseId: string;
  name: string;
  isDefault: boolean;
}

interface WarehouseDto {
  warehouseID: string;
  name: string;
  isDefault: boolean;
}

export async function fetchDefaultWarehouse(): Promise<Warehouse | null> {
  const list = await api.get<WarehouseDto[]>('/api/warehouses');
  const found = list.find((w) => w.isDefault) ?? list[0];
  return found ? { warehouseId: found.warehouseID, name: found.name, isDefault: found.isDefault } : null;
}
