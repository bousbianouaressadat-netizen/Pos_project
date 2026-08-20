export interface CartLine {
  productId: string;
  nameAR: string;
  unitPrice: number;
  qty: number;
  discountAmount: number;
  taxRate: number;
}

export function cartLineTotal(line: CartLine): number {
  const subtotal = line.qty * line.unitPrice - line.discountAmount;
  const tax = subtotal * (line.taxRate / 100);
  return subtotal + tax;
}
