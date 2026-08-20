import { api } from '../apiClient';
import { CartLine } from '../../components/pos/cartTypes';

interface CreateSaleInvoiceRequest {
  customerID: string | null;
  warehouseID: string;
  invoiceDiscountAmount: number;
  paidAmount: number;
  paymentMethod: string;
  status: 'Held' | 'Completed';
  lines: { productID: string; qty: number; unitPrice: number; discountAmount: number }[];
}

export async function createSaleInvoice(params: {
  customerId: string | null;
  warehouseId: string;
  lines: CartLine[];
  paymentMethod: string;
  paidAmount: number;
  status: 'Held' | 'Completed';
}): Promise<{ id: string }> {
  const body: CreateSaleInvoiceRequest = {
    customerID: params.customerId,
    warehouseID: params.warehouseId,
    invoiceDiscountAmount: 0,
    paidAmount: params.paidAmount,
    paymentMethod: params.paymentMethod,
    status: params.status,
    lines: params.lines.map((l) => ({
      productID: l.productId,
      qty: l.qty,
      unitPrice: l.unitPrice,
      discountAmount: l.discountAmount,
    })),
  };

  return api.post<{ id: string }>('/api/sales/invoices', body);
}
