import { useState } from 'react';
import Modal from '../overlays/Modal';
import './PaymentModal.css';

export type PaymentMethod = 'Cash' | 'Card' | 'CCP' | 'Debt' | 'Mixed';

interface PaymentModalProps {
  totalAmount: number;
  hasCustomer: boolean;
  onClose: () => void;
  onConfirm: (method: PaymentMethod, paidAmount: number) => void;
}

const methodLabels: Record<PaymentMethod, string> = {
  Cash: 'نقدي',
  Card: 'بطاقة',
  CCP: 'CCP',
  Debt: 'دين',
  Mixed: 'دفع متعدد',
};

export default function PaymentModal({ totalAmount, hasCustomer, onClose, onConfirm }: PaymentModalProps) {
  const [method, setMethod] = useState<PaymentMethod>('Cash');
  const [paidAmount, setPaidAmount] = useState(totalAmount);

  const remaining = totalAmount - paidAmount;

  return (
    <Modal title="إتمام الدفع" onClose={onClose} width={440}>
      <div className="payment-modal__total">{totalAmount.toLocaleString()} دج</div>

      <div className="payment-modal__methods">
        {(Object.keys(methodLabels) as PaymentMethod[]).map((m) => (
          <button
            key={m}
            className={`payment-modal__method-btn ${method === m ? 'payment-modal__method-btn--active' : ''}`}
            onClick={() => {
              setMethod(m);
              if (m === 'Debt') setPaidAmount(0);
              if (m !== 'Debt' && m !== 'Mixed') setPaidAmount(totalAmount);
            }}
            disabled={m === 'Debt' && !hasCustomer}
            title={m === 'Debt' && !hasCustomer ? 'اختر عميلًا أولًا للبيع بالدين' : undefined}
          >
            {methodLabels[m]}
          </button>
        ))}
      </div>

      {(method === 'Mixed' || method === 'Debt') && (
        <div className="payment-modal__paid-field">
          <label>المبلغ المدفوع الآن</label>
          <input
            type="number"
            value={paidAmount}
            onChange={(e) => setPaidAmount(Number(e.target.value))}
            min={0}
            max={totalAmount}
          />
          {remaining > 0 && hasCustomer && (
            <p className="payment-modal__remaining">الباقي ({remaining.toLocaleString()} دج) يُسجَّل كدين على العميل</p>
          )}
        </div>
      )}

      <button className="payment-modal__confirm" onClick={() => onConfirm(method, paidAmount)}>
        تأكيد الدفع (F8)
      </button>
    </Modal>
  );
}
