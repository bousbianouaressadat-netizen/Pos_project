import { CartLine, cartLineTotal } from './cartTypes';
import { usePermissions } from '../../hooks/usePermissions';
import './Cart.css';

interface CartProps {
  lines: CartLine[];
  onQtyChange: (productId: string, qty: number) => void;
  onDiscountChange: (productId: string, discount: number) => void;
  onPriceChange: (productId: string, price: number) => void;
  onRemove: (productId: string) => void;
}

export default function Cart({ lines, onQtyChange, onDiscountChange, onPriceChange, onRemove }: CartProps) {
  const { has } = usePermissions();

  if (lines.length === 0) {
    return <div className="cart cart--empty">السلة فارغة — ابحث عن منتج أو امسح الباركود لإضافته</div>;
  }

  return (
    <div className="cart">
      {lines.map((line) => (
        <div key={line.productId} className="cart__line">
          <div className="cart__line-main">
            <span className="cart__line-name">{line.nameAR}</span>
            <button className="cart__line-remove" onClick={() => onRemove(line.productId)} aria-label="حذف">
              ✕
            </button>
          </div>

          <div className="cart__line-controls">
            <div className="cart__qty">
              <button onClick={() => onQtyChange(line.productId, Math.max(1, line.qty - 1))}>−</button>
              <span>{line.qty}</span>
              <button onClick={() => onQtyChange(line.productId, line.qty + 1)}>+</button>
            </div>

            {has('CanChangePrice') ? (
              <input
                type="number"
                className="cart__price-input"
                value={line.unitPrice}
                onChange={(e) => onPriceChange(line.productId, Number(e.target.value))}
              />
            ) : (
              <span className="cart__price-static">{line.unitPrice.toLocaleString()} دج</span>
            )}

            {has('CanDiscount') && (
              <input
                type="number"
                className="cart__discount-input"
                placeholder="خصم"
                value={line.discountAmount || ''}
                onChange={(e) => onDiscountChange(line.productId, Number(e.target.value) || 0)}
              />
            )}

            <span className="cart__line-total">{cartLineTotal(line).toLocaleString()} دج</span>
          </div>
        </div>
      ))}
    </div>
  );
}
