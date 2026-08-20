import { useState } from 'react';
import Modal from '../overlays/Modal';
import './CustomerSelector.css';

export interface CustomerOption {
  customerId: string;
  name: string;
  balance: number;
}

const mockCustomers: CustomerOption[] = [
  { customerId: 'c1', name: 'محل الأمل', balance: 4200 },
  { customerId: 'c2', name: 'ورشة النور', balance: -1800 },
  { customerId: 'c3', name: 'مؤسسة الرحمة', balance: 0 },
];

interface CustomerSelectorProps {
  selected: CustomerOption | null;
  onSelect: (customer: CustomerOption | null) => void;
  onClose: () => void;
}

export default function CustomerSelector({ selected, onSelect, onClose }: CustomerSelectorProps) {
  const [search, setSearch] = useState('');

  const filtered = mockCustomers.filter((c) => c.name.includes(search));

  return (
    <Modal title="اختيار العميل" onClose={onClose} width={420}>
      <input
        className="customer-selector__search"
        placeholder="ابحث عن عميل..."
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        autoFocus
      />

      <button
        className={`customer-selector__item ${!selected ? 'customer-selector__item--active' : ''}`}
        onClick={() => { onSelect(null); onClose(); }}
      >
        عميل نقدي (بدون تسجيل)
      </button>

      {filtered.map((c) => (
        <button
          key={c.customerId}
          className={`customer-selector__item ${selected?.customerId === c.customerId ? 'customer-selector__item--active' : ''}`}
          onClick={() => { onSelect(c); onClose(); }}
        >
          <span>{c.name}</span>
          <span className={c.balance > 0 ? 'customer-selector__debt' : 'customer-selector__credit'}>
            {c.balance > 0 ? `دين: ${c.balance.toLocaleString()} دج` : 'لا يوجد دين'}
          </span>
        </button>
      ))}
    </Modal>
  );
}
